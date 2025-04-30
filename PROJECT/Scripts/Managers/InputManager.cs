using Godot;
using System;

namespace Com.IsartDigital.OneButtonGame.Managers
{
    public partial class InputManager : Node
    {
        [Signal] public delegate void TapEventHandler(int pNTap);
        [Signal] public delegate void StartedHoldingEventHandler(int pNPrevTap);
        [Signal] public delegate void StoppedHoldingEventHandler();

        public const string CLICK = "click";
        public const float TAP_THRESHOLD = 0.15f;

        public static InputManager Instance { get; private set; }
        public static bool Activated
        {
            set
            {
                Instance.SetProcess(value);
                Instance.SetProcessInput(value);
            }
        }

        private Action<float> clickTimer = null;
        private bool holding = false;
        private int nPrevTap;
        private float elapsedTime;

        public override void _Ready()
        {
            #region Singleton
            if (Instance != null)
            {
                GD.Print("Error : " + nameof(InputManager) + " already exists. The new one is being freed...");
                QueueFree();
                return;
            }
            Instance = this;
            #endregion
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
            if (pEvent.IsActionPressed(CLICK) && !holding)
                StartPressedTimer();
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
                holding = true;
                EmitSignal(SignalName.StartedHolding, nPrevTap);
                StopTimer();
            }
        }

        private void ClickReleased()
        {
            if (!holding)
                StartReleasedTimer();
            else
                StopHolding();
        }

        private void StartReleasedTimer()
        {
            StopTimer();
            nPrevTap++;
            clickTimer = ReleasedTimer;
        }

        private void ReleasedTimer(float pDelta)
        {
            elapsedTime += pDelta;
            if(elapsedTime >= TAP_THRESHOLD)
            {
                EmitSignal(SignalName.Tap, nPrevTap);
                StopTimer();
                nPrevTap = default;
            }
        }

        private void StopHolding()
        {
            holding = false;
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
                Instance = null;
            base.Dispose(pDisposing);
        }
    }
}
