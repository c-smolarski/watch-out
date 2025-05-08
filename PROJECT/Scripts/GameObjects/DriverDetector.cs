using Com.IsartDigital.WatchOut.GameObjects.Mobiles;
using Godot;
using System;

// Author : Camille Smolarski

namespace Com.IsartDigital.WatchOut.GameObjects
{
    public partial class DriverDetector : GameObject
    {
        [Signal] public delegate void DriverDetectedEventHandler();

        [Export] protected bool OnlyDetectsPlayer = true;

        protected Mobile detectedDriver;

        public override void _Ready()
        {
            base._Ready();
            if (OnlyDetectsPlayer)
                CollisionLayer = CollisionMask = Player.COLLISION_LAYER;
            AreaExited += OnAreaLeft;
        }

        protected override void OnHit(Area2D pArea)
        {
            if (pArea is not Mobile || pArea is Pedestrian || detectedDriver != null)
                return;
            EmitSignal(SignalName.DriverDetected);
            OnDriverEntered((Mobile)pArea);
        }

        private void OnAreaLeft(Area2D pArea)
        {
            if (pArea is not Mobile || pArea is Pedestrian || pArea != detectedDriver)
                return;
            OnDriverLeft((Mobile)pArea);
        }

        protected virtual void OnDriverEntered(Mobile pDriver)
        {
            detectedDriver = pDriver;
        }

        protected virtual void OnDriverLeft(Mobile pDriver)
        {
            detectedDriver = null;
        }
    }
}
