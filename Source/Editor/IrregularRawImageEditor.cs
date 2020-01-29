using UnityEngine;
using UnityEditor;
using UnityEditor.UI;
using System;
using System.Linq;

using RectPoint = IrregularRawImage.RectPoint;
[CustomEditor(typeof(IrregularRawImage))]
public class IrregularRawImageEditor : RawImageEditor
{
    Texture2D texMull=null;

    IrregularRawImage m_Img;
    Rect rect;
    // SerializedProperty m_Points;
    SerializedProperty m_keepOriginSize;

    protected bool showPath = false;
    protected bool dirty = false;
    protected int curSize = 0;
    protected delegate float FloatGetter();
    protected delegate void FloatSetter(float f);
    protected override void OnEnable()
    {
        base.OnEnable();
        m_Img = target as IrregularRawImage;
        m_Img.doPolygonUpdate = true;
        //rectTransform = m_Img.rectTransform;
         
        // m_Points = serializedObject.FindProperty("path");
        m_keepOriginSize = serializedObject.FindProperty("keepOriginSize");
    }
    void OnChange()
    {
        Undo.RecordObject(m_Img, "Inspector");
        dirty = true;
    }
    void FloatField(string label, FloatGetter getter, FloatSetter setter)
    {
        //EditorGUILayout.LabelField(label, GUILayout.MaxWidth(25));
        EditorGUI.BeginChangeCheck();
        float newValue = EditorGUILayout.FloatField(label, getter(), GUILayout.MaxWidth(120));
        if (EditorGUI.EndChangeCheck())
        {
            setter(newValue);
        }
    }
    void RectPointField(int index, RectPoint rc)
    {
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField(index.ToString(), GUILayout.MaxWidth(40));
        FloatField("X", ()=>{return rc.x;}, (value)=>{rc.x = value;});
        FloatField("Y", ()=>{return rc.y;}, (value)=>{rc.y = value;});
        FloatField("U", ()=>{return rc.u;}, (value)=>{rc.x += (rc.u - value) * rect.width; rc.u = value;});
        FloatField("V", ()=>{return rc.v;}, (value)=>{rc.y += (rc.v - value) * rect.height;rc.v = value;});
        
        if (GUILayout.Button("-", GUILayout.MaxWidth(20), GUILayout.MaxHeight(20)))
        {
            OnChange();
            m_Img.path.RemoveAt(index);
            curSize--;
        }
        if (GUILayout.Button("+", GUILayout.MaxWidth(20), GUILayout.MaxHeight(20)))
        {
            OnChange();
            m_Img.path.Insert(index, new RectPoint());
            curSize++;
        }
        EditorGUILayout.EndHorizontal();
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        dirty = false;
        rect = m_Img.rectTransform.rect;
        EditorGUILayout.PropertyField(m_keepOriginSize, true);
        curSize = m_Img.path.Count;
        showPath=EditorGUILayout.Foldout(showPath, "Path");
        if(showPath)
        {
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.BeginHorizontal();
            EditorGUI.indentLevel=1;
            EditorGUILayout.LabelField("size", GUILayout.MaxWidth(200));
            int newSize = EditorGUILayout.IntField(curSize);
            if (EditorGUI.EndChangeCheck())
            {
                OnChange();
                if (newSize < 0)
                    newSize = 0;
                if (newSize < curSize)
                    m_Img.path.RemoveRange(newSize, curSize - newSize);
                else if (newSize > curSize)
                {
                    if (newSize > m_Img.path.Capacity)
                        m_Img.path.Capacity = newSize;
                    m_Img.path.AddRange(Enumerable.Repeat(new RectPoint(), newSize - curSize));
                }
            }
            EditorGUILayout.EndHorizontal();
            curSize = m_Img.path.Count;
            float f = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 30;
            for(int i=0; i<curSize;i++)
                RectPointField(i, m_Img.path[i]);
            EditorGUIUtility.labelWidth = f;
        }
        //EditorGUI.EndProperty();

        EditorGUI.indentLevel=0;
        m_Img.doPolygonUpdate = true;
        //EditorGUILayout.PropertyField(m_edgeTex, true);
        Texture2D tex = EditorGUILayout.ObjectField("EdgeTexture", texMull, typeof(Texture2D), true) as Texture2D;
        if(tex!=null)
        {
            string path = AssetDatabase.GetAssetPath(tex.GetInstanceID());
            TextureImporter textureImporter = AssetImporter.GetAtPath(path) as TextureImporter;
            TextureImporterSettings cacheSettings = new TextureImporterSettings();
            textureImporter.ReadTextureSettings(cacheSettings);

            TextureImporterSettings tmp = new TextureImporterSettings();
            textureImporter.ReadTextureSettings(tmp);
            tmp.readable = true;
            textureImporter.SetTextureSettings(tmp);
            AssetDatabase.ImportAsset(path);

            SobelEdgeDetection sobel = new SobelEdgeDetection();
            var targetT2d = sobel.Detect(tex);

            //Rect rect = rectTransform.rect;

            m_Img.path = EdgeUtil.GetPoints(targetT2d, tex.width, tex.height).Select(v =>
            {
                return new RectPoint(v.x - tex.width * 0.5f, v.y - tex.height * 0.5f);
            }).ToList();

            textureImporter.SetTextureSettings(cacheSettings);
            AssetDatabase.ImportAsset(path);
            AssetDatabase.Refresh();

            Debug.Log("Edge Detect Done");
            dirty = true;
        }
        this.serializedObject.ApplyModifiedProperties();
        
        if(dirty)
            EditorUtility.SetDirty(target);
    }

    void OnSceneGUI()
    {
        var trans = m_Img.transform;
        Handles.color = Color.green;
        Handles.DrawPolyLine(m_Img.pathCore.Select(v =>
            trans.TransformPoint(v)).ToArray());
    }
}
