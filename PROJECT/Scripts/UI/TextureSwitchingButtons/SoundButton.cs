using Com.IsartDigital.WatchOut.Utils.Paths;
using Godot;
using System;

// Author : Camille Smolarski

namespace Com.IsartDigital.WatchOut.UI.TextureSwitchingButtons
{
    public partial class SoundButton : TextureSwitchingButton
    {
        protected override void OnPress()
        {
            base.OnPress();
            int lBusIndx = AudioServer.GetBusIndex(SoundPath.MAIN_SOUND_BUS);
            AudioServer.SetBusMute(lBusIndx, !AudioServer.IsBusMute(lBusIndx));
        }
    }
}