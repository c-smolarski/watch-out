using Com.IsartDigital.Utils.Tweens;
using Com.IsartDigital.WatchOut.GameObjects.Mobiles;
using Com.IsartDigital.WatchOut.Managers;
using Godot;
using System;

// Author : Camille Smolarski

namespace Com.IsartDigital.WatchOut
{
    public partial class Level : Node2D
    {
        [Export] public float AllocatedTime { get; private set; }
        [Export] public Player Player { get; private set; }
        [Export] private bool playerAutoAppear = true;

        [ExportGroup("Win")]
        [Export] private float winDelay = 1f;
        [Export] private Area2D winArea;

        [ExportGroup("Camera")]
        [Export] private Marker2D initialCameraPos;
        [Export] private float cameraDelayBeforeMoving;
        [Export] private float cameraMoveDuration;

        public bool SoftFailed { get; private set; }
        private bool hardFailed;
        private Callable startCallable;

        public override void _Ready()
        {
            base._Ready();
            GetViewport().GetCamera2D().GlobalPosition = default;
            startCallable = Callable.From(StartLevel);
            WinAreaInit();
            SignalBus.Instance.LevelSoftFailed += OnSoftFail;
            SignalBus.Instance.LevelHardFailed += OnHardFail;
            CameraMovements();
        }

        private void WinAreaInit()
        {
            winArea.CollisionLayer = winArea.CollisionMask = Player.COLLISION_LAYER;
            winArea.AreaEntered += OnPlayerCompletesLevel;
        }

        private void CameraMovements()
        {
            if (initialCameraPos == null)
            {
                LevelManager.Instance.Connect(
                    LevelManager.SignalName.LevelLoaded,
                    startCallable);
                return;
            }

            Camera2D lCamera = GetViewport().GetCamera2D();
            lCamera.GlobalPosition = initialCameraPos.GlobalPosition;

            Tween lTween = CreateTween();
            lTween.TweenProperty(lCamera, TweenProp.GLOBAL_POSITION, Vector2.Zero, cameraMoveDuration)
                .SetTrans(Tween.TransitionType.Cubic)
                .SetEase(Tween.EaseType.InOut)
                .SetDelay(cameraDelayBeforeMoving);
            lTween.Connect(Tween.SignalName.Finished, startCallable);
        }

        private void StartLevel()
        {
            if (LevelManager.Instance.IsConnected(LevelManager.SignalName.LevelLoaded, startCallable))
                LevelManager.Instance.Disconnect(LevelManager.SignalName.LevelLoaded, startCallable);

            if (playerAutoAppear)
                Player.Appear();
        }

        private void OnHardFail(string pTranslationKey)
        {
            if (!hardFailed)
            {
                hardFailed = true;
                winArea.AreaEntered -= OnPlayerCompletesLevel;
            }
        }

        private void OnSoftFail(string pFailMessage)
        {
            SoftFailed = true;
        }

        private void OnPlayerCompletesLevel(Area2D pArea)
        {
            if (pArea is not GameObjects.Mobiles.Player)
                return;

            Player.OnLevelEnd();

            GetTree().CreateTimer(winDelay, false).Connect(
                Timer.SignalName.Timeout,
                Callable.From(() => SignalBus.Instance.EmitSignal(SignalBus.SignalName.LevelCompleted)));
        }

        protected override void Dispose(bool pDisposing)
        {
            SignalBus.Instance.LevelSoftFailed -= OnSoftFail;
            SignalBus.Instance.LevelHardFailed -= OnHardFail;
            if (IsInstanceValid(winArea) && !hardFailed)
                winArea.AreaEntered -= OnPlayerCompletesLevel;
            base.Dispose(pDisposing);
        }
    }
}
