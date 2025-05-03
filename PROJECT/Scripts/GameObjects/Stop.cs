using Com.IsartDigital.OneButtonGame.GameObjects.Mobiles;
using Godot;
using System;

// Author : Camille Smolarski

namespace Com.IsartDigital.OneButtonGame.GameObjects
{
    public partial class Stop : GameObject
    {
        [Signal] public delegate void DriverStoppedEventHandler();
        [Signal] public delegate void DriverSteppedOnLineEventHandler();
        [Signal] public delegate void DriverRunnedOverEventHandler();

        [Export] private Area2D stopLine;
        [Export] private bool onlyDetectsPlayer = true;

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

            if (onlyDetectsPlayer)
                CollisionLayer = CollisionMask = Player.COLLISION_LAYER;
        }

        protected override void OnHit(Area2D pArea)
        {
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

            if (!driverFinishedWaiting || ((Mobile)pArea).Direction == (int)Mobile.GearMode.REVERSE)
                EmitSignal(SignalName.DriverSteppedOnLine);
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
                    EmitSignal(SignalName.DriverRunnedOver);
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
                stopLine.AreaEntered += OnDriverStepsOnLine;
                stopLine.AreaExited += OnDriverLeavesLine;
            }
            base.Dispose(pDisposing);
        }

    }
}
