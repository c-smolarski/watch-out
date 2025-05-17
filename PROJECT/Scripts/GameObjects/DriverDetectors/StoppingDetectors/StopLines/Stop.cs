using Com.IsartDigital.WatchOut.Enums;
using Com.IsartDigital.WatchOut.GameObjects.Mobiles;
using Com.IsartDigital.WatchOut.Managers;
using Com.IsartDigital.Utils.Tweens;
using Godot;
using System;

// Author : Camille Smolarski

namespace Com.IsartDigital.WatchOut.GameObjects.DriverDetectors.StoppingDetectors.StopLines
{
    public partial class Stop : StoppingLine
    {
        #region Exports
        [Export] private Node2D sign;
        [Export] private Polygon2D redSignBG;
        #endregion

        #region Consts
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

        private Tween shaderTween;
        private ShaderMaterial signShader;

        public override void _Ready()
        {
            base._Ready();
            redSignBG.Material = signShader = (ShaderMaterial)redSignBG.Material.Duplicate();
        }

        /*
         * EVENTS METHODS
         */

        protected override void OnDriverLeft(Vehicle pVehicle)
        {
            base.OnDriverLeft(pVehicle);
            if (!pVehicle.OverlapsArea(Line))
                DriverStopsWaiting();
            else
                ShaderFadeOut();
        }

        protected override void OnDriverLeavesLine(Area2D pArea)
        {
            base.OnDriverLeavesLine(pArea);
            DriverStopsWaiting();
        }

        private void DriverStopsWaiting()
        {
            driverCanGo = false;
            ShaderFadeOut();
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
            driverCanGo = true;
            SucessAnim();
        }

        /*
         * VISUALS METHODS
         */

        private void SucessAnim()
        {
            GameManager.Vibrate(VibrationDuration.SHORT);
            Tween lTween = CreateTween();
            lTween.TweenProperty(sign, TweenProp.SCALE, sign.Scale * SUCCESS_ANIM_MULT, ANIM_DEFAULT_DURATION)
                .SetTrans(Tween.TransitionType.Back)
                .SetEase(Tween.EaseType.In);
            lTween.TweenProperty(sign, TweenProp.SCALE, sign.Scale, ANIM_DEFAULT_DURATION * 2f)
                .SetTrans(Tween.TransitionType.Bounce)
                .SetEase(Tween.EaseType.Out);
        }

        private void ShaderFadeOut()
        {
            shaderTween = CreateTween();
            shaderTween.TweenProperty(this, nameof(ShaderAlpha), 0f, ANIM_DEFAULT_DURATION * 2f);
            shaderTween.Connect(Tween.SignalName.Finished, Callable.From(ResetShaderValues));
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

    }
}
