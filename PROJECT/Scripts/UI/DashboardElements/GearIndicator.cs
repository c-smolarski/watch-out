using Com.IsartDigital.Utils.Tweens;
using Com.IsartDigital.WatchOut.Managers;
using Godot;
using System;
using System.Reflection;

// Author : Camille Smolarski

namespace Com.IsartDigital.WatchOut.UI
{
    public abstract partial class GearIndicator : DashboardElement
    {
        [Export] protected Label GearLabel { get; private set; }

        protected abstract void OnPlayerGearChange(int pNewGear);
    }
}
