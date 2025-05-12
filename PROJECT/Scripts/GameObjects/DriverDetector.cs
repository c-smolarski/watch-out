using Com.IsartDigital.WatchOut.GameObjects.Mobiles;
using Godot;
using System;
using System.Collections.Generic;

// Author : Camille Smolarski

namespace Com.IsartDigital.WatchOut.GameObjects
{
    public partial class DriverDetector : GameObject
    {
        [Signal] public delegate void DriverDetectedEventHandler();

        [Export] protected bool DetectsPlayer = true;
        [Export] protected bool DetectsNPC = false;

        protected Dictionary<uint, bool> collisionLayersDict;

        protected Mobile detectedDriver;

        public override void _Ready()
        {
            base._Ready();
            CollisionsDictInit(ref collisionLayersDict);
            CollisionInit(this);
            AreaExited += OnAreaLeft;
        }

        protected override void OnHit(Area2D pArea)
        {
            if (pArea is not Vehicle || detectedDriver != null)
                return;
            EmitSignal(SignalName.DriverDetected);
            OnDriverEntered((Vehicle)pArea);
        }

        private void OnAreaLeft(Area2D pArea)
        {
            if (pArea is not Vehicle || pArea != detectedDriver)
                return;
            OnDriverLeft((Vehicle)pArea);
        }

        protected virtual void OnDriverEntered(Vehicle pVehicle)
        {
            detectedDriver = pVehicle;
        }

        protected virtual void OnDriverLeft(Vehicle pVehicle)
        {
            detectedDriver = null;
        }

        protected virtual void CollisionsDictInit(ref Dictionary<uint, bool> pCollisionDict)
        {
            pCollisionDict = new() {
                { Player.COLLISION_LAYER, DetectsPlayer},
                { Mobile.NPC_COLLISION_LAYER, DetectsNPC}};
        }

        protected void CollisionInit(Area2D pArea)
        {
            pArea.CollisionLayer = pArea.CollisionMask = default;
            foreach (uint lLayer in collisionLayersDict.Keys)
            {
                pArea.SetCollisionLayerValue((int)lLayer, collisionLayersDict[lLayer]);
                pArea.SetCollisionMaskValue((int)lLayer, collisionLayersDict[lLayer]);
            }
        }

        protected override void Dispose(bool pDisposing)
        {
            AreaExited -= OnAreaLeft;
            base.Dispose(pDisposing);
        }
    }
}
