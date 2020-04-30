using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Extensions
{
    public static Bounds OrthographicBounds(this Camera camera)
    {
        float screenAspect = (float)Screen.width / (float)Screen.height;
        float cameraHeight = camera.orthographicSize * 2;
        Bounds bounds = new Bounds(
            camera.transform.position,
            new Vector3(cameraHeight * screenAspect, cameraHeight, 0));
        return bounds;
    }

    public static void SetAlpha(this SpriteRenderer sprite, float a)
    {
        var color = sprite.color;
        color.a = a;
        sprite.color = color;
    }
}
