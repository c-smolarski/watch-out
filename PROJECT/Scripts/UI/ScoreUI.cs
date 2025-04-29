using Godot;
using Range = Godot.Range;
using System;
using System.Collections.Generic;

namespace Com.IsartDigital.OneButtonGame.UI
{
    public partial class ScoreUI : Control
    {
        [Export] private Label labelMultiplier;
        [Export] private Range scoreBar;

        private const int MAX_VALUE = 5000;
        private char MULT_CHAR = 'X';

        public static List<ScoreUI> Instances { get; private set; } = new();

        private int currentScore;

        public override void _Ready()
        {
            base._Ready();
            Instances.Add(this);
            scoreBar.MaxValue = MAX_VALUE;
        }

        public void AddScore(int pScore, float pMult)
        {
            DisplayScore(currentScore += (int)(pScore * pMult));
            DisplayMult(pMult);
        }

        private void DisplayScore(int pScore)
        {
            int lTotalScore = 0;
            foreach(ScoreUI lUI in Instances)
                lTotalScore += lUI.currentScore;

            if (lTotalScore > MAX_VALUE)
                foreach (ScoreUI lUI in Instances)
                    lUI.scoreBar.Value = lUI.currentScore * MAX_VALUE / lTotalScore;
            else
                scoreBar.Value = currentScore;
        }

        public void OnScoreMultReset()
        {
            DisplayMult(Dumpster.BASE_MULTIPLIER);
        }

        private void DisplayMult(float pMult)
        {
            labelMultiplier.Text = MULT_CHAR + pMult.ToString();
        }

        protected override void Dispose(bool disposing)
        {
            if (Instances.Contains(this))
                Instances.Remove(this);
            base.Dispose(disposing);
        }
    }
}