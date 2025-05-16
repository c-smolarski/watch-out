using Com.IsartDigital.WatchOut.GameObjects.DriverDetectors;
using Com.IsartDigital.WatchOut.Managers;
using Com.IsartDigital.WatchOut.Utils;
using Godot;
using System;

// Author : Camille Smolarski

namespace Com.IsartDigital.WatchOut.GameObjects.Mobiles
{
    public partial class Pedestrian : Mobile
    {
        [Export] private float minForwardSpeed = 100f;

        public const uint COLLISION_LAYER = 4;
        private const float MODULATE_PATH_DIVIDER = 3f;
        private const float ACCIDENT_FRICTION = 0.98f;
        private const float ACCIDENT_SPEED_THRESHOLD = 10f;
        private const string COLLIDER_PATH = "collider";
        private const string DISABLED = "disabled";

        private CollisionShape2D collider;
        private float accidentSpeed;
        private Vector2 accidentDirection;

        public override void _Ready()
        {
            base._Ready();
            collider = GetNode<CollisionShape2D>(COLLIDER_PATH);
            collider.SetDeferred(DISABLED, true);
            MoveParticles.Emitting = false;
        }

        public override void _Process(double pDelta)
        {
            base._Process(pDelta);
            if (accidentSpeed > ACCIDENT_SPEED_THRESHOLD)
            {
                GlobalPosition += accidentDirection * accidentSpeed * (float)pDelta;
                accidentSpeed *= ACCIDENT_FRICTION;
            }
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
            collider.SetDeferred(DISABLED, false);
            SetModulateAlpha(0f);
            MoveParticles.Emitting = true;
            StartMovingForward();
        }

        protected override void Move(float pDelta)
        {
            base.Move(pDelta);
            SetModulateAlpha(
                pathFollow.ProgressRatio < 1f / MODULATE_PATH_DIVIDER ? pathFollow.ProgressRatio * MODULATE_PATH_DIVIDER :
                pathFollow.ProgressRatio > 1f - (1f / MODULATE_PATH_DIVIDER) ? (1f - pathFollow.ProgressRatio) * MODULATE_PATH_DIVIDER : 1f
            );
        }

        protected override void OnReachPathEnd()
        {
            QueueFree();
        }

        protected override void OnHit(Area2D pArea)
        {
            if (pArea is Pedestrian)
                return;
            base.OnHit(pArea);
        }

        protected override void OnAccident(Mobile pMobile)
        {
            StopMoving();
            if (pMobile is Vehicle)
            {
                accidentSpeed = pMobile.Speed;
                accidentDirection = pMobile.VelocityDirection;
            }
        }

        private void SetModulateAlpha(float pValue)
        {
            Color lColor = Modulate;
            lColor.A = pValue;
            Modulate = lColor;
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
