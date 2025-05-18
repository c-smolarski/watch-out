using Com.IsartDigital.WatchOut.Managers;
using Com.IsartDigital.WatchOut.Utils.Paths;
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

        private const float TIME_TO_MIN_VOLUME = 1.5f;

        private int lightIdx;
        private bool muteSirens;
        private Tween lightsTween;
        private AudioStreamPlayer sirens;
        private float elapsedTime;

        public override void _Ready()
        {
            base._Ready();
            if (sirensOnReady)
                TurnOnSirens();
        }

        public override void _Process(double pDelta)
        {
            base._Process(pDelta);
            if (muteSirens)
            {
                if (!IsInstanceValid(sirens))
                {
                    muteSirens = false;
                    sirens = null;
                    return;
                }

                elapsedTime += (float)pDelta;
                float lValue = Mathf.Clamp(1 - elapsedTime / TIME_TO_MIN_VOLUME, 0, 1);

                sirens.VolumeDb = Mathf.LinearToDb(lValue);

                if (lValue == default)
                {
                    muteSirens = false;
                    sirens.QueueFree();
                    sirens = null;
                }

            }
        }

        protected override void OnAccident(Mobile pMobile)
        {
            TurnOffSirens();
            muteSirens = true;
            base.OnAccident(pMobile);
        }

        protected override void OnReachPathEnd()
        {
            base.OnReachPathEnd();
            muteSirens = true;
            TurnOffSirens();
        }

        private void TurnOnSirens()
        {
            TurnOffSirens();

            if (sirens == null)
                sirens = SoundManager.Instance.Play(SoundManager.Instance.Sirens, true, SoundPath.SFX_SOUND_BUS);

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
            if (IsInstanceValid(lightsTween))
                lightsTween.Kill();
            lightsTween = null;

            foreach (PointLight2D lLight in lights)
            {
                lLight.Energy = default;
                lLight.Enabled = false;
            }
        }

        protected override void Dispose(bool pDisposing)
        {
            if (IsInstanceValid(sirens) && sirens.Playing)
                sirens.QueueFree();
            base.Dispose(pDisposing);
        }
    }
}
