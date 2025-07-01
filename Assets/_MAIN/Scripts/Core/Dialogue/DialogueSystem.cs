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
        //private bool isRunningConversation = false;
        private List<string> fullLines;
        //private List<string> subLinesToContinue = null;
        private Coroutine conversationProcess;

        // �Ƿ��������жԻ�

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
            if (VoiceManager.isBlockingDialogue)
            {
                Debug.Log("對話暫停：錄音或撥放 UI 開啟中");
                return;
            }

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
            fullLines = conversation;

            if (conversationProcess != null)
                StopCoroutine(conversationProcess);

            conversationProcess = StartCoroutine(RunConversation(conversation));
            return conversationProcess;
        }

        private IEnumerator RunConversation(List<string> lines)
        {
            int i = 0;
            while (i < lines.Count)
            {
                string line = lines[i].Trim();

                // 先檢查是否是 [goto]
                if (line.StartsWith("[goto"))
                {
                    string pattern = @"\[goto\s+([^\]]+)\]";
                    var match = System.Text.RegularExpressions.Regex.Match(line, pattern);
                    if (match.Success)
                    {
                        string targetLabel = match.Groups[1].Value.Trim();
                        Debug.Log($"跳轉到 {targetLabel}");

                        // 重新抽取指定 label 的 lines
                        lines = ExtractLinesFromLabel(targetLabel, fullLines);
                        i = 0;
                        continue;
                    }
                }

                // 選項區塊
                if (line == "[option]")
                {
                    i++;
                    List<(string text, List<string> subLines)> options = new();

                    while (i < lines.Count && lines[i].Trim() != "[/option]")
                    {
                        string optLine = lines[i].Trim();
                        if (optLine.StartsWith("-") && optLine.Contains("->"))
                        {
                            string[] parts = optLine.Substring(1).Split("->");
                            string optionText = parts[0].Trim();
                            string jumpTo = parts[1].Trim();
                            List<string> subLines = ExtractLinesFromLabel(jumpTo, fullLines);
                            options.Add((optionText, subLines));
                        }
                        i++;
                    }

                    List<string> optionTexts = options.ConvertAll(o => o.text);
                    bool optionSelected = false;
                    int chosenIndex = -1;

                    ShowOptions(optionTexts, selectedIndex =>
                    {
                        optionSelected = true;
                        chosenIndex = selectedIndex;
                    });

                    while (!optionSelected)
                        yield return null;

                    // 直接切換進選到的分支
                    lines = options[chosenIndex].subLines;
                    i = 0;
                    continue;
                }

                yield return SayLine(line);
                i++;
            }
        }



        // 把單行對話抽出成 Coroutine
        private IEnumerator SayLine(string line)
        {
            DIALOGUE_LINE parsed = DialogueParser.Parse(line);

            if (parsed.hasSpeaker)
            {
                Character character = CharacterManager.instance.GetCharacter(parsed.speakerData.name, createIfDoesNotExist: true);
                character.UpdateTextCustomizationsOnScreen();
                DialogueSystem.instance.ApplySpeakerDataToDialogueContainer(parsed.speakerData.name);
                DialogueSystem.instance.ShowSpeakerName(parsed.speakerData.displayName);

                if (parsed.speakerData.makeCharacterEnter && (!character.isVisible && !character.isRevealing))
                    character.Show();

                if (parsed.speakerData.isCastingPosition)
                    character.MoveToPosition(parsed.speakerData.castPosition);

                if (parsed.speakerData.isCastingExpression)
                {
                    foreach (var exp in parsed.speakerData.CastExpressions)
                        character.OnReceiveCastingExpression(exp.Layer, exp.expression);
                }
            }
            else
            {
                DialogueSystem.instance.HideSpeakerName();
            }
            if (!parsed.hasDialogue)
            {
                if (parsed.hasCommands)
                    yield return CommandManager.instance.ExecuteAll(parsed.commandsData);
                yield break;
            }
            // 執行指令
            if (parsed.hasCommands && parsed.commandsData?.commands != null && parsed.commandsData.commands.Count > 0)
            {
                yield return CommandManager.instance.ExecuteAll(parsed.commandsData);
            }

            // 顯示對話文字（用原本的 SayText function）
            yield return conversationManager.Say(parsed.speakerData?.displayName ?? "", parsed.dialogueData);

            // 等待文字播放完成
            yield return new WaitUntil(() => !conversationManager.isRunning);
        }

        // 用來根據標籤抽取分支
        private List<string> ExtractLinesFromLabel(string label, List<string> allLines)
        {
            List<string> extracted = new List<string>();
            bool foundLabel = false;
            string targetLabel = $"#{label}";

            foreach (var line in allLines)
            {
                if (line.Trim() == targetLabel)
                {
                    foundLabel = true;
                    continue;
                }

                if (foundLabel)
                {
                    if (line.StartsWith("#") && line.Trim() != targetLabel)
                        break;

                    extracted.Add(line);
                }
            }
            return extracted;
        }

        // ... 你原本 ShowOptions 等方法
        public void ShowOptions(List<string> options, System.Action<int> onOptionSelected)
        {
            var optionList = new List<(string, System.Action)>(); // 強制指定 tuple 型別

            for (int i = 0; i < options.Count; i++)
            {
                int index = i; // 非常重要，避免 closure 錯誤
                optionList.Add((options[i], new System.Action(() => {
                    dialogueContainer.optionPanel.Hide();
                    onOptionSelected?.Invoke(index);
                })));
            }

            dialogueContainer.optionPanel.ShowOptions(optionList);
        }
        
    }
}