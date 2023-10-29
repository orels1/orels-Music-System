This is an excerpt from the full documentation.

Check it out here: https://musicsystem.orels.sh

## Installation

- Add to the VRChat Creator Companion using this link: [Add to the VCC](vcc://vpm/addRepo?url=https://orels1.github.io/orels-Music-System/index.json)
- Make sure the "orels-Music-System" Repository is added and selected in the Settings screen
- Open your World project and add the "ORL Music System" Package

## Setup

- Make a new GameObject and add the `MusicSystem` component to it
- Create two AudioSources, set them to 2D and set everything else the way you like
- Drag the AudioSources into the MusicSystem's `Source A` and `Source B` slots

Now let's add some music that will play all across your world!

- Create another GameObject anywhere in the scene and add the `GlobalPlaylist` component to it
- Set the Music System reference to your MusicSystem GameObject. If you only have one - a button will be shown to quickly assign it
- Add your Audio Clips to the `Playlist` array
- Configure the settings to your liking

> **Most settings have tooltips or other help messages!**

If you need localized music playback for specific areas of your world - you can utilize `LocalPlaylist` components

- Add the `LocalPlaylist` component to any GameObject
- Configure it similarly to the `GlobalPlaylist` component
- Add a Trigger Collider to the same game object, and you should be good to go!


If you ever need to stop the music when the player enters an area - you can use the `MuteZone` component

- Add `MuteZone` to any GameObject with a Trigger collider and it should work automatically

**That's it for a general overview! If you want to learn about all the settings - check out the [full docs](/docs)**

If you ever encounter any issues - [hop by the Discord](https://discord.gg/orels1) and ask for help!