using UnityEditor.Callbacks;
using UnityEngine;

namespace ORL.MusicSystem
{
    public class MusicSystemScenePostProcess
    {
        [PostProcessScene(-100)]
        public static void PostProcessMusicScripts()
        {
            var musicSystem = GameObject.FindObjectOfType<MusicSystem>();
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