
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
    [HelpURL("https://musicsystem.orels.sh/docs/#music-system")]
    public class MusicSystem : UdonSharpBehaviour
    {
        public AudioSource sourceA;
        public AudioSource sourceB;

        [NonSerialized]
        public Playlist currentPlaylist;

        private PlaybackState _state;
        
        public PlaybackState State => _state;
        
        private AudioSource _sourceARef;
        private AudioSource _sourceBRef;

        private float _sourceVolume = 1f;

        public float PlaybackTime
        {
            get { 
                var playingSource = GetPlayingSource();
                return playingSource == null ? 0f : playingSource.time;
            }
        }

        private bool _switching = false;
        public bool Switching => _switching;

        private DataList _playlistStack;

        private bool _paused;
        public bool Paused => _paused;

        // Unity does not allow to seek the audi source that was not playing on the same frame
        // As a result - we save the resume time and seek on the next frame
        private float _resumeTime;
        // We also have to save volume if we're using a CUT switch
        // Otherwise there will be an audible pop for 1 frame
        private float _resumeVolume;

        public bool PlayTrack(AudioClip track)
        {
            if (!SwitchSources())
            {
                return false;
            }

            _sourceARef.clip = track;
            return true;
        }

        public bool SwitchTrack(AudioClip newTrack, float volume, float resumeTime)
        {
            if (PlayTrack(newTrack))
            {
                _sourceARef.Play();
                
                // will seek 1 frame later
                if (resumeTime > 0f)
                {
                    _sourceARef.volume = 0f;
                    _resumeVolume = volume;
                    _resumeTime = resumeTime;
                    SendCustomEventDelayedFrames(nameof(ResumeTrack), 1);
                }
                else
                {
                    _sourceARef.volume = volume;
                }
                return true;
            }

            return false;
        }

        public void SetSwitching(bool value)
        {
            _switching = value;
        }
        
        private float _fadeTime;
        public bool FadeTrack(AudioClip newTrack, float fadeTime, float volume, float resumeTime)
        {
            _fadeTime = fadeTime;
            _sourceVolume = volume;
            
            Debug.Log($"[MusicSystem] Fading in track {newTrack.name} with {fadeTime} fade time, volume {volume} at time {resumeTime}");
            
            if (resumeTime > 0f)
            {
                _resumeTime = resumeTime;
                SendCustomEventDelayedFrames(nameof(ResumeTrack), 1);
            }

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

        public void ResumeTrack()
        {
            _sourceARef.time = _resumeTime;
            if (_resumeVolume > 0f)
            {
                _sourceARef.volume = _resumeVolume;
            }
            _resumeTime = 0f;
            _resumeVolume = 0f;
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
        
        public bool SwitchPlaylist(Playlist newPlaylist)
        {
            if (_playlistStack == null)
            {
                _playlistStack = new DataList();
            }
            if (currentPlaylist == null)
            {
                Debug.Log($"[MusicSystem] Current playlist was empty, switching to {newPlaylist.name}");
                _playlistStack.Add(newPlaylist);
                currentPlaylist = newPlaylist;
                
                // We can be still fading out after passing through an idle zone
                if (_state == PlaybackState.FadeOut)
                {
                    Debug.Log("[MusicSystem] Currently fading out, wait for that to pass");
                    return true;
                }
                
                newPlaylist.Engage();
                return true;
            }

            if (_playlistStack.Contains(newPlaylist))
            {
                Debug.Log($"[MusicSystem] Playlist {newPlaylist.name} already in stack, ignoring");
                return false;
            }
            
            Debug.Log($"[MusicSystem] Switching playlist to {newPlaylist.name}");
            if (_playlistStack.Count > 0)
            {
                ((Playlist)_playlistStack[_playlistStack.Count - 1].Reference).Disengage();
            }
            _playlistStack.Add(newPlaylist);
            currentPlaylist = newPlaylist;
            return true;
        }

        public bool SwitchPlaylistBack()
        {
            if (_playlistStack.Count == 0) return false;

            var currentPlayingSource = GetPlayingSource();
            // if fading out last playlist
            // the new playlist is not engaged yet
            // and there is enough time left in the track
            // gracefully fade back in
            if (currentPlayingSource != null &&
                _state == PlaybackState.FadeOut &&
                _playlistStack.Count > 1 &&
                !currentPlaylist.Engaged
            )
            {
                var lastPlaylist = (Playlist) _playlistStack[_playlistStack.Count - 1].Reference;
                if (currentPlayingSource.clip.length - currentPlayingSource.time >= lastPlaylist.EndThreshold)
                {
                    _state = PlaybackState.FadeIn;
                    currentPlaylist = lastPlaylist;
                    var swapSourceA = _sourceARef;
                    _sourceARef = _sourceBRef;
                    _sourceBRef = swapSourceA;
                    // we need to manually set the fade timer to start from a correct spot
                    _fadeTimer = Mathf.Lerp(0f, _sourceVolume, currentPlayingSource.volume) * _fadeTime;
                    _playlistStack.RemoveAt(_playlistStack.Count - 1);
                    Debug.Log("[MusicSystem] Old playlist still fading out, fading back in");
                    return true;
                }
            }
            
            
            ((Playlist)_playlistStack[_playlistStack.Count - 1].Reference).Disengage();
            _playlistStack.RemoveAt(_playlistStack.Count - 1);
            
            if (_playlistStack.Count == 0)
            {
                Debug.Log("[MusicSystem] No playlists left, idling");
                currentPlaylist = null;
                return true;
            }
            currentPlaylist = (Playlist) _playlistStack[_playlistStack.Count - 1].Reference;

            return true;
        }

        public void FadeOut(float time, float targetVolume)
        {
            if (_sourceARef.isPlaying)
            {
                SwitchSources();
            }
            _fadeTime = time;
            _fadeTimer = Mathf.Lerp(targetVolume, 0f, _sourceBRef.volume) * _fadeTime;
            _state = PlaybackState.FadeOut;
        }

        private void Update()
        {
            if (currentPlaylist == null)
            {
                if (_state == PlaybackState.FadeOut)
                {
                    if (DoFadeOut())
                    {
                        _state = PlaybackState.Idle;
                    }
                }
                return;
            }
            
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
                var resumeTime = _sourceARef.time;
                _sourceARef.Play();
                _sourceARef.time = resumeTime;
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
            currentPlaylist.SavePlaybackTime();
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
