using Godot;
using System;

// Author : Camille Smolarski

namespace Com.IsartDigital.OneButtonGame.UI
{
    public partial class TouchIcon : Control
    {
        [Export] private Polygon2D handPolygon;
        [Export] private AnimationPlayer player;

        public override void _Ready()
        {
            base._Ready();
            handPolygon.Color = RenderingServer.Singleton.GetDefaultClearColor();
        }
    }
}
