using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using System.Linq;
using System;
using UnityEngine.Events;
using CHARACTERS;

namespace COMMANDS
{
    public class CommandManager : MonoBehaviour
    {
        // �������ʶ����ʹ�õ�ŷָ���
        private const char SUB_COMMAND_IDENTIFIER = '.';

        // �����ݿⳣ������
        public const string DATABASE_CHARACTERS_BASE = "characters";          // ������ɫ�������ݿ�
        public const string DATABASE_CHARACTERS_SPRITE = "characters_sprite"; // �����ɫ�������ݿ�
        public const string DATABASE_CHARACTERS_LIVE2D = "characters_live2D"; // Live2D��ɫ�������ݿ�
        public const string DATABASE_CHARACTERS_Model3D = "characters_model3D"; // 3Dģ�ͽ�ɫ�������ݿ�

        // ����ʵ��
        public static CommandManager instance { get; private set; }

        // ���������ݿ�
        private CommandDatabase database;

        // ���������ݿ��ֵ�
        private Dictionary<string, CommandDatabase> subDatabases = new Dictionary<string, CommandDatabase>();

        // ��ǰ�����������б�
        public List<CommandProcess> activeProcesses = new List<CommandProcess>();

        // ��ȡ����������̣���������Ľ��̣�
        private CommandProcess topProcess => activeProcesses.Last();

        // ��ʼ���������������ݿ�
        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                database = new CommandDatabase();  // �������������ݿ�

                // ʹ�÷�������������ݿ���չ
                Assembly assembly = Assembly.GetExecutingAssembly();
                Type[] extensionTypes = assembly.GetTypes().Where(t => t.IsSubclassOf(typeof(CMD_DatabaseExtension))).ToArray();

