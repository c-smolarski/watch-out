using Com.IsartDigital.OneButtonGame.Managers;
using Com.IsartDigital.OneButtonGame.Utils;
using Godot;
using System;

namespace Com.IsartDigital.OneButtonGame
{
    public partial class Waste : GameObject
    {
        private const float MAX_SPEED = 1250f;

        private float speed, speedMultiplier, elapsedTime;
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

        protected override void OnHit(Area2D pArea)
        {
            if (pArea is not Dumpster)
                return;
        }

        private void SetDirection()
        {
            float lDistance, lMinDistance = Mathf.Inf;
            foreach (Dumpster lDump in Dumpster.DumpsterInstances)
            {
                lDistance = (lDump.GlobalPosition - GlobalPosition).Length();
                if (lDump != closestDump && lMinDistance * lMinDistance > lDistance * lDistance)
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
            speed = Mathf.Clamp(
                Mathf.Pow(1 + (elapsedTime * 1 / 2) * speedMultiplier, 12),
                0,
                MAX_SPEED
            );
        }

        public static Waste Create(WasteType pType, Vector2 pPos, float pSpeedMultiplier = 1.0f)
        {
            Waste lWaste = NodeCreator.CreateNode<Waste>(
                LevelManager.Instance.WasteScene,
                GameManager.Instance.GameContainer,
                pPos
            );
            lWaste.wasteType = pType;
            lWaste.speedMultiplier = pSpeedMultiplier;
            return lWaste;
        }
    }
}
