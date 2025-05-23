﻿using Com.IsartDigital.WatchOut.GameObjects.Mobiles;
using Com.IsartDigital.WatchOut.UI;
using Com.IsartDigital.WatchOut.Utils;
using Com.IsartDigital.Utils.Tweens;
using Godot;
using System;
using Com.IsartDigital.WatchOut.Enums;

// Author : Camille Smolarski

namespace Com.IsartDigital.WatchOut.Managers
{
    public partial class UIManager : Node
    {
        #region Exports
        [Export] private TouchIcon touchIcon;
        [Export] private ScoreTimer scoreTimer;
        [ExportGroup("Menus")]
        [Export] private Control menuContainer;
        [ExportGroup("Transition")]
        [Export] private ColorRect transitionRect;
        [Export] private Label accidentLabel;
        [Export] private Control transLabelContainer;
        [Export] private Label successLabel;
        [Export] private Label successMessageLabel;
        [Export] private Label scoreLabel;
        [ExportGroup("PackedScenes")]
        [Export] private PackedScene packedMainMenu;
        [Export] private PackedScene PackedManualDashboard;
        [Export] private PackedScene PackedElectricDashboard;
        #endregion

        #region Consts
        private const float SKIP_DURATION = 10f;
        private const float TRANS_IN_DURATION = 2.5f;
        private const float TRANS_OUT_DURATION = 1f;
        private const float TOUCH_ICON_DELAY = 0.5f;
        private const float LABEL_DELAY = TRANS_IN_DURATION - TRANS_IN_DURATION / 4f;

        private const string T_KEY_SUCCESS = "LABEL_SUCCESS";
        private const string T_KEY_FAILED = "LABEL_FAILED";
        private const string T_KEY_WELCOME = "LABEL_WELCOME";
        private const string T_KEY_LEVEL_MESSAGE = "MESSAGE_LEVEL";
        private const string SCORE_PREFIX = "SCORE : ";

        private const string RECT_VIEWPORT_SIZE_SHADER_PARAM = "viewportSize";
        private const string RECT_CIRCLE_RATIO_SHADER_PARAM = "circleRatio";
        #endregion

        public static UIManager Instance { get; private set; }
        private float TransitionCircleSize
        {
            get => (float)transRectShaderMat.GetShaderParameter(RECT_CIRCLE_RATIO_SHADER_PARAM);
            set => transRectShaderMat.SetShaderParameter(RECT_CIRCLE_RATIO_SHADER_PARAM, value);
        }

        private Control dashboard;
        private Control mainMenu;
        private Tween currentTrans;
        private Action transOutAction;
        private ShaderMaterial transRectShaderMat;
        private bool listeningTaps;

        public override void _Ready()
        {
            #region Singleton
            if (Instance != null)
            {
                GD.PrintErr("Error : " + nameof(UIManager) + " already exists. The new one is being freed...");
                QueueFree();
                return;
            }
            Instance = this;
            #endregion

            SignalBus.Instance.GameStarted += OnGameStart;
            InputManager.Instance.Touched += OnTouch;
            SignalBus.Instance.LevelCompleted += OnLevelComplete;
            SignalBus.Instance.LevelSoftFailed += OnLevelSoftFail;
            SignalBus.Instance.LevelHardFailed += OnLevelHardFail;

            TransitionInit();
            LoadMainMenu();
        }

        private void OnGameStart()
        {
            mainMenu?.QueueFree();
        }

        private void OnTouch()
        {
            if (listeningTaps)
            {
                if (currentTrans != null)
                {
                    currentTrans.CustomStep(SKIP_DURATION);
                    currentTrans = null;
                    return;
                }
                scoreTimer.Visible = false;
                listeningTaps = false;
                GameManager.Vibrate(VibrationDuration.SHORT);
                StartTransOut();

                if (transOutAction != null)
                {
                    transOutAction();
                    transOutAction = null;
                }
            }
        }

        /*
         * ON LEVEL EVENT METHODS
         */

        private void OnLevelComplete()
        {
            if (!LevelManager.CurrentLevel.SoftFailed)
            {
                successLabel.Text = Tr(T_KEY_SUCCESS);
                successMessageLabel.Text = Tr(T_KEY_LEVEL_MESSAGE + (LevelManager.CurrentLevelNumber + 1));
            }

            scoreLabel.Visible = scoreTimer.Visible;
            scoreLabel.Text = LevelManager.CurrentLevel.SoftFailed ? default : SCORE_PREFIX + scoreTimer.CurrentScore;
        }

        private void OnLevelSoftFail(string pTranslationKey)
        {
            successLabel.Text = Tr(T_KEY_FAILED);
            successMessageLabel.Text = Tr(pTranslationKey);
        }

        private void OnLevelHardFail(string pTranslationKey)
        {
            accidentLabel.Text = Tr(pTranslationKey);
        }

