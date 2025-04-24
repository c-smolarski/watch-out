using Com.IsartDigital.OneButtonGame.Managers;
using Com.IsartDigital.OneButtonGame.Utils;
using Godot;
using System;

namespace Com.IsartDigital.OneButtonGame
{
    public partial class Waste : Node2D
    {
        private const float BASE_SPEED = 1f;

        private float speed, speedModifier, elapsedTime;
        private Dumpster closestDump;
        private Vector2 direction, velocity;
        private WasteType wasteType = WasteType.GENERAL_WASTE;

        public override void _Ready()
        {
            base._Ready();
            SetDirection();
        }

        public override void _Process(double pDelta)
        {
            base._Process(pDelta);
            Move((float)pDelta);
        }

        private void SetDirection()
        {
            float lDistance, lMinDistance = Mathf.Inf;
            foreach (Dumpster lDump in Dumpster.DumpsterInstances)
            {
                lDistance = (lDump.GlobalPosition - GlobalPosition).Length();
                if (lMinDistance * lMinDistance > lDistance * lDistance)
                {
                    lMinDistance = lDistance;
                    closestDump = lDump;
                }
            }

            direction = (closestDump.GlobalPosition - GlobalPosition).Normalized();
        }

        private void Move(float pDelta)
        {
            Position += direction * speed * pDelta;
            elapsedTime += pDelta;
            speed = Mathf.Pow(1 + elapsedTime, 8);
            GD.Print(speed);
        }

        public static Waste Create(WasteType pType, Vector2 pPos, float pSpeedModifier = 1.0f)
        {
            Waste lWaste = NodeCreator.CreateNode<Waste>(
                GameManager.Instance.WasteScene,
                GameManager.Instance.GameContainer,
                pPos
            );
            lWaste.wasteType = pType;
            lWaste.speedModifier = pSpeedModifier;
            return lWaste;
        }
    }
}
