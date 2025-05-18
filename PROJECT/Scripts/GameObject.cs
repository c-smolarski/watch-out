using Godot;
using System;
using System.Collections.Generic;

// Author : Camille Smolarski

namespace Com.IsartDigital.WatchOut
{
    public abstract partial class GameObject : Area2D
    {
        protected Action<float> doAction = default;
        protected Dictionary<uint, bool> collisionLayersDict;
        public override void _Ready()
        {
            base._Ready();
            AreaEntered += OnHit;
        }

        public override void _Process(double pDelta)
        {
            base._Process(pDelta);
            if (doAction != null)
                doAction((float)pDelta);
        }

        protected abstract void OnHit(Area2D pArea);

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
            AreaEntered -= OnHit;
            base.Dispose(pDisposing);
        }
    }
}
