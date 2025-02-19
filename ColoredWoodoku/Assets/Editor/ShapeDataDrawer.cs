using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Shapedata),false)]
//[CanEditMultipleObjects]
[System.Serializable]
public class ShapeDataDrawer : Editor
{
    private Shapedata ShapeDataInstance => target as Shapedata;

    public override void OnInspectorGUI()
    {
      serializedObject.Update();
        ClearBoarButton();
        EditorGUILayout.Space();
        DrawColumnInputFields();
        EditorGUILayout.Space();

        if(ShapeDataInstance.board != null && ShapeDataInstance.columns >0 && ShapeDataInstance.rows >0)
        {
            Drawboardtable();
        }
        serializedObject.ApplyModifiedProperties();

        if(GUI.changed)
        {
            EditorUtility.SetDirty(ShapeDataInstance);
        }

    }

    private void ClearBoarButton()
    {
        if(GUILayout.Button("Clear Button"))
        {
            ShapeDataInstance.Clear();
        }
    }

    private void DrawColumnInputFields()
    {
        var columnsTemp = ShapeDataInstance.columns;
        var rowsTemp = ShapeDataInstance.rows;

        ShapeDataInstance.columns = EditorGUILayout.IntField("columns",ShapeDataInstance.columns);
        ShapeDataInstance.rows = EditorGUILayout.IntField("rows", ShapeDataInstance.rows);

        if((ShapeDataInstance.columns != columnsTemp || ShapeDataInstance.rows != rowsTemp) && ShapeDataInstance.columns > 0 && ShapeDataInstance.rows>0)
        {
           ShapeDataInstance.CreateNewBoard(); 
        }
    }

    private void Drawboardtable()
    {
        var tableStayle = new GUIStyle("box");
        tableStayle.padding= new RectOffset(10,10,10,10);
        tableStayle.margin.left = 32;

        var headerColumnStyle = new GUIStyle();
        headerColumnStyle.fixedWidth = 65;
        headerColumnStyle.alignment = TextAnchor.MiddleCenter;

        var RowStyle = new GUIStyle();
        RowStyle.fixedHeight = 25;
        RowStyle.alignment = TextAnchor.MiddleCenter;

        var DataFieldStyle = new GUIStyle(EditorStyles.miniButtonMid);
        DataFieldStyle.normal.background = Texture2D.grayTexture;
        DataFieldStyle.onNormal.background = Texture2D.whiteTexture;
        
        for(var row=0;row<ShapeDataInstance.rows;row++)
        {
            EditorGUILayout.BeginHorizontal(headerColumnStyle);

            for( var column=0; column < ShapeDataInstance.columns; column++)
            {
                EditorGUILayout.BeginHorizontal(RowStyle);
                var data = EditorGUILayout.Toggle(ShapeDataInstance.board[row].column[column], DataFieldStyle);
                ShapeDataInstance.board[row].column[column]= data;
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndHorizontal();
        }

    }

}
