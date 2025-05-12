using Com.IsartDigital.Utils.Tweens;
using Com.IsartDigital.WatchOut.Managers;
using Godot;
using System;

// Author : Camille Smolarski

namespace Com.IsartDigital.WatchOut.Scripts.UI
{
    public partial class StartButton : Button
    {
        [Export] private float startDelay = 3f;
        [Export] private Color colorAfterClick = Color.Color8(45, 45, 45);

        private const string STYLEBOX_PATH = "normal";
        private const string BG_COLOR_PATH = "bg_color";
        private const string CORNER_RADIUS = "corner_radius_top_left";

        private const float SCALE_ANIM_DURATION = 0.15f;
        private const float GROW_ANIM_DURATION = 0.5f;

        private Vector2 PressedScale => baseScale * 0.9f;

        private Color BgColor
        {
            get => (Color)styleBox.Get(BG_COLOR_PATH);
            set => styleBox.Set(BG_COLOR_PATH, value);
        }

        private StyleBox styleBox;
        private Vector2 baseScale;

        public override void _Ready()
        {
            base._Ready();
            styleBox = GetThemeStylebox(STYLEBOX_PATH);
            BgColor = RenderingServer.GetDefaultClearColor();
            baseScale = Scale;
            Pressed += OnPressed;

        }

        private void OnPressed()
        {
            Disabled = true;

            float lHMargin = (float)styleBox.Get(CORNER_RADIUS);

            Tween lTween = CreateTween()
                .SetEase(Tween.EaseType.In);
            lTween.TweenProperty(this, TweenProp.SCALE, PressedScale, SCALE_ANIM_DURATION);
            lTween.TweenProperty(this, TweenProp.SCALE, baseScale, SCALE_ANIM_DURATION)
                .SetTrans(Tween.TransitionType.Bounce);
            lTween.Parallel().TweenProperty(this, TweenProp.SIZE_X, GetViewportRect().Size.X + lHMargin * 2f, GROW_ANIM_DURATION)
                .SetTrans(Tween.TransitionType.Circ);
            lTween.Parallel().TweenProperty(this, TweenProp.POSITION_X, GetViewportRect().Position.X - lHMargin, GROW_ANIM_DURATION)
                .SetTrans(Tween.TransitionType.Circ);
            lTween.Parallel().TweenProperty(this, nameof(BgColor), colorAfterClick, GROW_ANIM_DURATION);

            GetTree().CreateTimer(startDelay, false)
                .Connect(Timer.SignalName.Timeout, Callable.From(OnStart));

            UIManager.Instance.SetTransLabelsTextDefault();
        }

        private void OnStart()
        {
            InputManager.Activated = true;
            UIManager.Instance.StartTransIn(
                            () => SignalBus.Instance.EmitSignal(SignalBus.SignalName.GameStarted));
        }

        protected override void Dispose(bool pDisposing)
        {
            Pressed -= OnPressed;
            base.Dispose(pDisposing);
        }
    }
}
