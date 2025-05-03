using Com.IsartDigital.OneButtonGame.Utils;
using Godot;
using System;

// Author : Camille Smolarski

namespace Com.IsartDigital.OneButtonGame.Managers
{
    public partial class LevelManager : Node
    {
        public static LevelManager Instance { get; private set; }
        public static Level CurrentLevel { get; private set; }

        private int currentLevelNumber;

        public override void _Ready()
        {
            #region Singleton
            if (Instance != null)
            {
                GD.PrintErr("Error : " + nameof(LevelManager) + " already exists. The new one is being freed...");
                QueueFree();
                return;
            }
            Instance = this;
            #endregion
            SignalBus.Instance.GameStarted += OnGameStart;
        }

        private void OnGameStart()
        {
            InputManager.Activated = true;
            LoadLevel(1);
            GetTree().CreateTimer(3f).Timeout += () => LoadLevel(1);
        }

        private async void LoadLevel(int pLevelNumber)
        {
            if (CurrentLevel != null)
            {
                CurrentLevel.QueueFree();
                await ToSignal(CurrentLevel, Level.SignalName.TreeExited);
            }

            CurrentLevel = NodeCreator.CreateNode<Level>(
                GD.Load<PackedScene>(
                    FilePath.FetchFilePathFromFolder(
                        FilePath.LEVELS_FOLDER,
                        --pLevelNumber)),
                GameManager.Instance.GameContainer);

            UIManager.Instance.CreateDashboard(CurrentLevel.Player.GearBox);

            currentLevelNumber = pLevelNumber;
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
