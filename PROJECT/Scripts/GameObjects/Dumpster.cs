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

        private WasteType currentWasteType;

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
        }

        private void NextDumpsterType()
        {
            SetWasteType((int)currentWasteType + 1);
        }

        private void SetWasteType(int pTypeValue)
        {
            if (Enum.IsDefined(typeof(WasteType), pTypeValue))
                currentWasteType = (WasteType)pTypeValue;
            else
                currentWasteType = default;
            Modulate = GetDumpsterColor(currentWasteType);
        }

        private Color GetDumpsterColor(WasteType pType)
        {
            switch (pType)
            {
                case WasteType.GENERAL_WASTE:
                    return Colors.Black;
                case WasteType.RECYCLE:
                    return Colors.Yellow;
                case WasteType.PAPER:
                    return Colors.Blue;
                case WasteType.GLASS:
                    return Colors.Green;
                case WasteType.COMPOST:
                    return Colors.SaddleBrown;
            }
            return Colors.White;
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
