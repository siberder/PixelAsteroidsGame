using System.Collections;
using JetBrains.Annotations;
using UnityEngine;

namespace Utils
{
    [PublicAPI]
    public class CoroutinesRunner : MonoBehaviour
    {
        private static MonoBehaviour runner;

        public static MonoBehaviour Runner
        {
            get
            {
                if (!runner)
                {
                    runner =
                        new GameObject(nameof(CoroutinesRunner)).AddComponent<CoroutinesRunner>();
                }

                return runner;
            }
        }
    }
}