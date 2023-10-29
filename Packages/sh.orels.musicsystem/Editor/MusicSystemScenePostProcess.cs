using UnityEditor.Callbacks;
using UnityEngine;

namespace ORL.MusicSystem
{
    public class MusicSystemScenePostProcess
    {
        [PostProcessScene(-100)]
        public static void PostProcessMusicScripts()
        {
            var musicSystems = GameObject.FindObjectsOfType<MusicSystem>();
            if (musicSystems.Length == 0) return;
            if (musicSystems.Length > 1)
            {
                Debug.Log("Multiple MusicSystems found in scene, will rely on manual assignment");
                return;
            }

            var musicSystem = musicSystems[0];
            var localPlaylists = GameObject.FindObjectsOfType<Playlist>();
            foreach (var localPlaylist in localPlaylists)
            {
                localPlaylist.musicSystem = musicSystem;
            }
            var muteZones = GameObject.FindObjectsOfType<MuteZone>();
            foreach (var muteZone in muteZones)
            {
                muteZone.musicSystem = musicSystem;
            }
        }
    }
}