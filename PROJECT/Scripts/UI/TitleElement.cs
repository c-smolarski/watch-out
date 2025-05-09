using Godot;
using System;

// Author : Camille Smolarski

namespace Com.IsartDigital.WatchOut.UI
{
    public abstract partial class TitleElement : Control
    {

        protected const float FALL_ANIM_DURATION = 1f;
        protected const float FIRST_BOUCE_TIME = FALL_ANIM_DURATION * 0.333f;

        protected abstract void PlayAnim();
    }
}
