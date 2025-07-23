using CHARACTERS;
using DIALOGUE;
using System.Collections;
using UnityEngine;

public class GraphicLayerTesting : MonoBehaviour
{
    // Start is called before the first frame update
    private void Start()
    {
        StartCoroutine(Running());
    }

    private IEnumerator Running()
    {
        GraphicPanel panel = GraphicPanelManager.instance.GetPanel("Background");
        GraphicLayer layer = panel.GetLayer(0, true);

        yield return new WaitForSeconds(1);

        Texture blendTex = Resources.Load<Texture>("Graphics/Transition Effects/hurricane");
        layer.SetTexture("Graphics/BG Images/2", blendingTexture: blendTex);

        yield return new WaitForSeconds(1);

        //layer.SetVideo("Graphics/BG Videos/Fantasy Landscape",transitionSpeed: 0.1f, useAudio: true);
        layer.SetVideo("Graphics/BG Videos/Fantasy Landscape", blendingTexture: blendTex);

        yield return new WaitForSeconds(3);

        layer.currentGraphic.FadeOut();

        yield return new WaitForSeconds(2);

        Debug.Log(layer.currentGraphic);
    }

    private IEnumerator RunningLayers()
    {
        GraphicPanel panel = GraphicPanelManager.instance.GetPanel("Background");
        GraphicLayer layer0 = panel.GetLayer(0, true);
        GraphicLayer layer1 = panel.GetLayer(1, true);

        layer0.SetVideo("Graphics/BG Videos/Nebula");
        layer1.SetTexture("Graphics/BG Images/Spaceshipinterior");

        yield return new WaitForSeconds(2);

        GraphicPanel cinematic = GraphicPanelManager.instance.GetPanel("Cinematic");
        GraphicLayer cinLayer = cinematic.GetLayer(0, true);

        Character bhx = CharacterManager.instance.CreateCharacter("白河杏", true);

        yield return bhx.Say("我要看啤酒烧烤的新卡");

        cinLayer.SetTexture("Graphics/Gallery/card_after_training");

        yield return DialogueSystem.instance.Say("Narrator", "这是我喜欢的卡面");

        cinLayer.Clear();

        yield return new WaitForSeconds(1);

        panel.Clear();
    }
}