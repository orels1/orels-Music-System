using System;
using ORL.MusicSystem;
using UdonSharp;
using UnityEngine;
using UnityEngine.Serialization;
using VRC.SDKBase;
using VRC.Udon;

namespace ORL.MusicSystem
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class Playlist : UdonSharpBehaviour
    {
        public MusicSystem musicSystem;

        [HideInInspector]
        public bool isGlobal;
        
        public AudioClip[] playlist;
        public bool shufflePlaylist;
        public float volume = 1f;

        protected AudioClip[] _shuffledPlaylist;
        
        [Header("Playlist Switching")]
        [Tooltip("Controls how to switch into the playlist")]
        public PlaylistSwitchType playlistSwitchInType;
        public float playlistSwitchInFadeTime;
        [Tooltip("Controls how to switch out of the playlist")]
        public PlaylistSwitchType playlistSwitchOutType;
        public float playlistSwitchOutFadeTime;

        
        [Header("Transitions")]
        [Tooltip("Controls how to switch between tracks")]
        public SwitchType trackTransitionType;
        
        [Tooltip("Fades the audio in and out and in when changing tracks\nAcoustic tracks are better off unfaded")]
        public float fadeTime;
        
        [Tooltip("0 - Track A, 1 - Track B")]
        public AnimationCurve crossFadeCurve;
        [Tooltip("Controls how long the cross-fade will last")]
        public float crossFadeTime;

        [Header("Music Breaks")]
        [Tooltip("Allows randomizing the pause between songs")]
        public bool randomizePause;
        public float randomizePauseMin;
        public float randomizePauseMax = 30f;

        [Tooltip("Sets a static timeout between tracks, in seconds")]
        public float staticPause = 5f;

        [Header("Long Break")]
        [Tooltip("Inserts a long pause between playing music after X tracks have played")]
        public bool longBreak;

        [Tooltip("Controls the minimum amount of tracks to be played before a long break")]
        public int longBreakTrackCount;

        [FormerlySerializedAs("resetOnSwitch")]
        [Tooltip("Resets the long break counter when switching playlists")]
        public bool longBreakResetOnSwitch;

        [Tooltip("Sets the long break time, in seconds")]
        public float longBreakTime = 120f;
        

        protected int _trackIndex;
        protected int _totalTracksPlayed;
        protected bool _engaged;
        // this is set when the playlist triggered a successful switch but has not engaged yet
        protected bool _engaging;

        public bool Engaged => _engaged;
        
        protected float _pauseTimer;
        protected float _waitFor;
        
        protected float _playbackTime;

        public float EndThreshold => trackTransitionType == SwitchType.Fade ? fadeTime + 0.1f : crossFadeTime + 0.1f;

        private void Start()
        {
            Init();
        }

        protected virtual void Init()
        {
        }

        public virtual void Engage()
        {
            Debug.Log($"[MusicSystem][{name}] Engaged");
            _engaged = true;
            _engaging = false;
            musicSystem.SetSwitching(false);
        }

        public virtual void Disengage()
        {
            _engaged = false;
            _engaging = false;
            SavePlaybackTime();
            musicSystem.SetSwitching(true);
            if (longBreakResetOnSwitch)
            {
                _totalTracksPlayed = 0;
            }
            Debug.Log($"[MusicSystem][{name}] Disengaged, playback time {_playbackTime}");

            switch (playlistSwitchOutType)
            {
                case PlaylistSwitchType.Fade:
                    musicSystem.FadeOut(playlistSwitchOutFadeTime, volume);
                    break;
                case PlaylistSwitchType.Cut:
                    Debug.Log($"[MusicSystem][{name}] Cutting playlist");
                    musicSystem.PauseSources(this);
                    musicSystem.SetState(this, PlaybackState.Paused);
                    break;
            }
        }

        public void SavePlaybackTime()
        {
            _playbackTime = musicSystem.PlaybackTime +
                            (playlistSwitchOutType == PlaylistSwitchType.Fade ? playlistSwitchOutFadeTime : 0f);
        }

        public virtual PlaybackState HandleIdle()
        {
            return HandleUnpause(PlaybackState.Idle);
        }

        public virtual PlaybackState HandlePause(PlaybackState currentState)
        {
            if (musicSystem.Paused) return currentState;
            _pauseTimer += Time.deltaTime;
            if (_pauseTimer >= _waitFor)
            {
                return HandleUnpause(currentState);
            }
            return currentState;
        }

        public virtual PlaybackState HandleUnpause(PlaybackState currentState)
        {
            Debug.Log($"[MusicSystem][{name}] Unpausing");
            
            if (musicSystem.Switching)
            {
                return HandleSwitchIn(currentState);
            }
            
            var currTrackTimeRemaining = _shuffledPlaylist[_trackIndex].length - _playbackTime;
            var canResume = _playbackTime > 0f && currTrackTimeRemaining > playlistSwitchInFadeTime &&
                            currTrackTimeRemaining > EndThreshold;  
            
            AudioClip newTrack;
            if (!canResume)
            {
                _playbackTime = 0f;
                newTrack = _shuffledPlaylist[ScheduleNextTrack()];
                Debug.Log($"[MusicSystem][{name}] Can't resume, scheduling next track");
            }
            else
            {
                _trackIndex = Mathf.Max(0, _trackIndex - 1);
                newTrack = _shuffledPlaylist[_trackIndex];
                Debug.Log($"[MusicSystem][{name}] Can resume from {_playbackTime} in {newTrack.name}");
            }
            
            switch (trackTransitionType)
            {
                case SwitchType.Fade:
                    if (musicSystem.FadeTrack(newTrack, fadeTime, volume, _playbackTime))
                    {
                        _trackIndex++;
                        _totalTracksPlayed++;
                        return PlaybackState.FadeIn;
                    }
                    return currentState;
                // can't cross-fade if the playlist was paused/stopped
                case SwitchType.CrossFade:
                    if (musicSystem.FadeTrack(newTrack, crossFadeTime, volume, _playbackTime))
                    {
                        _trackIndex++;
                        _totalTracksPlayed++;
                        return PlaybackState.FadeIn;
                    }
                    return currentState;
                case SwitchType.Cut:
                    if (musicSystem.SwitchTrack(newTrack, volume, _playbackTime))
                    {
                        _trackIndex++;
                        _totalTracksPlayed++;
                        return PlaybackState.Playing;
                    }

                    return currentState;
                default:
                    return currentState;
            }
        }

        private PlaybackState HandleSwitchIn(PlaybackState currentState)
        {
            var currTrackTimeRemaining = _shuffledPlaylist[_trackIndex].length - _playbackTime;
            var canResume = _playbackTime > 0f && currTrackTimeRemaining > playlistSwitchInFadeTime &&
                            currTrackTimeRemaining > EndThreshold;
            
            AudioClip newTrack;
            if (!canResume)
            {
                _playbackTime = 0f;
                newTrack = _shuffledPlaylist[ScheduleNextTrack()];
                Debug.Log("[MusicSystem][{name}] Can't resume, scheduling next track");
            }
            else
            {
                _trackIndex = Mathf.Max(0, _trackIndex - 1);
                newTrack = _shuffledPlaylist[_trackIndex];
                Debug.Log($"[MusicSystem][{name}] Can resume from {_playbackTime} in {newTrack.name}");
            }
            
            Debug.Log($"[MusicSystem][{name}] System is switching - perform special handling");
            switch (playlistSwitchInType)
            {
                case PlaylistSwitchType.Fade:
                    if (musicSystem.FadeTrack(newTrack, playlistSwitchInFadeTime, volume, _playbackTime))
                    {
                        _trackIndex++;
                        _totalTracksPlayed++;
                        Engage();
                        _playbackTime = 0f;
                        return PlaybackState.FadeIn;
                    }
                    return currentState;
                case PlaylistSwitchType.Cut:
                    if (musicSystem.SwitchTrack(newTrack, volume, _playbackTime))
                    {
                        _trackIndex++;
                        _totalTracksPlayed++;
                        Engage();
                        _playbackTime = 0f;
                        return PlaybackState.Playing;
                    }

                    return currentState;
                default:
                    return currentState;
            }
        }

        public virtual PlaybackState HandleTrackEnded(PlaybackState currentState)
        {
            Debug.Log($"[MusicSystem][{name}] Track Ended");

            // If the system is paused - it is taking controll atm
            if (musicSystem.Paused)
            {
                _waitFor = -1;
                _pauseTimer = 1;
                return PlaybackState.Paused;
            }

            if (musicSystem.Switching)
            {
                return HandleSwitchIn(currentState);
            }
            
            var pauseFor = HandlePauseLogic();
            if (longBreak && longBreakTrackCount > 0 && _totalTracksPlayed % longBreakTrackCount == 0)
            {
                pauseFor = longBreakTime;
                Debug.Log($"[MusicSystem][{name}] Long break for {pauseFor} seconds");
            }
            if (pauseFor > 0)
            {
                _pauseTimer = 0;
                _waitFor = pauseFor;
                return PlaybackState.Paused;
            }
            
            var newTrack = _shuffledPlaylist[ScheduleNextTrack()];

            switch (trackTransitionType)
            {
                case SwitchType.Fade:
                    if (musicSystem.FadeTrack(newTrack, fadeTime, volume, 0f))
                    {
                        _trackIndex++;
                        _totalTracksPlayed++;
                        return PlaybackState.FadeIn;
                    }
                    return currentState;
                // can't cross-fade if the playlist was paused/stopped
                case SwitchType.CrossFade:
                    if (musicSystem.FadeTrack(newTrack, crossFadeTime, volume, 0f))
                    {
                        _trackIndex++;
                        _totalTracksPlayed++;
                        return PlaybackState.FadeIn;
                    }
                    return currentState;
                case SwitchType.Cut:
                    if (musicSystem.SwitchTrack(newTrack, volume, 0f))
                    {
                        _trackIndex++;
                        _totalTracksPlayed++;
                        return PlaybackState.Playing;
                    }

                    return currentState;
                default:
                    return currentState;
            }
        }
        
        public virtual PlaybackState HandleTrackEnding(PlaybackState currentState)
        {
            Debug.Log($"[MusicSystem][{name}] Track Ending");
            var newTrack = _shuffledPlaylist[ScheduleNextTrack()];
            switch (trackTransitionType)
            {
                case SwitchType.Fade:
                    return PlaybackState.FadeOut;
                case SwitchType.CrossFade:
                    if (longBreak && longBreakTrackCount > 0 && _totalTracksPlayed % longBreakTrackCount == 0)
                    {
                        return PlaybackState.FadeOut;
                    }
                    if (musicSystem.CrossFadeTrack(newTrack, crossFadeTime, crossFadeCurve, volume))
                    {
                        _trackIndex++;
                        _totalTracksPlayed++;
                        return PlaybackState.CrossFade;
                    }
                    return currentState;
                // cut plays until the end
                case SwitchType.Cut:
                default:
                    return PlaybackState.TrackEnding;
            }
        }

        public virtual PlaybackState HandleTrackFadedOut(PlaybackState currentState)
        {
            Debug.Log($"[MusicSystem][{name}] Fade-out ended");
            return HandleTrackEnded(currentState);
        }

        public virtual PlaybackState HandleTrackFadedIn(PlaybackState currentState)
        {
            Debug.Log($"[MusicSystem][{name}] Fade-in ended");
            return PlaybackState.Playing;
        }

        public virtual PlaybackState HandleCrossFaded(PlaybackState currentState)
        {
            Debug.Log($"[MusicSystem][{name}] Cross-fade ended");
            return PlaybackState.Playing;
        }
        
        public virtual PlaybackState HandleStopped(PlaybackState currentState)
        {
            return PlaybackState.Idle;
        }

        /// <summary>
        /// Handles all the pause settings and logic
        /// </summary>
        /// <returns>True if should pause</returns>
        private float HandlePauseLogic()
        {
            if (!randomizePause)
            {
                if (staticPause > 0)
                {
                    var waitFor = staticPause;
                    Debug.Log($"[MusicSystem][{name}] Paused for {waitFor} seconds");
                    return staticPause;
                }

                return 0;
            }

            {
                var waitFor = UnityEngine.Random.Range(randomizePauseMin, randomizePauseMax);
                Debug.Log($"[MusicSystem][{name}] Paused for {waitFor} seconds");
                return waitFor;
            }
        }

        protected int ScheduleNextTrack()
        {
            if (_trackIndex < _shuffledPlaylist.Length - 1)
            {
                Debug.Log($"[MusicSystem][{name}] Scheduling next track {_trackIndex + 1}");
                return _trackIndex;
            }

            if (shufflePlaylist)
            {
                ShuffleArray(_shuffledPlaylist);
            }

            Debug.Log($"[MusicSystem][{name}] End reached, scheduling first track after re-shuffle");
            _trackIndex = 0;
            return _trackIndex;
        }

        protected static void ShuffleArray<T>(T[] array)
        {
            int n = array.Length;
            for (int i = 0; i <= n - 2; i++)
            {
                int r = UnityEngine.Random.Range(i, n);
                T t = array[r];
                array[r] = array[i];
                array[i] = t;
            }
        }
    }
}