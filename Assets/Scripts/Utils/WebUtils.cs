using System.Collections;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Networking;
using System.Web;

namespace Utils
{
    [PublicAPI]
    public class WebUtils
    {
        public static string GetQueryArg(string key, object value)
        {
            return
                $"{UnityWebRequest.EscapeURL(key)}={UnityWebRequest.EscapeURL(value.ToString())}";
        }

        public static Coroutine SendGetRequest<T>(string url, System.Action<T> onDone = null)
        {
            return CoroutinesRunner.Runner.StartCoroutine(SendGetRequestCoroutine(url, onDone));
        }

        private static IEnumerator SendGetRequestCoroutine<T>(string url,
            System.Action<T> onDone = null)
        {
            using (var request = UnityWebRequest.Get(url))
            {
                yield return request.SendWebRequest();

                if (request.IsSuccess())
                {
                    onDone?.Invoke(ProcessResponse<T>(request.downloadHandler.text));
                }
                else
                {
                    HandleUnsuccessfulRequest(request);
                    onDone?.Invoke(default);
                }
            }
        }

        public static Coroutine SendPostRequest<T>(string url, object body,
            System.Action<T> onDone = null)
        {
            return CoroutinesRunner.Runner.StartCoroutine(
                SendPostRequestCoroutine(url, body, onDone));
        }

        private static IEnumerator SendPostRequestCoroutine<T>(string url, object body,
            System.Action<T> onDone = null)
        {
            var data = JsonUtility.ToJson(body);
            using (var request = new UnityWebRequest(url, "POST"))
            {
                byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(data);
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");

                yield return request.SendWebRequest();

                if (request.IsSuccess())
                {
                    onDone?.Invoke(ProcessResponse<T>(request.downloadHandler.text));
                }
                else
                {
                    HandleUnsuccessfulRequest(request);
                    onDone?.Invoke(default);
                }
            }
        }

        private static T ProcessResponse<T>(string text)
        {
            if (GameManager.Instance.debugMode)
            {
                Debug.Log($"Response: {text}");
            }

            try
            {
                var result = JsonUtility.FromJson<T>(text);
                return result;
            }
            catch (System.Exception e)
            {
                Debug.LogError("Error while processing response");
                Debug.LogException(e);
                return default;
            }
        }

        private static void HandleUnsuccessfulRequest(UnityWebRequest request)
        {
            if (GameManager.Instance.debugMode)
            {
                Debug.LogError("Unsuccessful request!\n" +
                               $"URL: {request.url}\n" +
                               $"Response code: {request.responseCode}\n" +
                               $"Response body: {request.downloadHandler.text}");
            }
        }

        public static void StopRequestCoroutine(Coroutine coroutine)
        {
            if (coroutine == null) return;
            CoroutinesRunner.Runner.StopCoroutine(coroutine);
        }
    }
}