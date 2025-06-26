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
        // 子命令标识符（使用点号分隔）
        private const char SUB_COMMAND_IDENTIFIER = '.';

        // 子数据库常量定义
        public const string DATABASE_CHARACTERS_BASE = "characters";          // 基础角色命令数据库
        public const string DATABASE_CHARACTERS_SPRITE = "characters_sprite"; // 精灵角色命令数据库
        public const string DATABASE_CHARACTERS_LIVE2D = "characters_live2D"; // Live2D角色命令数据库
        public const string DATABASE_CHARACTERS_Model3D = "characters_model3D"; // 3D模型角色命令数据库

        // 单例实例
        public static CommandManager instance { get; private set; }

        // 主命令数据库
        private CommandDatabase database;

        // 子命令数据库字典
        private Dictionary<string, CommandDatabase> subDatabases = new Dictionary<string, CommandDatabase>();

        // 当前活动的命令进程列表
        public List<CommandProcess> activeProcesses = new List<CommandProcess>();

        // 获取顶部命令进程（最后启动的进程）
        private CommandProcess topProcess => activeProcesses.Last();

        // 初始化单例和命令数据库
        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                database = new CommandDatabase();  // 创建主命令数据库

                // 使用反射加载所有数据库扩展
                Assembly assembly = Assembly.GetExecutingAssembly();
                Type[] extensionTypes = assembly.GetTypes().Where(t => t.IsSubclassOf(typeof(CMD_DatabaseExtension))).ToArray();

                // 调用每个扩展类的Extend方法
                foreach (Type extension in extensionTypes)
                {
                    MethodInfo extendMethod = extension.GetMethod("Extend");
                    extendMethod.Invoke(null, new object[] { database });
                }
            }
            else
                DestroyImmediate(gameObject);  // 确保单例唯一性
        }

        // 执行命令的主要入口点
        public CoroutineWrapper Execute(string commandName, params string[] args)
        {
            // 检查是否为子命令（包含点号）
            if (commandName.Contains(SUB_COMMAND_IDENTIFIER))
                return ExecuteSubCommand(commandName, args);

            // 从主数据库获取命令委托
            Delegate command = database.GetCommand(commandName);

            if (command == null)
                return null;  // 命令不存在

            // 启动命令进程
            return StartProcess(commandName, command, args);
        }

        // 执行子命令
        private CoroutineWrapper ExecuteSubCommand(string commandName, string[] args)
        {
            // 分割数据库名和命令名
            string[] parts = commandName.Split(SUB_COMMAND_IDENTIFIER);
            string databaseName = string.Join(SUB_COMMAND_IDENTIFIER, parts.Take(parts.Length - 1));
            string subCommandName = parts.Last();

            // 检查子数据库是否存在
            if (subDatabases.ContainsKey(databaseName))
            {
                Delegate command = subDatabases[databaseName].GetCommand(subCommandName);
                if (command != null)
                {
                    return StartProcess(commandName, command, args);
                }
                else
                {
                    Debug.LogError($"在子命令数据库 '{databaseName}' 中未找到命令 '{subCommandName}'");
                    return null;
                }
            }

            // 尝试作为角色命令执行
            string characterName = databaseName;
            if (CharacterManager.instance.HasCharacter(characterName))
            {
                // 将角色名作为第一个参数
                List<string> newArgs = new List<string>(args);
                newArgs.Insert(0, characterName);
                args = newArgs.ToArray();

                // 执行角色命令
                return ExecuteCharacterCommand(subCommandName, args);
            }

            Debug.LogError($"子命令数据库 '{databaseName}' 不存在，无法执行命令 '{subCommandName}'");
            return null;
        }

        // 执行角色命令
        private CoroutineWrapper ExecuteCharacterCommand(string commandName, params string[] args)
        {
            Delegate command = null;

            // 首先尝试基础角色命令
            CommandDatabase db = subDatabases[DATABASE_CHARACTERS_BASE];
            if (db.HasCommand(commandName))
            {
                command = db.GetCommand(commandName);
                return StartProcess(commandName, command, args);
            }

            // 根据角色类型选择对应的子数据库
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
                    // 3D模型角色暂未实现
                    //case Character.CharacterType.Model3D:
                    //    db = subDatabases[DATABASE_CHARACTERS_Model3D];
                    //    break;
            }

            // 获取特定角色类型的命令
            command = db.GetCommand(commandName);

            if (command != null)
                return StartProcess(commandName, command, args);

            Debug.LogError($"在角色类型 '{args[0]}' 上无法执行 '{commandName}' 命令，该命令可能不存在");
            return null;
        }

        // 启动新命令进程
        private CoroutineWrapper StartProcess(string commandName, Delegate command, string[] args)
        {
            // 创建唯一的进程ID
            System.Guid processID = System.Guid.NewGuid();

            // 创建命令进程对象
            CommandProcess cmd = new CommandProcess(processID, commandName, command, null, args, null);
            activeProcesses.Add(cmd);

            // 启动协程执行命令
            Coroutine co = StartCoroutine(RunningProcess(cmd));

            // 包装协程并存储引用
            cmd.runningProcess = new CoroutineWrapper(this, co);

            return cmd.runningProcess;
        }

        // 停止当前顶部进程
        public void StopCurrentProcess()
        {
            if (topProcess != null)
                KillProcess(topProcess);
        }

        // 停止所有活动进程
        public void StopAllProcesses()
        {
            foreach (var c in activeProcesses)
            {
                if (c.runningProcess != null && !c.runningProcess.IsDone)
                    c.runningProcess.Stop();  // 停止协程

                c.onTerminateAction?.Invoke();  // 执行终止回调
            }

            activeProcesses.Clear();  // 清空进程列表
        }

        // 运行命令进程的协程
        private IEnumerator RunningProcess(CommandProcess process)
        {
            // 等待命令执行完成
            yield return WaitingForProcessToComplete(process.command, process.args);

            // 命令完成后移除进程
            KillProcess(process);
        }

        // 终止指定进程
        public void KillProcess(CommandProcess cmd)
        {
            activeProcesses.Remove(cmd);  // 从活动列表移除

            // 停止关联的协程
            if (cmd.runningProcess != null && !cmd.runningProcess.IsDone)
                cmd.runningProcess.Stop();

            // 执行终止回调
            cmd.onTerminateAction?.Invoke();
        }

        // 等待命令完成的协程
        private IEnumerator WaitingForProcessToComplete(Delegate command, string[] args)
        {
            // 根据委托类型动态调用命令
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

        // 为当前顶部进程添加终止回调
        public void AddTerminationActionToCurrentProcess(UnityAction action)
        {
            CommandProcess process = topProcess;

            if (process == null)
                return;

            // 创建或获取终止事件
            if (process.onTerminateAction == null)
                process.onTerminateAction = new UnityEvent();

            // 添加回调
            process.onTerminateAction.AddListener(action);
        }

        // 创建子命令数据库
        public CommandDatabase CreateSubDatabase(string name)
        {
            name = name.ToLower();

            // 检查数据库是否已存在
            if (subDatabases.TryGetValue(name, out CommandDatabase db))
            {
                Debug.LogWarning($"子命令数据库 '{name}' 已存在，返回现有数据库");
                return db;
            }

            // 创建新数据库
            CommandDatabase newDatabase = new CommandDatabase();
            subDatabases.Add(name, newDatabase);

            return newDatabase;
        }
    }
}