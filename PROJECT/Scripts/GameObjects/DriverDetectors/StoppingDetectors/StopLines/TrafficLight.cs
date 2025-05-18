using Godot;
using System;

// Author : Camille Smolarski

namespace Com.IsartDigital.WatchOut.GameObjects.DriverDetectors.StoppingDetectors.StopLines
{
    public partial class TrafficLight : StoppingLine
    {
        [Signal] public delegate void TurnedGreenEventHandler();
        [Signal] public delegate void TurnedRedEventHandler();

        #region Exports
        [Export] private bool startGreen;
        [Export] private float greenLightDuration = 8f;
        [Export] private float redLightDuration = 4f;
        [ExportGroup("Lights")]
        [Export] private bool showBgLight = true;
        [Export] private Node2D backgroundLight;
        [Export] private Polygon2D redLight;
        [Export] private Polygon2D orangeLight;
        [Export] private Polygon2D greenLight;
        #endregion

        #region Consts
        private const string SHADER_ALPHA_PARAM = "alpha";
        private const string SHADER_ANGLE_PARAM = "angle";
        private const string SHADER_COLOR_PARAM = "color";
        private const string SHADER_PREV_COLOR_PARAM = "prev_color";
        private const float ORANGE_TIME_MULT = 2f / 3f;
        #endregion

        #region Shader parameters
        private float ShaderAngle
        {
            get => (float)lightShader.GetShaderParameter(SHADER_ANGLE_PARAM);
            set => lightShader.SetShaderParameter(SHADER_ANGLE_PARAM, value);
        }

        private Color ShaderPrevColor
        {
            get => (Color)lightShader.GetShaderParameter(SHADER_PREV_COLOR_PARAM);
            set => lightShader.SetShaderParameter(SHADER_PREV_COLOR_PARAM, value);
        }
        #endregion

        public bool IsGreen => currentLight == greenLight;
        protected override float StopTime => lightTime;

        private static readonly Color offModulate = new(0.4f, 0.4f, 0.4f);

        private Polygon2D currentLight;
        private ShaderMaterial lightShader;
        private float lightTime;

        public override void _Ready()
        {
            base._Ready();
            backgroundLight.Material = lightShader = (ShaderMaterial)backgroundLight.Material.Duplicate();
            lightShader.SetShaderParameter(SHADER_ALPHA_PARAM, showBgLight ? 1 : 0);
            startTimerOnDriverDetected = false;
            currentLight = startGreen ? greenLight : redLight;
            driverCanGo = startGreen;
            StartWaitTimer();
        }

        /*
         * LIGHT TIMER
         */

        protected override void StartWaitTimer()
        {
            TurnLightOn(driverCanGo ? greenLight : redLight);
            lightTime = currentLight == greenLight ? greenLightDuration : redLightDuration;
            SetShaderColor();
            base.StartWaitTimer();
        }

        protected override void WaitTimerProgressed(float pElapsedTime)
        {
            base.WaitTimerProgressed(pElapsedTime);
            ShaderAngle = pElapsedTime * 360f / lightTime;
            if (driverCanGo && pElapsedTime > ORANGE_TIME_MULT * greenLightDuration && currentLight != orangeLight)
                LightUpOrange();
        }

        protected override void WaitTimerCompleted()
        {
            base.WaitTimerCompleted();
            driverCanGo = !driverCanGo;
            EmitSignal(driverCanGo ? SignalName.TurnedGreen : SignalName.TurnedRed);
            StartWaitTimer();
        }

        /*
         * COLOR METHODS 
         */

        private void SetShaderColor()
        {
            ShaderPrevColor = currentLight.Color;
            lightShader.SetShaderParameter(SHADER_COLOR_PARAM, currentLight == redLight ? greenLight.Color : redLight.Color);
            ShaderAngle = default;
        }

        private void TurnLightOn(Polygon2D pLight)
        {
            currentLight = pLight;
            foreach (Polygon2D lLight in new Polygon2D[] { redLight, orangeLight, greenLight })
                lLight.Modulate = offModulate;
            pLight.Modulate = Colors.White;
        }

        private void LightUpOrange()
        {
            TurnLightOn(orangeLight);
            Tween lTween = CreateTween();
            lTween.TweenProperty(this, nameof(ShaderPrevColor), orangeLight.Color, ORANGE_TIME_MULT * greenLightDuration * 0.2f);
        }
    }
}
