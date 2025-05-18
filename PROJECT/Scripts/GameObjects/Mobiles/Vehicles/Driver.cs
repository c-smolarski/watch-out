using Com.IsartDigital.WatchOut.GameObjects.DriverDetectors.StoppingDetectors.StopLines;
using Godot;
using System;
using System.Collections.Generic;

// Author : Camille Smolarski

namespace Com.IsartDigital.WatchOut.GameObjects.Mobiles
{
    public partial class Driver : Vehicle
    {
        #region Exports
        [Export] private bool visibleOnReady;
        [Export] private bool startOnReady;
        [Export] private bool manualBrakeReverse;
        [Export] private bool stopAtRedLights = true;
        [ExportGroup("Mobile detector")]
        [Export] private bool stopForPlayer = true;
        [Export] private bool stopForNPC = true;
        #endregion

        private const string MOBILE_DETECTOR_PATH = "mobileDetect";

        private Area2D mobileDetector;
        private TrafficLight light;

        public override void _Ready()
        {
            base._Ready();
            mobileDetector = GetNode<Area2D>(MOBILE_DETECTOR_PATH);
            CollisionsDictInit(ref collisionLayersDict);
            CollisionInit(mobileDetector);

            Visible = visibleOnReady;
            if (startOnReady)
                StartMovingForward();

            mobileDetector.AreaEntered += OnMobileDetected;
            mobileDetector.AreaExited += OnMobileLeft;
        }

        protected override void OnHit(Area2D pArea)
        {
            if (stopAtRedLights && pArea is TrafficLight && !((TrafficLight)pArea).IsGreen)
            {
                StartBraking(ManualBrakeForce);
                (light = (TrafficLight)pArea).TurnedGreen += OnGreenLight;
            }

            base.OnHit(pArea);
        }

        private void OnGreenLight()
        {
            DisconnectLight();
            StartMovingForward();
        }

        private void DisconnectLight()
        {
            if (!IsInstanceValid(light))
                return;

            light.TurnedGreen -= OnGreenLight;
            light = null;
        }

        protected override void StartMoving(GearMode pDirection)
        {
            Visible = true;
            base.StartMoving(pDirection);
        }

        private void Brake()
        {
            StartBraking(EngineBrakeForce);
        }

        protected override void OnBrakeStop()
        {
            if (manualBrakeReverse)
                base.OnBrakeStop();
        }

        protected override void OnReachPathEnd()
        {
            Visible = false;
            base.OnReachPathEnd();
        }

        private void CollisionsDictInit(ref Dictionary<uint, bool> pCollisionDict)
        {
            pCollisionDict = new() {
                { Player.COLLISION_LAYER, stopForPlayer},
                { NPC_COLLISION_LAYER, stopForNPC}};
        }

        private void OnMobileDetected(Area2D pArea)
        {
            if (!CanDetect(pArea))
                return;
            StartBraking(ManualBrakeForce);
        }

        private void OnMobileLeft(Area2D pArea)
        {
            if (!CanDetect(pArea))
                return;

            foreach (Area2D lArea in mobileDetector.GetOverlappingAreas())
                if (lArea is Mobile && lArea != this)
                    return;

            StartMovingForward();
        }

        private bool CanDetect(Area2D pArea)
        {
            return pArea != this && pArea is Mobile && ((Mobile)pArea).CollisionsEnabled;
        }

        protected override void Dispose(bool pDisposing)
        {
            if (IsInstanceValid(mobileDetector))
            {
                mobileDetector.AreaEntered -= OnMobileDetected;
                mobileDetector.AreaExited -= OnMobileLeft;
                mobileDetector = null;
            }
            DisconnectLight();
            base.Dispose(pDisposing);
        }
    }
}
