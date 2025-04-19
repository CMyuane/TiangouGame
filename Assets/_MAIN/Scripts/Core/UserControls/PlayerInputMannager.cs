using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DIALOGUE
{
    public class PlayerInputMannager : MonoBehaviour
    {
        /// <summary>
        /// 管理玩家输入的类，用于触发对话系统的事件。
        /// </summary>
        // Start 在游戏开始时调用
        // Start is called before the first frame update
        void Start()
        {

        }

        // Update 每帧调用一次，用于检测玩家输入
        // Update is called once per frame
        void Update()
        {
            // 检测空格键或回车键按下，触发对话推进
            if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
                PromptAdvance();
        }

        /// <summary>
        /// 提示对话系统推进到下一步。
        /// </summary>
        public void PromptAdvance()
        {
            DialogueSystem.instance.OnUserPrompt_Next();
        }
    }
}
