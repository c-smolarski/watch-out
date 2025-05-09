using System;

// Author : Camille Smolarski

namespace Com.IsartDigital.WatchOut.GameObjects.Mobiles
{
    public partial class Driver : Mobile
    {
        public override void _Ready()
        {
            base._Ready();
            Visible = false;
        }

        protected override void StartMoving(GearMode pDirection)
        {
            Visible = true;
            base.StartMoving(pDirection);
        }

        protected override void OnReachPathEnd()
        {
            Visible = false;
            base.OnReachPathEnd();
        }
    }
}
