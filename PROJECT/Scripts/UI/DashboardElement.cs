using Com.IsartDigital.Utils.Tweens;
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
            Modulate = Colors.Transparent;
            SignalBus.Instance.PlayerAppearing += OnPlayerAppear;
            Player.TreeExiting += OnPlayerLeaveTree;
        }

        private void OnPlayerAppear()
        {
            Tween lTween = CreateTween();
            lTween.TweenProperty(this, TweenProp.MODULATE, Colors.White, Player.APPEAR_FADE_DURATION * 0.5f);
        }

        protected virtual void OnPlayerLeaveTree()
        {
            SignalBus.Instance.PlayerAppearing -= OnPlayerAppear;
            Player.TreeExiting -= OnPlayerLeaveTree;
            QueueFree();
        }
    }
}
