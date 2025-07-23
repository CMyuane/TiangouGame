﻿using CHARACTERS;
using System.Collections.Generic;
using UnityEngine;

//对话系统
namespace DIALOGUE
{
    /// <summary>
    /// 对话系统类，负责管理对话的显示和逻辑。
    /// </summary>
    public class DialogueSystem : MonoBehaviour
    {
        [SerializeField] private DialogueSystemConfigurationSO _config;
        public DialogueSystemConfigurationSO config => _config; // 对话系统配置文件

        //对话容器和序列化字段
        public DialogueContainer dialogueContainer = new DialogueContainer();

        private ConversationManager conversationManager;// 对话管理器
        private TextArchitect architect;// 文本构建器

        public static DialogueSystem instance { get; private set; }              // 单例模式实例

        // 对话系统事件，用于响应用户输入
        public delegate void DialogueSystemEvent();

        public event DialogueSystemEvent onUserPrompt_Next;

        // 是否正在运行对话
        public bool isRunningConversation => conversationManager.isRunning;

        public DialogueContinuePrompt prompt;

        private void Awake()
        {
            // 初始化单例
            if (instance == null)
            {
                instance = this;
                Initialize();
            }
            else
                DestroyImmediate(gameObject);
        }

        private bool _initialized = false;

        /// <summary>
        /// 初始化对话系统。
        /// </summary>
        private void Initialize()
        {
            if (_initialized)
                return;
            // 初始化文本构建器和对话管理器
            architect = new TextArchitect(dialogueContainer.dialogueText);
            //architect = new TextArchitect(dialogueContainer.dialogueText, TABuilder.BuilderTypes.Typewriter);
            conversationManager = new ConversationManager(architect);
        }

        /// <summary>
        /// 用户提示对话系统推进到下一步。
        /// </summary>
        public void OnUserPrompt_Next()
        {
            onUserPrompt_Next?.Invoke();
        }

        public void ApplySpeakerDataToDialogueContainer(string speakerName)
        {
            Character character = CharacterManager.instance.GetCharacter(speakerName);
            CharacterConfigData config = character != null ? character.config : CharacterManager.instance.GetCharacterConfig(speakerName);

            ApplySpeakerDataToDialogueContainer(config);
        }

        public void ApplySpeakerDataToDialogueContainer(CharacterConfigData config)
        {
            dialogueContainer.SetDialogueColor(config.dialogueColor);
            dialogueContainer.SetDialogueFont(config.dialogueFont);
            dialogueContainer.nameContainer.SetNameColor(config.nameColor);
            dialogueContainer.nameContainer.SetNameFont(config.nameFont);
        }

        /// <summary>
        /// 显示说话者的名字。
        /// </summary>
        /// <param name="speakerName">说话者名字</param>
        public void ShowSpeakerName(string speakerName = "")
        {
            if (speakerName.ToLower() != "narrator")
                dialogueContainer.nameContainer.Show(speakerName);
            else
                HideSpeakerName();
        }

        /// <summary>
        /// 隐藏说话者名字。
        /// </summary>
        public void HideSpeakerName() => dialogueContainer.nameContainer.Hide();

        /// <summary>
        /// 显示一行对话。
        /// </summary>
        /// <param name="speaker">说话者名字</param>
        /// <param name="dialogue">对话内容</param>
        /// 说话者和对话内容格式为 "说话者 \"对话内容\"" 250715在对话者后增加一个空格
        public Coroutine Say(string speaker, string dialogue)
        {
            List<string> conversation = new List<string>() { $"{speaker}\"{dialogue}\"" };
            return Say(conversation);
        }

        /// <summary>
        /// 显示多行对话。
        /// </summary>
        /// <param name="conversation">对话内容列表</param>
        public Coroutine Say(List<string> conversation)
        {
            return conversationManager.StartConversation(conversation);
        }
    }
}