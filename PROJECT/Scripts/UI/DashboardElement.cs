using Com.IsartDigital.WatchOut.GameObjects.Mobiles;
using Com.IsartDigital.WatchOut.Managers;
using Godot;
using System;

// Author : Camille Smolarski

namespace Com.IsartDigital.WatchOut.UI
{
    public abstract partial class DashboardElement : Control
    {
        protected Player Player { get; private set; }

        public override void _Ready()
        {
            base._Ready();
            Player = LevelManager.CurrentLevel.Player;
            SelfModulate = RenderingServer.Singleton.GetDefaultClearColor();
            Player.TreeExiting += OnPlayerLeaveTree;
        }

        protected virtual void OnPlayerLeaveTree()
        {
            Player.TreeExiting -= OnPlayerLeaveTree;
            QueueFree();
        }
    }
}
