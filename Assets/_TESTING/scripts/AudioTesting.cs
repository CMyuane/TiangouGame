using CHARACTERS;
using DIALOGUE;
using System.Collections;
using UnityEngine;

using Character = CHARACTERS.Character;

public class AudioTesting : MonoBehaviour
{
    // Start is called before the first frame update
    private void Start()
    {
        StartCoroutine(Running3());
    }

    // Update is called once per frame
    private Character CreateCharacter(string name) => CharacterManager.instance.CreateCharacter(name);

    private IEnumerator Running2()
    {
        Character_Sprite bhx = CreateCharacter("白河杏") as Character_Sprite;
        Character me = CreateCharacter("我") as Character;
        bhx.Show();

        AudioManager.instance.PlaySoundEffect("Audio/SFX/RadioStatic", loop: true);

        yield return me.Say("打开收音机");

        AudioManager.instance.StopSoundEffect("RadioStatic");
        AudioManager.instance.PlayVoice("Audio/Voices/wakeup");

        bhx.Say("好吵，关了");

        //yield return new WaitForSeconds(0.5f);

        //AudioManager.instance.PlaySoundEffect("Audio/SFX/thunder_strong_01");

        //yield return new WaitForSeconds(1f);
        //bhx.Animate("Hop");
        //bhx.TransitionSprite(bhx.GetSprite("bhx_jingya"));
        //bhx.Say("天呐，这是什么声音？");
    }

    private IEnumerator Running()
    {
        yield return new WaitForSeconds(1);
        Character_Sprite bhx = CreateCharacter("白河杏") as Character_Sprite;
        bhx.Show();

        yield return DialogueSystem.instance.Say("Narrator", "看一下");

        GraphicPanelManager.instance.GetPanel("background").GetLayer(0, true).SetTexture("Graphics/BG Images/5");
        AudioManager.instance.PlayTrack("Audio/Music/caigaoqiao", volumeCap: 0.5f);
        AudioManager.instance.PlayVoice("Audio/Voices/wakeup");

        bhx.SetSprite(bhx.GetSprite("bhx_putong"), 0);
        yield return bhx.Say("也行");

        //AudioChannel channel = new AudioChannel(1);

        yield return bhx.Say("我可以在这里做什么？");

        GraphicPanelManager.instance.GetPanel("background").GetLayer(0, true).SetTexture("Graphics/BG Images/EngineRoom");
        AudioManager.instance.PlayTrack("Audio/Music/diana", volumeCap: 0.5f);

        yield return null;
    }

    private IEnumerator Running3()
    {
        Character_Sprite bhx = CreateCharacter("白河杏") as Character_Sprite;
        Character me = CreateCharacter("我") as Character;
        bhx.Show();

        GraphicPanelManager.instance.GetPanel("background").GetLayer(0, true).SetTexture("Graphics/BG Images/villagenight");

        AudioManager.instance.PlayTrack("Audio/Ambience/RainyMood", 0);
        AudioManager.instance.PlayTrack("Audio/Music/caigaoqiao", 1, pitch: 0.7f);

        yield return bhx.Say("多频道音频测试");

        AudioManager.instance.StopTrack(1);
    }
}