using UnityEngine;
using UnityEditor;
using UnityEditor.UI;
using System.Linq;

[CustomEditor(typeof(IrregularRawImage))]
public class IrregularRawImageEditor : RawImageEditor
{
    Texture2D texMull=null;

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
        //EditorGUILayout.PropertyField(m_edgeTex, true);
        Texture2D tex = EditorGUILayout.ObjectField("EdgeTexture", texMull, typeof(Texture2D), true) as Texture2D;
        if(tex!=null)
        {
            string path = AssetDatabase.GetAssetPath(tex.GetInstanceID());
            TextureImporter textureImporter = AssetImporter.GetAtPath(path) as TextureImporter;
            TextureImporterSettings cacheSettings = new TextureImporterSettings();
            textureImporter.ReadTextureSettings(cacheSettings);

            //将Texture临时设置为可读写
            TextureImporterSettings tmp = new TextureImporterSettings();
            textureImporter.ReadTextureSettings(tmp);
            tmp.readable = true;
            textureImporter.SetTextureSettings(tmp);
            AssetDatabase.ImportAsset(path);

            SobelEdgeDetection sobel = new SobelEdgeDetection();
            var targetT2d = sobel.Detect(tex);

            m_Img.path = EdgeUtil.GetPoints(targetT2d, tex.width, tex.height).Select(v =>
            {
                return new Vector4(v.x, v.y, 0.5f, 0.5f);
            }).ToList();

            textureImporter.SetTextureSettings(cacheSettings);
            AssetDatabase.ImportAsset(path);
            AssetDatabase.Refresh();

            Debug.Log("预处理完成");
        }
        this.serializedObject.ApplyModifiedProperties();
    }

    void OnSceneGUI()
    {
        var trans = m_Img.transform;
        Handles.color = Color.green;
        Handles.DrawPolyLine(m_Img.pathCore.Select(v =>
            trans.TransformPoint(v)).ToArray());
    }
}
