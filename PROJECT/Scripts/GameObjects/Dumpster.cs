using Com.IsartDigital.OneButtonGame.Components;
using Com.IsartDigital.OneButtonGame.Managers;
using Godot;
using System;
using System.Collections.Generic;

namespace Com.IsartDigital.OneButtonGame
{
    public partial class Dumpster : GameObject
    {
        [Signal] public delegate void ScoredEventHandler(int pScore, float pMult);
        [Signal] public delegate void ScoreMultResetEventHandler();

        [Export] private ClickableArea mouseDetector;

        public const float BASE_MULTIPLIER = 1f;

        public static List<Dumpster> Instances { get; private set; } = new();

        public Waste.Category CurrentCategory { get; private set; }

        private float scoreMultiplier = BASE_MULTIPLIER;
        private int wasteStreak;

        public override void _Ready()
        {
            base._Ready();
            SetWasteType(default);
            Instances.Add(this);
            mouseDetector.Clicked += NextDumpsterType;
            SignalBus.Instance.EmitSignal(SignalBus.SignalName.DumpsterAppeared, this);
        }

        protected override void OnHit(Area2D pArea)
        {
            if (pArea is not Waste)
                return;

            Waste lWaste = (Waste)pArea;

            if (lWaste.CurrentCategory == CurrentCategory)
                ScorePoints(lWaste.Score);
            else if (!lWaste.HasAlreadyBounced)
                ResetStreak();
        }

        private void ScorePoints(int pScore)
        {
            wasteStreak += 1;
            scoreMultiplier = BASE_MULTIPLIER + (wasteStreak / 5) * 0.5f;
            EmitSignal(SignalName.Scored, pScore, scoreMultiplier);
        }

        private void ResetStreak()
        {
            wasteStreak = 0;
            scoreMultiplier = BASE_MULTIPLIER;
            EmitSignal(SignalName.ScoreMultReset);
        }

        private void NextDumpsterType()
        {
            SetWasteType((int)CurrentCategory + 1);
        }

        private void SetWasteType(int pCategoryValue)
        {
            CurrentCategory = Waste.GetCategoryFromValue(pCategoryValue);
            Modulate = Waste.GetColorFromCategory(CurrentCategory);
        }

        protected override void Dispose(bool pDisposing)
        {
            SignalBus.Instance.EmitSignal(SignalBus.SignalName.DumpDisposed, this);

            if (Instances.Contains(this))
                Instances.Remove(this);
            if (IsInstanceValid(mouseDetector))
                mouseDetector.Clicked -= NextDumpsterType;
            base.Dispose(pDisposing);
        }
    }
}
