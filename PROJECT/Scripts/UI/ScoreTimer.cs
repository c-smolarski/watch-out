using Com.IsartDigital.OneButtonGame.Managers;
using Godot;
using System;

// Author : Camille Smolarski

namespace Com.IsartDigital.OneButtonGame.UI
{
    public partial class ScoreTimer : Control
    {
        [Export] private TextureRect timerHand;
        [Export] private Label scoreLabel;
        [Export] private Panel scorePanel;

        private const int START_SCORE = 5000;
        private const float SCORE_TIME_MULT = 0.75f;
        private const string PANEL_STYLEBOX = "panel";
        private const string STYLEBOX_COLOR = "bg_color";
        private const string SCORE_PREFIX = "0000";

        private bool active;
        private int currentScore;
        private float totalTime, timeLeft;

        public override void _Ready()
        {
            base._Ready();
            scorePanel.GetThemeStylebox(PANEL_STYLEBOX).Set(STYLEBOX_COLOR, RenderingServer.GetDefaultClearColor());

            SignalBus.Instance.PlayerActivated += StartTimer;
            SignalBus.Instance.LevelCompleted += PauseTimer;
            SignalBus.Instance.Connect(
                SignalBus.SignalName.LevelHardFailed,
                Callable.From((string pMessage) => PauseTimer()));

            scoreLabel.Text = START_SCORE.ToString();
        }

        public override void _Process(double pDelta)
        {
            base._Process(pDelta);
            if (active)
            {
                timeLeft -= (float)pDelta;
                if (timeLeft < 0)
                    StopTimer();

                timerHand.Rotation = Mathf.Tau - (timeLeft * Mathf.Tau / totalTime);
                if (timeLeft < totalTime * SCORE_TIME_MULT)
                {
                    currentScore = (int)(timeLeft * START_SCORE / (totalTime * SCORE_TIME_MULT));
                    scoreLabel.Text = (SCORE_PREFIX + currentScore)[^4..];
                }
            }
        }

        private void StartTimer()
        {
            StopTimer();
            active = true;
            totalTime = timeLeft = LevelManager.CurrentLevel.AllocatedTime;
            timerHand.Rotation = default;
            currentScore = START_SCORE;
        }

        private void PauseTimer()
        {
            active = false;
        }

        private void StopTimer()
        {
            active = false;
            timerHand.Rotation = timeLeft = default;
        }

        protected override void Dispose(bool pDisposing)
        {
            SignalBus.Instance.PlayerActivated -= StartTimer;
            SignalBus.Instance.LevelCompleted -= StopTimer;
            base.Dispose(pDisposing);
        }
    }
}