                // ����ÿ����չ���Extend����
                foreach (Type extension in extensionTypes)
                {
                    MethodInfo extendMethod = extension.GetMethod("Extend");
                    extendMethod.Invoke(null, new object[] { database });
                }
            }
            else
                DestroyImmediate(gameObject);  // ȷ������Ψһ��
        }

        // ִ���������Ҫ��ڵ�
        public CoroutineWrapper Execute(string commandName, params string[] args)
        {
            // ����Ƿ�Ϊ�����������ţ�
            if (commandName.Contains(SUB_COMMAND_IDENTIFIER))
                return ExecuteSubCommand(commandName, args);

            // �������ݿ��ȡ����ί��
            Delegate command = database.GetCommand(commandName);

            if (command == null)
                return null;  // �������

            // �����������
            return StartProcess(commandName, command, args);
        }

        // ִ��������
        private CoroutineWrapper ExecuteSubCommand(string commandName, string[] args)
        {
            // �ָ����ݿ�����������
            string[] parts = commandName.Split(SUB_COMMAND_IDENTIFIER);
            string databaseName = string.Join(SUB_COMMAND_IDENTIFIER, parts.Take(parts.Length - 1));
            string subCommandName = parts.Last();

            // ��������ݿ��Ƿ����
            if (subDatabases.ContainsKey(databaseName))
            {
                Delegate command = subDatabases[databaseName].GetCommand(subCommandName);
                if (command != null)
                {
                    return StartProcess(commandName, command, args);
                }
                else
                {
                    Debug.LogError($"�����������ݿ� '{databaseName}' ��δ�ҵ����� '{subCommandName}'");
                    return null;
                }
            }

            // ������Ϊ��ɫ����ִ��
            string characterName = databaseName;
            if (CharacterManager.instance.HasCharacter(characterName))
            {
                // ����ɫ����Ϊ��һ������
                List<string> newArgs = new List<string>(args);
                newArgs.Insert(0, characterName);
                args = newArgs.ToArray();

                // ִ�н�ɫ����
                return ExecuteCharacterCommand(subCommandName, args);
            }

            Debug.LogError($"���������ݿ� '{databaseName}' �����ڣ��޷�ִ������ '{subCommandName}'");
            return null;
        }

        // ִ�н�ɫ����
        private CoroutineWrapper ExecuteCharacterCommand(string commandName, params string[] args)
        {
            Delegate command = null;

            // ���ȳ��Ի�����ɫ����
            CommandDatabase db = subDatabases[DATABASE_CHARACTERS_BASE];
            if (db.HasCommand(commandName))
            {
                command = db.GetCommand(commandName);
                return StartProcess(commandName, command, args);
            }

            // ���ݽ�ɫ����ѡ���Ӧ�������ݿ�
            CharacterConfigData characterConfigData = CharacterManager.instance.GetCharacterConfig(args[0]);
            switch (characterConfigData.characterType)
            {
                case Character.CharacterType.Sprite:
                case Character.CharacterType.SpriteSheet:
                    db = subDatabases[DATABASE_CHARACTERS_SPRITE];
                    break;
                case Character.CharacterType.Live2D:
                    db = subDatabases[DATABASE_CHARACTERS_LIVE2D];
                    break;
                    // 3Dģ�ͽ�ɫ��δʵ��
                    //case Character.CharacterType.Model3D:
                    //    db = subDatabases[DATABASE_CHARACTERS_Model3D];
                    //    break;
            }

            // ��ȡ�ض���ɫ���͵�����
            command = db.GetCommand(commandName);

            if (command != null)
                return StartProcess(commandName, command, args);

            Debug.LogError($"�ڽ�ɫ���� '{args[0]}' ���޷�ִ�� '{commandName}' �����������ܲ�����");
            return null;
        }

        // �������������
        private CoroutineWrapper StartProcess(string commandName, Delegate command, string[] args)
        {
            // ����Ψһ�Ľ���ID
            System.Guid processID = System.Guid.NewGuid();

            // ����������̶���
            CommandProcess cmd = new CommandProcess(processID, commandName, command, null, args, null);
            activeProcesses.Add(cmd);

            // ����Э��ִ������
            Coroutine co = StartCoroutine(RunningProcess(cmd));

            // ��װЭ�̲��洢����
            cmd.runningProcess = new CoroutineWrapper(this, co);

            return cmd.runningProcess;
        }

        // ֹͣ��ǰ��������
        public void StopCurrentProcess()
        {
            if (topProcess != null)
                KillProcess(topProcess);
        }

        // ֹͣ���л����
        public void StopAllProcesses()
        {
            foreach (var c in activeProcesses)
            {
                if (c.runningProcess != null && !c.runningProcess.IsDone)
                    c.runningProcess.Stop();  // ֹͣЭ��

                c.onTerminateAction?.Invoke();  // ִ����ֹ�ص�
            }

            activeProcesses.Clear();  // ��ս����б�
        }

        // ����������̵�Э��
        private IEnumerator RunningProcess(CommandProcess process)
        {
            // �ȴ�����ִ�����
            yield return WaitingForProcessToComplete(process.command, process.args);

            // ������ɺ��Ƴ�����
            KillProcess(process);
        }

        // ��ָֹ������
        public void KillProcess(CommandProcess cmd)
        {
            activeProcesses.Remove(cmd);  // �ӻ�б��Ƴ�

            // ֹͣ������Э��
            if (cmd.runningProcess != null && !cmd.runningProcess.IsDone)
                cmd.runningProcess.Stop();

            // ִ����ֹ�ص�
            cmd.onTerminateAction?.Invoke();
        }

        // �ȴ�������ɵ�Э��
        private IEnumerator WaitingForProcessToComplete(Delegate command, string[] args)
        {
            // ����ί�����Ͷ�̬��������
            if (command is Action)
                command.DynamicInvoke();
            else if (command is Action<string>)
                command.DynamicInvoke(args[0]);
            else if (command is Action<string[]>)
                command.DynamicInvoke((object)args);
            else if (command is Func<IEnumerator>)
                yield return ((Func<IEnumerator>)command)();
            else if (command is Func<string, IEnumerator>)
                yield return ((Func<string, IEnumerator>)command)(args[0]);
            else if (command is Func<string[], IEnumerator>)
                yield return ((Func<string[], IEnumerator>)command)(args);
        }

        // Ϊ��ǰ�������������ֹ�ص�
        public void AddTerminationActionToCurrentProcess(UnityAction action)
        {
            CommandProcess process = topProcess;

            if (process == null)
                return;

            // �������ȡ��ֹ�¼�
            if (process.onTerminateAction == null)
                process.onTerminateAction = new UnityEvent();

            // ��ӻص�
            process.onTerminateAction.AddListener(action);
        }

        // �������������ݿ�
        public CommandDatabase CreateSubDatabase(string name)
        {
            name = name.ToLower();

            // ������ݿ��Ƿ��Ѵ���
            if (subDatabases.TryGetValue(name, out CommandDatabase db))
            {
                Debug.LogWarning($"���������ݿ� '{name}' �Ѵ��ڣ������������ݿ�");
                return db;
            }

            // ���������ݿ�
            CommandDatabase newDatabase = new CommandDatabase();
            subDatabases.Add(name, newDatabase);

            return newDatabase;
        }
    }
}