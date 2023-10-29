using UnityEditor;

namespace ORL.MusicSystem
{
    [CustomEditor(typeof(GlobalPlaylist))]
    public class GlobalPlaylistEditor : PlaylistEditor
    {
        protected override void PostHeaderGUI()
        {
            serializedObject.Update();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("autoPlay"));
            serializedObject.ApplyModifiedProperties();
        }
    }
}