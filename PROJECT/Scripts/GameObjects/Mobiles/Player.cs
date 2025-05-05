using Com.IsartDigital.OneButtonGame.Managers;
using Com.IsartDigital.Utils.Tweens;
using Godot;
using System;

// Author : Camille Smolarski

namespace Com.IsartDigital.OneButtonGame.GameObjects.Mobiles
{
    public partial class Player : Mobile
    {
        [Signal] public delegate void ChangedGearModeEventHandler(int pNewGear);

        [Export] private Marker2D AppearAnimStartPos;
        [Export] public GearBoxType GearBox { get; private set; } = GearBoxType.ELECTRIC;

        public const uint COLLISION_LAYER = 2;
        private const string T_KEY_ACCIDENT = "MESSAGE_ACCIDENT";

        public enum GearBoxType
        {
            NO_GEARBOX,
            MANUAL,
            ELECTRIC
        }

        protected override GearMode SelectedGearMode
        {
            get => base.SelectedGearMode;
            set => EmitSignal(SignalName.ChangedGearMode, (int)(base.SelectedGearMode = value));
        }

        private bool inputConnected;

        public override void _Ready()
        {
            base._Ready();
            SignalBus.Instance.LevelCompleted += OnLevelComplete;
        }

        protected override void OnAccident()
        {
            base.OnAccident();
            DisconnectInputs();
            SignalBus.Instance.EmitSignal(SignalBus.SignalName.LevelHardFailed, T_KEY_ACCIDENT);
        }

        private void ConnectInputs()
        {
            if (inputConnected)
                return;
            inputConnected = true;
            InputManager.Instance.StartedHolding += OnInputHold;
            InputManager.Instance.StoppedHolding += OnInputRelease;
        }

        private void DisconnectInputs()
        {
            if (!inputConnected)
                return;
            inputConnected = false;
            InputManager.Instance.StartedHolding -= OnInputHold;
            InputManager.Instance.StoppedHolding -= OnInputRelease;
        }

        public void Appear()
        {
            if (!IsInstanceValid(AppearAnimStartPos))
            {
                ConnectInputs();
                return;
            }

            Tween lTween = CreateTween();
            lTween.TweenProperty(this, TweenProp.GLOBAL_POSITION, GlobalPosition, 0.75f)
                .From(AppearAnimStartPos.GlobalPosition)
                .SetTrans(Tween.TransitionType.Quad)
                .SetEase(Tween.EaseType.Out);
            lTween.Connect(
                Tween.SignalName.Finished,
                Callable.From(ConnectInputs));
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

        private void OnLevelComplete()
        {
            DisconnectInputs();
            StartMovingForward();
        }

        protected override void Dispose(bool pDisposing)
        {
            SignalBus.Instance.LevelCompleted -= OnLevelComplete;
            DisconnectInputs();
            base.Dispose(pDisposing);
        }
    }
}
