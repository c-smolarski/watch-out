using Godot;
using System;

// Author : Camille Smolarski

namespace Com.IsartDigital.OneButtonGame.UI
{
    public partial class TouchIcon : Control
    {
        [Export] private Polygon2D handPolygon;
        [Export] private AnimationPlayer animPlayer;

        public bool Playing => PlayingAnimation != null;
        public TouchAnim? PlayingAnimation { get; private set; }
        public bool Looping { get; private set; }

        private StringName Finished => AnimationPlayer.SignalName.AnimationFinished;

        private Callable resetValuesCallable, loopAnimCallable;
        private float loopTime;
        private uint loopAnimToken;

        public enum TouchAnim
        {
            RESET,
            DOUBLE_TAP,
            HOLD,
            TAP,
            TAP_HOLD
        }

        public override void _Ready()
        {
            base._Ready();
            handPolygon.Color = RenderingServer.Singleton.GetDefaultClearColor();
            resetValuesCallable = new Callable(this, MethodName.ResetValues);
            loopAnimCallable = new Callable(this, MethodName.LoopAnim);
        }

        public void Play(TouchAnim pAnim, float? pSecBetweenLoops = null)
        {
            Stop();
            PlayingAnimation = pAnim;
            if (pSecBetweenLoops != null)
            {
                GenerateLoopAnimToken();
                Looping = true;
                loopTime = pSecBetweenLoops ?? default;
                animPlayer.Connect(Finished, loopAnimCallable);
            }
            else
                animPlayer.Connect(Finished, resetValuesCallable);
            PlayCurrentAnim();
        }

        private void PlayCurrentAnim()
        {
            animPlayer.Play(animPlayer.GetAnimationList()[(int)PlayingAnimation]);
        }

        private async void LoopAnim(StringName pAnimName)
        {
            if (PlayingAnimation == null)
                return;

            uint lTempToken = loopAnimToken;
            await ToSignal(
                GetTree().CreateTimer(loopTime, false),
                Timer.SignalName.Timeout);

            if (lTempToken == loopAnimToken && PlayingAnimation != null)
                PlayCurrentAnim();
        }

        private void GenerateLoopAnimToken()
        {
            uint lTemp = default;
            while (loopAnimToken == lTemp)
                lTemp = (uint)new Random().Next() + (uint)new Random().Next();
            loopAnimToken = lTemp;
        }

        public void Stop()
        {
            animPlayer.Stop();
            ResetValues();
        }

        private void ResetValues(StringName pAnimName = default)
        {
            PlayingAnimation = null;
            if (Looping)
            {
                loopTime = default;
                loopAnimToken = default;
                if (animPlayer.IsConnected(Finished, loopAnimCallable))
                    animPlayer.Disconnect(Finished, loopAnimCallable);
            }
            if (animPlayer.IsConnected(Finished, resetValuesCallable))
                animPlayer.Disconnect(Finished, resetValuesCallable);
        }

        protected override void Dispose(bool pDisposing)
        {
            Stop();
            base.Dispose(pDisposing);
        }
    }
}
