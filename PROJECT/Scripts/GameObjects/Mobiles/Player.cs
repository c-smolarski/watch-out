using Com.IsartDigital.OneButtonGame.Managers;
using Godot;
using System;

// Author : Camille Smolarski

namespace Com.IsartDigital.OneButtonGame.GameObjects.Mobiles
{
    public partial class Player : Mobile
    {
        [Signal] public delegate void ChangedGearModeEventHandler(int pNewGear);

        [Export] public GearBoxType GearBox { get; private set; } = GearBoxType.ELECTRIC;

        public enum GearBoxType
        {
            NO_GEARBOX,
            MANUAL,
            ELECTRIC
        }

        public const uint COLLISION_LAYER = 2;

        protected override GearMode SelectedGearMode
        {
            get => base.SelectedGearMode;
            set => EmitSignal(SignalName.ChangedGearMode, (int)(base.SelectedGearMode = value));
        }

        private bool inputConnected = true;

        public override void _Ready()
        {
            base._Ready();
            InputManager.Instance.StartedHolding += OnInputHold;
            InputManager.Instance.StoppedHolding += OnInputRelease;
        }

        protected override void OnHit(Area2D pArea)
        {
            base.OnHit(pArea);
            if (pArea is Mobile)
                DisconnectInput();
        }

        public void Appear()
        {

        }

        private void OnInputHold(int pNPrevTap)
        {
            if (pNPrevTap == 0)
                StartMovingForward();
            else if (IsMoving)
                StartBraking(BrakeForce);
            else
                StartMovingBackward();
        }

        private void OnInputRelease()
        {
            if (IsMoving)
                StartBraking(EngineBrakeForce);
        }

        private void DisconnectInput()
        {
            inputConnected = false;
            InputManager.Instance.StartedHolding -= OnInputHold;
            InputManager.Instance.StoppedHolding -= OnInputRelease;
        }

        protected override void Dispose(bool pDisposing)
        {
            if (inputConnected)
                DisconnectInput();
            base.Dispose(pDisposing);
        }
    }
}
