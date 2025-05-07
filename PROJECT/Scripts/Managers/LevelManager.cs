using Com.IsartDigital.OneButtonGame.Utils;
using Godot;
using System;

// Author : Camille Smolarski

namespace Com.IsartDigital.OneButtonGame.Managers
{
    public partial class LevelManager : Node
    {
        [Signal] public delegate void LevelLoadedEventHandler();

        public static LevelManager Instance { get; private set; }
        public static Level CurrentLevel { get; private set; }

        public static int CurrentLevelNumber { get; private set; }

        private bool isLoadingLevel;
        private Callable transitionCallable;
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
            SignalBus.Instance.LevelHardFailed += OnLevelFail;
            SignalBus.Instance.LevelCompleted += OnLevelComplete;
        }

        private void OnGameStart()
        {
            InputManager.Activated = true;
            CurrentLevelNumber = default;
            LoadNextLevel();
        }

        private void OnLevelComplete()
        {
            if (isLoadingLevel)
                return;
            isLoadingLevel = true;

            UIManager.Instance.StartTransIn(CurrentLevel.SoftFailed ? ReloadLevel : LoadNextLevel);
        }

        private void OnLevelFail(string pFailMessage)
        {
            if (isLoadingLevel)
                return;
            isLoadingLevel = true;

            UIManager.Instance.StartTransIn(ReloadLevel, true);
        }

        private void LoadNextLevel()
        {
            LoadLevel(CurrentLevelNumber + 1);
        }

        private void ReloadLevel()
        {
            LoadLevel(CurrentLevelNumber);
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
                        pLevelNumber - 1)),
                GameManager.Instance.GameContainer);

            UIManager.Instance.CreateDashboard(CurrentLevel.Player.GearBox);

            CurrentLevelNumber = pLevelNumber;
            isLoadingLevel = false;

            EmitSignal(SignalName.LevelLoaded);
        }

        protected override void Dispose(bool pDisposing)
        {
            SignalBus.Instance.GameStarted -= OnGameStart;
            SignalBus.Instance.LevelCompleted -= OnLevelComplete;
            SignalBus.Instance.LevelHardFailed -= OnLevelFail;
            if (Instance == this)
                Instance = null;
            base.Dispose(pDisposing);
        }
    }
}
