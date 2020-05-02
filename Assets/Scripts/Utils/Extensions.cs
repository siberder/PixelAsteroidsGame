using UnityEngine;
using UnityEngine.Networking;

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

    public static bool IsSuccess(this UnityWebRequest request)
    {
        if (request.isNetworkError) { return false; }

        if (request.responseCode == 0) { return true; }
        if (request.responseCode == (long)System.Net.HttpStatusCode.OK) { return true; }

        return false;
    }
}
