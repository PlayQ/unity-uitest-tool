using UnityEngine;

namespace PlayQ.UITestTools
{
    public static class CoroutineProvider
    {
        private static EmptyMono mono;
        public static MonoBehaviour Mono
        {
            get
            {
                if (!mono)
                {
                    var go = new GameObject();
                    mono = go.AddComponent<EmptyMono>();
                    mono.gameObject.name = "UI_TEST_TOOL_COROUTINE";
                    GameObject.DontDestroyOnLoad(mono);
                }
                return mono;
            }
        }

        private class EmptyMono : MonoBehaviour
        {
            
        }
    }
}