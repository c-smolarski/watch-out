using Com.IsartDigital.WatchOut.Utils;
using Godot;
using System;

// Author : Camille Smolarski

namespace Com.IsartDigital.WatchOut.Managers
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
            CurrentLevelNumber = default;
            LoadNextLevel();
            SoundManager.PlayMusic(SoundManager.Instance.StreetLoop, SoundManager.Instance);
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
            if (!LoadLevel(CurrentLevelNumber + 1))
                UIManager.Instance.LoadMainMenu();
        }

        private void ReloadLevel()
        {
            LoadLevel(CurrentLevelNumber);
        }

        private bool LoadLevel(int pLevelNumber)
        {
            if (IsInstanceValid(CurrentLevel))
                CurrentLevel.Free();

            string lFilePath = FilePath.FetchFilePathFromFolder(FilePath.LEVELS_FOLDER, pLevelNumber - 1);

            if (lFilePath == null)
                return false;

            CurrentLevel = NodeCreator.CreateNode<Level>(
                GD.Load<PackedScene>(lFilePath),
                GameManager.Instance.GameContainer);

            CurrentLevelNumber = pLevelNumber;
            UIManager.Instance.CreateDashboard(CurrentLevel.Player.GearBox);

            isLoadingLevel = false;
            EmitSignal(SignalName.LevelLoaded);
            return true;
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
