using Com.IsartDigital.Utils.Tweens;
using Godot;
using System;

// Author : Camille Smolarski

namespace Com.IsartDigital.WatchOut.GameObjects.Mobiles.Drivers
{
    public partial class Cop : Driver
    {
        [Export] private PointLight2D[] lights;
        [Export] private float lightMaxStrength = 1f;
        [Export] private float lightTime = 0.75f;
        [Export] private bool sirensOnReady;

        private int lightIdx;
        private Tween lightsTween;

        public override void _Ready()
        {
            base._Ready();
            if (sirensOnReady)
                TurnOnSirens();
        }

        protected override void OnReachPathEnd()
        {
            base.OnReachPathEnd();
            TurnOffSirens();
        }

        private void TurnOnSirens()
        {
            if (IsInstanceValid(lightsTween))
                TurnOffSirens();

            lightsTween = CreateTween();
            lightsTween.TweenProperty(lights[lightIdx], (string)PointLight2D.PropertyName.Enabled, true, default);
            lightsTween.TweenProperty(lights[lightIdx], (string)PointLight2D.PropertyName.Energy, lightMaxStrength, lightTime * 0.5f);
            lightsTween.TweenProperty(lights[lightIdx], (string)PointLight2D.PropertyName.Energy, 0f, lightTime * 0.5f);
            lightsTween.TweenProperty(lights[lightIdx], (string)PointLight2D.PropertyName.Enabled, false, default);
            lightsTween.Connect(Tween.SignalName.Finished, Callable.From(TurnOnSirens));

            lightIdx = ++lightIdx % lights.Length;
        }

        private void TurnOffSirens()
        {
            lightsTween.Kill();
            lightsTween = null;
            foreach (PointLight2D lLight in lights)
            {
                lLight.Energy = default;
                lLight.Enabled = false;
            }
        }
    }
}
