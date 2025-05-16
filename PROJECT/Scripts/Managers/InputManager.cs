using Godot;
using System;

// Author : Camille Smolarski

namespace Com.IsartDigital.WatchOut.Managers
{
    public partial class InputManager : Node
    {
        [Signal] public delegate void TouchedEventHandler();
        [Signal] public delegate void TapEventHandler();
        [Signal] public delegate void MultipleTapsEventHandler(int pNTap);
        [Signal] public delegate void StartedHoldingEventHandler(int pNPrevTap);
        [Signal] public delegate void StoppedHoldingEventHandler();

        public const string CLICK = "click";
        private const float TAP_THRESHOLD = 0.3f;

        public static InputManager Instance { get; private set; }
        public static bool IsHolding { get; private set; } = false;
        public static bool Activated
        {
            set
            {
                Instance.SetProcess(value);
                Instance.SetProcessInput(value);
            }
        }

        private Action<float> clickTimer = null;
        private int nPrevTap;
        private float elapsedTime;

        public override void _Ready()
        {
            #region Singleton
            if (Instance != null)
            {
                GD.PrintErr("Error : " + nameof(InputManager) + " already exists. The new one is being freed...");
                QueueFree();
                return;
            }
            Instance = this;
            #endregion
            Activated = false;
        }

        public override void _Process(double pDelta)
        {
            base._Process(pDelta);
            if(clickTimer != null)
                clickTimer((float)pDelta);
        }

        public override void _Input(InputEvent pEvent)
        {
            base._Input(pEvent);
            if (pEvent.IsActionPressed(CLICK) && !IsHolding)
            {
                StartPressedTimer();
                EmitSignal(SignalName.Touched);
            }
            else if (pEvent.IsActionReleased(CLICK))
                ClickReleased();
        }

        private void StartPressedTimer()
        {
            StopTimer();
            clickTimer = PressedTimer;
        }

        private void PressedTimer(float pDelta)
        {
            elapsedTime += pDelta;
            if (elapsedTime >= TAP_THRESHOLD)
            {
                IsHolding = true;
                EmitSignal(SignalName.StartedHolding, nPrevTap);
                StopTimer();
            }
        }

        private void ClickReleased()
        {
            if (!IsHolding)
                StartReleasedTimer();
            else
                StopHolding();
        }

        private void StartReleasedTimer()
        {
            StopTimer();
            EmitSignal(SignalName.Tap);
            nPrevTap++;
            clickTimer = ReleasedTimer;
        }

        private void ReleasedTimer(float pDelta)
        {
            elapsedTime += pDelta;
            if(elapsedTime >= TAP_THRESHOLD)
            {
                EmitSignal(SignalName.MultipleTaps, nPrevTap);
                StopTimer();
                nPrevTap = default;
            }
        }

        private void StopHolding()
        {
            IsHolding = false;
            nPrevTap = default;
            EmitSignal(SignalName.StoppedHolding);
        }

        private void StopTimer()
        {
            clickTimer = null;
            elapsedTime = default;
        }

        protected override void Dispose(bool pDisposing)
        {
            if (Instance == this)
            {
                IsHolding = false;
                Instance = null;
            }
            base.Dispose(pDisposing);
        }
    }
}
