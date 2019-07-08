using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using UnityEditor.SceneManagement;

public class HeightMapBuilder
{
    public static Texture2D Create(Bounds aabb, Collider collider, int scale = 10, int maxSize = 1024, bool horizontalFlip = true, bool verticalFlip = true, int edgeMinPoolingSize = 3)
    {
        int width = Mathf.CeilToInt(aabb.size.x * scale);
        int height = Mathf.CeilToInt(aabb.size.z * scale);
        if (width > maxSize)
        {
            height = height * maxSize / width;
            width = maxSize;
        }
        if (height > maxSize)
        {
            width = width * maxSize / height;
            height = maxSize;
        }

        width = (width / 2) * 2;
        height = (height / 2) * 2;

        const float depthOffset = 0.1f;
        float depth = aabb.size.y;
        float minDepth = aabb.min.y;
        //float maxDepth = aabb.max.y;

        float scaleW = aabb.size.x / width;
        float scaleH = aabb.size.z / height;

        Ray ray = new Ray(Vector3.zero, Vector3.down);
        RaycastHit hit = new RaycastHit();
        Vector3 cornerStart = aabb.min + new Vector3(0, depth + depthOffset, 0);
        int texWidth = width / 2;
        int texHeight = height / 2;
        Color[] colors = new Color[texWidth * texHeight];
        Texture2D heightMap = new Texture2D(texWidth, texHeight);
        for (int i = 0; i < texWidth; i++)
        {
            for (int j = 0; j < texHeight; j++)
            {
                Color c = new Color();

                if (horizontalFlip && verticalFlip)
                {
                    /* a b
                     * g r */
                    ray.origin = cornerStart + new Vector3((i * 2 + 0) * scaleW, 0, (j * 2 + 0) * scaleH);
                    if (collider.Raycast(ray, out hit, depth + depthOffset + 0.1f))
                        c.a = ((hit.point.y - minDepth) / depth) * 254 / 255;
                    else
                        c.a = 1;
                    ray.origin = cornerStart + new Vector3((i * 2 + 1) * scaleW, 0, (j * 2 + 0) * scaleH);
                    if (collider.Raycast(ray, out hit, depth + depthOffset + 0.1f))
                        c.b = ((hit.point.y - minDepth) / depth) * 254 / 255;
                    else
                        c.b = 1;
                    ray.origin = cornerStart + new Vector3((i * 2 + 0) * scaleW, 0, (j * 2 + 1) * scaleH);
                    if (collider.Raycast(ray, out hit, depth + depthOffset + 0.1f))
                        c.g = ((hit.point.y - minDepth) / depth) * 254 / 255;
                    else
                        c.g = 1;
                    ray.origin = cornerStart + new Vector3((i * 2 + 1) * scaleW, 0, (j * 2 + 1) * scaleH);
                    if (collider.Raycast(ray, out hit, depth + depthOffset + 0.1f))
                        c.r = ((hit.point.y - minDepth) / depth) * 254 / 255;
                    else
                        c.r = 1;
                    colors[(texHeight - 1 - j) * texWidth + (texWidth - 1 - i)] = c;
                    //heightMap.SetPixel(texWidth - 1 - i, texHeight - 1 - j, c);
                }
                else if (horizontalFlip)
                {
                    /* g r
                     * a b */
                    ray.origin = cornerStart + new Vector3((i * 2 + 0) * scaleW, 0, (j * 2 + 0) * scaleH);
                    if (collider.Raycast(ray, out hit, depth + depthOffset + 0.1f))
                        c.g = ((hit.point.y - minDepth) / depth) * 254 / 255;
                    else
                        c.g = 1;
                    ray.origin = cornerStart + new Vector3((i * 2 + 1) * scaleW, 0, (j * 2 + 0) * scaleH);
                    if (collider.Raycast(ray, out hit, depth + depthOffset + 0.1f))
                        c.r = ((hit.point.y - minDepth) / depth) * 254 / 255;
                    else
                        c.r = 1;
                    ray.origin = cornerStart + new Vector3((i * 2 + 0) * scaleW, 0, (j * 2 + 1) * scaleH);
                    if (collider.Raycast(ray, out hit, depth + depthOffset + 0.1f))
                        c.a = ((hit.point.y - minDepth) / depth) * 254 / 255;
                    else
                        c.a = 1;
                    ray.origin = cornerStart + new Vector3((i * 2 + 1) * scaleW, 0, (j * 2 + 1) * scaleH);
                    if (collider.Raycast(ray, out hit, depth + depthOffset + 0.1f))
                        c.b = ((hit.point.y - minDepth) / depth) * 254 / 255;
                    else
                        c.b = 1;
                    colors[j * texWidth + (texWidth - 1 - i)] = c;
                    //heightMap.SetPixel(texWidth - 1 - i, j, c);
                }
                else if (verticalFlip)
                {
                    /* b a
                     * r g */
                    ray.origin = cornerStart + new Vector3((i * 2 + 0) * scaleW, 0, (j * 2 + 0) * scaleH);
                    if (collider.Raycast(ray, out hit, depth + depthOffset + 0.1f))
                        c.b = ((hit.point.y - minDepth) / depth) * 254 / 255;
                    else
                        c.b = 1;
                    ray.origin = cornerStart + new Vector3((i * 2 + 1) * scaleW, 0, (j * 2 + 0) * scaleH);
                    if (collider.Raycast(ray, out hit, depth + depthOffset + 0.1f))
                        c.a = ((hit.point.y - minDepth) / depth) * 254 / 255;
                    else
                        c.a = 1;
                    ray.origin = cornerStart + new Vector3((i * 2 + 0) * scaleW, 0, (j * 2 + 1) * scaleH);
                    if (collider.Raycast(ray, out hit, depth + depthOffset + 0.1f))
                        c.r = ((hit.point.y - minDepth) / depth) * 254 / 255;
                    else
                        c.r = 1;
                    ray.origin = cornerStart + new Vector3((i * 2 + 1) * scaleW, 0, (j * 2 + 1) * scaleH);
                    if (collider.Raycast(ray, out hit, depth + depthOffset + 0.1f))
                        c.g = ((hit.point.y - minDepth) / depth) * 254 / 255;
                    else
                        c.g = 1;
                    //heightMap.SetPixel(i, texHeight - 1 - j, c);
                    colors[(texHeight - 1 - j) * texWidth + i] = c;
                }
                else
                {
                    /* r g
                     * b a */
                    ray.origin = cornerStart + new Vector3((i * 2 + 0) * scaleW, 0, (j * 2 + 0) * scaleH);
                    if (collider.Raycast(ray, out hit, depth + depthOffset + 0.1f))
                        c.r = ((hit.point.y - minDepth) / depth) * 254 / 255;
                    else
                        c.r = 1;
                    ray.origin = cornerStart + new Vector3((i * 2 + 1) * scaleW, 0, (j * 2 + 0) * scaleH);
                    if (collider.Raycast(ray, out hit, depth + depthOffset + 0.1f))
                        c.g = ((hit.point.y - minDepth) / depth) * 254 / 255;
                    else
                        c.g = 1;
                    ray.origin = cornerStart + new Vector3((i * 2 + 0) * scaleW, 0, (j * 2 + 1) * scaleH);
                    if (collider.Raycast(ray, out hit, depth + depthOffset + 0.1f))
                        c.b = ((hit.point.y - minDepth) / depth) * 254 / 255;
                    else
                        c.b = 1;
                    ray.origin = cornerStart + new Vector3((i * 2 + 1) * scaleW, 0, (j * 2 + 1) * scaleH);
                    if (collider.Raycast(ray, out hit, depth + depthOffset + 0.1f))
                        c.a = ((hit.point.y - minDepth) / depth) * 254 / 255;
                    else
                        c.a = 1;
                    //heightMap.SetPixel(i, j, c);
                    colors[j * texWidth + i] = c;
                }
            }
        }

        if (edgeMinPoolingSize > 0)
        {
            Color[] newColors = new Color[texWidth * texHeight];
            for (int i = 0; i < texWidth; i++)
            {
                for (int j = 0; j < texHeight; j++)
                {
                    newColors[j * texWidth + i] = EdgeMinPooling(colors, i, j, texWidth, texHeight, edgeMinPoolingSize);
                }
            }
            colors = newColors;
        }
        heightMap.SetPixels(colors);
        heightMap.Apply();
        return heightMap;
    }

    public static Color EdgeMinPooling(Color[] colors, int x, int y, int width, int height, int poolSize)
    {
        // pool size can be 1 3 5 7...
        int len = colors.Length;
        int i_s = Mathf.Max(0, x - poolSize / 2);
        int i_e = Mathf.Min(width - 1, x + poolSize / 2);
        int j_s = Mathf.Max(0, y - poolSize / 2);
        int j_e = Mathf.Min(height - 1, y + poolSize / 2);
        Color c = colors[y * width + x];
        for (int i = i_s; i <= i_e; i++)
        {
            for (int j = j_s; j <= j_e; j++)
            {
                Color c2 = colors[j * width + i];
                float min = Mathf.Min(c2.r, c2.g, c2.b, c2.a);
                if (c.r >= 0.999f) c.r = min;
                if (c.g >= 0.999f) c.g = min;
                if (c.b >= 0.999f) c.b = min;
                if (c.a >= 0.999f) c.a = min;
            }
        }
        return c;
    }
}
