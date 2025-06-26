using CHARACTERS;
using COMMANDS;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DIALOGUE
{
    /// <summary>
    /// 对话管理器类，负责管理对话的流程，包括对话的显示、命令的执行以及用户输入的处理。
    /// </summary>
    public class ConversationManager
    {
        // 获取对话系统的单例实例
        private DialogueSystem dialogueSystem => DialogueSystem.instance;
        // 当前运行的对话协程
        private Coroutine process = null;
        // 是否正在运行对话
        public bool isRunning => process != null;

        // 文本构建器，用于显示对话内容
        private TextArchitect architect = null;

        // 用户输入提示标志
        private bool userPrompt = false;

        /// <summary>
        /// 构造函数，初始化对话管理器。
        /// </summary>
        /// <param name="architect">文本构建器实例</param>
        public ConversationManager(TextArchitect architect)
        {
            this.architect = architect;
            // 订阅用户输入事件
            dialogueSystem.onUserPrompt_Next += OnUserPrompt_Next;
        }

        /// <summary>
        /// 用户输入事件的回调函数，设置用户提示标志为 true。
        /// </summary>
        private void OnUserPrompt_Next()
        {
            userPrompt = true;
        }

        /// <summary>
        /// 开始一段对话。
        /// </summary>
        /// <param name="conversation">对话内容列表</param>
        public Coroutine StartConversation(List<string> conversation)
        {
            // 停止当前对话（如果有）
            StopConversation();
            // 启动新的对话协程
            process = dialogueSystem.StartCoroutine(RuningConversation(conversation));

            return process;
        }

        /// <summary>
        /// 停止当前对话。
        /// </summary>
        public void StopConversation()
        {
            if (!isRunning)
                return;
            // 停止当前对话协程
            dialogueSystem.StopCoroutine(process);
            process = null;
        }

        /// <summary>
        /// 对话运行的主协程，逐行处理对话内容。
        /// </summary>
        /// <param name="conversation">对话内容列表</param>
        /// <returns>协程枚举器</returns>
        IEnumerator RuningConversation(List<string> conversation)
        {
            for (int i = 0; i < conversation.Count; i++)
            {
                // 跳过空行
                if (string.IsNullOrWhiteSpace(conversation[i]))
                    continue;

                // 解析当前行的对话内容
                DIALOGUE_LINE line = DialogueParser.Parse(conversation[i]);

                // 如果有对话内容，运行对话逻辑
                if (line.hasDialogue)
                    yield return Line_RunDialogue(line);

                // 如果有命令，运行命令逻辑
                if (line.hasCommands)
                    yield return Line_RunCommands(line);

                if (line.hasDialogue)
                //等待用户输入
                {
                    yield return WaitForUserInput();

                    CommandManager.instance.StopAllProcesses(); // 停止所有命令处理
                }

            }
        }

        /// <summary>
        /// 处理一行对话的逻辑。
        /// </summary>
        /// <param name="line">对话行对象</param>
        /// <returns>协程枚举器</returns>
        IEnumerator Line_RunDialogue(DIALOGUE_LINE line)
        {
            //说话人姓名展示与否
            if (line.hasSpeaker)
                HandleSpeakerLogic(line.speakerData);
            // else
            //     dialogueSystem.HideSpeakerName();

            // 构建并显示对话内容
            yield return BuildLineSegments(line.dialogueData);

            //等待用户输入
            // yield return WaitForUserInput();

        }

        private void HandleSpeakerLogic(DL_SPEAKER_DATA speakerData)
        {

            bool characterMustBeCreated = (speakerData.makeCharacterEnter || speakerData.isCastingPosition || speakerData.isCastingExpression);

            Character character = CharacterManager.instance.GetCharacter(speakerData.name, createIfDoesNotExist: characterMustBeCreated);
            // 如果说话者不存在，则创建角色
            if (speakerData.makeCharacterEnter && (!character.isVisible && !character.isRevealing))
                character.Show();

            dialogueSystem.ShowSpeakerName(speakerData.displayName);

            DialogueSystem.instance.ApplySpeakerDataToDialogueContainer(speakerData.name);

            if (speakerData.isCastingPosition)
                character.MoveToPosition(speakerData.castPosition);

            if (speakerData.isCastingExpression)
            {
                foreach (var ce in speakerData.CastExpressions)
                    character.OnReceiveCastingExpression(ce.Layer, ce.expression);
            }
        }

        /// <summary>
        /// 处理一行命令的逻辑。
        /// </summary>
        /// <param name="line">对话行对象</param>
        /// <returns>协程枚举器</returns>
        IEnumerator Line_RunCommands(DIALOGUE_LINE line)
        {
            // 打印命令内容（可扩展为实际命令处理逻辑）
            // Debug.Log(line.commandsData);
            List<DL_COMAND_DATA.Command> commands = line.commandsData.commands;

            foreach (DL_COMAND_DATA.Command command in commands)
            {
                if (command.waitForCompletion || command.name == "wait")
                {
                    CoroutineWrapper cw = CommandManager.instance.Execute(command.name, command.arguments);
                    while (!cw.IsDone)
                    {
                        if (userPrompt)
                        {
                            CommandManager.instance.StopCurrentProcess();
                            userPrompt = false;
                        }
                        yield return null;
                    }
                }
                else
                    CommandManager.instance.Execute(command.name, command.arguments);
            }
            yield return null;
        }

        IEnumerator BuildLineSegments(DL_DIALOGUE_DATA line)
        {
            // 逐段构建对话内容
            for (int i = 0; i < line.segments.Count; i++)
            {
                DL_DIALOGUE_DATA.DIALOGUE_SEGMENT segment = line.segments[i];
                // 构建并显示对话内容
                yield return WaitForDialogueSegmentSignalToBeTriggered(segment);
                // 等待用户输入
                yield return BuildDialogue(segment.dialogue, segment.appendText);
            }
        }

        IEnumerator WaitForDialogueSegmentSignalToBeTriggered(DL_DIALOGUE_DATA.DIALOGUE_SEGMENT segment)
        {
            switch (segment.startSignal)
            {
                case DL_DIALOGUE_DATA.DIALOGUE_SEGMENT.StartSignal.C:
                case DL_DIALOGUE_DATA.DIALOGUE_SEGMENT.StartSignal.A:
                    yield return WaitForUserInput();
                    break;
                case DL_DIALOGUE_DATA.DIALOGUE_SEGMENT.StartSignal.WC:
                case DL_DIALOGUE_DATA.DIALOGUE_SEGMENT.StartSignal.WA:
                    yield return new WaitForSeconds(segment.signalDelay);
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// 构建并显示对话内容。
        /// </summary>
        /// <param name="dialogue">对话内容字符串</param>
        /// <returns>协程枚举器</returns>
        IEnumerator BuildDialogue(string dialogue, bool append = false)
        {
            if (!append)
                architect.Build(dialogue);// 清空文本构建器
            else
                architect.Append(dialogue);// 追加文本构建器    
                                           // 使用文本构建器构建对话内容
                                           // architect.Build(dialogue);
                                           // 等待对话内容构建完成
            while (architect.isBuilding)
            {
                // 如果用户提示加速或完成对话
                if (userPrompt)
                {
                    if (!architect.hurryUp)
                        architect.hurryUp = true;// 加速显示
                    else
                        architect.ForceComplete();// 强制完成显示

                    userPrompt = false;
                }
                yield return null;
            }
        }

        /// <summary>
        /// 等待用户输入的协程。
        /// </summary>
        /// <returns>协程枚举器</returns>
        IEnumerator WaitForUserInput()
        {
            // 等待用户输入提示
            while (!userPrompt)
                yield return null;

            userPrompt = false;
        }
    }
}