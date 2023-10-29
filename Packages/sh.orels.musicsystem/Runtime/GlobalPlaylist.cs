
using System;
using System.Runtime.Remoting.Messaging;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace ORL.MusicSystem
{
    [HelpURL("https://musicsystem.orels.sh/docs/#global-playlist")]
    public class GlobalPlaylist : Playlist
    {
        public bool autoPlay = true;

        protected override void Init()
        {
            isGlobal = true;
            _shuffledPlaylist = new AudioClip[playlist.Length];
            playlist.CopyTo(_shuffledPlaylist, 0);
            if (shufflePlaylist)
            {
                ShuffleArray(_shuffledPlaylist);
            }

            if (autoPlay)
            {
                musicSystem.SwitchPlaylist(this);
            }
        }
    }
}
