using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(TileSlot)),CanEditMultipleObjects]

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
                ((TileSlot)targetTile).RotateTile(-1);
            }
        }

        if (GUILayout.Button("右轉", GUILayout.Width(twobuttonWidth)))
        {            
            foreach (var targetTile in targets)
            {
                ((TileSlot)targetTile).RotateTile(1);
            }
        }

        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();

        if (GUILayout.Button("Y - 0.1", GUILayout.Width(twobuttonWidth)))
        {
            foreach (var targetTile in targets)
            {
                ((TileSlot)targetTile).ADjustY(-1);
            }
        }

        if (GUILayout.Button("Y + 0.1", GUILayout.Width(twobuttonWidth)))
        {
            foreach (var targetTile in targets)
            {
                ((TileSlot)targetTile).ADjustY(1);
            }
        }

        GUILayout.EndHorizontal();

        GUILayout.Label("地板選項", centeredStyle);


        GUILayout.BeginHorizontal();

        if (GUILayout.Button("Field", GUILayout.Width(twobuttonWidth)))
        {
            GameObject newTile = FindFirstObjectByType<TileSetHolder>().tileField;

            foreach (var targetTile in targets)
            {
                ((TileSlot)targetTile).SwitchTile(newTile);
            }
        }

        if (GUILayout.Button("Road", GUILayout.Width(twobuttonWidth)))
        {
            GameObject newTile = FindFirstObjectByType<TileSetHolder>().tileRoad;

            foreach (var targetTile in targets)
            {
                ((TileSlot)targetTile).SwitchTile(newTile);
            }
        }

        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();

        if (GUILayout.Button("Sidway", GUILayout.Width(oneButtonWidth)))
        {
            GameObject newTile = FindFirstObjectByType<TileSetHolder>().tileSideway;

            foreach (var targetTile in targets)
            {
                ((TileSlot)targetTile).SwitchTile(newTile);
            }
        }

        GUILayout.EndHorizontal();


        GUILayout.Label("角落選項", centeredStyle);


        GUILayout.BeginHorizontal();

        if (GUILayout.Button("Inner Corner", GUILayout.Width(twobuttonWidth)))
        {
            GameObject newTile = FindFirstObjectByType<TileSetHolder>().tileInnerCorner;

            foreach (var targetTile in targets)
            {
                ((TileSlot)targetTile).SwitchTile(newTile);
            }
        }

        if (GUILayout.Button("Outer Corner", GUILayout.Width(twobuttonWidth)))
        {
            GameObject newTile = FindFirstObjectByType<TileSetHolder>().tileOuterCorner;

            foreach (var targetTile in targets)
            {
                ((TileSlot)targetTile).SwitchTile(newTile);
            }
        }

        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();

        if (GUILayout.Button("Small Inner Corner", GUILayout.Width(twobuttonWidth)))
        {
            GameObject newTile = FindFirstObjectByType<TileSetHolder>().tileInnerCornerSmall;

            foreach (var targetTile in targets)
            {
                ((TileSlot)targetTile).SwitchTile(newTile);
            }
        }

        if (GUILayout.Button("Small Outer Corner", GUILayout.Width(twobuttonWidth)))
        {
            GameObject newTile = FindFirstObjectByType<TileSetHolder>().tileOuterCornerSmall;

            foreach (var targetTile in targets)
            {
                ((TileSlot)targetTile).SwitchTile(newTile);
            }
        }

        GUILayout.EndHorizontal();

        GUILayout.Label("橋及斜坡選項", centeredStyle);

        GUILayout.BeginHorizontal();

        if (GUILayout.Button("Hill 1", GUILayout.Width(threebuttonWidth)))
        {
            GameObject newTile = FindFirstObjectByType<TileSetHolder>().tileHill_1;

            foreach (var targetTile in targets)
            {
                ((TileSlot)targetTile).SwitchTile(newTile);
            }
        }

        if (GUILayout.Button("Hill 2", GUILayout.Width(threebuttonWidth)))
        {
            GameObject newTile = FindFirstObjectByType<TileSetHolder>().tileHill_2;

            foreach (var targetTile in targets)
            {
                ((TileSlot)targetTile).SwitchTile(newTile);
            }
        }

        if (GUILayout.Button("Hill 3", GUILayout.Width(threebuttonWidth)))
        {
            GameObject newTile = FindFirstObjectByType<TileSetHolder>().tileHill_3;

            foreach (var targetTile in targets)
            {
                ((TileSlot)targetTile).SwitchTile(newTile);
            }
        }

        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();

        if (GUILayout.Button("Bridge with Field", GUILayout.Width(threebuttonWidth)))
        {
            GameObject newTile = FindFirstObjectByType<TileSetHolder>().tileBridgeField;

            foreach (var targetTile in targets)
            {
                ((TileSlot)targetTile).SwitchTile(newTile);
            }
        }

        if (GUILayout.Button("Bridge with Road", GUILayout.Width(threebuttonWidth)))
        {
            GameObject newTile = FindFirstObjectByType<TileSetHolder>().tileBridgeRoad;

            foreach (var targetTile in targets)
            {
                ((TileSlot)targetTile).SwitchTile(newTile);
            }
        }

        if (GUILayout.Button("Bridge with Sideway", GUILayout.Width(threebuttonWidth)))
        {
            GameObject newTile = FindFirstObjectByType<TileSetHolder>().tileBridgeSideway;

            foreach (var targetTile in targets)
            {
                ((TileSlot)targetTile).SwitchTile(newTile);
            }
        }

        GUILayout.EndHorizontal();
    }
}
