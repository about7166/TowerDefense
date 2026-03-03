using UnityEditor;
using UnityEngine;
using UnityEditor.SceneManagement; // ★ 新增：用於標記場景變動

[CustomEditor(typeof(TileSlot)), CanEditMultipleObjects]
public class TileSlotEditor : Editor
{
    private GUIStyle centeredStyle;

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        base.OnInspectorGUI();

        centeredStyle = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter,
            fontStyle = FontStyle.Bold,
            fontSize = 14
        };

        float oneButtonWidth = (EditorGUIUtility.currentViewWidth - 25);
        float twobuttonWidth = (EditorGUIUtility.currentViewWidth - 25) / 2;
        float threebuttonWidth = (EditorGUIUtility.currentViewWidth - 25) / 3;

        GUILayout.Label("位置和旋轉選項", centeredStyle);

        GUILayout.BeginHorizontal();

        if (GUILayout.Button("左轉", GUILayout.Width(twobuttonWidth)))
        {
            foreach (var targetTile in targets)
            {
                TileSlot slot = (TileSlot)targetTile;
                Undo.RecordObject(slot.transform, "Rotate Tile"); // ★ 紀錄復原
                slot.RotateTile(-1);
                MarkDirty(slot); // ★ 標記變動
            }
        }

        if (GUILayout.Button("右轉", GUILayout.Width(twobuttonWidth)))
        {
            foreach (var targetTile in targets)
            {
                TileSlot slot = (TileSlot)targetTile;
                Undo.RecordObject(slot.transform, "Rotate Tile");
                slot.RotateTile(1);
                MarkDirty(slot);
            }
        }

        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();

        if (GUILayout.Button("Y - 0.1", GUILayout.Width(twobuttonWidth)))
        {
            foreach (var targetTile in targets)
            {
                TileSlot slot = (TileSlot)targetTile;
                Undo.RecordObject(slot.transform, "Adjust Y");
                slot.ADjustY(-1);
                MarkDirty(slot);
            }
        }

        if (GUILayout.Button("Y + 0.1", GUILayout.Width(twobuttonWidth)))
        {
            foreach (var targetTile in targets)
            {
                TileSlot slot = (TileSlot)targetTile;
                Undo.RecordObject(slot.transform, "Adjust Y");
                slot.ADjustY(1);
                MarkDirty(slot);
            }
        }

        GUILayout.EndHorizontal();

        GUILayout.Label("地板選項", centeredStyle);

        GUILayout.BeginHorizontal();

        if (GUILayout.Button("Can Build", GUILayout.Width(threebuttonWidth)))
        {
            ApplyTileChange(FindFirstObjectByType<TileSetHolder>().tileCanBuild);
        }

        if (GUILayout.Button("No Build", GUILayout.Width(threebuttonWidth)))
        {
            ApplyTileChange(FindFirstObjectByType<TileSetHolder>().tileNoBuild);
        }

        if (GUILayout.Button("Road", GUILayout.Width(threebuttonWidth)))
        {
            ApplyTileChange(FindFirstObjectByType<TileSetHolder>().tileRoad);
        }

        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();

        if (GUILayout.Button("Sidway", GUILayout.Width(oneButtonWidth)))
        {
            ApplyTileChange(FindFirstObjectByType<TileSetHolder>().tileSideway);
        }

        GUILayout.EndHorizontal();

        GUILayout.Label("角落選項", centeredStyle);

        GUILayout.BeginHorizontal();

        if (GUILayout.Button("Inner Corner", GUILayout.Width(twobuttonWidth)))
        {
            ApplyTileChange(FindFirstObjectByType<TileSetHolder>().tileInnerCorner);
        }

        if (GUILayout.Button("Outer Corner", GUILayout.Width(twobuttonWidth)))
        {
            ApplyTileChange(FindFirstObjectByType<TileSetHolder>().tileOuterCorner);
        }

        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();

        if (GUILayout.Button("Small Inner Corner", GUILayout.Width(twobuttonWidth)))
        {
            ApplyTileChange(FindFirstObjectByType<TileSetHolder>().tileInnerCornerSmall);
        }

        if (GUILayout.Button("Small Outer Corner", GUILayout.Width(twobuttonWidth)))
        {
            ApplyTileChange(FindFirstObjectByType<TileSetHolder>().tileOuterCornerSmall);
        }

        GUILayout.EndHorizontal();

        GUILayout.Label("橋及斜坡選項", centeredStyle);

        GUILayout.BeginHorizontal();

        if (GUILayout.Button("Hill 1", GUILayout.Width(threebuttonWidth)))
        {
            ApplyTileChange(FindFirstObjectByType<TileSetHolder>().tileHill_1);
        }

        if (GUILayout.Button("Hill 2", GUILayout.Width(threebuttonWidth)))
        {
            ApplyTileChange(FindFirstObjectByType<TileSetHolder>().tileHill_2);
        }

        if (GUILayout.Button("Hill 3", GUILayout.Width(threebuttonWidth)))
        {
            ApplyTileChange(FindFirstObjectByType<TileSetHolder>().tileHill_3);
        }

        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();

        if (GUILayout.Button("Bridge with Field", GUILayout.Width(threebuttonWidth)))
        {
            ApplyTileChange(FindFirstObjectByType<TileSetHolder>().tileBridgeField);
        }

        if (GUILayout.Button("Bridge with Road", GUILayout.Width(threebuttonWidth)))
        {
            ApplyTileChange(FindFirstObjectByType<TileSetHolder>().tileBridgeRoad);
        }

        if (GUILayout.Button("Bridge with Sideway", GUILayout.Width(threebuttonWidth)))
        {
            ApplyTileChange(FindFirstObjectByType<TileSetHolder>().tileBridgeSideway);
        }

        GUILayout.EndHorizontal();

        GUILayout.Label("關卡按鈕", centeredStyle);

        GUILayout.BeginHorizontal();

        if (GUILayout.Button("Level Button Tile", GUILayout.Width(oneButtonWidth)))
        {
            ApplyTileChange(FindFirstObjectByType<TileSetHolder>().levelSelectTile);
        }

        GUILayout.EndHorizontal();
    }

    // ★ 抽取出來的共用邏輯：負責切換地塊並標記存檔
    private void ApplyTileChange(GameObject newTilePrefab)
    {
        if (newTilePrefab == null) return;

        foreach (var targetTile in targets)
        {
            TileSlot slot = (TileSlot)targetTile;

            // 1. 紀錄變動前的狀態 (支援 Undo)
            Undo.RecordObject(slot.gameObject, "Switch Tile");

            // 2. 執行切換地塊
            slot.SwitchTile(newTilePrefab);

            // 3. 標記變動
            MarkDirty(slot);
        }
    }

    // ★ 標記地塊與場景需要存檔
    private void MarkDirty(TileSlot slot)
    {
        EditorUtility.SetDirty(slot);
        EditorUtility.SetDirty(slot.gameObject);

        // 強制標記當前場景為 "Dirty" (標題會出現 * 號)
        if (!Application.isPlaying)
        {
            EditorSceneManager.MarkSceneDirty(slot.gameObject.scene);
        }
    }
}