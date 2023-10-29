
using System;
using System.Runtime.Remoting.Messaging;
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Data;
using VRC.SDKBase;
using VRC.Udon;

namespace ORL.MusicSystem
{
    public enum PlaybackState
    {
        Idle,
        Playing,
        Paused,
        FadeIn,
        FadeOut,
        TrackEnding,
        CrossFade
    }

    public enum SwitchType
    {
        Cut,
        Fade,
        CrossFade
    }

    public enum PlaylistSwitchType
    {
        Cut,
        Fade
    }
    
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class MusicSystem : UdonSharpBehaviour
    {
        public AudioSource sourceA;
        public AudioSource sourceB;

        [NonSerialized]
        public Playlist currentPlaylist;
        [NonSerialized]
        public Playlist lastPlaylist;

        private PlaybackState _state;
        
        public PlaybackState State => _state;
        
        private AudioSource _sourceARef;
        private AudioSource _sourceBRef;

        private float _sourceVolume = 1f;

        public float PlaybackTime => GetPlayingSource().time;
        private bool _switching = false;
        public bool Switching => _switching;

        private DataList _playlistStack;

        private bool _paused;
        public bool Paused => _paused;
        
        private void Start()
        {
            _playlistStack = new DataList();
        }

        public bool PlayTrack(AudioClip track)
        {
            if (!SwitchSources())
            {
                return false;
            }

            _sourceARef.clip = track;
            return true;
        }

        public bool SwitchTrack(AudioClip newTrack, float volume)
        {
            if (PlayTrack(newTrack))
            {
                _sourceARef.Play();
                _sourceARef.volume = volume;
                return true;
            }

            return false;
        }

        public void SetSwitching(bool value)
        {
            _switching = value;
        }
        
        private float _fadeTime;
        public bool FadeTrack(AudioClip newTrack, float fadeTime, float volume)
        {
            _fadeTime = fadeTime;
            _sourceVolume = volume;
            
            Debug.Log($"[MusicSystem] Fading in track {newTrack.name} with {fadeTime} fade time and volume {volume}");

            return PlayTrack(newTrack);
        }

        private float _crossFadeTime;
        private AnimationCurve _crossFadeCurve;
        public bool CrossFadeTrack(AudioClip newTrack, float crossFadeTime, AnimationCurve crossFadeCurve, float volume)
        {
            _crossFadeTime = crossFadeTime;
            _crossFadeCurve = crossFadeCurve;
            _sourceVolume = volume;

            return PlayTrack(newTrack);
        }

        public void SetState(Playlist sender, PlaybackState newState)
        {
            if (sender != currentPlaylist && _playlistStack.Count > 1 && sender != _playlistStack[_playlistStack.Count - 2])
            {
                Debug.LogError($"[MusicSystem] Received state change from {sender.name} but it's not the current or last playlist");
                return;
            }
            
            Debug.Log($"[MusicSystem] Received state change from {sender.name} to {newState.ToString()}");
            _state = newState;
        }
        
        private AudioSource GetUnusedSource()
        {
            if (!sourceA.isPlaying) return sourceA;
            if (!sourceB.isPlaying) return sourceB;
            return null;
        }

        private AudioSource GetPlayingSource()
        {
            if (sourceA.isPlaying) return sourceA;
            if (sourceB.isPlaying) return sourceB;
            return null;
        }

        private bool SwitchSources()
        {
            var unusedSource = GetUnusedSource();
            if (unusedSource == null)
            {
                Debug.LogError("[MusicSystem] Unable to find a free source to use");
                return false;
            }
            if (_sourceARef == null || _sourceARef.clip == null || !_sourceARef.isPlaying)
            {
                Debug.Log("[MusicSystem] Nothing was playing on source A - assigning to source A");
                _sourceARef = unusedSource;
                return true;
            }
            
            _sourceBRef = _sourceARef;
            _sourceARef = unusedSource;
            
            Debug.Log("[MusicSystem] Something was playing - swapping sources");
            return true;
        }
        
        public void PauseSources(Playlist sender)
        {
            if (sender != currentPlaylist && _playlistStack.Count > 1 && sender != _playlistStack[_playlistStack.Count - 2])
            {
                Debug.LogError($"[MusicSystem] Received pause request from {sender.name} but it's not the current or last playlist");
                return;
            }

            Debug.Log("[MusicSystem] Pausing sources");
            GetPlayingSource().Pause();
        }
        
        public void SwitchPlaylist(Playlist newPlaylist)
        {
            if (currentPlaylist == null)
            {
                _playlistStack.Add(newPlaylist);
                currentPlaylist = newPlaylist;
                newPlaylist.Engage();
                return;
            }

            if (_playlistStack.Contains(newPlaylist)) return;
            
            Debug.Log($"[MusicSystem] Switching playlist to {newPlaylist.name}");
            if (_playlistStack.Count > 0)
            {
                ((Playlist)_playlistStack[_playlistStack.Count - 1].Reference).Disengage();
            }
            _playlistStack.Add(newPlaylist);
            currentPlaylist = newPlaylist;
        }

        public void SwitchPlaylistBack()
        {
            if (_playlistStack.Count == 0) return;
            
            ((Playlist)_playlistStack[_playlistStack.Count - 1].Reference).Disengage();
            _playlistStack.RemoveAt(_playlistStack.Count - 1);
            
            if (_playlistStack.Count == 0)
            {
                Debug.Log("[MusicSystem] No playlists left, idling");
                currentPlaylist = null;
                return;
            }
            currentPlaylist = (Playlist) _playlistStack[_playlistStack.Count - 1].Reference;
        }

        public void FadeOut(float time)
        {
            if (_sourceARef.isPlaying)
            {
                SwitchSources();
            }
            _fadeTime = time;
            _state = PlaybackState.FadeOut;
        }

        private void Update()
        {
            if (currentPlaylist == null) return;
            
            switch (_state)
            {
                case PlaybackState.Idle:
                    _state = currentPlaylist.HandleIdle();
                    break;
                case PlaybackState.Paused:
                    _state = currentPlaylist.HandlePause(_state);
                    break;
                case PlaybackState.FadeIn:
                    if (DoFadeIn())
                    {
                        _state = currentPlaylist.HandleTrackFadedIn(_state);
                    }
                    break;
                case PlaybackState.FadeOut:
                    if (DoFadeOut())
                    {
                        _state = currentPlaylist.HandleTrackFadedOut(_state);
                    }
                    break;
                case PlaybackState.CrossFade:
                    if (DoCrossFade())
                    {
                        _state = currentPlaylist.HandleCrossFaded(_state);
                    }
                    break;
                case PlaybackState.Playing:
                    if (_sourceARef.clip.length - _sourceARef.time <= currentPlaylist.EndThreshold)
                    {
                        _state = currentPlaylist.HandleTrackEnding(_state);
                        if (_state == PlaybackState.FadeOut)
                        {
                            SwitchSources();
                        }
                        Debug.Log($"[MusicSystem] Track ending, new state {_state.ToString()}");
                    }
                    break;
                case PlaybackState.TrackEnding:
                    if (!_sourceARef.isPlaying)
                    {
                        _state = currentPlaylist.HandleTrackEnded(_state);
                        Debug.Log($"[MusicSystem] Track ended, new state {_state.ToString()}");
                    }
                    break;
            }
        }


        private float _fadeTimer;

        /// <summary>
        /// Fades the Track A in
        /// </summary>
        /// <returns>True when done</returns>
        private bool DoFadeIn()
        {
            // Start the fade
            if (!_sourceARef.isPlaying)
            {
                _sourceARef.volume = 0;
                _sourceARef.Play();
                _fadeTimer = 0;
            }

            // End the fade
            if (FadeTrack(_sourceARef, true))
            {
                _fadeTimer = 0;
                return true;
            }
            
            return false;
        }

        /// <summary>
        /// Fades the Track B out
        /// </summary>
        /// <returns>True when done</returns>
        private bool DoFadeOut()
        {
            if (FadeTrack(_sourceBRef, false))
            {
                _sourceBRef.Stop();
                _fadeTimer = 0;
                return true;
            }
            
            return false;
        }
        
       
        /// <summary>
        /// Fades the target in or out
        /// </summary>
        /// <param name="target">Target Source</param>
        /// <param name="fadeIn">Fade in or Fade out</param>
        /// <returns>True when fade ended</returns>
        private bool FadeTrack(AudioSource target, bool fadeIn)
        {
            if (_fadeTimer > _fadeTime)
            {
                return true;
            }
            _fadeTimer += Time.deltaTime;
            var alpha = Mathf.Clamp01(_fadeTimer / _fadeTime);
            // Debug.Log($"[MusicSystem][Global] Fading track {target.clip.name} to {alpha} [{_fadeTimer}]");
            target.volume = fadeIn ? Mathf.Lerp(0f, _sourceVolume, alpha) : Mathf.Lerp(_sourceVolume, 0f, alpha);
            return false;
        }

        private float _crossFadeTimer;

        private bool DoCrossFade()
        {
            if (!_sourceARef.isPlaying)
            {
                _sourceARef.volume = 0;
                _sourceARef.Play();
                _crossFadeTimer = 0;
            }
            
            if (_crossFadeTimer > _crossFadeTime)
            {
                _sourceBRef.Stop();
                _crossFadeTimer = 0;
                return true;
            }
            
            var alpha = Mathf.Clamp01(_crossFadeTimer / _crossFadeTime);
            var curveAlpha = _crossFadeCurve.Evaluate(alpha);
            _sourceARef.volume = Mathf.Lerp(0f, _sourceVolume, curveAlpha);
            _sourceBRef.volume = Mathf.Lerp(_sourceVolume, 0f, curveAlpha);
            _crossFadeTimer += Time.deltaTime;

            return false;
        }

        public void Pause()
        {
            if (currentPlaylist == null) return;
            _paused = true;
            SwitchSources();   
            if (currentPlaylist.playlistSwitchOutType == PlaylistSwitchType.Fade)
            {
                _state = PlaybackState.FadeOut;
                return;
            }
            
            PauseSources(currentPlaylist);
            _state = PlaybackState.Paused;
        }

        public void HardPause()
        {
            if (currentPlaylist == null) return;
            _paused = true;
            PauseSources(currentPlaylist);
            _state = PlaybackState.Paused;
        }

        public void Unpause()
        {
            if (currentPlaylist == null) return;
            if (!_paused) return;
            _paused = false;
            // if (_sourceARef.clip == null)
            // {
            //     var oldRef = _sourceARef;
            //     _sourceARef = _sourceBRef;
            //     _sourceBRef = oldRef;
            // }
        }
    }
}
