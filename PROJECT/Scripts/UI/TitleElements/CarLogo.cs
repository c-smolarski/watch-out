using Com.IsartDigital.Utils.Effects;
using Com.IsartDigital.Utils.Tweens;
using Com.IsartDigital.WatchOut.Enums;
using Com.IsartDigital.WatchOut.Managers;
using Com.IsartDigital.WatchOut.Scripts.UI;
using Godot;
using System;

// Author : Camille Smolarski

namespace Com.IsartDigital.WatchOut.UI
{
    public partial class CarLogo : TitleElement
    {
        [Export] private TextureRect[] tires;
        [Export] private StartButton startButton;
        [Export] private CpuParticles2D idleParticles;
        [Export] private CpuParticles2D movingParticles;
        [Export] private Shaker shaker;

        private const string POLYGON_CONTAINER_PATH = "polygons";

        private const float IDLE_TIRE_SPEED = 1.25f;
        private const float START_MOVE_TIRE_SPEED = 20f;
        private const float MOVE_TIRE_SPEED = 200f;

        private const float BOUCE_DURATION = 0.1f;
        private const float MOVE_DURATION = 1.5f;
        private const float START_MOVE_DURATION = MOVE_DURATION * 0.3f;
        private const float MOVE_ANIM_X_MARGIN = 500f;

        private static readonly Vector2 bounceStretch = new Vector2(1.25f, 0.8f);

        private Vector2 baseScale;
        private float currentTireSpeed;
        private Node2D polygonContainer;

        public override void _Ready()
        {
            base._Ready();
            baseScale = Scale;
            polygonContainer = GetNode<Node2D>(POLYGON_CONTAINER_PATH);
            StartIdleAnim();
        }

        public override void _Process(double pDelta)
        {
            base._Process(pDelta);
            RotateTires(currentTireSpeed, (float)pDelta);
        }

        private void StartIdleAnim()
        {
            currentTireSpeed = IDLE_TIRE_SPEED;
        }

        protected override void PlayAnim()
        {
            idleParticles.Emitting = false;
            Tween lTween = CreateTween();
            lTween.TweenProperty(this, TweenProp.GLOBAL_POSITION_Y, startButton.GlobalPosition.Y, FALL_ANIM_DURATION)
                .SetTrans(Tween.TransitionType.Bounce)
                .SetEase(Tween.EaseType.Out);
            lTween.Parallel().TweenProperty(this, TweenProp.GLOBAL_POSITION_X, (GlobalPosition.X - GetViewportRect().Position.X) * 0.5f, FALL_ANIM_DURATION)
                .SetTrans(Tween.TransitionType.Quad)
                .SetEase(Tween.EaseType.In);
            lTween.Parallel().TweenProperty(this, nameof(currentTireSpeed), 0f, FALL_ANIM_DURATION);
            lTween.Parallel().TweenProperty(this, TweenProp.SCALE, baseScale * bounceStretch, BOUCE_DURATION)
                .SetDelay(FIRST_BOUCE_TIME)
                .Connect(PropertyTweener.SignalName.Finished, Callable.From(OnBounce));
            lTween.Parallel().TweenProperty(this, TweenProp.SCALE, baseScale, BOUCE_DURATION)
                .SetDelay(FIRST_BOUCE_TIME + BOUCE_DURATION);

            lTween.TweenProperty(movingParticles, (string)CpuParticles2D.PropertyName.Emitting, true, 0f);
            lTween.TweenProperty(this, TweenProp.GLOBAL_POSITION_X, GetViewportRect().Size.X * 0.5f, MOVE_DURATION)
                .SetTrans(Tween.TransitionType.Cubic)
                .SetEase(Tween.EaseType.InOut);
            lTween.Parallel().TweenProperty(this, nameof(currentTireSpeed), START_MOVE_TIRE_SPEED, START_MOVE_DURATION);
            lTween.Parallel().TweenProperty(this, nameof(currentTireSpeed), 0f, MOVE_DURATION * 0.5f)
                .SetTrans(Tween.TransitionType.Cubic)
                .SetEase(Tween.EaseType.In)
                .SetDelay(START_MOVE_DURATION);
            lTween.Parallel().TweenProperty(movingParticles, (string)CpuParticles2D.PropertyName.Emitting, false, 0f)
                .SetDelay(START_MOVE_DURATION);

            lTween.TweenProperty(movingParticles, (string)CpuParticles2D.PropertyName.Emitting, true, 0f);
            lTween.TweenProperty(this, TweenProp.GLOBAL_POSITION_X, GetViewportRect().Size.X + MOVE_ANIM_X_MARGIN, MOVE_DURATION)
                .SetTrans(Tween.TransitionType.Cubic)
                .SetEase(Tween.EaseType.In);
            lTween.Parallel().TweenProperty(this, nameof(currentTireSpeed), MOVE_TIRE_SPEED, MOVE_DURATION)
                .SetTrans(Tween.TransitionType.Cubic)
                .SetEase(Tween.EaseType.In);
        }

        private void OnBounce()
        {
            shaker.Start();
            GameManager.Vibrate(VibrationDuration.SHORT);
        }

        private void RotateTires(float pSpeed, float pDelta)
        {
            foreach (TextureRect lTire in tires)
                lTire.Rotation = (lTire.Rotation + pDelta * pSpeed) % Mathf.Tau;
        }
    }
}
