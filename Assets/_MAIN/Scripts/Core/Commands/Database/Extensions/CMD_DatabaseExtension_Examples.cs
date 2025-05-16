using COMMANDS;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.VersionControl;
using UnityEngine;

namespace TESTING
{

    /// <summary>
    ///  此脚本将用作示例了解将来如何将指令添加到命令系统
    /// </summary>
    public class CMD_DatabaseExtension_Examples : CMD_DatabaseExtension
    {
        new public static void Extend(CommandDatabase database)
        {
            // 这里添加你想要的命令
            database.AddCommand("print", new Action(PrintDefaultMessage));
            database.AddCommand("print_lp", new Action<string>(PrintUserMessage));
            database.AddCommand("print_mp", new Action<string[]>(PrintLines));
            //lambda
            database.AddCommand("lambda", new Action(() => { Debug.Log("Printing a default message to console from lambda command."); }));
            database.AddCommand("lambada_lp", new Action<string>((arg) => { Debug.Log($"Log User Message: '{arg}'"); }));
            database.AddCommand("lambada_mp", new Action<string[]>((arg) => { Debug.Log(string.Join(", ", arg)); }));

            database.AddCommand("process", new Func<IEnumerator>(SimpleProcess));
            database.AddCommand("process_lp", new Func<string, IEnumerator>(LineProcess));
            database.AddCommand("process_mp", new Func<string[], IEnumerator>(MultiLineProcess));

            database.AddCommand("moveCharDemo", new Func<string, IEnumerator>(MoveCharacter));

        }

        private static void PrintDefaultMessage()
        {
            Debug.Log("Printing a default message to console.");
        }

        private static void PrintUserMessage(string message)
        {
            Debug.Log($"User Massage: '{message}'");
        }

        private static void PrintLines(string[] lines)
        {
            int i = 1;
            foreach (string line in lines)
            {
                Debug.Log($"{i++}. '{line}'");
            }
        }

        private static IEnumerator SimpleProcess()
        {
            for (int i = 1; i <= 5; i++)
            {
                Debug.Log($"Process Running... [{i}]");
                yield return new WaitForSeconds(1);
            }
        }

        private static IEnumerator LineProcess(string data)
        {
            if (int.TryParse(data, out int num))
            {
                for (int i = 1; i <= num; i++)
                {
                    Debug.Log($"Process Running... [{i}]");
                    yield return new WaitForSeconds(1);
                }
            }
        }

        private static IEnumerator MultiLineProcess(string[] data)
        {
            foreach (string line in data)
            {
                Debug.Log($"Process Message: '{line}'");
                yield return new WaitForSeconds(0.5f);
            }
        }

        private static IEnumerator MoveCharacter(string direction)
        {
            //假设你有一个名为“Image”的游戏对象，它是一个角色的图像
            //并且你想要移动它到左边或右边
            bool left = direction.ToLower() == "left";
            //获取角色的Transform组件
            Transform character = GameObject.Find("Image").transform;
            float moveSpeed = 15;
            //假设你想要移动到-8或8的x坐标
            //你可以根据需要调整这些值
            float targetX = left ? -8 : 8;
            //获取当前角色的x坐标
            //假设角色的初始位置是0
            float currentX = character.position.x;
            //使用Mathf.MoveTowards来平滑移动角色到目标位置
            //Mathf.MoveTowards(current, target, maxDistanceDelta)函数将返回一个新的值，该值在当前值和目标值之间移动，最大距离为maxDistanceDelta
            while (Mathf.Abs(targetX - currentX) > 0.1f)
            {
                // Debug.Log($"Moving character to {(left ? "left" : "right")} [{currentX}/{targetX}]");
                currentX = Mathf.MoveTowards(currentX, targetX, moveSpeed * Time.deltaTime);
                character.position = new Vector3(currentX, character.position.y, character.position.z);
                yield return null;
            }
        }
    }
}