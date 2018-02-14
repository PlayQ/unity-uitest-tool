using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

namespace PlayQ.UITestTools
{
    public static partial class UITestBase
    {
        public static EventSystem FindCurrentEventSystem()
        {
            return GameObject.FindObjectOfType<EventSystem>();
        }
        
        public static void LoadSceneForSetUp(string sceneName)
        {
            SceneManager.LoadScene(sceneName, LoadSceneMode.Additive);
        }

        public static GameObject FindObjectByPixels(float x, float y)
        {
            return UITestUtils.FindObjectByPixels(x, y);
        }

        public static GameObject FindObjectByPercents(float x, float y)
        {
            return UITestUtils.FindObjectByPercents(x, y);
        }
    }
}