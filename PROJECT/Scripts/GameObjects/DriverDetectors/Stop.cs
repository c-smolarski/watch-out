using Com.IsartDigital.OneButtonGame.GameObjects.Mobiles;
using Com.IsartDigital.OneButtonGame.Managers;
using Com.IsartDigital.Utils.Tweens;
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
        [Export] private Polygon2D linePolygon;
        [Export] private Polygon2D redSignBG;

        private const string T_KEY_STEPPED_ON = "STOP_STEPPED_ON";
        private const string T_KEY_BACKED_ON = "STOP_BACKED_ON";
        private const string T_KEY_RUNNED_OVER = "STOP_RUNNED_OVER";

        private const string SIGN_SHADER_ALPHA_PARAM = "mixAlpha";
        private const string SIGN_SHADER_ANGLE_PARAM = "angle";
        private const int SIGN_SHADER_ANGLE_MAX_VALUE = 360;

        private const float STOP_TIME = 3f;
        private const float STEP_ANIM_DURATION = 1f;

        private float ShaderAlpha
        {
            get => (float)signShader.GetShaderParameter(SIGN_SHADER_ALPHA_PARAM);
            set => signShader.SetShaderParameter(SIGN_SHADER_ALPHA_PARAM, value);
        }

        private float elapsedTime;
        private bool driverFinishedWaiting;
        private Tween shaderTween;
        private Mobile driverWaiting;
        private ShaderMaterial signShader;

        public override void _Ready()
        {
            base._Ready();
            if (OnlyDetectsPlayer)
                stopLine.CollisionLayer = stopLine.CollisionMask = Player.COLLISION_LAYER;
            AreaExited += OnDriverLeavesStopArea;
            stopLine.AreaEntered += OnDriverStepsOnLine;
            stopLine.AreaExited += OnDriverLeavesLine;
            redSignBG.Material = signShader = (ShaderMaterial)redSignBG.Material.Duplicate();
        }

        /*
         * EVENTS METHODS
         */

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
                {
                    StepAnim();
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
                    DriverStopsWaiting();
                else
                {
                    EmitSignal(SignalName.DriverRunnedOver);
                    if (pArea is Player)
                    {
                        StepAnim();
                        SignalBus.Instance.EmitSignal(SignalBus.SignalName.LevelSoftFailed, T_KEY_RUNNED_OVER);
                    }
                }
            }
        }

        /*
         * VISUALS METHODS
         */

        private void DriverStopsWaiting()
        {
            driverFinishedWaiting = false;
            shaderTween = CreateTween();
            shaderTween.TweenProperty(this, nameof(ShaderAlpha), 0f, 0.5f);
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

        private void StepAnim()
        {
            Tween lTween = CreateTween()
                .SetTrans(Tween.TransitionType.Circ)
                .SetEase(Tween.EaseType.Out);
            lTween.TweenProperty(linePolygon, TweenProp.COLOR, Colors.DarkRed, STEP_ANIM_DURATION);
        }

        /*
         * TIMER METHODS
         */

        private void StartWaitTimer()
        {
            doAction = WaitTimer;
            if (elapsedTime != default)
                elapsedTime = default;
            ResetShaderValues();
        }

        private void WaitTimer(float pDelta)
        {
            elapsedTime += pDelta;
            signShader.SetShaderParameter(SIGN_SHADER_ANGLE_PARAM, elapsedTime * SIGN_SHADER_ANGLE_MAX_VALUE / STOP_TIME);
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
