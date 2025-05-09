using Com.IsartDigital.Utils.Tweens;
using Godot;
using System;

// Author : Camille Smolarski

namespace Com.IsartDigital.WatchOut.UI.TitleElements
{
    public partial class WatchSign : TitleElement
    {
        [Export] private Control outSign;
        [Export] private float rotOnClick = -10f;
        [Export] private float rotOnBounce = -180f;

        private const float SIGN_ROTATION_ANIM_DURATION = 3f;
        private const float SIGN_FALL_ANIM_DELAY = 1f;
        private const float SIGN_FALL_ANIM_DURATION = 1f;

        protected override void PlayAnim()
        {
            Tween lTween = CreateTween();
            lTween.TweenProperty(outSign, TweenProp.ROTATION, outSign.Rotation - Mathf.DegToRad(rotOnClick), FIRST_BOUCE_TIME)
                .SetTrans(Tween.TransitionType.Quad)
                .SetEase(Tween.EaseType.InOut);
            lTween.Parallel().TweenProperty(this, TweenProp.ROTATION, Mathf.DegToRad(rotOnClick), FIRST_BOUCE_TIME)
                .SetTrans(Tween.TransitionType.Quad)
                .SetEase(Tween.EaseType.InOut);
            lTween.TweenProperty(this, TweenProp.ROTATION, Mathf.DegToRad(rotOnBounce * 0.5f), SIGN_ROTATION_ANIM_DURATION * 0.5f)
                .SetTrans(Tween.TransitionType.Expo)
                .SetEase(Tween.EaseType.In);
            lTween.TweenProperty(this, TweenProp.ROTATION, Mathf.DegToRad(rotOnBounce), SIGN_ROTATION_ANIM_DURATION)
                .SetTrans(Tween.TransitionType.Elastic)
                .SetEase(Tween.EaseType.Out);
            lTween.Parallel().TweenProperty(this, TweenProp.GLOBAL_POSITION_Y, GetViewportRect().Size.Y, SIGN_FALL_ANIM_DURATION)
                .SetTrans(Tween.TransitionType.Cubic)
                .SetEase(Tween.EaseType.In)
                .SetDelay(SIGN_FALL_ANIM_DELAY);
        }
    }
}
