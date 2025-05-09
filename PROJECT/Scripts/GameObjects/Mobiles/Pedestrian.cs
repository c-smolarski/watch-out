using Com.IsartDigital.WatchOut.GameObjects.DriverDetectors;
using Com.IsartDigital.WatchOut.Managers;
using Com.IsartDigital.WatchOut.Utils;
using Com.IsartDigital.Utils.Tweens;
using Godot;
using System;

// Author : Camille Smolarski

namespace Com.IsartDigital.WatchOut.GameObjects.Mobiles
{
    public partial class Pedestrian : Mobile
    {
        [Export] private float minForwardSpeed = 100f;

        private const string COLLIDER_PATH = "collider";
        private const string DISABLED = "disabled";

        private const float TIME_TO_APPEAR = 2f;

        private CollisionShape2D collider;

        public override void _Ready()
        {
            base._Ready();
            collider = GetNode<CollisionShape2D>(COLLIDER_PATH);
            collider.SetDeferred(DISABLED, true);
            ExhaustParticels.Emitting = false;
        }

        private void Init()
        {
            GlobalPosition = pathFollow.GlobalPosition;
            pathFollow.Loop = false;
            maxForwardSpeed = GD.Randf() * (maxForwardSpeed - minForwardSpeed) + minForwardSpeed;
            Appear();
        }

        public override void Appear()
        {
            base.Appear();
            Tween lTween = CreateTween();
            lTween.TweenProperty(this, TweenProp.MODULATE_ALPHA, 1f, TIME_TO_APPEAR)
                .From(0f);
            lTween.Connect(
                Tween.SignalName.Finished,
                Callable.From(OnAppeared));
        }

        private void OnAppeared()
        {
            collider.SetDeferred(DISABLED, false);
            ExhaustParticels.Emitting = true;
            StartMovingForward();
        }

        protected override void OnReachPathEnd()
        {
            StopMoving();
            ExhaustParticels.Emitting = false;
            collider.SetDeferred(DISABLED, true);
            Tween lTween = CreateTween();
            lTween.TweenProperty(this, TweenProp.MODULATE_ALPHA, 0f, TIME_TO_APPEAR)
                .From(1f);
            lTween.Connect(
                Tween.SignalName.Finished,
                Callable.From(QueueFree));
        }

        protected override void OnAccident(Mobile pDriver)
        {
            base.OnAccident(pDriver);
        }

        public static Pedestrian Create(Crossing pCrossing)
        {
            Pedestrian lPedesterian = NodeCreator.CreateNode<Pedestrian>(
                GameManager.Instance.PackedPedestrian,
                pCrossing
                );

            pCrossing.Path.AddChild(lPedesterian.pathFollow = new PathFollow2D());
            lPedesterian.Init();
            return lPedesterian;
        }
    }
}
