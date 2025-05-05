using Com.IsartDigital.OneButtonGame.GameObjects.Mobiles;
using Godot;
using System;

// Author : Camille Smolarski

namespace Com.IsartDigital.OneButtonGame.GameObjects
{
    public partial class DriverDetector : GameObject
    {
        [Signal] public delegate void DriverDetectedEventHandler();
        [Export] private bool onlyDetectsPlayer = true;

        public override void _Ready()
        {
            base._Ready();
            if (onlyDetectsPlayer)
                CollisionLayer = CollisionMask = Player.COLLISION_LAYER;
        }

        protected override void OnHit(Area2D pArea)
        {
            if (pArea is Mobile)
                EmitSignal(SignalName.DriverDetected);
        }
    }
}
