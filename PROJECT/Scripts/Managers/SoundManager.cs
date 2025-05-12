using Com.IsartDigital.WatchOut.Components;
using Com.IsartDigital.WatchOut.Utils.Paths;
using Godot;
using System;
using System.Collections.Generic;

// Author : SMOLARSKI Camille

namespace Com.IsartDigital.WatchOut.Managers
{
    public partial class SoundManager : Node
    {
        [ExportCategory("Musics")]
        [Export] public AudioStreamOggVorbis MusicMainMenu { get; private set; }
        [ExportCategory("Ambients")]
        [Export] public AudioStreamOggVorbis StreetLoop { get; private set; }
        [ExportCategory("SFX")]
        [Export] public AudioStreamOggVorbis Sirens { get; private set; }
        [Export] public AudioStreamOggVorbis Horn { get; private set; }
        [Export] public AudioStreamOggVorbis Accident { get; private set; }

        private const float MUSIC_TRANSITION_TIME = 1.5f;

        public static SoundManager Instance;
        public static AudioStreamPlayer MusicPlayer { get; private set; }

        private List<AudioStreamOggVorbis> cooldownList = new();
        private List<AudioStreamPlayer> playersList = new();

        public override void _Ready()
        {
            #region Singleton

            if (Instance != null)
            {
                QueueFree();
                GD.Print(nameof(SoundManager) + "Instance already exists, destroying the last added");
                return;
            }

            Instance = this;
            #endregion
        }

        //Useful for transitions
        private void OnTreeProcessModeChanged()
        {
            if (GetTree().Paused)
                PauseAllSounds();
            else
                ResumeAllSounds();
        }

        /*
         * PLAY METHODS
         */

        public static void PlayMusic(AudioStreamOggVorbis pStream, Node pContainer)
        {
            if (MusicPlayer != null && MusicPlayer.Stream == pStream)
                return;

            AudioStreamPlayer lOldMusic = MusicPlayer;
            MusicPlayer = new AudioStreamPlayer();
            pStream.Loop = true;
            MusicPlayer.Stream = pStream;
            MusicPlayer.Bus = SoundPath.MUSIC_SOUND_BUS;
            MusicPlayer.ProcessMode = ProcessModeEnum.Always;
            MusicPlayer.Autoplay = true;
            pContainer.CallDeferred(Node.MethodName.AddChild, MusicPlayer);

            if (IsInstanceValid(lOldMusic))
                AudioTransitioner.Create(MusicPlayer, lOldMusic, MUSIC_TRANSITION_TIME, pContainer);
        }

        public bool PlaySFX(AudioStreamOggVorbis pStream, float pTimeBetweenSFX = default)
        {
            if(pTimeBetweenSFX != default)
            {
                if (cooldownList.Contains(pStream))
                    return false;

                cooldownList.Add(pStream);
                if (IsInsideTree())
                    GetTree().CreateTimer(pTimeBetweenSFX, false)
                        .Connect(
                        Timer.SignalName.Timeout,
                        Callable.From(() => cooldownList.Remove(pStream)));
            }

            Play(pStream, false, SoundPath.SFX_SOUND_BUS);
            return true;
        }

        public AudioStreamPlayer Play(AudioStreamOggVorbis pStream, bool pIsLooping = false, string pBus = SoundPath.MAIN_SOUND_BUS)
        {
            AudioStreamPlayer lPlayer = new();
            pStream.Loop = pIsLooping;
            lPlayer.Stream = pStream;
            lPlayer.Bus = pBus;
            playersList.Add(lPlayer);
            lPlayer.Finished += () => OnPlayerFinished(lPlayer);
            AddChild(lPlayer);
            lPlayer.Play();
            return lPlayer;
        }

        private void OnPlayerFinished(AudioStreamPlayer pPlayer)
        {
            playersList.Remove(pPlayer);
            pPlayer.QueueFree();
        }

        /*
         * PAUSE/RESUME METHODS
         */

        public void PauseAllSounds()
        {
            foreach (AudioStreamPlayer lPlayer in playersList)
                lPlayer.StreamPaused = true;
        }

        public void ResumeAllSounds()
        {
            foreach (AudioStreamPlayer lPlayer in playersList)
                lPlayer.StreamPaused = false;
        }

        public static float DbFromPercentage(float pPercentage)
        {
            pPercentage = Mathf.Clamp(pPercentage, 0, 100);
            return Mathf.LinearToDb(pPercentage / 100);
        }

        protected override void Dispose(bool pDisposing)
        {
            if (Instance == this)
                Instance = null;
            base.Dispose(pDisposing);
        }

    }
}
