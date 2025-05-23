﻿using Com.IsartDigital.WatchOut.GameObjects;
using Godot;
using System;

// Author : Camille Smolarski

namespace Com.IsartDigital.WatchOut.UI.DashboardElements.DashboardGauges
{
    public partial class SpeedOMeter : DashboardGauge
    {
        public override void _Process(double pDelta)
        {
            base._Process(pDelta);
            float lPlayerSpeed = Player.Speed - Mobile.MIN_SPEED_THRESHOLD;
            Pointer.Rotation = Mathf.Lerp(Pointer.Rotation, ToPointerRot(lPlayerSpeed < 0 ? 0 : Mathf.Sqrt(lPlayerSpeed) * 2f), 0.25f);
        }
    }
}
