using Godot;
using System;

// Author : Julien Fournier

namespace Com.IsartDigital.OneButtonGame.Ticks
{
	public abstract partial class TickEvent : AudioStreamPlayer
	{
        protected AudioEffectSpectrumAnalyzerInstance spectrum;

        private const float FREQ_MAX = 11050.0f;
        private const float MIN_DB = 60.0f;
        protected int busIndex;

        // Seuil de décibel auquel doit être déclenché l'event
        private const float ENERGY_THRESHOLD = 0.3f;

        // quand le son dépase ENERGY_THRESOLD (seuil de décibel)
        // réglez TIMER_INTERVAL pour qu'il n'écoute plus le dépassement de db avant
        // avant TIMER_INTERVAL secondes 
        // attention si vous avez certain clicks trop rapprochés certains pourraient
        // être ignoré avec un interval trop long
        // En revanche un interval trop court générera des events non désirés
        protected float timerInterval = 0.3f;

        // timer pour éviter de trigger des events plusieurs fois pendant la durée d'un click
        // un genre de cooldown
        private float timer;


        public override void _Ready()
        {
            busIndex = AudioServer.GetBusIndex(Name.ToString());
            spectrum = AudioServer.GetBusEffectInstance(busIndex, 0) as AudioEffectSpectrumAnalyzerInstance;

            // empêche le bug d'energy résiduelle.
            Finished += QueueFree;
        }


        public override void _Process(double pDelta)
        {
            base._Process(pDelta);

            Vector2 magnitude = spectrum.GetMagnitudeForFrequencyRange(0, FREQ_MAX, (int)AudioEffectSpectrumAnalyzerInstance.MagnitudeMode.Average);

            float energy = Mathf.Clamp((MIN_DB + Mathf.LinearToDb(magnitude.Length())) / MIN_DB, 0, 1);

            //printez ici l'energy pour savoir à combien caler le THRESHOLD
            //GD.Print(energy);

            if (energy > ENERGY_THRESHOLD && timer >= timerInterval)
            {
                
                timer = 0;
                OnBeat();
            }

            timer += (float)pDelta;
        }

        protected abstract void OnBeat();
    }
}