        /*
         * TRANSITION METHODS
         */

        private void TransitionInit()
        {
            Color lModulate = accidentLabel.Modulate;
            lModulate.A = 0f;
            accidentLabel.Modulate = lModulate;
            transRectShaderMat = (ShaderMaterial)transitionRect.Material;
            transitionRect.Color = RenderingServer.Singleton.GetDefaultClearColor();
            transRectShaderMat.SetShaderParameter(RECT_VIEWPORT_SIZE_SHADER_PARAM, GetViewport().GetVisibleRect().Size);
        }

        public void StartTransIn(Action pActionOnOutStart = null, bool pHardFail = false)
        {
            Tween lTween = CreateTween()
                .SetEase(Tween.EaseType.In)
                .SetTrans(Tween.TransitionType.Quad)
                .SetParallel();
            lTween.TweenProperty(this, nameof(TransitionCircleSize), 1f, TRANS_IN_DURATION)
                .From(0f);
            lTween.TweenProperty(pHardFail ? accidentLabel : transLabelContainer, TweenProp.MODULATE_ALPHA, 1f, TRANS_IN_DURATION).SetDelay(LABEL_DELAY);
            lTween.TweenProperty(touchIcon, TweenProp.MODULATE_ALPHA, 1f, TOUCH_ICON_DELAY)
                .SetDelay(LABEL_DELAY + TRANS_IN_DURATION)
                .Connect(
                    PropertyTweener.SignalName.Finished,
                    Callable.From(StartTouchIconAnim));

            lTween.Connect(
                Tween.SignalName.Finished,
                Callable.From(OnTransInFinished));

            if (pActionOnOutStart != null)
                transOutAction = pActionOnOutStart;
            listeningTaps = true;
            currentTrans = lTween;
        }

        private void OnTransInFinished()
        {
            listeningTaps = true;
            currentTrans = null;
        }

        private void StartTransOut()
        {
            transitionRect.MouseFilter = Control.MouseFilterEnum.Stop;
            Tween lTween = CreateTween()
                .SetParallel();
            lTween.TweenProperty(accidentLabel, TweenProp.MODULATE_ALPHA, 0f, TRANS_OUT_DURATION);
            lTween.TweenProperty(transLabelContainer, TweenProp.MODULATE_ALPHA, 0f, TRANS_OUT_DURATION);
            lTween.TweenProperty(this, nameof(TransitionCircleSize), 0f, TRANS_OUT_DURATION);

            lTween.TweenProperty(touchIcon, TweenProp.MODULATE_ALPHA, 0f, TOUCH_ICON_DELAY);
            lTween.Connect(
                    Tween.SignalName.Finished,
                    Callable.From(OnTransOutFinished));
        }

        private void OnTransOutFinished()
        {
            transitionRect.MouseFilter = Control.MouseFilterEnum.Ignore;
            touchIcon.Stop();
        }

        /*
         * TOUCH ICON METHODS
         */

        private void StartTouchIconAnim()
        {
            touchIcon.Play(TouchIcon.TouchAnim.TAP, TOUCH_ICON_DELAY);
        }

        /*
         * DASHBOARD METHODS
         */

        public void CreateDashboard(Player.GearBoxType pGearBoxType)
        {
            PackedScene lScene = default;
            switch (pGearBoxType)
            {
                case Player.GearBoxType.MANUAL:
                    lScene = PackedManualDashboard;
                    break;
                case Player.GearBoxType.ELECTRIC:
                    lScene = PackedElectricDashboard;
                    break;
                default:
                case Player.GearBoxType.NO_GEARBOX:
                    return;
            }
            dashboard = NodeCreator.CreateNode<Control>(lScene, GameManager.Instance.UIContainer);
        }

        /*
         * MAIN MENU METHODS
         */

        public void LoadMainMenu()
        {
            mainMenu = NodeCreator.CreateNode<Control>(packedMainMenu, menuContainer);
            SoundManager.PlayMusic(SoundManager.Instance.MusicMainMenu, SoundManager.Instance);
        }

        public void SetTransLabelsTextDefault()
        {
            successLabel.Text = Tr(T_KEY_WELCOME);
            successMessageLabel.Text = Tr(T_KEY_LEVEL_MESSAGE + 1);
            scoreLabel.Visible = default;
        }

        protected override void Dispose(bool pDisposing)
        {
            SignalBus.Instance.GameStarted -= OnGameStart;
            InputManager.Instance.Touched -= OnTouch;
            SignalBus.Instance.LevelCompleted -= OnLevelComplete;
            SignalBus.Instance.LevelSoftFailed -= OnLevelSoftFail;
            SignalBus.Instance.LevelHardFailed -= OnLevelHardFail;

            if (Instance == this)
                Instance = null;
            base.Dispose(pDisposing);
        }
    }
}
