using Godot;
using System;

namespace Com.IsartDigital.OneButtonGame
{
    public partial class WasteSpawner : Polygon2D
    {
        private WasteType lastSpawnedWasteType = default;
        private int[] nextProbability = new int[15] { -1, -1, 0, 1, 1, 1, 1, 1, 2, 2, 2, 2, 3, 3, 3 }; 

        private void SpawnWaste(Vector2 pGlobalPos)
        {
            if (!Geometry2D.IsPointInPolygon(ToLocal(pGlobalPos), Polygon))
                return;
        }
    }
}
