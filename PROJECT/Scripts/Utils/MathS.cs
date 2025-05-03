using Godot;
using System;

// Author : Camille SMOLARSKI

namespace Com.IsartDigital.OneButtonGame.Utils
{
    public static class MathS
    {
        public static float Dot(Vector2 pA, Vector2 pB)
        {
            return pA.X * pB.X + pA.Y * pB.Y;
        }
    }
}
