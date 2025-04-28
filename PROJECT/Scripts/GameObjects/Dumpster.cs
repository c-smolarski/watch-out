using Com.IsartDigital.OneButtonGame.Components;
using Godot;
using System;
using System.Collections.Generic;

namespace Com.IsartDigital.OneButtonGame
{
    public partial class Dumpster : GameObject
    {
        [Export] private ClickableArea mouseDetector;

        public static List<Dumpster> DumpsterInstances { get; private set; } = new();

        public Waste.Category CurrentCategory { get; private set; }

        public override void _Ready()
        {
            base._Ready();
            SetWasteType(default);
            DumpsterInstances.Add(this);
            mouseDetector.Clicked += NextDumpsterType;
        }

        protected override void OnHit(Area2D pArea)
        {
            if (pArea is not Waste)
                return;

            if (((Waste)pArea).CurrentCategory == CurrentCategory)
                ScorePoints();
        }

        private void ScorePoints()
        {
            throw new NotImplementedException();
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
            if (DumpsterInstances.Contains(this))
                DumpsterInstances.Remove(this);
            if (IsInstanceValid(mouseDetector))
                mouseDetector.Clicked -= NextDumpsterType;
            base.Dispose(pDisposing);
        }
    }
}
