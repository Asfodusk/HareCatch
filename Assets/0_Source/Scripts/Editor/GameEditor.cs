#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

// Рисует стандартные поля компонента Game + кнопку очистки сохранения.
[CustomEditor(typeof(Game))]
public class GameEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // Обычные поля компонента
        DrawDefaultInspector();

        GUILayout.Space(8);

        Game game = (Game)target;
        if (GUILayout.Button("Очистить сохранённую GameData"))
        {
            game.DeleteSave();
            EditorUtility.SetDirty(game); // пометить объект изменённым, чтобы сброс сохранился
        }
    }
}
#endif
