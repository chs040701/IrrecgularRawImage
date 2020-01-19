/* ==============================================================================
 * 功能描述：Sobel算子边缘检测
 * 创 建 者：shuchangliu
 * ==============================================================================*/

using UnityEngine;
using System.Collections;

public class SobelEdgeDetection {

    /// <summary>
    /// 只对超过alpha阈值的像素点进行边缘检测
    /// </summary>
    private float _alphaThreshold;
    private float _grayscaleThreshold;

    /// <summary>
    /// sobel算子
    /// </summary>
    public static int[,] sobelRectGx = new int[3,3]
    {
        { -1, 0, 1 },
        { -2, 0, 2 },
        { -1, 0, 1 }
    };
    public static int[,] sobelRectGy = new int[3,3]
    {
        { -1, -2, -1 },
        { 0, 0, 0 },
        { 1, 2, 1 }
    };

    private Texture2D _sourceTexture;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sourceTexture"></param>
    /// <param name="alphaThreshold"></param>
    /// <param name="grayscaleThreshold"></param>
    /// <returns></returns>
    public bool[,] Detect(Texture2D sourceTexture, float alphaThreshold = 0.9f, float grayscaleThreshold = 0.01f)
    {
        _sourceTexture = sourceTexture;
        _alphaThreshold = alphaThreshold;
        _grayscaleThreshold = grayscaleThreshold;

        int width = sourceTexture.width, height = sourceTexture.height;
        int widthExt = width + 2, heightExt = height + 2;
        float[,] src = new float[widthExt, heightExt];

        for (int i = 1; i <= width; i++)
            for (int j = 1; j <= height; j++)
                src[i, j] = GetBlackOrWhitePixel(i, j);
        for (int i = 1; i <= width; i++)
        {
            src[i, 0] = src[i,1];
            src[i, height + 1] = src[i, height];
        }
        
        for (int j = 0; j < heightExt; j++)
        {
            src[0, j] = src[1, j];
            src[width + 1, j] = src[width, j];
        }
        bool[,] target = new bool[width,  height];

        float gx, gy;
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (src[i+1, j+1] < 1.0f)
                {
                    target[i, j] = false;
                    continue;
                }

                gx = sobelRectGx[0, 0] * src[i, j] + sobelRectGx[0, 1] * src[i + 1, j] + sobelRectGx[0, 2] * src[i + 2, j] +
                    sobelRectGx[1, 0] * src[i, j + 1] + sobelRectGx[1, 1] * src[i + 1, j + 1] + sobelRectGx[1, 2] * src[i + 2, j + 1] +
                    sobelRectGx[2, 0] * src[i, j + 2] + sobelRectGx[2, 1] * src[i + 1, j + 2] + sobelRectGx[2, 2] * src[i + 2, j + 2];

                gy = sobelRectGy[0, 0] * src[i, j] + sobelRectGy[0, 1] * src[i + 1, j] + sobelRectGy[0, 2] * src[i + 2, j] +
                    sobelRectGy[1, 0] * src[i, j + 1] + sobelRectGy[1, 1] * src[i + 1, j + 1] + sobelRectGy[1, 2] * src[i + 2, j + 1] +
                    sobelRectGy[2, 0] * src[i, j + 2] + sobelRectGy[2, 1] * src[i + 1, j + 2] + sobelRectGy[2, 2] * src[i + 2, j + 2];
                target[i, j] =  (gx * gx + gy * gy > this._grayscaleThreshold);
            }
        }


        return target;
    }

    public float GetBlackOrWhitePixel(int x, int y)
    {
        return (GetPixel(x, y).a >= _alphaThreshold) ? 1.0f : 0.0f;
    }

    public Color GetPixel(int x, int y)
    {
        return _sourceTexture.GetPixel(x, y);
    }

    public double GetGray(Color c)
    {
        return 0.299*c.r + 0.587*c.g + 0.114*c.b;
    }
	
}
