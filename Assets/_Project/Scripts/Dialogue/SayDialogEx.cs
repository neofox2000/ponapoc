using System;
using UnityEngine;

namespace Fungus
{
    public class SayDialogEx : SayDialog
    {
        public Action<string, Character> OnSay;

        public override void Say(string text, bool clearPrevious, bool waitForInput, bool fadeWhenDone, bool stopVoiceover, bool waitForVO, AudioClip voiceOverClip, Action onComplete)
        {
            base.Say(text, clearPrevious, waitForInput, fadeWhenDone, stopVoiceover, waitForVO, voiceOverClip, onComplete);
            if (OnSay != null) OnSay.Invoke(text, speakingCharacter);
        }
    }
}