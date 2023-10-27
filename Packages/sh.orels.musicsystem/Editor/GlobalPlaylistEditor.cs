using UnityEditor;

namespace ORL.MusicSystem
{
    [CustomEditor(typeof(GlobalPlaylist))]
    public class GlobalPlaylistEditor : PlaylistEditor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("autoPlay"));
            serializedObject.ApplyModifiedProperties();
            base.OnInspectorGUI();
        }
    }
}