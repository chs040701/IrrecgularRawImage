using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System.Linq;
using System;

public class IrregularRawImage : RawImage, ICanvasRaycastFilter
{
    [System.Serializable]
    public class RectPoint
    {
        public float x, y, u, v;
        public RectPoint(float _x = 0.0f, float _y = 0.0f, float _u = 0.5f, float _v =0.5f)
        {
            x= _x; y = _y; u = _u; v = _v;
        }
        public RectPoint(Vector4 vec)
        {
            x= vec.x; y = vec.y; u = vec.z; v = vec.w;
        }

        public static implicit operator RectPoint(Vector4 vec)
        {
            return new RectPoint(vec.x, vec.y, vec.z, vec.w);
        }
        public static implicit operator Vector4(RectPoint pt)
        {
            return new Vector4(pt.x, pt.y, pt.u, pt.v);
        }
    }

    protected class Triangulator
    {
        private Vector2[] mPoints;

        public Triangulator(Vector2[] points)
        {
            mPoints = points;
        }

        public int[] Triangulate()
        {
            List<int> indices = new List<int>();

            int n = mPoints.Length;
            if (n < 3) return indices.ToArray();

            int[] V = new int[n];
            if (Area() > 0)
            {
                for (int v = 0; v < n; v++)
                    V[v] = v;
            }
            else
            {
                for (int v = 0; v < n; v++)
                    V[v] = (n - 1) - v;
            }

            int nv = n;
            int count = 2 * nv;
            for (int m = 0, v = nv - 1; nv > 2;)
            {
                if ((count--) <= 0)
                    return indices.ToArray();

                int u = v;
                if (nv <= u)
                    u = 0;
                v = u + 1;
                if (nv <= v)
                    v = 0;
                int w = v + 1;
                if (nv <= w)
                    w = 0;

                if (Snip(u, v, w, nv, V))
                {
                    int a, b, c, s, t;
                    a = V[u];
                    b = V[v];
                    c = V[w];
                    indices.Add(a);
                    indices.Add(b);
                    indices.Add(c);
                    m++;
                    for (s = v, t = v + 1; t < nv; s++, t++)
                        V[s] = V[t];
                    nv--;
                    count = 2 * nv;
                }
            }

            indices.Reverse();
            return indices.ToArray();
        }

        private float Area()
        {
            int n = mPoints.Length;
            float A = 0.0f;
            for (int p = n - 1, q = 0; q < n; p = q++)
            {
                Vector2 pval = mPoints[p];
                Vector2 qval = mPoints[q];
                A += pval.x * qval.y - qval.x * pval.y;
            }
            return (A * 0.5f);
        }

        private bool Snip(int u, int v, int w, int n, int[] V)
        {
            int p;
            Vector2 A = mPoints[V[u]];
            Vector2 B = mPoints[V[v]];
            Vector2 C = mPoints[V[w]];
            if (Mathf.Epsilon > (((B.x - A.x) * (C.y - A.y)) - ((B.y - A.y) * (C.x - A.x))))
                return false;
            for (p = 0; p < n; p++)
            {
                if ((p == u) || (p == v) || (p == w))
                    continue;
                Vector2 P = mPoints[V[p]];
                if (InsideTriangle(A, B, C, P))
                    return false;
            }
            return true;
        }

