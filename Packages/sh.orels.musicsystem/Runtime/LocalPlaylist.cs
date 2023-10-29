
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace ORL.MusicSystem
{
    [HelpURL("https://musicsystem.orels.sh/docs/#local-playlist")]
    public class LocalPlaylist : Playlist
    {
        [Header("Zone Settings")]
        [Tooltip("Minimum amount of time the player must stay in the zone before switching tracks")]
        public float playerStayDelay = 5f;

        private float _stayTime;
        private float _lastEnterTime;

        protected override void Init()
        {
            _shuffledPlaylist = new AudioClip[playlist.Length];
            playlist.CopyTo(_shuffledPlaylist, 0);
            if (shufflePlaylist)
            {
                ShuffleArray(_shuffledPlaylist);
            }
        }
        
        public override void OnPlayerTriggerEnter(VRCPlayerApi player)
        {
            _lastEnterTime = Time.timeSinceLevelLoad;
            if (playerStayDelay > 0f)
            {
                Debug.Log($"[MusicSystem][{name}] Player entered, waiting stay time");
                _switchAttempted = false;
                _stayTime = 0f;
                return;
            }

            Debug.Log($"[MusicSystem][{name}] Player entered, no stay time, engaging");
            musicSystem.SwitchPlaylist(this);
        }

        private bool _switchAttempted;

        public override void OnPlayerTriggerStay(VRCPlayerApi player)
        {
            if (_engaged) return;
            if (playerStayDelay > 0f)
            {
                _stayTime += Time.deltaTime;
                if (_stayTime >= playerStayDelay && !_switchAttempted)
                {
                    Debug.Log($"[MusicSystem][{name}] Stay time ended, engaging");
                    _switchAttempted = true;
                    musicSystem.SwitchPlaylist(this);
                }
            }
        }

        public override void OnPlayerTriggerExit(VRCPlayerApi player)
        {
            if (!_engaged) return;
            _stayTime = 0f;
            if (playerStayDelay > 0)
            {
                SendCustomEventDelayedSeconds(nameof(SwitchBack), playerStayDelay);
                return;
            }
            
            Debug.Log($"[MusicSystem][{name}] Switching back");
            musicSystem.SwitchPlaylistBack();
        }

        public void SwitchBack()
        {
            if (!_engaged) return;
            // ignore cases where we re-entered
            if (Time.timeSinceLevelLoad < _lastEnterTime + playerStayDelay) return;
            
            Debug.Log($"[MusicSystem][{name}] Switching back");
            musicSystem.SwitchPlaylistBack();
        }
    }
}
