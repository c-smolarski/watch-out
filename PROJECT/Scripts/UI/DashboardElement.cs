using Com.IsartDigital.OneButtonGame.GameObjects.Mobiles;
using Com.IsartDigital.OneButtonGame.Managers;
using Com.IsartDigital.OneButtonGame.Utils;
using Godot;
using System;

// Author : Camille Smolarski

namespace Com.IsartDigital.OneButtonGame.UI
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
