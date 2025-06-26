using System;
using UnityEngine;

public class CMD_VoiceCommands : CMD_DatabaseExtension
{
    // �p new ��U���ޓI���@�C���s�� override
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
