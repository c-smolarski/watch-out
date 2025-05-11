using Com.IsartDigital.WatchOut.Managers;
using Godot;
using System;

// Author : Camille Smolarski

namespace Com.IsartDigital.WatchOut.Components
{
    public partial class AudioTransitioner : Node
    {
        public bool paused;
        private float elapsedTime, targetTime;
        private AudioStreamPlayer transIn, transOut;

        public override void _Process(double pDelta)
        {
            base._Process(pDelta);
            if (!paused)
            {
                elapsedTime += (float)pDelta;
                if (IsInstanceValid(transIn) && IsInstanceValid(transIn) && elapsedTime < targetTime)
                {
                    transIn.VolumeDb = Mathf.LinearToDb(elapsedTime / targetTime);
                    transOut.VolumeDb = Mathf.LinearToDb((targetTime - elapsedTime) / targetTime);
                }
                else
                    QueueFree();
            }
        }

        public static AudioTransitioner Create(AudioStreamPlayer pTransIn, AudioStreamPlayer pTransOut, float pTransTime, Node pContainer = default)
        {
            AudioTransitioner lTransitioner = new AudioTransitioner();
            lTransitioner.transIn = pTransIn;
            lTransitioner.transOut = pTransOut;
            lTransitioner.targetTime = pTransTime;
            if (pContainer == default)
                SoundManager.Instance?.AddChild(lTransitioner);
            else
                pContainer.AddChild(lTransitioner);
            return lTransitioner;
        }

        protected override void Dispose(bool pDisposing)
        {
            if (IsInstanceValid(transIn))
                transIn.VolumeDb = SoundManager.DbFromPercentage(100);
            if (IsInstanceValid(transOut))
                transOut.QueueFree();
            base.Dispose(pDisposing);
        }
    }
}
