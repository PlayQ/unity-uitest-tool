using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PlayQ.UITestTools
{
    public class ScreenShotTests
    {
        private bool sceneUnloaded;
        
        [SetUp]
        public IEnumerator LoadSceneSetUp()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;

            sceneLoaded = false;

            SceneManager.LoadScene("1", LoadSceneMode.Additive);

            while (!sceneLoaded)
            {
                yield return null;
            }
        }

        void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            sceneLoaded = true;

            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private bool sceneLoaded;

        [TearDown]
        public IEnumerator UnloadSceneTearDown()
        {
            SceneManager.sceneUnloaded += OnSceneUnloaded;

            sceneUnloaded = false;

            SceneManager.UnloadSceneAsync("1");

            while (!sceneUnloaded)
            {
                yield return null;
            }
        }

        void OnSceneUnloaded(Scene scene)
        {
            sceneUnloaded = true;

            SceneManager.sceneUnloaded -= OnSceneUnloaded;
        }
        
        [UnityTest]
        public IEnumerator TestMakeScreenShot()
        {
            yield return Interact.MakeScreenShot("test_screen");
        }
        
        [UnityTest]
        public IEnumerator TestMakeScreenShotReference()
        {
            yield return Interact.MakeScreenShotReference("reference");
        }
        
        [UnityTest]
        public IEnumerator TestMakeScreenShotAndCompare()
        {
            yield return Interact.MakeScreenShotReference("reference");
            yield return Interact.MakeScreenshotAndCompare("test", "reference");
        }
      
    }
}