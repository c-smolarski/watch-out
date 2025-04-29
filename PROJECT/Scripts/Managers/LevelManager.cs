using Godot;
using System;

namespace Com.IsartDigital.OneButtonGame.Managers
{
    public partial class LevelManager : Node
    {
        public static LevelManager Instance { get; private set; }

        public override void _Ready()
        {
            #region Singleton
            if (Instance != null)
            {
                GD.Print("Error : " + nameof(LevelManager) + " already exists. The new one is being freed...");
                QueueFree();
                return;
            }
            Instance = this;
            #endregion
            SignalBus.Instance.GameStarted += OnGameStart;
        }

        private void OnGameStart()
        {
        }

        protected override void Dispose(bool pDisposing)
        {
            SignalBus.Instance.GameStarted -= OnGameStart;
            if (Instance == this)
                Instance = null;
            base.Dispose(pDisposing);
        }
    }
}
