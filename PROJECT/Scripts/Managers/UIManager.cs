using Com.IsartDigital.OneButtonGame.UI;
using Com.IsartDigital.OneButtonGame.Utils;
using Godot;
using System;
using System.Collections.Generic;

namespace Com.IsartDigital.OneButtonGame.Managers
{
    public partial class UIManager : Node
    {
        [ExportGroup("Nodes")]
        [Export] private CanvasLayer canvasLayer;
        [ExportGroup("PackedScenes")]
        [Export] private PackedScene packedScoreUI;

        public static UIManager Instance { get; private set; }

        private Dictionary<Dumpster, ScoreUI> dumpDict = new();

        public override void _Ready()
        {
            #region Singleton
            if (Instance != null)
            {
                GD.Print("Error : " + nameof(UIManager) + " already exists. The new one is being freed...");
                QueueFree();
                return;
            }
            Instance = this;
            #endregion
            SignalBus.Instance.DumpsterAppeared += InitDumpsterUI;
            SignalBus.Instance.DumpDisposed += OnDumpDispose;
        }

        private void InitDumpsterUI(Dumpster pDump)
        {
            ScoreUI lScore = NodeCreator.CreateNode<ScoreUI>(packedScoreUI, canvasLayer);
            dumpDict.Add(pDump, lScore);
            pDump.Scored += lScore.AddScore;
            pDump.ScoreMultReset += lScore.OnScoreMultReset;
        }

        private void OnDumpDispose(Dumpster pDump)
        {
            pDump.Scored -= dumpDict[pDump].AddScore;
            pDump.ScoreMultReset -= dumpDict[pDump].OnScoreMultReset;
            dumpDict[pDump].QueueFree();
            dumpDict.Remove(pDump);
        }

        protected override void Dispose(bool pDisposing)
        {
            SignalBus.Instance.DumpsterAppeared -= InitDumpsterUI;
            SignalBus.Instance.DumpDisposed -= OnDumpDispose;

            if (Instance == this)
                Instance = null;
            base.Dispose(pDisposing);
        }
    }
}
