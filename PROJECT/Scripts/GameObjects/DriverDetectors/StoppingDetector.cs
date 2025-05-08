using Com.IsartDigital.WatchOut.Managers;
using Com.IsartDigital.Utils.Tweens;
using Godot;
using System;

// Author : Camille Smolarski

namespace Com.IsartDigital.WatchOut.GameObjects.DriverDetectors
{
    public abstract partial class StoppingDetector : DriverDetector
    {
        [Signal] public delegate void DriverStoppedEventHandler();

        private const float STEP_ANIM_DURATION = 1f;

        protected abstract float StopTime { get; }

        private float elapsedTime;

        protected override void OnDriverEntered(Mobile pDriver)
        {
            base.OnDriverEntered(pDriver);
            StartWaitTimer();

        }

        protected override void OnDriverLeft(Mobile pDriver)
        {
            base.OnDriverLeft(pDriver);
            StopWaitTimer();
        }

        private void StartWaitTimer()
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
                WaitTimerCompleted();
                StopWaitTimer();
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

            Tween lTween = CreateTween()
                .SetTrans(Tween.TransitionType.Circ)
                .SetEase(Tween.EaseType.Out);
            lTween.TweenProperty(pNode, TweenProp.MODULATE, Colors.DarkRed, STEP_ANIM_DURATION);
        }
    }
}
