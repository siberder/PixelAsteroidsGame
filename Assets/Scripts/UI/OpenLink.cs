using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OpenLink : MonoBehaviour
{
    public string link = "https://";

    public void OpenURL()
    {
        Application.OpenURL(link);
    }
}
