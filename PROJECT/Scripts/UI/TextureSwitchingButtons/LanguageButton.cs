using Godot;
using System;

// Author : Camille Smolarski

namespace Com.IsartDigital.WatchOut.UI.TextureSwitchingButtons
{
    public partial class LanguageButton : TextureSwitchingButton
    {
        [Export] private string[] locales;

        protected override void OnPress()
        {
            base.OnPress();
            TranslationServer.SetLocale(locales[TexIndex]);
        }
    }
}
