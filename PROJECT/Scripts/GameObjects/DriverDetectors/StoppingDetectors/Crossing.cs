using Com.IsartDigital.WatchOut.GameObjects.Mobiles;
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

// Author : Camille Smolarski

namespace Com.IsartDigital.WatchOut.GameObjects.DriverDetectors
{
    public partial class Crossing : StoppingDetector
    {
        [Export] private Node2D zebraLinesContainer;
        [Export] private Area2D[] otherPartsOfCrossing;
        [Export] public Path2D Path { get; private set; }

        private const float PATH_EMPTY_THRESHOLD = 0.6f;
        private const float SPAWN_TIME_THRESHOLD = 3f;
        private const float STOP_TIME = 1.5f;

        private const string T_KEY_STOPPED_ON = "CROSSING_STOPPED_ON";
        private const string T_KEY_PEDESTRIAN_WALKING = "CROSSING_PEDESTRIAN_WALKING";

        protected override float StopTime => STOP_TIME;

        private static readonly List<float> defaultSpawnTimeList = new() { 1f, 1.5f, 2f, 5f, 6f, 8f, 8f, 10f };
        private static readonly List<float> modifierSpawnTimeList = new() { 3f, 5f, 5f, 6f, 8f };
        private List<float> spawnTimeList;

        public override void _Ready()
        {
            base._Ready();
            ResetSpawnList();
            SpawnPedestrian();
        }

        protected override void CollisionsDictInit(ref Dictionary<uint, bool> pCollisionDict)
        {
            base.CollisionsDictInit(ref pCollisionDict);
            pCollisionDict.Add(Pedestrian.COLLISION_LAYER, true);
        }

        protected override void OnDriverEntered(Vehicle pVehicle)
        {
            base.OnDriverEntered(pVehicle);
            if (pVehicle is Player)
            {
                List<Area2D> lAreaList = GetOverlappingAreas().ToList();
                if (otherPartsOfCrossing != null)
                    foreach (Area2D lArea in otherPartsOfCrossing)
                        lAreaList.AddRange(lArea.GetOverlappingAreas());
                
                foreach (Area2D lArea in lAreaList)
                    if (lArea is Pedestrian)
                        PlayerSteppedOnWrongObject(zebraLinesContainer, T_KEY_PEDESTRIAN_WALKING);
            }
        }

        protected override void WaitTimerCompleted()
        {
            base.WaitTimerCompleted();
            if (detectedDriver is Player)
                PlayerSteppedOnWrongObject(zebraLinesContainer, T_KEY_STOPPED_ON);
        }

        private void SpawnPedestrian()
        {
            if (IsPathAvailable())
                Pedestrian.Create(this);

            float lTimeToNextSpawn = spawnTimeList[new Random().Next(spawnTimeList.Count)];

            if (lTimeToNextSpawn <= SPAWN_TIME_THRESHOLD)
                spawnTimeList.AddRange(modifierSpawnTimeList);
            else if (spawnTimeList != defaultSpawnTimeList)
                ResetSpawnList();

            GetTree().CreateTimer(lTimeToNextSpawn)
                .Connect(
                    Timer.SignalName.Timeout,
                    Callable.From(SpawnPedestrian));
        }

        private void ResetSpawnList()
        {
            spawnTimeList = new();
            spawnTimeList.AddRange(defaultSpawnTimeList);
        }

        private bool IsPathAvailable()
        {
            if (Path.GetChildCount() != 0)
            {
                bool lAvailable = true;
                foreach (PathFollow2D lFollow in Path.GetChildren())
                    if (lFollow.ProgressRatio < PATH_EMPTY_THRESHOLD)
                    {
                        lAvailable = false;
                        break;
                    }
                if (lAvailable)
                    return true;
            }
            else
                return true;
            return false;
        }
    }
}
