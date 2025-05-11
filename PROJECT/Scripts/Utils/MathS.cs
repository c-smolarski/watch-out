using Godot;
using System;

// Author : Camille SMOLARSKI

namespace Com.IsartDigital.WatchOut.Utils
{
    public static class MathS
    {
        public static Vector2 PolarToCartesian(float pRadius, float pAngle)
        {
            return pRadius * new Vector2(MathF.Cos(pAngle), MathF.Sin(pAngle));
        }

        public static float Dot(Vector2 pA, Vector2 pB)
        {
            return pA.X * pB.X + pA.Y * pB.Y;
        }
    }
}
