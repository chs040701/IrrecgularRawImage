using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Collections;

public class EdgeUtil
{

    private static Dictionary<Vector2, bool> pointCheckFlags;

    public static List<Vector2> GetPoints(bool[,] t2d, int width, int height)
    {
        List<Vector2> ret = new List<Vector2>();
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (t2d[i, j])
                    ret.Add(new Vector2(i, j));
            }
        }
        Debug.Log(ret.Count);
        //string str = "";
        //foreach (var v in ret)
        //    str += v.x + "," + v.y + "\n";
        ret = CreateOutside(ret);
        //Debug.Log(str);
        ret = PointReduce1(ret, 7);
        ret = PointReduce0(ret);
        return ret;
    }

    private static List<Vector2> CreateOutside(List<Vector2> corners)
    {
        List<Vector2> orderedCorners = new List<Vector2>();
        List<Vector2> indexs = corners.OrderBy(a => a.x).ToList();
        Vector2 cur = indexs[0];
        Vector2 next = Vector2.zero;
        Vector2 start = cur;
        float minAngle;
        Vector2 vector = new Vector2(0, 1);
        Vector2 tmpVector = Vector2.zero;

        bool hasFindTarget;
        int targetDistance;

        orderedCorners.Add(cur);
        //生成外围边
        do
        {
            minAngle = 360;
            //泛洪法查找可能尽可能近的点组成边
            hasFindTarget = false;
            targetDistance = 0;
            while (!hasFindTarget)
            {
                targetDistance += 1;
                foreach (var temp in corners)
                {
                    if (temp == cur) continue;
                    if (Vector2.SqrMagnitude(temp - cur) > targetDistance) continue;//从身边8个格子开始查找起

                    tmpVector.x = temp.x - cur.x;
                    tmpVector.y = temp.y - cur.y;

                    float tmpAngle = Vector3.Angle(vector, tmpVector);
                    if (tmpAngle < minAngle)
                    {
                        minAngle = tmpAngle;
                        next = temp;
                        hasFindTarget = true;
                    }
                }
            }

            if (orderedCorners.Count >= 2)
            {
                vector = next - orderedCorners[orderedCorners.Count - 2];
                tmpVector = orderedCorners[orderedCorners.Count - 1] - orderedCorners[orderedCorners.Count - 2];

                //找到新顶点的角度与上次迭代的顶点角度相同，说明两顶点共边，可以删除上次迭代的顶点
                if (Vector3.Angle(vector, tmpVector) == 0)
                {
                    orderedCorners.RemoveAt(orderedCorners.Count - 1);
                }
            }

            //加入新顶点
            orderedCorners.Add(next);
            corners.Remove(next);
            //
            vector.x = next.x - cur.x;
            vector.y = next.y - cur.y;
            cur = next;
            next = Vector2.zero;
        } while (cur != start);

        return orderedCorners;
    }

    private static List<Vector2> PointReduce1(List<Vector2> corners, int jump, float angThreshold = 0.05f)
    {
        List<Vector2> result = new List<Vector2>();
        int icount = corners.Count;
        float fmax = -1;//用于保存局部最大值
        int imax = -1;
        bool bstart = false;
        if (icount < jump * 2 + 2)
            return corners;
        for (int i = 0; i < icount; i++)
        {
            Vector2 pa = corners[(i + icount - jump) % icount];
            Vector2 pb = corners[(i + icount + jump) % icount];
            Vector2 pc = corners[i];
            //两支撑点距离
            float fa = (pa - pb).magnitude;
            float fb = (pa - pc).magnitude + (pb - pc).magnitude;
            float fang = fa / fb;
            float fsharp = 1 - fang;
            if (fsharp > angThreshold)
            {
                bstart = true;
                if (fsharp > fmax)
                {
                    fmax = fsharp;
                    imax = i;
                }
            }
            else
            {
                if (bstart)
                {
                    //Debug.Log(imax);
                    result.Add(corners[imax]);
                    imax = -1;
                    fmax = -1;
                    bstart = false;
                }
            }
        }

        return result;
    }

    private static List<Vector2> PointReduce0(List<Vector2> corners, float lenThreshold = 2.9f)
    {
        List<Vector2> result = new List<Vector2>();
        int icount = corners.Count;
        if (icount < 3) return corners;

        Vector2 p0 = corners[icount - 1];
        for (int i = 0; i < icount; i++)
        {
            Vector2 pa = corners[i];
            if ((pa - p0).magnitude < lenThreshold)
            {
                if (++i >= icount)
                    break;
                p0 = corners[i];
            }
            else
                p0 = pa;
            result.Add(p0);
        }

        return result;
    }
}
