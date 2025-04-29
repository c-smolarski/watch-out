using Godot;
using System;

namespace Com.IsartDigital.OneButtonGame.Managers
{
    public partial class SignalBus : Node
    {
        [Signal] public delegate void GameStartedEventHandler();

        [Signal] public delegate void DumpsterAppearedEventHandler(Dumpster pDump);
        [Signal] public delegate void DumpDisposedEventHandler(Dumpster pDump);
        public static SignalBus Instance { get; private set; }

        public override void _Ready()
        {
            #region Singleton
            if (Instance != null)
            {
                GD.Print("Error : " + nameof(SignalBus) + " already exists. The new one is being freed...");
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
