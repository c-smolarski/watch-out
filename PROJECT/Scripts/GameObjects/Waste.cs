using Com.IsartDigital.OneButtonGame.Managers;
using Com.IsartDigital.OneButtonGame.Utils;
using Godot;
using System;

namespace Com.IsartDigital.OneButtonGame
{
    public partial class Waste : GameObject
    {
        private const float MAX_SPEED = 1250f;
        private const float MOVE_SPEED_POW = 14;

        public enum Category
        {
            GENERAL_WASTE,
            RECYCLE,
            PAPER,
            GLASS,
            COMPOST
        }

        public static int UnlockedCategories { get; private set; } = 2;

        public Category CurrentCategory { get; private set; } = default;
        public int Score { get; private set; }
        public bool HasAlreadyBounced { get; private set; }
        private Vector2 DirectionToTarget => (currentTarget.GlobalPosition - GlobalPosition).Normalized();

        private Node2D currentTarget;
        private bool isBouncing;
        private Vector2 direction, rotateStartDirection, velocity;
        private float speed, speedMultiplier, elapsedTime, rotateTime;

        public override void _Process(double pDelta)
        {
            base._Process(pDelta);
            float lDelta = (float)pDelta;
            Move(lDelta);
            if (isBouncing)
                RotateToDirection(lDelta);
        }

        protected override void OnHit(Area2D pArea)
        {
            if (pArea is not Dumpster)
                return;

            if (((Dumpster)pArea).CurrentCategory == CurrentCategory || HasAlreadyBounced)
                QueueFree();
            else
                BounceOff();
        }

        private void DirectionInit()
        {
            currentTarget = GetClosestDump();
            direction = DirectionToTarget;
        }

        private void BounceOff()
        {
            direction = new Vector2(direction.X, -direction.Y);
            isBouncing = HasAlreadyBounced = true;
            rotateStartDirection = direction;
            rotateTime = 0;
        }

        private void Move(float pDelta)
        {
            Position += direction * speed * pDelta;
            elapsedTime += pDelta;
            speed = Mathf.Clamp(
                Mathf.Pow(1 + (elapsedTime * 0.5f) * speedMultiplier, MOVE_SPEED_POW),
                0,
                MAX_SPEED
            );
        }

        private void RotateToDirection(float pDelta)
        {
            rotateTime += pDelta;
            if (!direction.IsEqualApprox(DirectionToTarget))
                direction = rotateStartDirection.Lerp(DirectionToTarget, rotateTime);
            else
                isBouncing = false;
        }

        private Dumpster GetClosestDump()
        {
            Dumpster lClosestDump = null;
            float lDistance, lMinDistance = Mathf.Inf;
            foreach (Dumpster lDump in Dumpster.Instances)
            {
                lDistance = (lDump.GlobalPosition - GlobalPosition).Length();
                if (lDump != currentTarget && lMinDistance * lMinDistance > lDistance * lDistance)
                {
                    lMinDistance = lDistance;
                    lClosestDump = lDump;
                }
            }
            return lClosestDump;
        }

        //Returns category according to UnlockedCategories.
        public static Category GetCategoryFromValue(int pCategoryValue)
        {
            pCategoryValue = UnlockedCategories == 1 ? (pCategoryValue % 2 == 0 ? 0 : 1) : pCategoryValue % (UnlockedCategories + 1);
            if (pCategoryValue < 0)
                pCategoryValue = 0;
            return (Category)pCategoryValue;
        }

        public static Color GetColorFromCategory(Category pCategory)
        {
            switch (pCategory)
            {
                case Category.GENERAL_WASTE:
                    return Colors.Black;
                case Category.RECYCLE:
                    return Colors.Yellow;
                case Category.PAPER:
                    return Colors.Blue;
                case Category.GLASS:
                    return Colors.Green;
                case Category.COMPOST:
                    return Colors.SaddleBrown;
            }
            return Colors.White;
        }

        public static Waste Create(Category pCategory, Vector2 pPos, int pScore, float pSpeedMultiplier = 1.0f)
        {
            Waste lWaste = NodeCreator.CreateNode<Waste>(
                LevelManager.Instance.WasteScene,
                GameManager.Instance.GameContainer,
                pPos
            );
            lWaste.DirectionInit();
            lWaste.Score = pScore;
            lWaste.CurrentCategory = pCategory;
            lWaste.Modulate = GetColorFromCategory(lWaste.CurrentCategory);
            lWaste.speedMultiplier = pSpeedMultiplier;
            return lWaste;
        }
    }
}
