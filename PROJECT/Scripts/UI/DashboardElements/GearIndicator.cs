using Godot;
using System;

// Author : Camille Smolarski

namespace Com.IsartDigital.OneButtonGame.UI
{
    public abstract partial class GearIndicator : DashboardElement
    {
        [Export] protected Label GearLabel { get; private set; }

        protected abstract void OnPlayerGearChange(int pNewGear);
    }
}
