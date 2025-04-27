using Godot;
using System;

namespace Com.IsartDigital.OneButtonGame
{
    public abstract partial class GameObject : Area2D
    {
        public override void _Ready()
        {
            base._Ready();
            AreaEntered += OnHit;
        }

        protected abstract void OnHit(Area2D pArea);
    }
}
