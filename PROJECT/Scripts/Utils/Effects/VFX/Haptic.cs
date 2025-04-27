using Godot;
using System;

namespace Com.IsartDigital.Utils.Effects {

    public partial class Haptic : Sprite2D {
        [ExportGroup("General")]

        [Export] private int[] joyPads= new int[0];

        [Export] private float duration = 2f;
        [Export(PropertyHint.Range, "0,1")] private float weakMagnitude = 0.5f;
        [Export(PropertyHint.Range, "0,1")] private float strongMagnitude = 0.5f;

        [Export] private bool showVibration = true;

        [ExportGroup("Attack")]

        [Export] private Tween.TransitionType transition_ = Tween.TransitionType.Sine;
        [Export] private Tween.EaseType ease_ = Tween.EaseType.InOut;
        [Export] private float duration_ = 0.25f;

        [ExportGroup("Release")]

        [Export] private Tween.TransitionType _transition = Tween.TransitionType.Sine;
        [Export] private Tween.EaseType _ease = Tween.EaseType.InOut;
        [Export] private float _duration = 0.25f;

        private Tween vibration;
        private float intensity;
        private Shaker shaker;

        public override void _Ready() {
            SetProcess(false);
            shaker = (Shaker)GetNode("Shaker");
            shaker.Initialize(duration_, duration, _duration, new Vector2(weakMagnitude, strongMagnitude));
        }

        public void Start() {

            intensity = 0f;

            vibration = CreateTween();
            vibration.TweenProperty(this, nameof(intensity), 1, duration_).SetTrans(transition_).SetEase(ease_);
            vibration.TweenInterval(duration);
            vibration.TweenProperty(this, nameof(intensity), 0, _duration).SetTrans(_transition).SetEase(_ease);
            vibration.Finished += Stop;
            if (showVibration) shaker.Start();
            SetProcess(true);
        }

        public override void _Process(double delta) {
            int lLength = joyPads.Length;
            for(int i = 0;i<lLength;i++) {
                if (Input.IsJoyKnown(i)) Input.StartJoyVibration(i, Mathf.Clamp(weakMagnitude*intensity,0f,1f), Mathf.Clamp(strongMagnitude * intensity, 0f, 1f));
            }
        }

        public void Stop() {
            int lLength = joyPads.Length;
            for (int i = 0; i < lLength; i++) {
                if (Input.IsJoyKnown(i)) Input.StopJoyVibration(i);
            }
            vibration?.Kill();
            vibration = null;
            shaker.Stop();
            SetProcess(false);
        }
        public bool isPlaying() {
            return IsProcessing();
        }

    }

    public partial class Shaker : Node {

        public void Initialize(float pDuration_, float pDuration, float p_Duration,Vector2 pAmplitude) {
            duration_ = pDuration_;
            duration = pDuration;
            _duration = p_Duration;
            amplitude = pAmplitude*5f;
        }
    }

}
