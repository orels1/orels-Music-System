
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace ORL.MusicSystem
{
    [HelpURL("https://musicsystem.orels.sh/docs/#mute-zone")]
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class MuteZone : UdonSharpBehaviour
    {
        public MusicSystem musicSystem;
        
        [Header("Zone Settings")]
        [Tooltip("Minimum amount of time the player must stay in the zone before switching tracks")]
        public float playerStayDelay = 1f;
        [Tooltip("Pause immediately")]
        public bool hardPause;
        
        private float _stayTime;
        private float _lastEnterTime;
        private bool _engaged;
        
        public override void OnPlayerTriggerEnter(VRCPlayerApi player)
        {
            if (!player.isLocal) return;
            _lastEnterTime = Time.timeSinceLevelLoad;
            if (playerStayDelay > 0f)
            {
                Debug.Log($"[MusicSystem][{name}] Player entered, waiting stay time");
                _pauseAttempted = false;
                _stayTime = 0f;
                return;
            }

            Debug.Log($"[MusicSystem][{name}] Player entered, no stay time, pausing");
            if (hardPause)
            {
                musicSystem.HardPause();
            }
            else
            {
                musicSystem.Pause();
            }
            _engaged = true;
        }

        private bool _pauseAttempted;

        public override void OnPlayerTriggerStay(VRCPlayerApi player)
        {
            if (!player.isLocal) return;
            if (_engaged) return;
            if (playerStayDelay > 0f)
            {
                _stayTime += Time.deltaTime;
                if (_stayTime >= playerStayDelay && !_pauseAttempted)
                {
                    Debug.Log($"[MusicSystem][{name}] Stay time ended, pausing");
                    _pauseAttempted = true;
                    _engaged = true;
                    if (hardPause)
                    {
                        musicSystem.HardPause();
                    }
                    else
                    {
                        musicSystem.Pause();
                    }
                }
            }
        }

        public override void OnPlayerTriggerExit(VRCPlayerApi player)
        {
            if (!player.isLocal) return;
            if (!_engaged) return;
            _stayTime = 0f;
            if (playerStayDelay > 0)
            {
                SendCustomEventDelayedSeconds(nameof(Unpause), playerStayDelay);
                return;
            }

            _engaged = false;
        }

        public void Unpause()
        {
            if (!_engaged) return;
            _engaged = false;
            // ignore cases where we re-entered
            if (Time.timeSinceLevelLoad < _lastEnterTime + playerStayDelay) return;
            
            musicSystem.Unpause();
            
            Debug.Log($"[MusicSystem][{name}] Unpausing");
        }
    }
}

