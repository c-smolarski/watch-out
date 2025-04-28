using Godot;
using System;

namespace Com.IsartDigital.OneButtonGame.Managers
{
    public partial class GameManager : Node
    {
        [ExportGroup("Nodes")]
        [Export] public Node2D GameContainer { get; private set; }

        private const string ANDROID = "Android";
        private const string IOS = "iOS";

        public static GameManager Instance { get; private set; }

        public static bool PlayingOnMobile { get; private set; }

        public override void _Ready()
        {
            #region Singleton
            if (Instance != null)
            {
                GD.Print("Error : " + nameof(GameManager) + " already exists. The new one is being freed...");
                QueueFree();
                return;
            }
            Instance = this;
            #endregion

            PlayingOnMobile = OS.GetName() == ANDROID || OS.GetName() == IOS;

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
