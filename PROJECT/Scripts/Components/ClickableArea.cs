using Com.IsartDigital.WoolyWay.Managers;
using Godot;
using System;

// Author : Camille Smolarski

namespace Com.IsartDigital.WoolyWay.Components
{
    //Button is only a rectangle. This, paired with a CollisionPolygon can be any shape.
    public partial class ClickableArea : Area2D
    {
        [Signal] public delegate void ClickedEventHandler();

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

        public override void _Input(InputEvent @event)
        {
            base._Input(@event);
            if (IsHovered && Input.IsActionJustPressed(InputManager.UI_CLICK))
                EmitSignal(SignalName.Clicked);
        }

        private void SetHovered()
        {
            IsHovered = true;
        }

        private void SetUnhovered()
        {
            IsHovered = false;
        }

        public void SetActive(bool pEnabled = true)
        {
            if (!pEnabled)
                SetUnhovered();
            Collider.SetDeferred(DISABLED, !pEnabled);
        }
    }
}
