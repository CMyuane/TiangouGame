using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DIALOGUE;

namespace TESTING
{
    public class Testing_Architect : MonoBehaviour
    {
        DialogueSystem ds;
        TextArchitect architect;

        public TABuilder.BuilderTypes bm = TABuilder.BuilderTypes.Fade;

        string[] lines = new string[5]
        {
            "The wind carries away in glee the tinkling of your anklet bells.",
            "The sun smiles and watches your toilet.",
            "The sky watches over you when you sleep in your mother's arms, and the morning comes tiptoe to your bed and kisses your eyes.",
            "The wind carries away in glee the tinkling of your anklet bells.",
            "The fairy mistress of dreams is coming towards you, flying through the twilight sky."
        };
        // Start is called before the first frame update
        void Start()
        {
            ds = DialogueSystem.instance;
            architect = new TextArchitect(ds.dialogueContainer.dialogueText, bm);
            architect.speed = 0.5f;
        }

        // Update is called once per frame
        void Update()
        {
            if (bm != architect.builderType)
            {
                architect.SetBuilderType(bm);
                architect.Stop();
            }

            if (Input.GetKeyDown(KeyCode.S))
                architect.Stop();

            string longLine = "The fairy mistress of dreams is coming towards you, flying through the twilight sky.The fairy mistress of dreams is coming towards you, flying through the twilight sky.The wind carries away in glee the tinkling of your anklet bells.The sun smiles and watches your toilet. The sky watches over you when you sleep in your mother's arms, and the morning comes tiptoe to your bed and kisses your eyes.";
            if (Input.GetKeyDown(KeyCode.Space))
            {
                if (architect.isBuilding)
                {
                    if (!architect.hurryUp)
                        architect.hurryUp = true;
                    else
                        architect.ForceComplete();
                }
                else
                    architect.Build(longLine);
                //architect.Build(lines[Random.Range(0, lines.Length)]);
            }
            else if (Input.GetKeyDown(KeyCode.A))
            {
                architect.Append(longLine);
                //architect.Append(lines[Random.Range(0, lines.Length)]);
            }
        }
         
    }
}
