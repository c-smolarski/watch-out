using Com.IsartDigital.Utils;
using Com.IsartDigital.Utils.Effects;
using Com.IsartDigital.WatchOut.Enums;
using Godot;
using System;

// Author : Camille Smolarski

namespace Com.IsartDigital.WatchOut.Managers
{
    public partial class GameManager : Node
    {
        [ExportGroup("Nodes")]
        [Export] public Node2D GameContainer { get; private set; }
        [Export] public Control UIContainer { get; private set; }
        [ExportSubgroup("Shaker")]
        [Export] private Shaker lightShaker;
        [Export] private Shaker midShaker;
        [Export] private Shaker strongShaker;
        [ExportGroup("PackedScenes")]
        [Export] public PackedScene PackedPedestrian { get; private set; }

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
            OS.RequestPermissions();
        }

        public static void Shake(ScreenShakeForce pShakeForce)
        {
            switch (pShakeForce)
            {
                case ScreenShakeForce.LIGHT:
                    Vibrate(VibrationDuration.SHORT);
                    Instance.lightShaker.Start();
                    break;
                case ScreenShakeForce.MEDIUM:
                    Vibrate(VibrationDuration.MID);
                    Instance.midShaker.Start();
                    break;
                case ScreenShakeForce.STRONG:
                    Vibrate(VibrationDuration.LONG);
                    Instance.strongShaker.Start();
                    break;
            }
        }

        public static void Vibrate(VibrationDuration pVibration)
        {
            Input.VibrateHandheld((int)pVibration);
        }

        protected override void Dispose(bool pDisposing)
        {
            if (Instance == this)
                Instance = null;
            base.Dispose(pDisposing);
        }
    }
}
