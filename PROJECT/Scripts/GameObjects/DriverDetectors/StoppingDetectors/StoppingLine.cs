using Com.IsartDigital.WatchOut.GameObjects.Mobiles.Drivers;
using Com.IsartDigital.WatchOut.GameObjects.Mobiles;
using Godot;
using System;

// Author : Camille Smolarski

namespace Com.IsartDigital.WatchOut.GameObjects.DriverDetectors.StoppingDetectors
{
    public abstract partial class StoppingLine : StoppingDetector
    {
        [Signal] public delegate void DriverSteppedOnLineEventHandler();
        [Signal] public delegate void DriverRunnedOverEventHandler();

        [Export] protected Area2D Line { get; private set; }
        [Export] private Node2D linePolygon;
        [ExportGroup("Translation Keys")]
        [Export] private string tKeySteppedOn = DEFAULT_T_KEY;
        [Export] private string tKeyRunnedOver = DEFAULT_T_KEY;
        [Export] private string tKeyBackedOn = DEFAULT_T_KEY;

        private const string DEFAULT_T_KEY = "n/a";

        protected bool driverCanGo;

        public override void _Ready()
        {
            base._Ready();
            CollisionInit(Line);
            Line.AreaEntered += OnDriverStepsOnLine;
            Line.AreaExited += OnDriverLeavesLine;
        }
        protected virtual void OnDriverStepsOnLine(Area2D pArea)
        {
            if (pArea is not Vehicle)
                return;

            bool lAreaGoingReverse = ((Vehicle)pArea).Direction == (int)Vehicle.GearMode.REVERSE;
            if (!driverCanGo || lAreaGoingReverse)
            {
                EmitSignal(SignalName.DriverSteppedOnLine);
                if (pArea is Player)
                    PlayerSteppedOnWrongObject(linePolygon, lAreaGoingReverse ? tKeyBackedOn : tKeySteppedOn);
            }
        }

        protected virtual void OnDriverLeavesLine(Area2D pArea)
        {
            if (pArea is not Vehicle || pArea.OverlapsArea(this))
                return;

            if (pArea is not Cop && !driverCanGo)
            {
                EmitSignal(SignalName.DriverRunnedOver);
                if (pArea is Player)
                    PlayerSteppedOnWrongObject(linePolygon, tKeyRunnedOver);
            }
        }


        protected override void Dispose(bool pDisposing)
        {
            if (IsInstanceValid(Line))
            {
                Line.AreaEntered -= OnDriverStepsOnLine;
                Line.AreaExited -= OnDriverLeavesLine;
            }
            base.Dispose(pDisposing);
        }
    }
}
