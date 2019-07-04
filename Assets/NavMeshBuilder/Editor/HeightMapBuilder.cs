using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using UnityEditor.SceneManagement;

public class HeightMapBuilder
{
    public static Texture2D Create(Bounds aabb, Collider collider, int scale = 10, int maxSize = 1024, bool horizontalFlip = true, bool verticalFlip = true)
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
                    heightMap.SetPixel(texWidth - 1 - i, texHeight - 1 - j, c);
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
                    heightMap.SetPixel(texWidth - 1 - i, j, c);
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
                    heightMap.SetPixel(i, texHeight - 1 - j, c);
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
                    heightMap.SetPixel(i, j, c);
                }
            }
        }
        heightMap.Apply();
        return heightMap;
    }
}
