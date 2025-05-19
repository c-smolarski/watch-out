using Godot;
using System;

// Author : Camille Smolarski

namespace Com.IsartDigital.WatchOut.UI.TextureSwitchingButtons
{
    public partial class LanguageButton : TextureSwitchingButton
    {
        [Export] private string[] locales;

        public override void _Ready()
        {
            base._Ready();
            TranslationServer.SetLocale(locales[0]);
        }

        protected override void OnPress()
        {
            base.OnPress();
            TranslationServer.SetLocale(locales[TexIndex]);
        }
    }
}
