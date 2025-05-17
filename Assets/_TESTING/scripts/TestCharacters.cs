using CHARACTERS;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using DIALOGUE;
using TMPro;
using System.ComponentModel;

public class TestCharacters : MonoBehaviour
{
    public TMP_FontAsset tempFont;

    private Character CreateCharacter(string name) => CharacterManager.instance.CreateCharacter(name);
    

    // Start is called before the first frame update
    void Start()
    {
        //Character Generic = CharacterManager.instance.CreateCharacter("Generic");
        //Character nagasaki = CharacterManager.instance.CreateCharacter("nagasaki");
        //Character nagasaki2 = CharacterManager.instance.CreateCharacter("nagasaki");
        //Character shiranaihito = CharacterManager.instance.CreateCharacter("shiranaihito");
        StartCoroutine(Test());
    }

    IEnumerator Test()
    {
        Character_Sprite bhx = CreateCharacter("�׺���") as Character_Sprite;
        //Character_Sprite bhxblue = CreateCharacter("���׺��� as �׺���") as Character_Sprite;
        //Debug.Log($"guard1 is of type: {�׺���.GetType().Name}");
        Character_Sprite dl = CreateCharacter("����") as Character_Sprite;
        //Character_Sprite dlred = CreateCharacter("������ as ����") as Character_Sprite;
        //Character_Sprite Guard = CreateCharacter("Guard as ����") as Character_Sprite;
        //Character_Sprite Realin = CreateCharacter("Realin") as Character_Sprite;
        


        return null;






        //�������涶���������ȶ���
        //bhx.SetPosition(new Vector2 (0,0));
        //dl.SetPosition(new Vector2 (1,0));

        //yield return new WaitForSeconds(1);

        ////Sprite bodySprite = ����.GetSprite("1");
        ////Sprite faceSprite = ����.GetSprite("D_03����");
        //Sprite bodySprite = dl.GetSprite("1");
        //dl.TransitionSprite(dl.GetSprite("D_02����"),layer:1);
        //dl.Animate("Hop");
        //yield return dl.Say("����{a} ������䡣");

        //bhx.FaceRight();
        //bhx.TransitionSprite(bhx.GetSprite("bhx_jingya"));
        //bhx.Animate("Shiver",true);
        //yield return bhx.Say("�ҵ��죬{a} ΪʲôͻȻ��ô�䣿");
        //bhx.Animate("Shiver", false);
        //yield return bhx.Say("���ǻ�����ȥ��");




        //yield return null;


        //�������ȼ�
        //bhxblue.SetColor(Color.blue);
        //dlred.SetColor(Color.red);

        //dlred.SetPosition(new Vector2(0.2f, 0));
        //bhx.SetPosition(new Vector2(0.4f,0));
        //dl.SetPosition(new Vector2(0.6f, 0));
        //bhxblue.SetPosition(new Vector2(0.8f, 0));

        //yield return new WaitForSeconds(1f);

        //dlred.SetPriority(1000);
        //bhx.SetPriority(900);
        //dl.SetPriority(800);
        //bhxblue.SetPriority(700);

        //yield return new WaitForSeconds(1f);

        //CharacterManager.instance.SortCharacters(new string[] { "���׺���", "����" });

        //yield return new WaitForSeconds(1f);

        //CharacterManager.instance.SortCharacters();

        //yield return new WaitForSeconds(1f);

        //CharacterManager.instance.SortCharacters(new string[] { "������", "�׺���", "���׺���", "����" });
        ////bhx.SetPriority(1); 

        //yield return null;





        //���Է�ת�������Ͱ���
        //yield return new WaitForSeconds(1f);

        //dl.SetPosition(new Vector2(0.2f,0));
        //bhx.SetPosition(new Vector2(0.8f, 0));

        //yield return new WaitForSeconds(1f);

        //yield return bhx.Flip(0.3f);

        //yield return dl.Flip(1f);

        //yield return new WaitForSeconds(1f);

        //yield return dl.FaceRight(immediate:true);

        //yield return bhx.FaceLeft(immediate:true);

        //yield return new WaitForSeconds(1f);

        //dl.Highlight();
        //yield return dl.Say("���ţ��׺ӣ�������ʫ�ˡ�");

        //dl.UnHighlight();
        //bhx.Highlight();
        //yield return bhx.Say("��û����ô˵���ҡ�����������");

        //bhx.UnHighlight();
        //dl.Highlight();
        //yield return dl.Say("��ģ������ʫ�˵�Ǳ����");



        //yield return new WaitForSeconds(1);

        //yield return Realin.UnHighlight();

        //yield return new WaitForSeconds(1);

        //yield return Realin.TransitionColor(Color.red);

        //yield return new WaitForSeconds(1);

        //yield return Realin.Highlight();

        //yield return new WaitForSeconds(1);

        //yield return Realin.TransitionColor(Color.white);


        //���ǲ���ɫ�任
        //yield return Realin.TransitionColor(Color.red,speed:0.3f);
        //yield return Realin.TransitionColor(Color.blue);
        //yield return Realin.TransitionColor(Color.yellow);
        //yield return Realin.TransitionColor(Color.black);

        //yield return null;


        //����һ�������õ�ע��
        //yield return Realin.TransitionSprite(Realin.GetSprite("B_Default"),1);
        //Realin.TransitionSprite(Realin.GetSprite("B2"));


        //�׺���.Hide();
        //����.Hide();
        //Realin.Hide();
        //Guard.Show();

        //yield return new WaitForSeconds(1);

        //Sprite body = Realin.GetSprite("B2");
        //Sprite face = Realin.GetSprite("B_Laugh");
        //Realin.TransitionSprite(body);
        //yield return Realin.TransitionSprite(face, 1, 0.5f);
        ////Guard.SetSprite(s1);

        //Realin.MoveToPosition(Vector2.zero, 0.5f);

        //����.Show();
        //yield return ����.MoveToPosition(new Vector2(1, 0), 0.5f);


        //Realin.TransitionSprite(Realin.GetSprite("B_Scold"), layer: 1);

        //body = ����.GetSprite("1");
        //face = ����.GetSprite("D_03����");
        ////����.TransitionSprite(body); Ҫôָ��bodyͼ�㣬Ҫô��д���������������ʾ����
        //����.TransitionSprite(face, 1);

        //�׺���.Show();
        //yield return �׺���.MoveToPosition(Vector2.one, 1f, true);


        //Debug.Log($"�ɼ��� = {Realin.isVisible}");


        //yield return null;


        //���ǲ����ƶ�λ�ú��ٶ�
        //�׺���.SetPosition(Vector2.zero);
        //����.SetPosition(new Vector2(0.5f,0.5f));
        //����.SetPosition(Vector2.one);
        //Realin.SetPosition(new Vector2(2f,1f));


        //����.Show();
        //����.Show();
        //Realin.Show();


        //���ǲ����л�ͼ��
        //Sprite bodySprite = ����.GetSprite("1");
        //Sprite faceSprite = ����.GetSprite("D_03����");

        //����.SetSprite(bodySprite, 1);
        //����.SetSprite(faceSprite, 1);

        //yield return new WaitForSeconds(2f);

        //bodySprite = ����.GetSprite("1");
        //faceSprite = ����.GetSprite("D_06ϲ��");

        //����.SetSprite(bodySprite, 1);
        //����.SetSprite(faceSprite, 1);

        //yield return �׺���.Show();

        //yield return �׺���.MoveToPosition(Vector2.one, 2f, true);
        //yield return �׺���.MoveToPosition(Vector2.zero, 2f, false);



        //���ǲ�����ɫ
        //�׺���.SetNameColor(Color.red);
        //����.SetNameFont(tempFont);
        //����.SetDialogueColor(Color.red);
        //Realin.SetDialogueFont(tempFont);


        //yield return �׺���.Say("Hello, I'm Guard1.");
        //yield return ����.Say("Hello, I'm Guard2.");
        //yield return ����.Say("Hello, I'm Guard3.");
        //yield return Realin.Say("Hello, I'm Guard1 again.");

        //yield return null;
        //yield return new WaitForSeconds(1f);

        //Character bhx = CharacterManager.instance.CreateCharacter("�׺���");

        //yield return new WaitForSeconds(1f);

        //yield return bhx.Hide();

        //yield return new WaitForSeconds(0.5f);

        //yield return bhx.Show();

        //yield return bhx.Say("�Ҿ����Լ�����Զ������ȥ��{wa 1}ʲôҲ�������ҡ�");

        //Character ��ľ���� = CharacterManager.instance.CreateCharacter("��ľ����");
        //Character ��ú� = CharacterManager.instance.CreateCharacter("��ú�");
        //Character ������ = CharacterManager.instance.CreateCharacter("������");

        //List<string> lines = new List<string>()
        //{
        //    "��һ���Ҷ�ʮһ�ꡣ",
        //    "����һ���Ļƽ�ʱ����",
        //    "���кö�������",
        //    "���밮��{wa 1} ��ԣ�{wa 1} ������һ˲�������ϰ����밵���ơ�",
        //};
        //yield return ��ľ����.Say(lines);

        //��ľ����.SetNameColor(Color.red);
        //��ľ����.SetDialogueColor(Color.red);
        //��ľ����.SetNameFont(tempFont);
        //��ľ����.SetDialogueFont(tempFont);

        //yield return ��ľ����.Say(lines);

        //��ľ����.ResetConfigurationData();

        //yield return ��ľ����.Say(lines);

        //lines = new List<string>()
        //{
        //    "�����Ҳ�֪����{wa 1}������Ǹ������ܴ��Ĺ���...",
        //    "�����ҹ���ʮһ������ʱû��Ԥ������һ�㡣"
        //};
        //yield return ��ú�.Say(lines);

        //yield return ������.Say("�Ҿ����Լ�����Զ������ȥ��{wa 1}ʲôҲ�������ҡ�");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
