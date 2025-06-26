using System;
using UnityEngine;

public class CMD_VoiceCommands : CMD_DatabaseExtension
{
    // 用 new 隱藏父類的方法，而不是 override
    public new void Extend(CommandDatabase db)
    {
        db.AddCommand("showRecordButton", (Action)ShowRecordButton);
        db.AddCommand("showPlayButton", (Action)ShowPlayButton);
    }

    private void ShowRecordButton()
    {
        VoiceManager.instance.ShowRecordButton();
    }

    private void ShowPlayButton()
    {
        VoiceManager.instance.ShowPlayButton();
    }
}
