There will be proper documentation here.

For now this shall suffice:

- Add a MusicSystem component to your scene
- Add 2 2d audio sources as its children, configure as you wish
- Add them as Source A and Source B to the MusicSystem

Now configure the playlists

- For "main area" music - create an object and add a GlobalPlaylist component to it
- Configure the playlist as you wish
- For "sub-areas" - create an object with a trigger collider and a LocalPlaylist component to it
  - Set the playerStayDelay to the amount of seconds you want the system to wait before starting to switch to that playlist (0 to switch immediately)

The rest of the options should be explained via the component inspectors and via tooltips (most things have them, just hover!).

Helper stuff:

- You can add an object with a collider that mutes all music for a bit by adding a MuteZone component to it
- Selecting "hard mute" will immediately kill all music, otherwise it will respect the Switch Out settings of the current playlist
- You can send Pause/Unpause events to the MusicSystem to pause/unpause the music. HardPause will immediately kill all music