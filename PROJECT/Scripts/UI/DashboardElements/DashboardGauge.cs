using Godot;
using System;

// Author : Camille Smolarski

namespace Com.IsartDigital.OneButtonGame.UI
{
    public abstract partial class DashboardGauge : DashboardElement
    {
        [Export] protected Control Pointer { get; private set; }
        [Export] private float valueAtMaxRot = 90f;

        protected const float MIN_POINTER_ROT = -11.25f * Mathf.Pi / 180f;
        protected const float MAX_POINTER_ROT = Mathf.Pi * 0.5f;

        public override void _Ready()
        {
            base._Ready();
            Pointer.Rotation = MIN_POINTER_ROT;
        }

        protected float ToPointerRot(float pValue)
        {
            return MIN_POINTER_ROT + pValue * (MAX_POINTER_ROT - MIN_POINTER_ROT) / valueAtMaxRot;
        }
    }
}
