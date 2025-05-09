using Godot;
using System;

// Author : Camille Smolarski

namespace Com.IsartDigital.WatchOut.UI.TextureSwitchingButtons
{
    public partial class SoundButton : TextureSwitchingButton
    {
        private const string MASTER_BUS = "Master";

        protected override void OnPress()
        {
            base.OnPress();
            int lBusIndx = AudioServer.GetBusIndex(MASTER_BUS);
            AudioServer.SetBusMute(lBusIndx, !AudioServer.IsBusMute(lBusIndx));
        }
    }
}