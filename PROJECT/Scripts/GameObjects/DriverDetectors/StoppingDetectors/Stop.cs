using Com.IsartDigital.OneButtonGame.GameObjects.Mobiles;
using Com.IsartDigital.Utils.Tweens;
using Godot;
using System;

// Author : Camille Smolarski

namespace Com.IsartDigital.OneButtonGame.GameObjects.DriverDetectors
{
    public partial class Stop : StoppingDetector
    {
        #region Signals
        [Signal] public delegate void DriverSteppedOnLineEventHandler();
        [Signal] public delegate void DriverRunnedOverEventHandler();
        #endregion

        #region Exports
        [Export] private Area2D stopLine;
        [Export] private Polygon2D linePolygon;
        [Export] private Node2D sign;
        [Export] private Polygon2D redSignBG;
        #endregion

        #region Consts
        private const string T_KEY_STEPPED_ON = "STOP_STEPPED_ON";
        private const string T_KEY_BACKED_ON = "STOP_BACKED_ON";
        private const string T_KEY_RUNNED_OVER = "STOP_RUNNED_OVER";

        private const string SIGN_SHADER_ALPHA_PARAM = "mixAlpha";
        private const string SIGN_SHADER_ANGLE_PARAM = "angle";
        private const int SIGN_SHADER_ANGLE_MAX_VALUE = 360;

        private const float ANIM_DEFAULT_DURATION = 0.25f;
        private const float SUCCESS_ANIM_MULT = 1.3f;

        private const float STOP_TIME = 3f;
        #endregion

        protected override float StopTime => STOP_TIME;

        private float ShaderAlpha
        {
            get => (float)signShader.GetShaderParameter(SIGN_SHADER_ALPHA_PARAM);
            set => signShader.SetShaderParameter(SIGN_SHADER_ALPHA_PARAM, value);
        }

        private bool driverFinishedWaiting;
        private Tween shaderTween;
        private ShaderMaterial signShader;

        public override void _Ready()
        {
            base._Ready();
            if (OnlyDetectsPlayer)
                stopLine.CollisionLayer = stopLine.CollisionMask = Player.COLLISION_LAYER;
            stopLine.AreaEntered += OnDriverStepsOnLine;
            stopLine.AreaExited += OnDriverLeavesLine;
            redSignBG.Material = signShader = (ShaderMaterial)redSignBG.Material.Duplicate();
        }

        /*
         * EVENTS METHODS
         */

        protected override void OnDriverLeft(Mobile pDriver)
        {
            base.OnDriverLeft(pDriver);
            if (!pDriver.OverlapsArea(stopLine))
                DriverStopsWaiting();
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
                    PlayerSteppedOnWrongObject(linePolygon, lAreaGoingReverse ? T_KEY_BACKED_ON : T_KEY_STEPPED_ON);
            }
        }

        private void OnDriverLeavesLine(Area2D pArea)
        {
            if (pArea is not Mobile)
                return;

            if (!pArea.OverlapsArea(this))
            {
                if (driverFinishedWaiting)
                    DriverStopsWaiting();
                else
                {
                    EmitSignal(SignalName.DriverRunnedOver);
                    if (pArea is Player)
                        PlayerSteppedOnWrongObject(linePolygon, T_KEY_RUNNED_OVER);
                }
            }
        }

        /*
         * TIMER METHODS
         */

        protected override void WaitTimerProgressed(float pElapsedTime)
        {
            base.WaitTimerProgressed(pElapsedTime);
            signShader.SetShaderParameter(SIGN_SHADER_ANGLE_PARAM, pElapsedTime * SIGN_SHADER_ANGLE_MAX_VALUE / STOP_TIME);
        }

        protected override void WaitTimerCompleted()
        {
            base.WaitTimerCompleted();
            driverFinishedWaiting = true;
            SucessAnim();
        }

        protected override void WaitTimerStopped()
        {
            base.WaitTimerStopped();
            ResetShaderValues();
        }

        /*
         * VISUALS METHODS
         */

        private void DriverStopsWaiting()
        {
            driverFinishedWaiting = false;
            shaderTween = CreateTween();
            shaderTween.TweenProperty(this, nameof(ShaderAlpha), 0f, ANIM_DEFAULT_DURATION * 2f);
            shaderTween.Connect(Tween.SignalName.Finished, Callable.From(ResetShaderValues));
        }

        private void SucessAnim()
        {
            Tween lTween = CreateTween();
            lTween.TweenProperty(sign, TweenProp.SCALE, sign.Scale * SUCCESS_ANIM_MULT, ANIM_DEFAULT_DURATION)
                .SetTrans(Tween.TransitionType.Back)
                .SetEase(Tween.EaseType.In);
            lTween.TweenProperty(sign, TweenProp.SCALE, sign.Scale, ANIM_DEFAULT_DURATION * 2f)
                .SetTrans(Tween.TransitionType.Bounce)
                .SetEase(Tween.EaseType.Out);
        }

        private void ResetShaderValues()
        {
            ShaderAlpha = 1f;
            signShader.SetShaderParameter(SIGN_SHADER_ANGLE_PARAM, default);
            if (IsInstanceValid(shaderTween))
            {
                shaderTween.Kill();
                shaderTween = null;
            }
        }

        protected override void Dispose(bool pDisposing)
        {
            if (IsInstanceValid(stopLine))
            {
                stopLine.AreaEntered -= OnDriverStepsOnLine;
                stopLine.AreaExited -= OnDriverLeavesLine;
            }
            base.Dispose(pDisposing);
        }

    }
}
