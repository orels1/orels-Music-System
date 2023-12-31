﻿using System.Reflection;
using UdonSharpEditor;
using UnityEditor;
using UnityEngine;

namespace ORL.MusicSystem
{
    [CustomEditor(typeof(Playlist))]
    [CanEditMultipleObjects]
    public class PlaylistEditor: Editor
    {
        private SerializedProperty musicSystem;
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

        private bool _showSwitchHelp;
        
        private void OnEnable()
        {
            musicSystem = serializedObject.FindProperty("musicSystem");
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

            _showSwitchHelp = false;
        }
        public override void OnInspectorGUI()
        {
            var t = (Playlist) target;

            if (UdonSharpGUI.DrawDefaultUdonSharpBehaviourHeader(target))
            {
                return;
            }
            
            if (EditorApplication.isPlaying)
            {
                if (t.musicSystem != null && t.musicSystem.currentPlaylist == t)
                {
                    EditorGUILayout.LabelField("This is the current playlist", EditorStyles.boldLabel);
                    EditorGUILayout.Space(5);
                }
            }
            serializedObject.Update();

            EditorGUILayout.PropertyField(musicSystem);
            var musicSystems = GameObject.FindObjectsOfType<MusicSystem>();
            if (musicSystems.Length == 0)
            {
                EditorGUILayout.HelpBox("No MusicSystem found in scene, please add one", MessageType.Error);
                return;
            }

            if (musicSystems.Length == 1)
            {
                if (GUILayout.Button("Assign MusicSystem"))
                {
                    musicSystem.objectReferenceValue = musicSystems[0];
                }
            }

            if (musicSystems.Length > 1)
            {
                EditorGUILayout.HelpBox("Multiple MusicSystems found, will need to be manually assigned", MessageType.None);
            }
            
            EditorGUILayout.Space(5);

            serializedObject.ApplyModifiedProperties();
            
            PostHeaderGUI();
            
            serializedObject.Update();
            
            EditorGUILayout.PropertyField(playlist);
            EditorGUILayout.PropertyField(shufflePlaylist);
            EditorGUILayout.PropertyField(volume);
            EditorGUILayout.HelpBox("This sets the final volume of the Audio Source", MessageType.None);

            EditorGUILayout.PropertyField(playlistSwitchInType, new GUIContent("Switch In"));
            if (playlistSwitchInType.enumValueIndex == 1)
            {
                EditorGUILayout.PropertyField(playlistSwitchInFadeTime, new GUIContent("Fade Time"));
            }
            
            EditorGUILayout.PropertyField(playlistSwitchOutType, new GUIContent("Switch Out"));
            if (playlistSwitchOutType.enumValueIndex == 1)
            {
                EditorGUILayout.PropertyField(playlistSwitchOutFadeTime, new GUIContent("Fade Time"));
            }

            if (GUILayout.Button(_showSwitchHelp ? "Hide Help" : "Show Switch Help"))
            {
                _showSwitchHelp = !_showSwitchHelp;
            }
            if (_showSwitchHelp)
            {
                EditorGUILayout.HelpBox("This section controls how the playlist will be switched in and out of\nThe fade setting will make the music fade in or out when entering/exiting this playlist's effect area\nIf the playlist is global, this will happen when entering any local playlist or exiting back out of all of them", MessageType.None);
            }

            EditorGUILayout.PropertyField(trackTransitionType, new GUIContent("Transition Type"));
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
            
            EditorGUILayout.HelpBox("This controls the transition between tracks", MessageType.None);
            
            EditorGUILayout.PropertyField(randomizePause);
            if (randomizePause.boolValue)
            {
                EditorGUILayout.PropertyField(randomizePauseMin, new GUIContent("Pause Min"));
                EditorGUILayout.PropertyField(randomizePauseMax, new GUIContent("Pause Max"));
            }
            else
            {
                EditorGUILayout.PropertyField(staticPause);
            }
            
            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(longBreak);
            if (longBreak.boolValue)
            {
                EditorGUILayout.PropertyField(longBreakTrackCount, new GUIContent("Track Count"));
                EditorGUILayout.PropertyField(longBreakResetOnSwitch, new GUIContent("Reset on Switch"));
                EditorGUILayout.PropertyField(longBreakTime, new GUIContent("Break Time"));
            }
            
            serializedObject.ApplyModifiedProperties();
        }
        
        protected virtual void PostHeaderGUI()
        {
        }
    }
    
}