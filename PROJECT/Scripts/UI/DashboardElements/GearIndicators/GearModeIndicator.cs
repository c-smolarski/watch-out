using Com.IsartDigital.OneButtonGame.GameObjects;
using System;

// Author : Camille Smolarski

namespace Com.IsartDigital.OneButtonGame.UI.DashboardElements.GearIndicators
{
    public partial class GearModeIndicator : GearIndicator
    {
        public override void _Ready()
        {
            base._Ready();
            Player.ChangedGearMode += OnPlayerGearChange;
        }

        protected override void OnPlayerGearChange(int pNewGear)
        {
            Mobile.GearMode lGearMode = (Mobile.GearMode)pNewGear;
            GearLabel.Text = lGearMode.ToString()[..1];
        }

        protected override void OnPlayerLeaveTree()
        {
            Player.ChangedGearMode -= OnPlayerGearChange;
            base.OnPlayerLeaveTree();
        }
    }
}
