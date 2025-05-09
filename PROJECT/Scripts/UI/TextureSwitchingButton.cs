using Godot;
using System;

// Author : Camille Smolarski

namespace Com.IsartDigital.WatchOut.UI
{
    public abstract partial class TextureSwitchingButton : Button
    {
        [Export] private Texture2D[] textures;

        protected int TexIndex { get; private set; }

        public override void _Ready()
        {
            base._Ready();
            Pressed += OnPress;
        }

        protected virtual void OnPress()
        {
            TexIndex = (++TexIndex) % textures.Length;
            Icon = textures[TexIndex];
        }

        protected override void Dispose(bool pDisposing)
        {
            Pressed -= OnPress;
            base.Dispose(pDisposing);
        }
    }
}
