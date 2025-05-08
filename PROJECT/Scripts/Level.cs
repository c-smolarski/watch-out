using Com.IsartDigital.WatchOut.GameObjects.Mobiles;
using Com.IsartDigital.WatchOut.Managers;
using Godot;
using System;

// Author : Camille Smolarski

namespace Com.IsartDigital.WatchOut
{
    public partial class Level : Node2D
    {
        [Export] public float AllocatedTime { get; private set; }
        [Export] public Player Player { get; private set; }
        [Export] private bool playerAutoAppear = true;
        [Export] private Area2D winArea;

        public bool SoftFailed { get; private set; }
        private bool hardFailed;

        public override void _Ready()
        {
            base._Ready();
            WinAreaInit();
            SignalBus.Instance.LevelSoftFailed += OnSoftFail;
            SignalBus.Instance.LevelHardFailed += OnHardFail;
            if (playerAutoAppear)
                Player.Appear();
        }

        private void WinAreaInit()
        {
            winArea.CollisionLayer = winArea.CollisionMask = Player.COLLISION_LAYER;
            winArea.AreaEntered += OnPlayerCompletesLevel;
        }

        private void OnHardFail(string pTranslationKey)
        {
            if (!hardFailed)
            {
                hardFailed = true;
                winArea.AreaEntered -= OnPlayerCompletesLevel;
            }
        }

        private void OnSoftFail(string pFailMessage)
        {
            SoftFailed = true;
        }

        private void OnPlayerCompletesLevel(Area2D pArea)
        {
            if (pArea is not GameObjects.Mobiles.Player)
                return;

            SignalBus.Instance.EmitSignal(SignalBus.SignalName.LevelCompleted);
        }

        protected override void Dispose(bool pDisposing)
        {
            SignalBus.Instance.LevelSoftFailed -= OnSoftFail;
            SignalBus.Instance.LevelHardFailed -= OnHardFail;
            if (IsInstanceValid(winArea) && !hardFailed)
                winArea.AreaEntered -= OnPlayerCompletesLevel;
            base.Dispose(pDisposing);
        }
    }
}