        private bool InsideTriangle(Vector2 A, Vector2 B, Vector2 C, Vector2 P)
        {
            float ax, ay, bx, by, cx, cy, apx, apy, bpx, bpy, cpx, cpy;
            float cCROSSap, bCROSScp, aCROSSbp;

            ax = C.x - B.x; ay = C.y - B.y;
            bx = A.x - C.x; by = A.y - C.y;
            cx = B.x - A.x; cy = B.y - A.y;
            apx = P.x - A.x; apy = P.y - A.y;
            bpx = P.x - B.x; bpy = P.y - B.y;
            cpx = P.x - C.x; cpy = P.y - C.y;

            aCROSSbp = ax * bpy - ay * bpx;
            cCROSSap = cx * apy - cy * apx;
            bCROSScp = bx * cpy - by * cpx;

            return ((aCROSSbp >= 0.0f) && (bCROSScp >= 0.0f) && (cCROSSap >= 0.0f));
        }
    }
    protected bool m_doPolygonUpdate = false;
    public bool doPolygonUpdate
    {
        get{return m_doPolygonUpdate;}
        set{m_doPolygonUpdate = value;}
    }
    [SerializeField]
    public List<RectPoint> path = new List<RectPoint>();
    protected Vector2[] m_pathCore;
    public Vector2[] pathCore
    {get{return m_pathCore;}}
    [SerializeField]
    public bool keepOriginSize = false;
    protected Vector2 p = new Vector2();

    List<UIVertex> verts = new List<UIVertex>();
    List<int> indices = new List<int>();

    protected override void Awake()
    {
        base.Awake();
    }
    protected override void Start()
    {
        OnColliderUpdate();
        base.Start();
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        RegisterDirtyMaterialCallback(OnRectTransformDimensionsChange);
    }
    protected override void OnDisable()
    {
        UnregisterDirtyLayoutCallback(OnRectTransformDimensionsChange);
        base.OnDisable();
    }
    protected override void OnPopulateMesh(VertexHelper vh)
    {
        if (!IsActive() || verts == null || verts.Count<3)
        {
			base.OnPopulateMesh(vh);
			return;
		}
        vh.Clear();
        vh.AddUIVertexStream(verts, indices);
    }

    public void OnColliderUpdate()
    {
        if (path.Count < 3)
            return;
        Rect rect = rectTransform.rect;
        m_pathCore = path.Select(p =>
            new Vector2(p.x + (p.u - 0.5f) * rect.width, p.y + (p.v - 0.5f) * rect.height)).ToArray();
        Vector2 bound = (keepOriginSize && texture!=null) ? new Vector2(texture.width, texture.height) : new Vector2(rect.width, rect.height);
        verts = new List<UIVertex>(m_pathCore.Select(v =>
        {
            var vert = new UIVertex();
            vert.position = new Vector3(v.x, v.y, 0.0f);
            vert.color = color;
            vert.uv0 = new Vector2((v.x / bound.x + 0.5f) * uvRect.width + uvRect.x,
                (v.y / bound.y + 0.5f) * uvRect.height + uvRect.y);
            return vert;
        }).ToArray()
        );
        indices = new List<int>(new Triangulator(m_pathCore).Triangulate());
        //string str = "";
        //foreach(var i in indices)
        //{
        //    str += i + " ";
        //}
        //Debug.Log(str);
        SetVerticesDirty();
    }

    protected override void OnRectTransformDimensionsChange()
    {
        m_doPolygonUpdate = true;
    }
    void Update()
    {
        if (m_doPolygonUpdate)
        {
            m_doPolygonUpdate = false;
            OnColliderUpdate();
        }
    }

    public bool IsRaycastLocationValid(Vector2 sp, Camera eventCamera)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, sp, eventCamera, out p);
        return path.Count > 2 && ContainsPoint(m_pathCore);
    }
    //多边形顶点，屏幕点击坐标
    bool ContainsPoint(Vector2[] polyPoints)
    {
        int j = polyPoints.Length - 1;
        bool inside = false;

        for (int i = 0; i < polyPoints.Length; i++)
        {
            if (((polyPoints[i].y < p.y && p.y <= polyPoints[j].y) || (polyPoints[j].y < p.y && p.y <= polyPoints[i].y)) &&
                  (polyPoints[i].x + (p.y - polyPoints[i].y) / (polyPoints[j].y - polyPoints[i].y) * (polyPoints[j].x - polyPoints[i].x)) > p.x)
                inside = !inside;

            j = i;
        }
        return inside;
    }
}
