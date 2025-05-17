using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using CHARACTERS;

//�Ի�ϵͳ
namespace DIALOGUE
{
    /// <summary>
    /// �Ի�ϵͳ�࣬��������Ի�����ʾ���߼���
    /// </summary>
    public class DialogueSystem : MonoBehaviour
    {
        [SerializeField] private DialogueSystemConfigurationSO _config;
        public DialogueSystemConfigurationSO config => _config; // �Ի�ϵͳ�����ļ�

        //�Ի����������л��ֶ�
        public DialogueContainer dialogueContainer = new DialogueContainer();
        private ConversationManager conversationManager;// �Ի�������
        private TextArchitect architect;// �ı�������

        public static DialogueSystem instance{get; private set;}              // ����ģʽʵ��

        // �Ի�ϵͳ�¼���������Ӧ�û�����
        public delegate void DialogueSystemEvent();
        public event DialogueSystemEvent onUserPrompt_Next;

        // �Ƿ��������жԻ�
        public bool isRunningConversation => conversationManager.isRunning;

        private void Awake()
        {
            // ��ʼ������
            if (instance == null)
            {
                instance = this;
                Initialize();
            }
            else
                DestroyImmediate(gameObject);
        }

        bool _initialized = false;

        /// <summary>
        /// ��ʼ���Ի�ϵͳ��
        /// </summary>
        private void Initialize()
        {
            if (_initialized)
                return;
            // ��ʼ���ı��������ͶԻ�������
            architect = new TextArchitect(dialogueContainer.dialogueText);
            conversationManager = new ConversationManager(architect);
        }

        /// <summary>
        /// �û���ʾ�Ի�ϵͳ�ƽ�����һ����
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
        /// ��ʾ˵���ߵ����֡�
        /// </summary>
        /// <param name="speakerName">˵��������</param>
        public void ShowSpeakerName(string speakerName = "")
        {
            if (speakerName.ToLower() != "narrator")
                dialogueContainer.nameContainer.Show(speakerName);
            else
                HideSpeakerName();
        }

        /// <summary>
        /// ����˵�������֡�
        /// </summary>
        public void HideSpeakerName()=>dialogueContainer.nameContainer.Hide();

        /// <summary>
        /// ��ʾһ�жԻ���
        /// </summary>
        /// <param name="speaker">˵��������</param>
        /// <param name="dialogue">�Ի�����</param>
        public Coroutine Say(string speaker,string dialogue)
        {
            List<string> conversation = new List<string>() { $"{speaker}\"{dialogue}\"" };
            return Say(conversation);
        }

        /// <summary>
        /// ��ʾ���жԻ���
        /// </summary>
        /// <param name="conversation">�Ի������б�</param>
        public Coroutine Say(List<string> conversation)
        {
            return conversationManager.StartConversation(conversation);
        }
    }
}