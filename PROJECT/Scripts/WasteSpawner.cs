using Godot;
using System;
using System.Collections.Generic;

namespace Com.IsartDigital.OneButtonGame
{
    public partial class WasteSpawner : Polygon2D
    {
        private const float SPAWN_DISTANCE_MIN = 200f;

        private static readonly List<int> baseCategoryProbability = new() { 0, 1, 1, 1, 1, 1, 1, 2, 2, 2, 2, 2, 3, 3, 3 };
        private static readonly List<int> probaModifierRange = new() { 1, 1, 2 };

        private Rect2 polygonRect;
        private Vector2 lastSpawnPos = new();
        private RandomNumberGenerator rand = new();
        private Waste.Category lastCategory = default;
        private List<int> categoryProbability;

        private float elapsedTime;

        public override void _Ready()
        {
            base._Ready();
            categoryProbability = baseCategoryProbability;
            rand.Randomize();
            RectInit();
        }

        public override void _Process(double delta)
        {
            base._Process(delta);
            elapsedTime += (float)delta;
            if (elapsedTime > 1)
            {
                elapsedTime = 0;
                SpawnRandomWaste();
            }
        }

        private void RectInit()
        {
            polygonRect = new() { Position = Polygon[0], End = Polygon[0] };
            for (int i = 1; i < Polygon.Length - 1; i++)
            {
                polygonRect.Position = new(
                    Mathf.Min(polygonRect.Position.X, Polygon[i].X),
                    Mathf.Min(polygonRect.Position.Y, Polygon[i].Y)
                    );
                polygonRect.End = new(
                    Mathf.Max(polygonRect.End.X, Polygon[i].X),
                    Mathf.Max(polygonRect.End.Y, Polygon[i].Y)
                    );
            }
        }

        private void SpawnNextWaste()
        {
            SpawnWaste(Waste.GetCategoryFromValue((int)lastCategory + 1));
        }

        private void SpawnRandomWaste()
        {
            Waste.Category lCategory = Waste.GetCategoryFromValue(
                (int)lastCategory + categoryProbability[rand.RandiRange(0, categoryProbability.Count - 1)]);

            if (lCategory == lastCategory)
                categoryProbability.AddRange(probaModifierRange);
            else if (categoryProbability != baseCategoryProbability)
                categoryProbability = baseCategoryProbability;

            SpawnWaste(lCategory);
        }

        private void SpawnWaste(Waste.Category pCategoryValue)
        {
            Waste.Create(
                lastCategory = pCategoryValue,
                GetNewWasteSpawnPos()
                );
        }

        private Vector2 GetRandomPointInPolygon()
        {
            Vector2 lRandomPoint = new(
                (int)rand.RandfRange(polygonRect.Position.X, polygonRect.End.X),
                (int)rand.RandfRange(polygonRect.Position.Y, polygonRect.End.Y)
                );

            if (Geometry2D.IsPointInPolygon(lRandomPoint, Polygon))
                return lRandomPoint;
            else
                return GetRandomPointInPolygon();
        }

        private Vector2 GetNewWasteSpawnPos()
        {
            Vector2 lNewPos = GetRandomPointInPolygon();
            if ((lastSpawnPos - lNewPos).LengthSquared() > SPAWN_DISTANCE_MIN * SPAWN_DISTANCE_MIN)
                return lastSpawnPos = lNewPos;
            else
                return GetNewWasteSpawnPos();
        }
    }
}
