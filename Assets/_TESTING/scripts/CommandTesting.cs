using COMMANDS;
using System.Collections;
using UnityEngine;

public class CommandTesting : MonoBehaviour
{
    // Start is called before the first frame update
    private void Start()
    {
        //StartCoroutine(Running());
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftArrow))
            CommandManager.instance.Execute("moveCharDemo", "left");
        else if (Input.GetKeyDown(KeyCode.RightArrow))
            CommandManager.instance.Execute("moveCharDemo", "right");
    }

    // Update is called once per frame
    private IEnumerator Running()
    {
        yield return CommandManager.instance.Execute("print");
        yield return CommandManager.instance.Execute("print_lp", "Hello World!");
        yield return CommandManager.instance.Execute("print_mp", "Line 1", "Line 2", "Line 3");

        yield return CommandManager.instance.Execute("lambda");
        yield return CommandManager.instance.Execute("print_lp", "Hello lambda!");
        yield return CommandManager.instance.Execute("print_mp", "lambda 1", "lambda 2", "lambda 3");

        yield return CommandManager.instance.Execute("process");
        yield return CommandManager.instance.Execute("process_lp", "3");
        yield return CommandManager.instance.Execute("process_mp", "process 1", "process 2", "process 3");
    }
}