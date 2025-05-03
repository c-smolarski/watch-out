using Com.IsartDigital.OneButtonGame.GameObjects.Mobiles;
using Com.IsartDigital.OneButtonGame.Utils;
using Godot;
using System;

// Author : Camille Smolarski

namespace Com.IsartDigital.OneButtonGame.Managers
{
    public partial class UIManager : Node
    {
        [Export] private ColorRect transitionRect;
        [ExportGroup("PackedScenes")]
        [Export] private PackedScene PackedManualDashboard;
        [Export] private PackedScene PackedElectricDashboard;

        public static UIManager Instance { get; private set; }

        private Control dashboard;

        public override void _Ready()
        {
            #region Singleton
            if (Instance != null)
            {
                GD.PrintErr("Error : " + nameof(UIManager) + " already exists. The new one is being freed...");
                QueueFree();
                return;
            }
            Instance = this;
            #endregion
        }

        public void CreateDashboard(Player.GearBoxType pGearBoxType)
        {
            PackedScene lScene = default;
            switch (pGearBoxType)
            {
                case Player.GearBoxType.MANUAL:
                    lScene = PackedManualDashboard;
                    break;
                case Player.GearBoxType.ELECTRIC:
                    lScene = PackedElectricDashboard;
                    break;
                default:
                case Player.GearBoxType.NO_GEARBOX:
                    return;
            }
            dashboard = NodeCreator.CreateNode<Control>(lScene, GameManager.Instance.UIContainer);
        }

        protected override void Dispose(bool pDisposing)
        {
            if (Instance == this)
                Instance = null;
            base.Dispose(pDisposing);
        }
    }
}
