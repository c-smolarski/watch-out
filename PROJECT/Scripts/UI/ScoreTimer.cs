using Com.IsartDigital.WatchOut.Managers;
using Com.IsartDigital.Utils.Tweens;
using Godot;
using System;

// Author : Camille Smolarski

namespace Com.IsartDigital.WatchOut.UI
{
    public partial class ScoreTimer : Control
    {
        [Export] private TextureRect timerHand;
        [Export] private Label scoreLabel;
        [Export] private Panel scorePanel;

        private const float APPEAR_DURATION = 1.5f;
        private const int START_SCORE = 5000;
        private const float SCORE_TIME_MULT = 0.75f;

        private const string PANEL_STYLEBOX = "panel";
        private const string STYLEBOX_COLOR = "bg_color";
        private const string SCORE_PREFIX = "0000";

        public int CurrentScore { get; private set; }

        private bool active;
        private float totalTime, timeLeft;

        public override void _Ready()
        {
            base._Ready();
            scorePanel.GetThemeStylebox(PANEL_STYLEBOX).Set(STYLEBOX_COLOR, RenderingServer.GetDefaultClearColor());

            LevelManager.Instance.LevelLoaded += ResetTimer;
            SignalBus.Instance.GameStarted += OnGameStarted;
            SignalBus.Instance.PlayerActivated += StartTimer;
            SignalBus.Instance.LevelCompleted += PauseTimer;
            SignalBus.Instance.Connect(
                SignalBus.SignalName.LevelHardFailed,
                Callable.From((string pMessage) => PauseTimer()));

            scoreLabel.Text = START_SCORE.ToString();
            Modulate = Colors.Transparent;
        }

        private void OnGameStarted()
        {
            Tween lTween = CreateTween();
            lTween.TweenProperty(this, TweenProp.MODULATE, Colors.White, APPEAR_DURATION);
        }

        public override void _Process(double pDelta)
        {
            base._Process(pDelta);
            if (active)
            {
                timeLeft -= (float)pDelta;
                if (timeLeft < 0)
                    ResetTimer();

                timerHand.Rotation = Mathf.Tau - (timeLeft * Mathf.Tau / totalTime);
                if (timeLeft < totalTime * SCORE_TIME_MULT)
                {
                    CurrentScore = (int)(timeLeft * START_SCORE / (totalTime * SCORE_TIME_MULT));
                    scoreLabel.Text = (SCORE_PREFIX + CurrentScore)[^4..];
                }
            }
        }

        private void StartTimer()
        {
            ResetTimer();
            totalTime = timeLeft = LevelManager.CurrentLevel.AllocatedTime;
            timerHand.Rotation = default;
            active = true;
        }

        private void PauseTimer()
        {
            active = false;
        }

        private void ResetTimer()
        {
            active = false;
            timerHand.Rotation = timeLeft = default;
            scoreLabel.Text = (CurrentScore = START_SCORE).ToString();
        }

        protected override void Dispose(bool pDisposing)
        {
            LevelManager.Instance.LevelLoaded -= ResetTimer;
            SignalBus.Instance.GameStarted -= OnGameStarted;
            SignalBus.Instance.PlayerActivated -= StartTimer;
            SignalBus.Instance.LevelCompleted -= ResetTimer;
            base.Dispose(pDisposing);
        }
    }
}
