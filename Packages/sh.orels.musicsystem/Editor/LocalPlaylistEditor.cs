using UnityEditor;

namespace ORL.MusicSystem
{
    [CustomEditor(typeof(LocalPlaylist))]
    public class LocalPlaylistEditor : PlaylistEditor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("playerStayDelay"));
            serializedObject.ApplyModifiedProperties();
            base.OnInspectorGUI();
        }
    }
}