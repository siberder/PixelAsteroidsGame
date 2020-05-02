using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OpenLink : MonoBehaviour
{
    public string link = "https://";

    public void OpenURL()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        Application.ExternalEval($"window.open(\"{link}\",\"_blank\")");
#else
        Application.OpenURL(link);
#endif
    }
}
