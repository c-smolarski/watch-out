using Godot;
using System;

// Author : Camille Smolarski

namespace Com.IsartDigital.OneButtonGame.Managers
{
    public partial class GameManager : Node
    {
        [ExportGroup("Nodes")]
        [Export] public Node2D GameContainer { get; private set; }

        public static GameManager Instance { get; private set; }

        public override void _Ready()
        {
            #region Singleton
            if (Instance != null)
            {
                GD.PrintErr("Error : " + nameof(GameManager) + " already exists. The new one is being freed...");
                QueueFree();
                return;
            }
            Instance = this;
            #endregion
            SignalBus.Instance.EmitSignal(SignalBus.SignalName.GameStarted);
        }

        protected override void Dispose(bool pDisposing)
        {
            if (Instance == this)
                Instance = null;
            base.Dispose(pDisposing);
        }
    }
}
