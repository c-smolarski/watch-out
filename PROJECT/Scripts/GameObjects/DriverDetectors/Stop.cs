using Com.IsartDigital.OneButtonGame.GameObjects.Mobiles;
using Com.IsartDigital.OneButtonGame.Managers;
using Godot;
using System;

// Author : Camille Smolarski

namespace Com.IsartDigital.OneButtonGame.GameObjects
{
    public partial class Stop : DriverDetector
    {
        [Signal] public delegate void DriverStoppedEventHandler();
        [Signal] public delegate void DriverSteppedOnLineEventHandler();
        [Signal] public delegate void DriverRunnedOverEventHandler();

        [Export] private Area2D stopLine;

        private const string T_KEY_STEPPED_ON = "STOP_STEPPED_ON";
        private const string T_KEY_BACKED_ON = "STOP_BACKED_ON";
        private const string T_KEY_RUNNED_OVER = "STOP_RUNNED_OVER";

        private const float STOP_TIME = 3f;

        private float elapsedTime;
        private bool driverFinishedWaiting;
        private Mobile driverWaiting;

        public override void _Ready()
        {
            base._Ready();
            AreaExited += OnDriverLeavesStopArea;
            stopLine.AreaEntered += OnDriverStepsOnLine;
            stopLine.AreaExited += OnDriverLeavesLine;
        }

        protected override void OnHit(Area2D pArea)
        {
            base.OnHit(pArea);
            if (pArea is not Mobile || driverWaiting != null)
                return;
            driverWaiting = (Mobile)pArea;
            StartWaitTimer();
        }

        private void OnDriverLeavesStopArea(Area2D pArea)
        {
            if (pArea is not Mobile || pArea != driverWaiting)
                return;
            driverWaiting = null;
            StopWaitTimer();
            if (!pArea.OverlapsArea(stopLine))
                driverFinishedWaiting = false;
        }

        private void OnDriverStepsOnLine(Area2D pArea)
        {
            if (pArea is not Mobile)
                return;

            bool lAreaGoingReverse = ((Mobile)pArea).Direction == (int)Mobile.GearMode.REVERSE;
            if (!driverFinishedWaiting || lAreaGoingReverse)
            {
                EmitSignal(SignalName.DriverSteppedOnLine);
                if(pArea is Player)
                {
                    if (lAreaGoingReverse)
                        SignalBus.Instance.EmitSignal(SignalBus.SignalName.LevelSoftFailed, T_KEY_BACKED_ON);
                    else
                        SignalBus.Instance.EmitSignal(SignalBus.SignalName.LevelSoftFailed, T_KEY_STEPPED_ON);
                }
            }
        }

        private void OnDriverLeavesLine(Area2D pArea)
        {
            if (pArea is not Mobile)
                return;

            if (!pArea.OverlapsArea(this))
            {
                if (driverFinishedWaiting)
                    driverFinishedWaiting = false;
                else
                {
                    EmitSignal(SignalName.DriverRunnedOver);
                    if (pArea is Player)
                        SignalBus.Instance.EmitSignal(SignalBus.SignalName.LevelSoftFailed, T_KEY_RUNNED_OVER);
                }
            }
        }

        private void StartWaitTimer()
        {
            doAction = WaitTimer;
            if (elapsedTime != default)
                elapsedTime = default;
        }

        private void WaitTimer(float pDelta)
        {
            elapsedTime += pDelta;
            if (elapsedTime > STOP_TIME)
            {
                driverFinishedWaiting = true;
                EmitSignal(SignalName.DriverStopped);
                StopWaitTimer();
            }
        }

        private void StopWaitTimer()
        {
            doAction = null;
            elapsedTime = default;
        }

        protected override void Dispose(bool pDisposing)
        {
            AreaExited -= OnDriverLeavesStopArea;
            if (IsInstanceValid(stopLine))
            {
                stopLine.AreaEntered -= OnDriverStepsOnLine;
                stopLine.AreaExited -= OnDriverLeavesLine;
            }
            base.Dispose(pDisposing);
        }

    }
}
