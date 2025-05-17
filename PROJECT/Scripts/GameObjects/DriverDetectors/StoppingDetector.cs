using Com.IsartDigital.WatchOut.Managers;
using Com.IsartDigital.Utils.Tweens;
using Godot;
using System;
using Com.IsartDigital.WatchOut.Enums;
using Com.IsartDigital.WatchOut.GameObjects.Mobiles;

// Author : Camille Smolarski

namespace Com.IsartDigital.WatchOut.GameObjects.DriverDetectors
{
    public abstract partial class StoppingDetector : DriverDetector
    {
        [Signal] public delegate void DriverStoppedEventHandler();

        private const float STEP_ANIM_DURATION = 1f;

        protected abstract float StopTime { get; }

        protected bool startTimerOnDriverDetected = true;
        private float elapsedTime;

        protected override void OnDriverEntered(Vehicle pVehicle)
        {
            base.OnDriverEntered(pVehicle);
            if (startTimerOnDriverDetected)
                StartWaitTimer();
        }

        protected override void OnDriverLeft(Vehicle pVehicle)
        {
            base.OnDriverLeft(pVehicle);
            if (startTimerOnDriverDetected)
                StopWaitTimer();
        }

        protected virtual void StartWaitTimer()
        {
            StopWaitTimer();
            doAction = WaitTimer;
        }

        private void WaitTimer(float pDelta)
        {
            WaitTimerProgressed(elapsedTime += pDelta);
            if (elapsedTime > StopTime)
            {
                EmitSignal(SignalName.DriverStopped);
                StopWaitTimer();
                WaitTimerCompleted();
            }
        }

        private void StopWaitTimer()
        {
            doAction = null;
            elapsedTime = default;
            WaitTimerStopped();
        }

        protected virtual void WaitTimerProgressed(float pElapsedTime)
        { }

        protected virtual void WaitTimerCompleted()
        { }

        protected virtual void WaitTimerStopped()
        { }

        protected void PlayerSteppedOnWrongObject(Node2D pNode, string pMessage)
        {
            SignalBus.Instance.EmitSignal(SignalBus.SignalName.LevelSoftFailed, pMessage);
            GameManager.Vibrate(VibrationDuration.MID);
            Tween lTween = CreateTween()
                .SetTrans(Tween.TransitionType.Circ)
                .SetEase(Tween.EaseType.Out);
            lTween.TweenProperty(pNode, TweenProp.MODULATE, Colors.DarkRed, STEP_ANIM_DURATION);
        }
    }
}
