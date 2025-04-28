using Com.IsartDigital.OneButtonGame.Utils;
using Godot;
using System;

namespace Com.IsartDigital.OneButtonGame.Managers
{
    public partial class LevelManager : Node
    {
        [ExportGroup("PackedScenes")]
        [Export] public PackedScene WasteScene { get; private set; }
        [Export] public PackedScene DumpsterScene { get; private set; }

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
            Vector2 lViewportSize = GetViewport().GetVisibleRect().Size;
            NodeCreator.CreateNode<Dumpster>(
                DumpsterScene,
                GameManager.Instance.GameContainer,
                new Vector2(lViewportSize.X / 2, lViewportSize.Y / 8)
                );
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
