using Com.IsartDigital.OneButtonGame.GameObjects.Mobiles;
using Com.IsartDigital.OneButtonGame.Utils;
using Com.IsartDigital.Utils.Tweens;
using Godot;
using System;

// Author : Camille Smolarski

namespace Com.IsartDigital.OneButtonGame.Managers
{
    public partial class UIManager : Node
    {
        [Export] private ColorRect transitionRect;
        [ExportGroup("Transition Labels")]
        [Export] private Label accidentLabel;
        [Export] private Label successLabel;
        [Export] private Label successMessageLabel;
        [ExportGroup("PackedScenes")]
        [Export] private PackedScene PackedManualDashboard;
        [Export] private PackedScene PackedElectricDashboard;


        private const float TRANSITION_DURATION = 3f;
        private const float LABEL_DELAY = TRANSITION_DURATION - TRANSITION_DURATION / 6f;

        private const string T_KEY_SUCCESS = "LABEL_SUCCESS";
        private const string T_KEY_FAILED = "LABEL_FAILED";
        private const string T_KEY_LEVEL_MESSAGE = "MESSAGE_LEVEL";

        private const string RECT_VIEWPORT_SIZE_SHADER_PARAM = "viewportSize";
        private const string RECT_CIRCLE_RATIO_SHADER_PARAM = "circleRatio";
        
        public static UIManager Instance { get; private set; }
        private float TransitionCircleSize
        {
            get => (float)transRectShaderMat.GetShaderParameter(RECT_CIRCLE_RATIO_SHADER_PARAM);
            set => transRectShaderMat.SetShaderParameter(RECT_CIRCLE_RATIO_SHADER_PARAM, value);
        }

        private Control dashboard;
        private ShaderMaterial transRectShaderMat;

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

            SignalBus.Instance.LevelCompleted += OnLevelComplete;
            SignalBus.Instance.LevelSoftFailed += OnLevelSoftFail;
            SignalBus.Instance.LevelHardFailed += OnLevelHardFail;

            TransitionInit();
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

        public Tween StartTransIn(bool pHardFail = false)
        {
            Tween lTween = CreateTween()
                .SetEase(Tween.EaseType.In)
                .SetTrans(Tween.TransitionType.Quad)
                .SetParallel();
            lTween.TweenProperty(this, nameof(TransitionCircleSize), 1f, TRANSITION_DURATION)
                .From(0f);

            if(pHardFail)
                lTween.TweenProperty(accidentLabel, TweenProp.MODULATE_ALPHA, 1f, TRANSITION_DURATION).SetDelay(LABEL_DELAY);
            else
            {
                lTween.TweenProperty(successLabel, TweenProp.MODULATE_ALPHA, 1f, TRANSITION_DURATION).SetDelay(LABEL_DELAY);
                lTween.TweenProperty(successMessageLabel, TweenProp.MODULATE_ALPHA, 1f, TRANSITION_DURATION).SetDelay(LABEL_DELAY);
            }

            lTween.Connect(Tween.SignalName.Finished, new Callable(this, MethodName.StartTransOut));
            return lTween;
        }

        private void StartTransOut()
        {
            Tween lTween = CreateTween()
                .SetEase(Tween.EaseType.In)
                .SetTrans(Tween.TransitionType.Quad)
                .SetParallel();
            foreach (Label lLabel in new Label[] { accidentLabel, successLabel, successMessageLabel})
                lTween.TweenProperty(lLabel, TweenProp.MODULATE_ALPHA, 0f, TRANSITION_DURATION);
            lTween.TweenProperty(this, nameof(TransitionCircleSize), 0f, TRANSITION_DURATION);
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

        protected override void Dispose(bool pDisposing)
        {
            SignalBus.Instance.LevelCompleted -= OnLevelComplete;
            SignalBus.Instance.LevelSoftFailed -= OnLevelSoftFail;
            SignalBus.Instance.LevelHardFailed -= OnLevelHardFail;

            if (Instance == this)
                Instance = null;
            base.Dispose(pDisposing);
        }
    }
}
