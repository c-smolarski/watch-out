using Godot;
using System;

// Author : Camille Smolarski

namespace Com.IsartDigital.WatchOut.GameObjects.Mobiles
{
    public partial class Driver : Mobile
    {
        [Export] private bool visibleOnReady;

        public override void _Ready()
        {
            base._Ready();
            Visible = visibleOnReady;
        }

        protected override void StartMoving(GearMode pDirection)
        {
            Visible = true;
            base.StartMoving(pDirection);
        }

        private void Brake()
        {
            StartBraking(EngineBrakeForce);
        }

        protected override void OnReachPathEnd()
        {
            Visible = false;
            base.OnReachPathEnd();
        }
    }
}
