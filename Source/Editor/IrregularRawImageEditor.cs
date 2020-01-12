using UnityEngine;
using UnityEditor;
using UnityEditor.UI;

[CustomEditor(typeof(IrregularRawImage))]
public class IrregularRawImageEditor : RawImageEditor
{
    IrregularRawImage m_Img;
    SerializedProperty m_Points;
    SerializedProperty m_keepOriginSize;
    protected override void OnEnable()
    {
        base.OnEnable();
        m_Img = (target as IrregularRawImage).gameObject.GetComponent<IrregularRawImage>();
        m_Img.doPolygonUpdate = true;

        m_Points = serializedObject.FindProperty("path");
        m_keepOriginSize = serializedObject.FindProperty("keepOriginSize");
    }
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        EditorGUILayout.PropertyField(m_keepOriginSize, true);
        EditorGUILayout.PropertyField(m_Points, true);
        m_Img.doPolygonUpdate = true;

        this.serializedObject.ApplyModifiedProperties();
    }

    void OnSceneGUI()
    {
        var trans = m_Img.transform;
        int i;
        //Color prevHandleColor = Handles.color;
        Handles.color = Color.green;
        for (i =0;i<m_Img.pathCore.Length -1; i++)
            Handles.DrawLine(trans.TransformPoint(m_Img.pathCore[i]), trans.TransformPoint(m_Img.pathCore[i+1]));
        Handles.DrawLine(trans.TransformPoint(m_Img.pathCore[i]), trans.TransformPoint(m_Img.pathCore[0]));
        //Handles.color = prevHandleColor;
    }
}
