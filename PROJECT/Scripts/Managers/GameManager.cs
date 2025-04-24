using Com.IsartDigital.OneButtonGame.Utils;
using Godot;
using System;

namespace Com.IsartDigital.OneButtonGame.Managers
{
    public partial class GameManager : Node
    {
        [ExportGroup("Nodes")]
        [Export] public Node2D GameContainer { get; private set; }
        [ExportGroup("PackedScenes")]
        [Export] public PackedScene WasteScene { get; private set; }
        [Export] public PackedScene DumpsterScene { get; private set; }
        public static GameManager Instance { get; private set; }

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
            NodeCreator.CreateNode<Dumpster>(DumpsterScene, GameContainer, new Vector2(540, 300));
            Waste.Create(WasteType.GENERAL_WASTE, Vector2.Zero);
        }

        protected override void Dispose(bool pDisposing)
        {
            if (Instance == this)
                Instance = null;
            base.Dispose(pDisposing);
        }
    }
}
