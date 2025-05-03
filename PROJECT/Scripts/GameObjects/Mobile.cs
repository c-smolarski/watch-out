using Com.IsartDigital.OneButtonGame.Utils;
using Godot;
using System;

// Author : Camille Smolarski

namespace Com.IsartDigital.OneButtonGame.GameObjects
{
    public partial class Mobile : GameObject
    {
        #region Exports
        [Export] private PathFollow2D pathFollow;
        [Export] private bool autoRestartOnEnd = true;
        [ExportGroup("Physics")]
        [Export] private bool startAtMaxSpeed = default;
        [Export] private float maxForwardSpeed = 750f;
        [Export] private float maxBackwardSpeed= 250f;
        [Export] protected float TimeToMaxSpeed { get; private set; } = 1.5f;
        [Export] protected float EngineBrakeForce { get; private set; } = 0.1f;
        [Export] protected float BrakeForce { get; private set; } = 0.015f;
        [Export] protected float EmergencyBrakeForce { get; private set; } = 0.005f;
        #endregion

        public const float MIN_SPEED_THRESHOLD = 30f;
        private const float ACCEL_CURVE_POW = 1.5f;

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
        private Vector2 VelocityDirection => (GlobalPosition - lastPos).Normalized();

        private Vector2 lastPos;
        private bool collisionsEnabled, resetStartAtMaxSpeed;
        private float elapsedTime, baseRotation, maxSpeed, brakeForce;

        public override void _Ready()
        {
            base._Ready();
            resetStartAtMaxSpeed = startAtMaxSpeed;
            lastPos = GlobalPosition;
        }

        protected override void OnHit(Area2D pArea)
        {
            if (pArea is not Mobile || !collisionsEnabled  || !((Mobile)pArea).collisionsEnabled)
                return;

            Speed = maxForwardSpeed;
            Direction *= -Mathf.Sign(MathS.Dot(VelocityDirection, (pArea.GlobalPosition - GlobalPosition).Normalized()));
            StartBraking(EmergencyBrakeForce);
            ((Mobile)pArea).StartBraking(EmergencyBrakeForce);
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
            if (autoRestartOnEnd && !pathFollow.Loop && pathFollow.ProgressRatio >= 1)
                StopAndReset();
            else
                pathFollow.Progress += Speed * Direction * pDelta;

            lastPos = GlobalPosition;
            GlobalPosition = pathFollow.GlobalPosition;
            GlobalRotation = pathFollow.GlobalRotation - baseRotation;
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
                return;
            }
            Move(pDelta);
        }

        private void StopMoving()
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
