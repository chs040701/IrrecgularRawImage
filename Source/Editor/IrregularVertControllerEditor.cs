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
    protected bool dirty = false;
    private delegate float FloatGetter();
    private delegate void FloatSetter(float f);
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
    
    void FloatField(string label, FloatGetter getter, FloatSetter setter)
    {
        EditorGUILayout.LabelField(label, GUILayout.MaxWidth(25));
        EditorGUI.BeginChangeCheck();
        float newValue = EditorGUILayout.FloatField(getter());
        if (EditorGUI.EndChangeCheck())
        {
            dirty = true;
            setter(newValue);
        }
    }
    public override void OnInspectorGUI()
    {
        this.serializedObject.Update();
        dirty = false;
        //EditorGUILayout.PropertyField(this.m_UVRect, this.m_UVRectContent, new GUILayoutOption[0]);
        EditorGUILayout.PropertyField(m_index);
        //EditorGUILayout.PropertyField(m_Vec, m_VecContent, new GUILayoutOption[0]);
        // m_target.vec = EditorGUILayout.RectField("vec", m_target.vec);
        
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("vec", GUILayout.MaxWidth(25));
        FloatField("X", ()=>{return m_target.vec.x;}, (value)=>{m_target.vec.x = value;});
        FloatField("Y", ()=>{return m_target.vec.y;}, (value)=>{m_target.vec.y = value;});
        FloatField("U", ()=>{return m_target.vec.z;}, (value)=>{m_target.vec.z = value;});
        FloatField("V", ()=>{return m_target.vec.w;}, (value)=>{m_target.vec.w = value;});
        EditorGUILayout.EndHorizontal();

        serializedObject.ApplyModifiedProperties();
        if(dirty)
        {
            m_target.TryApplyProp(m_img);
            EditorUtility.SetDirty(target);
        }
    }
}
