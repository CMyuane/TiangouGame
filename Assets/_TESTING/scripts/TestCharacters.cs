using CHARACTERS;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using DIALOGUE;
using TMPro;
using System.ComponentModel;
using System.Data.SqlTypes;

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
        Character_Sprite bhx = CreateCharacter("白河杏") as Character_Sprite;
        //Character_Sprite bhxblue = CreateCharacter("蓝白河杏 as 白河杏") as Character_Sprite;
        //Debug.Log($"guard1 is of type: {白河杏.GetType().Name}");
        Character_Sprite dl = CreateCharacter("黛莉") as Character_Sprite;
        //Character_Sprite dlred = CreateCharacter("红黛莉 as 黛莉") as Character_Sprite;
        //Character_Sprite Guard = CreateCharacter("Guard as 守卫") as Character_Sprite;
        //Character_Sprite Realin = CreateCharacter("Realin") as Character_Sprite;
        Character_Sprite sc = CreateCharacter("女主") as Character_Sprite;
        Character_Sprite fox = CreateCharacter("魔性女子") as Character_Sprite;
        //sc.SetNameColor(Color.red);
        //fox.SetDialogueColor(Color.red);
        yield return sc.Say("lines");
        yield return fox.Say("lines");
        yield return null;






        //测试立绘抖动、跳动等动画
        //bhx.SetPosition(new Vector2 (0,0));
        //dl.SetPosition(new Vector2 (1,0));

        //yield return new WaitForSeconds(1);

        ////Sprite bodySprite = 黛莉.GetSprite("1");
        ////Sprite faceSprite = 黛莉.GetSprite("D_03伤心");
        //Sprite bodySprite = dl.GetSprite("1");
        //dl.TransitionSprite(dl.GetSprite("D_02惊讶"),layer:1);
        //dl.Animate("Hop");
        //yield return dl.Say("天呐{a} 外面好冷。");

        //bhx.FaceRight();
        //bhx.TransitionSprite(bhx.GetSprite("bhx_jingya"));
        //bhx.Animate("Shiver",true);
        //yield return bhx.Say("我的天，{a} 为什么突然这么冷？");
        //bhx.Animate("Shiver", false);
        //yield return bhx.Say("我们回屋里去！");




        //yield return null;


        //测试优先级
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

        //CharacterManager.instance.SortCharacters(new string[] { "蓝白河杏", "黛莉" });

        //yield return new WaitForSeconds(1f);

        //CharacterManager.instance.SortCharacters();

        //yield return new WaitForSeconds(1f);

        //CharacterManager.instance.SortCharacters(new string[] { "红黛莉", "白河杏", "蓝白河杏", "黛莉" });
        ////bhx.SetPriority(1); 

        //yield return null;





        //测试翻转、高亮和暗淡
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
        //yield return dl.Say("天呐，白河，你好像个诗人。");

        //dl.UnHighlight();
        //bhx.Highlight();
        //yield return bhx.Say("还没人这么说过我······");

        //bhx.UnHighlight();
        //dl.Highlight();
        //yield return dl.Say("真的，你很有诗人的潜力！");



        //yield return new WaitForSeconds(1);

        //yield return Realin.UnHighlight();

        //yield return new WaitForSeconds(1);

        //yield return Realin.TransitionColor(Color.red);

        //yield return new WaitForSeconds(1);

        //yield return Realin.Highlight();

        //yield return new WaitForSeconds(1);

        //yield return Realin.TransitionColor(Color.white);


        //这是测颜色变换
        //yield return Realin.TransitionColor(Color.red,speed:0.3f);
        //yield return Realin.TransitionColor(Color.blue);
        //yield return Realin.TransitionColor(Color.yellow);
        //yield return Realin.TransitionColor(Color.black);

        //yield return null;


        //这是一个测试用的注释
        //yield return Realin.TransitionSprite(Realin.GetSprite("B_Default"),1);
        //Realin.TransitionSprite(Realin.GetSprite("B2"));


        //白河杏.Hide();
        //黛莉.Hide();
        //Realin.Hide();
        //Guard.Show();

        //yield return new WaitForSeconds(1);

        //Sprite body = Realin.GetSprite("B2");
        //Sprite face = Realin.GetSprite("B_Laugh");
        //Realin.TransitionSprite(body);
        //yield return Realin.TransitionSprite(face, 1, 0.5f);
        ////Guard.SetSprite(s1);

        //Realin.MoveToPosition(Vector2.zero, 0.5f);

        //黛莉.Show();
        //yield return 黛莉.MoveToPosition(new Vector2(1, 0), 0.5f);


        //Realin.TransitionSprite(Realin.GetSprite("B_Scold"), layer: 1);

        //body = 黛莉.GetSprite("1");
        //face = 黛莉.GetSprite("D_03伤心");
        ////黛莉.TransitionSprite(body); 要么指定body图层，要么不写这个语句才能正常显示？？
        //黛莉.TransitionSprite(face, 1);

        //白河杏.Show();
        //yield return 白河杏.MoveToPosition(Vector2.one, 1f, true);


        //Debug.Log($"可见性 = {Realin.isVisible}");


        //yield return null;


        //这是测试移动位置和速度
        //白河杏.SetPosition(Vector2.zero);
        //黛莉.SetPosition(new Vector2(0.5f,0.5f));
        //守卫.SetPosition(Vector2.one);
        //Realin.SetPosition(new Vector2(2f,1f));


        //黛莉.Show();
        //守卫.Show();
        //Realin.Show();


        //这是测试切换图层
        //Sprite bodySprite = 黛莉.GetSprite("1");
        //Sprite faceSprite = 黛莉.GetSprite("D_03伤心");

        //黛莉.SetSprite(bodySprite, 1);
        //黛莉.SetSprite(faceSprite, 1);

        //yield return new WaitForSeconds(2f);

        //bodySprite = 黛莉.GetSprite("1");
        //faceSprite = 黛莉.GetSprite("D_06喜悦");

        //黛莉.SetSprite(bodySprite, 1);
        //黛莉.SetSprite(faceSprite, 1);

        //yield return 白河杏.Show();

        //yield return 白河杏.MoveToPosition(Vector2.one, 2f, true);
        //yield return 白河杏.MoveToPosition(Vector2.zero, 2f, false);



        //这是测试颜色
        //白河杏.SetNameColor(Color.red);
        //黛莉.SetNameFont(tempFont);
        //守卫.SetDialogueColor(Color.red);
        //Realin.SetDialogueFont(tempFont);


        //yield return 白河杏.Say("Hello, I'm Guard1.");
        //yield return 黛莉.Say("Hello, I'm Guard2.");
        //yield return 守卫.Say("Hello, I'm Guard3.");
        //yield return Realin.Say("Hello, I'm Guard1 again.");

        //yield return null;
        //yield return new WaitForSeconds(1f);

        //Character bhx = CharacterManager.instance.CreateCharacter("白河杏");

        //yield return new WaitForSeconds(1f);

        //yield return bhx.Hide();

        //yield return new WaitForSeconds(0.5f);

        //yield return bhx.Show();

        //yield return bhx.Say("我觉得自己会永远生猛下去，{wa 1}什么也锤不了我。");

        //Character 端木匿雅 = CharacterManager.instance.CreateCharacter("端木匿雅");
        //Character 沈济涵 = CharacterManager.instance.CreateCharacter("沈济涵");
        //Character 左念婷 = CharacterManager.instance.CreateCharacter("左念婷");

        //List<string> lines = new List<string>()
        //{
        //    "那一天我二十一岁。",
        //    "在我一生的黄金时代。",
        //    "我有好多奢望。",
        //    "我想爱，{wa 1} 想吃，{wa 1} 还想在一瞬间变成天上半明半暗的云。",
        //};
        //yield return 端木匿雅.Say(lines);

        //端木匿雅.SetNameColor(Color.red);
        //端木匿雅.SetDialogueColor(Color.red);
        //端木匿雅.SetNameFont(tempFont);
        //端木匿雅.SetDialogueFont(tempFont);

        //yield return 端木匿雅.Say(lines);

        //端木匿雅.ResetConfigurationData();

        //yield return 端木匿雅.Say(lines);

        //lines = new List<string>()
        //{
        //    "后来我才知道，{wa 1}生活就是个缓慢受锤的过程...",
        //    "可是我过二十一岁生日时没有预见到这一点。"
        //};
        //yield return 沈济涵.Say(lines);

        //yield return 左念婷.Say("我觉得自己会永远生猛下去，{wa 1}什么也锤不了我。");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
