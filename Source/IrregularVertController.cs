using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;


using RectPoint = IrregularRawImage.RectPoint;
[RequireComponent(typeof(IrregularRawImage))]
public class IrregularVertController : MonoBehaviour
{
    [SerializeField]
    public int idx = 0;
    private Vector4 m_vec = Vector4.zero;
    [SerializeField]
    public Vector4 vec = Vector4.zero;
    IrregularRawImage m_img;
    void OnEnable()
    {
        m_img = transform.GetComponent<IrregularRawImage>();
        if (idx < m_img.path.Count && idx >= 0)
        {
            m_vec = m_img.path[idx];
        }
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
            if (idx < img.path.Count && idx >= 0)
            {
                img.path[idx] = m_vec;
                img.doPolygonUpdate = true;
            }
        }
    }

}
