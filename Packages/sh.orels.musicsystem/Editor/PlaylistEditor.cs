using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace ORL.MusicSystem
{
    [CustomEditor(typeof(Playlist))]
    public class PlaylistEditor: Editor
    {
        // private SerializedProperty autoPlay;
        private SerializedProperty playlist;
        private SerializedProperty shufflePlaylist;
        private SerializedProperty volume;
        
        private SerializedProperty playlistSwitchInType;
        private SerializedProperty playlistSwitchInFadeTime;
        private SerializedProperty playlistSwitchOutType;
        private SerializedProperty playlistSwitchOutFadeTime;

        private SerializedProperty trackTransitionType;
        private SerializedProperty fadeTime;
        private SerializedProperty crossFadeCurve;
        private SerializedProperty crossFadeTime;
        
        private SerializedProperty randomizePause;
        private SerializedProperty randomizePauseMin;
        private SerializedProperty randomizePauseMax;
        private SerializedProperty staticPause;
        
        private SerializedProperty longBreak;
        private SerializedProperty longBreakTrackCount;
        private SerializedProperty longBreakResetOnSwitch;
        private SerializedProperty longBreakTime;
        
        private void OnEnable()
        {
            playlist = serializedObject.FindProperty("playlist");
            shufflePlaylist = serializedObject.FindProperty("shufflePlaylist");
            volume = serializedObject.FindProperty("volume");
            
            playlistSwitchInType = serializedObject.FindProperty("playlistSwitchInType");
            playlistSwitchInFadeTime = serializedObject.FindProperty("playlistSwitchInFadeTime");
            playlistSwitchOutType = serializedObject.FindProperty("playlistSwitchOutType");
            playlistSwitchOutFadeTime = serializedObject.FindProperty("playlistSwitchOutFadeTime");
            
            trackTransitionType = serializedObject.FindProperty("trackTransitionType");
            fadeTime = serializedObject.FindProperty("fadeTime");
            crossFadeCurve = serializedObject.FindProperty("crossFadeCurve");
            crossFadeTime = serializedObject.FindProperty("crossFadeTime");
            
            randomizePause = serializedObject.FindProperty("randomizePause");
            randomizePauseMin = serializedObject.FindProperty("randomizePauseMin");
            randomizePauseMax = serializedObject.FindProperty("randomizePauseMax");
            staticPause = serializedObject.FindProperty("staticPause");
            
            longBreak = serializedObject.FindProperty("longBreak");
            longBreakTrackCount = serializedObject.FindProperty("longBreakTrackCount");
            longBreakResetOnSwitch = serializedObject.FindProperty("longBreakResetOnSwitch");
            longBreakTime = serializedObject.FindProperty("longBreakTime");
        }
        public override void OnInspectorGUI()
        {
            var t = (Playlist) target;
            if (EditorApplication.isPlaying)
            {
                if (t.musicSystem != null && t.musicSystem.currentPlaylist == t)
                {
                    EditorGUILayout.LabelField("This is the current playlist", EditorStyles.boldLabel);
                    EditorGUILayout.Space(5);
                }
            }
            serializedObject.Update();

            
            EditorGUILayout.PropertyField(playlist);
            EditorGUILayout.PropertyField(shufflePlaylist);
            EditorGUILayout.PropertyField(volume);
            EditorGUILayout.HelpBox("This sets the final volume of the Audio Source", MessageType.None);

            EditorGUILayout.PropertyField(playlistSwitchInType);
            if (playlistSwitchInType.enumValueIndex == 1)
            {
                EditorGUILayout.PropertyField(playlistSwitchInFadeTime);
            }
            
            EditorGUILayout.PropertyField(playlistSwitchOutType);
            if (playlistSwitchOutType.enumValueIndex == 1)
            {
                EditorGUILayout.PropertyField(playlistSwitchOutFadeTime);
            }
            
            EditorGUILayout.HelpBox("This section controls how the playlist will be switched in and out of\nThe fade setting will make the music fade in or out when entering/exiting this playlist's effect area\nIf the playlist is global, this will happen when entering any local playlist or exiting back out of all of them", MessageType.None);

            EditorGUILayout.PropertyField(trackTransitionType);
            switch (trackTransitionType.enumValueIndex)
            {
                case 0:
                    break;
                case 1:
                    EditorGUILayout.PropertyField(fadeTime);
                    break;
                case 2:
                    EditorGUILayout.PropertyField(crossFadeCurve);
                    EditorGUILayout.PropertyField(crossFadeTime);
                    EditorGUILayout.HelpBox("Cross-Fade curve defines how the sources will be mixed\nAt 0 value - track B (the one you're switching from) will be at its full volume\nAt 1 value - track A (the one you're switching to) will be at its full volume\nTime (horizontal axis) should be in a 0-1 range", MessageType.None);
                    break;
            }
            
            EditorGUILayout.PropertyField(randomizePause);
            if (randomizePause.boolValue)
            {
                EditorGUILayout.PropertyField(randomizePauseMin);
                EditorGUILayout.PropertyField(randomizePauseMax);
            }
            else
            {
                EditorGUILayout.PropertyField(staticPause);
            }
            
            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(longBreak);
            if (longBreak.boolValue)
            {
                EditorGUILayout.PropertyField(longBreakTrackCount);
                EditorGUILayout.PropertyField(longBreakResetOnSwitch);
                EditorGUILayout.PropertyField(longBreakTime);
            }
            
            serializedObject.ApplyModifiedProperties();
        }
    }
}