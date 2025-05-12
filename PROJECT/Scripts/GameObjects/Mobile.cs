using Com.IsartDigital.WatchOut.Utils;
using Godot;
using System;

// Author : Camille Smolarski

namespace Com.IsartDigital.WatchOut.GameObjects
{
    public abstract partial class Mobile : GameObject
    {
        #region Exports
        [Export] protected PathFollow2D pathFollow;
        [Export] private bool autoResetPosOnEnd = true;
        [ExportGroup("Speed")]
        [Export] private bool startAtMaxSpeed;
        [Export] protected float maxForwardSpeed = 750f;
        [Export] private float maxBackwardSpeed = 250f;
        [Export] protected float TimeToMaxSpeed { get; private set; } = 1.5f;
        #endregion

        public const uint NPC_COLLISION_LAYER = 3;
        public const float MIN_SPEED_THRESHOLD = 50f;
        private const float ACCEL_CURVE_POW = 1.5f;

        private const string MOVE_PARTICLES_PATH = "Particles";

        public int Direction { get; protected set; }
        public float Speed { get; protected set; }
        public Vector2 VelocityDirection => (GlobalPosition - lastPos).Normalized();
        protected bool IsMoving => Speed > 0;
        protected GpuParticles2D MoveParticles { get; private set; }

        private Vector2 lastPos;
        private bool collisionsEnabled, resetStartAtMaxSpeed;
        private float elapsedTime, flutterTime, maxSpeed;

        public override void _Ready()
        {
            base._Ready();
            MoveParticles = GetNode<GpuParticles2D>(MOVE_PARTICLES_PATH);
            resetStartAtMaxSpeed = startAtMaxSpeed;
            lastPos = GlobalPosition;
        }

        protected override void OnHit(Area2D pArea)
        {
            if (pArea is not Mobile || !collisionsEnabled  || !((Mobile)pArea).collisionsEnabled)
                return;

            OnAccident((Mobile)pArea);
        }

        public virtual void Appear()
        { }

        protected abstract void OnAccident(Mobile pMobile);

        protected virtual void StartMovingForward()
        {
            StartMoving(true);
        }

        protected virtual void StartMovingBackward()
        {
            StartMoving(false);
        }

        protected void StartMoving(bool pForward)
        {
            int lNewDirection = pForward ? 1 : -1;
            if (Direction != lNewDirection)
                StopMoving();

            Direction = lNewDirection;
            maxSpeed = pForward ? maxForwardSpeed : maxBackwardSpeed;

            if (Speed < MIN_SPEED_THRESHOLD)
                Speed = MIN_SPEED_THRESHOLD;

            if (startAtMaxSpeed)
            {
                Speed = maxSpeed;
                startAtMaxSpeed = false;
            }

            if (!collisionsEnabled)
                collisionsEnabled = true;

            doAction = Accelerate;
        }

        private void Accelerate(float pDelta)
        {
            elapsedTime += pDelta;
            Speed += maxSpeed * pDelta * Mathf.Pow(elapsedTime / TimeToMaxSpeed, ACCEL_CURVE_POW);

            if (Speed >= maxSpeed)
            {
                Speed = maxSpeed;
                elapsedTime = default;
                doAction = Move;
            }

            Move(pDelta);
        }

        protected virtual void Move(float pDelta)
        {
            if (!pathFollow.Loop && pathFollow.ProgressRatio >= 1)
                OnReachPathEnd();

            pathFollow.Progress += Speed * Direction * pDelta;

            lastPos = GlobalPosition;
            GlobalPosition = pathFollow.GlobalPosition;
            GlobalRotation = pathFollow.GlobalRotation;
        }

        protected virtual void OnReachPathEnd()
        {
            if(autoResetPosOnEnd)
                StopAndReset();
        }


        protected virtual void StopMoving()
        {
            Speed = Direction = default;
            doAction = null;
        }

        private void StopAndReset()
        {
            collisionsEnabled = false;
            startAtMaxSpeed = resetStartAtMaxSpeed;
            pathFollow.ProgressRatio = default;
            StopMoving();
        }
    }
}
