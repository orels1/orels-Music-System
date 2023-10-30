using System;
using System.Reflection;
using UdonSharpEditor;
using UnityEditor;
using UnityEngine;
using VRC.SDK3.Data;

namespace ORL.MusicSystem
{
    [CustomEditor(typeof(MusicSystem))]
    public class MusicSystemEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            if (UdonSharpGUI.DrawDefaultUdonSharpBehaviourHeader(target))
            {
                return;
            }

            var t = (MusicSystem) target;
            var tType = target.GetType();
            var uB = UdonSharpEditorUtility.GetBackingUdonBehaviour(t);
            
            if (EditorApplication.isPlaying)
            {
                UdonSharpEditorUtility.CopyUdonToProxy(t);
                var sourceARef = (AudioSource) tType.GetField("_sourceARef", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(target);
                var sourceBRef = (AudioSource) tType.GetField("_sourceBRef", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(target);
                var targetVolume = (float) tType.GetField("_sourceVolume", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(target);
                var playlistStack = uB.GetProgramVariable<DataList>("_playlistStack");
                
                var currPlaylist = t.currentPlaylist;
                EditorGUILayout.LabelField("State", t.State.ToString());
                if (t.State == PlaybackState.Paused && currPlaylist != null)
                {
                    var waitFor = (float) currPlaylist.GetType()
                        .GetField("_waitFor", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(currPlaylist);
                    var pauseTimer = (float) currPlaylist.GetType().GetField("_pauseTimer", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(currPlaylist);
                    var timeLeft = waitFor - pauseTimer;
                    EditorGUILayout.LabelField("Waiting For", TimeSpan.FromSeconds(timeLeft).ToString("g"));
                }
                // if (t.Switching)
                // {
                //     var lastPlaylist = (Playlist) playlistStack[playlistStack.Count - 2].Reference;
                //     using (new EditorGUI.DisabledScope(true))
                //     {
                //         using (new GUILayout.HorizontalScope())
                //         {
                //             EditorGUILayout.ObjectField("From", lastPlaylist, typeof(Playlist), true);
                //             EditorGUILayout.ObjectField("To", currPlaylist, typeof(Playlist), true);
                //         }
                //     }
                // }

                if (currPlaylist != null)
                {
                    using (new EditorGUI.DisabledScope(true))
                    {
                        EditorGUILayout.ObjectField("Playlist", currPlaylist, typeof(Playlist), true);
                    }
                }
                
                EditorGUILayout.Space(5);
                var trackAVolume = sourceARef?.volume ?? 0f;
                var trackBVolume = sourceBRef?.volume ?? 0f;
                using (new GUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField("Track A", sourceARef?.clip?.name ?? "None");
                    EditorGUILayout.LabelField(trackAVolume.ToString("P"), GUILayout.Width(50));
                }
                using (new GUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField("Track B", sourceBRef?.clip?.name ?? "None");
                    EditorGUILayout.LabelField(trackBVolume.ToString("P"), GUILayout.Width(50));
                }
                targetVolume *= 1f;
                EditorGUILayout.LabelField("Target Volume", targetVolume.ToString("P"));
            }
            
            serializedObject.Update();

            var sourceAProp = serializedObject.FindProperty("sourceA");
            var sourceBProp = serializedObject.FindProperty("sourceB");
            
            EditorGUILayout.PropertyField(sourceAProp);
            EditorGUILayout.PropertyField(sourceBProp);
            
            serializedObject.ApplyModifiedProperties();
        }
    }
}