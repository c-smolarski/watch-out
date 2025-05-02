using Godot;
using System;

// Author : Camille Smolarski

namespace Com.IsartDigital.OneButtonGame
{
    public abstract partial class GameObject : Area2D
    {
        protected Action<float> doAction = default;

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

        protected override void Dispose(bool pDisposing)
        {
            AreaEntered -= OnHit;
            base.Dispose(pDisposing);
        }
    }
}
