using Com.IsartDigital.WatchOut.Utils;
using Godot;
using System;

// Author : Camille Smolarski

namespace Com.IsartDigital.WatchOut.GameObjects
{
    public partial class Mobile : GameObject
    {
        #region Exports
        [Export] protected PathFollow2D pathFollow;
        [Export] private bool autoRestartOnEnd = true;
        [Export] private bool animated = true;
        [ExportGroup("Physics")]
        [Export] private bool startAtMaxSpeed = default;
        [Export] protected float maxForwardSpeed = 750f;
        [Export] private float maxBackwardSpeed = 250f;
        [Export] protected float TimeToMaxSpeed { get; private set; } = 1.5f;
        [Export] protected float EngineBrakeForce { get; private set; } = 0.1f;
        [Export] protected float ManualBrakeForce { get; private set; } = 0.015f;
        [Export] protected float EmergencyBrakeForce { get; private set; } = 0.005f;
        #endregion

        public const float MIN_SPEED_THRESHOLD = 50f;
        private const float ACCEL_CURVE_POW = 1.5f;
        private const string POLYGONS_PATH = "polygons";
        private const string EXHAUST_PARTICLES_PATH = "Particles";

        public enum GearMode
        {
            PARKED = -2,
            REVERSE = -1,
            NEUTRAL = 0,
            DRIVE = 1
        }

        public int Direction { get; private set; }
        public float Speed { get; private set; }
        protected bool IsMoving => Speed > 0;
        protected virtual GearMode SelectedGearMode { get; set; } = GearMode.PARKED;
        protected GpuParticles2D ExhaustParticels { get; private set; }
        private Vector2 VelocityDirection => (GlobalPosition - lastPos).Normalized();

        private Vector2 polygonsScale, lastPos;
        private Node2D polygons;
        private bool collisionsEnabled, resetStartAtMaxSpeed;
        private float elapsedTime, flutterTime, baseRotation, maxSpeed, brakeForce;

        public override void _Ready()
        {
            base._Ready();
            ExhaustParticels = GetNode<GpuParticles2D>(EXHAUST_PARTICLES_PATH);
            polygons = GetNode<Node2D>(POLYGONS_PATH);
            resetStartAtMaxSpeed = startAtMaxSpeed;
            lastPos = GlobalPosition;
            polygonsScale = polygons.Scale;
        }

        public override void _Process(double pDelta)
        {
            base._Process(pDelta);
            if (animated)
            {
                flutterTime += (float)pDelta;
                if (flutterTime > 0.1f)
                    flutterTime = default;
                polygons.Scale = polygonsScale + Vector2.One * 0.3f * flutterTime;
            }
        }

        protected override void OnHit(Area2D pArea)
        {
            if (pArea is not Mobile || !collisionsEnabled  || !((Mobile)pArea).collisionsEnabled)
                return;

            Speed = maxForwardSpeed;
            Direction *= -Mathf.Sign(MathS.Dot(VelocityDirection, (pArea.GlobalPosition - GlobalPosition).Normalized()));
            StartBraking(EmergencyBrakeForce);
            OnAccident((Mobile) pArea);
        }

        public virtual void Appear()
        { }

        protected virtual void OnAccident(Mobile pDriver)
        {
            animated = ExhaustParticels.Emitting = false;
            polygons.Scale = polygonsScale;
        }

        public void StartMovingForward()
        {
            StartMoving(GearMode.DRIVE);
        }

        protected void StartMovingBackward()
        {
            StartMoving(GearMode.REVERSE);
        }

        private void StartMoving(GearMode pDirection)
        {
            if (pDirection != GearMode.DRIVE && pDirection != GearMode.REVERSE)
                throw new Exception("Invalid direction.");

            if (Direction == (int)GearMode.REVERSE)
                StopMoving();

            Direction = (int)(SelectedGearMode = pDirection);
            maxSpeed = SelectedGearMode == GearMode.DRIVE ? maxForwardSpeed : maxBackwardSpeed;

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

        private void Move(float pDelta)
        {
            if (!pathFollow.Loop && pathFollow.ProgressRatio >= 1)
                OnReachPathEnd();

            pathFollow.Progress += Speed * Direction * pDelta;

            lastPos = GlobalPosition;
            GlobalPosition = pathFollow.GlobalPosition;
            GlobalRotation = pathFollow.GlobalRotation - baseRotation;
        }

        protected virtual void OnReachPathEnd()
        {
            if(autoRestartOnEnd)
                StopAndReset();
        }

        protected void StartBraking(float pForce)
        {
            doAction = Brake;
            brakeForce = pForce;
            if (pForce == EngineBrakeForce)
                SelectedGearMode = GearMode.NEUTRAL;
        }

        private void Brake(float pDelta)
        {
            Speed *= Mathf.Pow(brakeForce, pDelta);
            if (Speed < MIN_SPEED_THRESHOLD)
            {
                StopMoving();
                if (brakeForce == ManualBrakeForce)
                    StartMovingBackward();
                return;
            }
            Move(pDelta);
        }

        protected void StopMoving()
        {
            SelectedGearMode = GearMode.PARKED;
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
