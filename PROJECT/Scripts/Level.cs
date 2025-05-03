using Com.IsartDigital.OneButtonGame.GameObjects.Mobiles;
using Godot;
using System;

// Author : Camille Smolarski

namespace Com.IsartDigital.OneButtonGame
{
    public partial class Level : Node2D
    {
        [Export] public Player Player { get; private set; }
        [Export] private bool playerAutoAppear = true;
        [Export] private Area2D winArea;

        private bool success = true;

        public override void _Ready()
        {
            base._Ready();
            WinAreaInit();
            if (playerAutoAppear)
                Player.Appear();
        }

        private void WinAreaInit()
        {
            winArea.CollisionLayer = winArea.CollisionMask = Player.COLLISION_LAYER;
            winArea.AreaEntered += OnPlayerCompletesLevel;
        }

        private void OnPlayerCompletesLevel(Area2D pArea)
        {
            if (pArea is not GameObjects.Mobiles.Player)
                return;
        }
    }
}
