using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
[RequireComponent(typeof(IrregularRawImage))]
public class IrregularVertController : MonoBehaviour
{
    [SerializeField]
    public int index = 0;
    private Vector4 m_vec = new Vector4(0.0f, 0.0f, 1f, 1f);
    [SerializeField]
    public Vector4 vec = new Vector4(0.0f, 0.0f, 1f, 1f);
    IrregularRawImage m_img;
    void OnEnable()
    {
        m_img = transform.GetComponent<IrregularRawImage>();
    }
    /// <summary>
    ///   <para>The RawImage texture coordinates.</para>
    /// </summary>
    protected  void OnDidApplyAnimationProperties()
    {
        TryApplyProp(m_img);
    }

    public void TryApplyProp(IrregularRawImage img)
    {
        if (m_vec != vec)
        {
            m_vec = vec;
            if (index < img.path.Length && index >= 0)
            {
                img.path[index] = m_vec;
                img.doPolygonUpdate = true;
            }
        }
    }

}
