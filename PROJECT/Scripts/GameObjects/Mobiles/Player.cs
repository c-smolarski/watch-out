using Com.IsartDigital.OneButtonGame.Managers;
using Godot;
using System;

namespace Com.IsartDigital.OneButtonGame.GameObjects.Mobiles
{
    public partial class Player : Mobile
    {
        [Export] private PathFollow2D pathFollow;
        [ExportGroup("Physics")]
        [Export] private float timeToMaxSpeed = 1.5f;
        [Export] private float maxForwardSpeed = 750f;
        [Export] private float maxBackwardSpeed = 250f;
        [Export] private float engineBrakeForce = 0.15f;
        [Export] private float brakeForce = 0.015f;

        private enum DriveDirection
        {
            BACKWARD = -1,
            STOPPED = 0,
            FORWARD = 1
        }

        private bool IsMoving => currentSpeed > 0;

        private Action<float> doAction = default;
        private DriveDirection direction = default;
        private float elapsedTime, baseRotation, currentSpeed, currentMaxSpeed, currentBrakeForce;

        public override void _Ready()
        {
            base._Ready();
            InputManager.Instance.StartedHolding += OnInputHold;
            InputManager.Instance.StoppedHolding += OnInputRelease;
        }

        public override void _Process(double pDelta)
        {
            base._Process(pDelta);
            float lDelta = (float)pDelta;
            if (currentSpeed >= 0 && doAction != null)
                doAction(lDelta);
        }

        private void OnInputHold(int pNPrevTap)
        {
            if (pNPrevTap == 0)
                StartMoving(DriveDirection.FORWARD, maxForwardSpeed);
            else if (IsMoving)
                StartBraking(brakeForce);
            else
                StartMoving(DriveDirection.BACKWARD, maxBackwardSpeed);
        }

        private void OnInputRelease()
        {
            if (IsMoving)
                StartBraking(engineBrakeForce);
        }

        private void StartMoving(DriveDirection pDirection, float pMaxSpeed)
        {
            currentMaxSpeed = pMaxSpeed;
            direction = pDirection;
            doAction = Accelerate;
        }

        private void Accelerate(float pDelta)
        {
            elapsedTime += pDelta;
            currentSpeed += currentMaxSpeed * pDelta * Mathf.Pow(elapsedTime / timeToMaxSpeed, 2f);

            if (currentSpeed >= currentMaxSpeed)
            {
                currentSpeed = currentMaxSpeed;
                elapsedTime = default;
                doAction = Move;
            }

            Move(pDelta);
        }

        private void Move(float pDelta)
        {
            pathFollow.Progress += currentSpeed * (float)direction * pDelta;
            GlobalPosition = pathFollow.GlobalPosition;
            GlobalRotation = pathFollow.GlobalRotation - baseRotation;
        }

        private void StartBraking(float pForce)
        {
            currentBrakeForce = pForce;
            doAction = Brake;
        }

        private void Brake(float pDelta)
        {
            currentSpeed *= Mathf.Pow(currentBrakeForce, pDelta);
            if (currentSpeed < 30)
            {
                StopMoving();
                return;
            }
            Move(pDelta);
        }

        private void StopMoving()
        {
            currentSpeed = 0;
            doAction = null;
        }
    }
}
