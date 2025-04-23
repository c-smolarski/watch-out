using Com.IsartDigital.WoolyWay.Managers;
using Godot;
using System;
using System.Collections.Generic;

// Author : Camille Smolarski

namespace Com.IsartDigital.WoolyWay.Components
{
	public partial class ResolutionRescaler : Node
	{
		[Export] private Node[] targets;
		[Export] private Vector2 baseResolution = new(1920, 1080);
		[Export] private RescaleMode mode;

		private Vector2 ResMultiplier => GetViewport().GetVisibleRect().Size / baseResolution;

		private List<Vector2> baseScales = new List<Vector2>();

		private enum RescaleMode
		{
			STRETCH,
			FIT_WIDTH,
			FIT_HEIGHT,
			FIT_LOWER,
			FIT_GREATER
		}

		public override void _Ready()
		{
			base._Ready();
			BaseScaleInit();
			GetViewport().SizeChanged += RescaleTargets;
			SignalBus.Instance.FullscreenButtonPressed += RescaleTargets;
			RescaleTargets();
			ProcessMode = ProcessModeEnum.Always;
		}

		private void BaseScaleInit()
		{
			foreach (Node lTarget in targets)
			{
				if (lTarget is Node2D)
					baseScales.Add(((Node2D)lTarget).Scale);
				else if (lTarget is Control)
					baseScales.Add(((Control)lTarget).Scale);
			}
		}

		private void RescaleTargets()
		{
			for (int i = 0; i < targets.Length; i++)
			{
				if (targets[i] is Node2D)
					((Node2D)targets[i]).Scale = GetNewScale(baseScales[i]);
				else if (targets[i] is Control)
					((Control)targets[i]).Scale = GetNewScale(baseScales[i]);
			}
		}

		private Vector2 GetNewScale(Vector2 pBaseScale)
		{
			switch (mode)
			{
				case RescaleMode.STRETCH:
					return pBaseScale * ResMultiplier;
				case RescaleMode.FIT_WIDTH:
					return pBaseScale * ResMultiplier.X;
				case RescaleMode.FIT_HEIGHT:
					return pBaseScale * ResMultiplier.Y;
				case RescaleMode.FIT_LOWER:
					return pBaseScale * Mathf.Min(ResMultiplier.X, ResMultiplier.Y);
				case RescaleMode.FIT_GREATER:
					return pBaseScale * Mathf.Max(ResMultiplier.X, ResMultiplier.Y);
				default:
					return Vector2.One;
			}
		}

        protected override void Dispose(bool disposing)
        {
			if(IsInstanceValid(GetViewport()))
				GetViewport().SizeChanged -= RescaleTargets;
            SignalBus.Instance.FullscreenButtonPressed -= RescaleTargets;
            base.Dispose(disposing);
        }
    }
}
