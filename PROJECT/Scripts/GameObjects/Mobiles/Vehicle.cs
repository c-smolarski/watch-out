using Com.IsartDigital.WatchOut.Managers;
using Com.IsartDigital.WatchOut.Utils;
using Godot;
using System;

namespace Com.IsartDigital.WatchOut.GameObjects.Mobiles
{
    public partial class Vehicle : Mobile
    {
        [Export] private bool animated = true;
        [ExportGroup("Brakes")]
        [Export] protected float EngineBrakeForce { get; private set; } = 0.2f;
        [Export] protected float ManualBrakeForce { get; private set; } = 0.015f;
        [Export] protected float EmergencyBrakeForce { get; private set; } = 0.005f;

        private const float FLUTTER_PER_SECOND = 10f;
        private const float FLUTTER_SCALE = 0.3f;
        private const float ACCIDENT_DELAY = 0.5f;

        private const string POLYGONS_PATH = "polygons";
        private const string ACCIDENT_PARTICLES_PATH = "accidentParticles";
        private const string EXPLOSION_PARTICLES_PATH = "explosionParticles";

        public enum GearMode
        {
            PARKED = -2,
            REVERSE = -1,
            NEUTRAL = 0,
            DRIVE = 1
        }

        protected virtual GearMode SelectedGearMode { get; set; } = GearMode.PARKED;

        private Node2D polygons;
        private Vector2 polygonsScale;
        private float flutterTime, brakeForce;
        private CpuParticles2D explosionParticles, accidentParticles;

        public override void _Ready()
        {
            base._Ready();
            accidentParticles = GetNode<CpuParticles2D>(ACCIDENT_PARTICLES_PATH);
            explosionParticles = GetNode<CpuParticles2D>(EXPLOSION_PARTICLES_PATH);
            polygons = GetNode<Node2D>(POLYGONS_PATH);
            polygonsScale = polygons.Scale;
        }

        public override void _Process(double pDelta)
        {
            base._Process(pDelta);
            if (animated)
            {
                flutterTime += (float)pDelta;
                if (flutterTime > 1 / FLUTTER_PER_SECOND)
                    flutterTime = default;
                polygons.Scale = polygonsScale + Vector2.One * FLUTTER_SCALE * flutterTime;
            }
        }

        protected override void OnAccident(Mobile pMobile)
        {
            if (pMobile is Vehicle)
            {
                Speed = maxForwardSpeed;
                Direction *= -Mathf.Sign(MathS.Dot(VelocityDirection, (pMobile.GlobalPosition - GlobalPosition).Normalized()));
                explosionParticles.Emitting = true;
                GetTree().CreateTimer(ACCIDENT_DELAY, false).Connect(
                    Timer.SignalName.Timeout,
                    Callable.From(AccidentJuiciness));
            }

            SoundManager.Instance.PlaySFX(SoundManager.Instance.Accident, ACCIDENT_DELAY);
            StartBraking(EmergencyBrakeForce);
            animated = MoveParticles.Emitting = false;
            polygons.Scale = polygonsScale;
        }

        private void AccidentJuiciness()
        {
            accidentParticles.Emitting = true;
            SoundManager.Instance.PlaySFX(SoundManager.Instance.Horn, ACCIDENT_DELAY);
        }

        protected override void StartMovingForward()
        {
            StartMoving(GearMode.DRIVE);
        }

        protected override void StartMovingBackward()
        {
            StartMoving(GearMode.REVERSE);
        }

        protected virtual void StartMoving(GearMode pDirection)
        {
            if (!MoveParticles.Emitting)
                return;

            if (pDirection != GearMode.DRIVE && pDirection != GearMode.REVERSE)
                throw new Exception("Invalid direction.");

            StartMoving(pDirection == GearMode.DRIVE);
            SelectedGearMode = pDirection;
        }

        protected virtual void StartBraking(float pForce)
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

        protected override void StopMoving()
        {
            SelectedGearMode = GearMode.PARKED;
            base.StopMoving();
        }
    }
}
