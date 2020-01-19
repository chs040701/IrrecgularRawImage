using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.UI;

[CustomEditor(typeof(IrregularVertController), true)]
[CanEditMultipleObjects]
public class IrregularVertControllerEditor : Editor
{
    private SerializedProperty m_Vec;
    private SerializedProperty m_index;
    private GUIContent m_VecContent;
    private IrregularVertController m_target;
    private IrregularRawImage m_img;
    // Start is called before the first frame update
    protected void OnEnable()
    { 
        //Debug.LogError(serializedObject); 
        this.m_VecContent = new GUIContent("point");
        m_index = serializedObject.FindProperty("idx");
        m_Vec = serializedObject.FindProperty("vec");
        m_target = (target as IrregularVertController);
        m_img = m_target.transform.GetComponent<IrregularRawImage>();
    }
    public override void OnInspectorGUI()
    {
        this.serializedObject.Update();
        //EditorGUILayout.PropertyField(this.m_UVRect, this.m_UVRectContent, new GUILayoutOption[0]);
        EditorGUILayout.PropertyField(m_index);
        //EditorGUILayout.PropertyField(m_Vec, m_VecContent, new GUILayoutOption[0]);
        m_target.vec = EditorGUILayout.Vector4Field("vec", m_target.vec);
        m_target.TryApplyProp(m_img);
        serializedObject.ApplyModifiedProperties();
    }
}
