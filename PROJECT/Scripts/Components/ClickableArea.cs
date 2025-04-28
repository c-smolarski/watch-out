using Com.IsartDigital.OneButtonGame.Managers;
using Com.IsartDigital.OneButtonGame.Utils;
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

        private Action onInput;
        private bool pressed; //prevents input echo

        public override void _Ready()
        {
            base._Ready();
            Collider = GetNode<CollisionShape2D>(COLLIDER_PATH);
            if (!GameManager.PlayingOnMobile)
            {
                MouseEntered += SetHovered;
                MouseExited += SetUnhovered;
                onInput = Click;
            }
            else
                onInput = CheckPosAndClick;
        }

        public override void _Input(InputEvent pEvent)
        {
            base._Input(pEvent);
            if(pEvent.IsAction(ActionInput.CLICK))
                onInput();
        }

        private void CheckPosAndClick()
        {
            IsHovered = Geometry2D.IsPointInPolygon(
                Collider.ToLocal(GetGlobalMousePosition()),
                ((ConvexPolygonShape2D)Collider.Shape).Points);
            Click();
        }

        private void Click()
        {
            if (IsHovered && Input.IsActionJustPressed(ActionInput.CLICK) && !pressed)
            {
                pressed = true;
                EmitSignal(SignalName.Clicked);
            }
            else if (Input.IsActionJustReleased(ActionInput.CLICK))
                pressed = false;
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

        protected override void Dispose(bool pDisposing)
        {
            if (!GameManager.PlayingOnMobile)
            {
                MouseEntered -= SetHovered;
                MouseExited -= SetUnhovered;
            }
            base.Dispose(pDisposing);
        }
    }
}
