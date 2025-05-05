using Godot;
using System;

// Author : Camille Smolarski

namespace Com.IsartDigital.OneButtonGame.Managers
{
    public partial class SignalBus : Node
    {
        [Signal] public delegate void GameStartedEventHandler();
        [Signal] public delegate void PlayerActivatedEventHandler();
        [Signal] public delegate void LevelCompletedEventHandler();
        [Signal] public delegate void LevelSoftFailedEventHandler(string pTranslationKey);
        [Signal] public delegate void LevelHardFailedEventHandler(string pTranslationKey);

        public static SignalBus Instance { get; private set; }

        public override void _Ready()
        {
            #region Singleton
            if (Instance != null)
            {
                GD.PrintErr("Error : " + nameof(SignalBus) + " already exists. The new one is being freed...");
                QueueFree();
                return;
            }
            Instance = this;
            #endregion
        }

        protected override void Dispose(bool pDisposing)
        {
            if (Instance == this)
                Instance = null;
            base.Dispose(pDisposing);
        }
    }
}
