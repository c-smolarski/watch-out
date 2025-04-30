using Com.IsartDigital.OneButtonGame.Managers;
using Godot;
using System;

namespace Com.IsartDigital.OneButtonGame.GameObjects.Mobiles
{
    public partial class Player : Mobile
    {
        public override void _Ready()
        {
            base._Ready();
            InputManager.Instance.Tap += (int i) => GD.Print("tap " + i);
            InputManager.Instance.StartedHolding += (int i) => GD.Print("started holding " + i);
            InputManager.Instance.StoppedHolding += () => GD.Print("stopped holding");
        }
    }
}
