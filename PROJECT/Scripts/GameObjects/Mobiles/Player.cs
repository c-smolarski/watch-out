using Com.IsartDigital.WatchOut.Managers;
using Com.IsartDigital.Utils.Tweens;
using Godot;
using System;
using Com.IsartDigital.Utils;

// Author : Camille Smolarski

namespace Com.IsartDigital.WatchOut.GameObjects.Mobiles
{
    public partial class Player : Mobile
    {
        [Signal] public delegate void ChangedGearModeEventHandler(int pNewGear);

        [Export] private Marker2D appearAnimStartPos;
        [Export] public GearBoxType GearBox { get; private set; } = GearBoxType.ELECTRIC;
        [Export] private bool stopOnLevelEnd;

        public const float APPEAR_FADE_DURATION = 1.5f;
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
            Visible = false;
            SignalBus.Instance.LevelCompleted += OnLevelComplete;
        }

        protected override void OnAccident(Mobile pDriver)
        {
            base.OnAccident(pDriver);
            if (!inputConnected)
                return;

            DisconnectInputs();
            GameManager.Shake(ScreenShakeForce.STRONG);
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

        public override void Appear()
        {
            base.Appear();

            Visible = true;
            SignalBus.Instance.EmitSignal(SignalBus.SignalName.PlayerAppearing);

            if (!IsInstanceValid(appearAnimStartPos))
            {
                OnPlayerAppeared();
                return;
            }

            Tween lTween = CreateTween();
            lTween.TweenProperty(this, TweenProp.GLOBAL_POSITION, GlobalPosition, APPEAR_FADE_DURATION)
                .SetTrans(Tween.TransitionType.Quad)
                .SetEase(Tween.EaseType.Out);
            lTween.Connect(
                Tween.SignalName.Finished,
                Callable.From(OnPlayerAppeared));
            GlobalPosition = appearAnimStartPos.GlobalPosition;
        }

        private void OnPlayerAppeared()
        {
            ConnectInputs();
            SignalBus.Instance.EmitSignal(SignalBus.SignalName.PlayerActivated);
        }

        private void OnInputHold(int pNPrevTap)
        {
            if (pNPrevTap == 0)
                StartMovingForward();
            else if (IsMoving)
                StartBraking(ManualBrakeForce);
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
            if (!stopOnLevelEnd)
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
