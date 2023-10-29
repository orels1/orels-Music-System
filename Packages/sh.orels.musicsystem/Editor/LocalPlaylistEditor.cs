using UnityEditor;

namespace ORL.MusicSystem
{
    [CustomEditor(typeof(LocalPlaylist))]
    [CanEditMultipleObjects]
    public class LocalPlaylistEditor : PlaylistEditor
    {
        protected override void PostHeaderGUI()
        {
            serializedObject.Update();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("playerStayDelay"));
            serializedObject.ApplyModifiedProperties();
        }
    }
}