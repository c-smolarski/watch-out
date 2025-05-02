using Com.IsartDigital.OneButtonGame.Managers;
using Godot;
using System;

// Author : Camille Smolarski

namespace Com.IsartDigital.OneButtonGame.Components
{
    //Button is only a rectangle. This, paired with a CollisionPolygon can be any shape.
    public partial class ClickableArea : Area2D
    {
        [Signal] public delegate void ClickedEventHandler();
        [Signal] public delegate void HoveredEventHandler();
        [Signal] public delegate void UnhoveredEventHandler();

        private const string COLLIDER_PATH = "collider";
        private const string DISABLED = "disabled";

        public bool IsHovered { get; private set; }
        public CollisionShape2D Collider { get; private set; }

        public override void _Ready()
        {
            base._Ready();
            Collider = GetNode<CollisionShape2D>(COLLIDER_PATH);
            MouseEntered += SetHovered;
            MouseExited += SetUnhovered;
        }

        public override void _Input(InputEvent pEvent)
        {
            base._Input(pEvent);
            if (IsHovered && Input.IsActionJustPressed(InputManager.CLICK))
                EmitSignal(SignalName.Clicked);
        }

        private void SetHovered()
        {
            IsHovered = true;
            EmitSignal(SignalName.Hovered);
        }

        private void SetUnhovered()
        {
            if (IsHovered)
            {
                IsHovered = false;
                EmitSignal(SignalName.Unhovered);
            }
        }

        public void SetActive(bool pEnabled = true)
        {
            if (!pEnabled)
                SetUnhovered();
            Collider.SetDeferred(DISABLED, !pEnabled);
        }
    }
}
