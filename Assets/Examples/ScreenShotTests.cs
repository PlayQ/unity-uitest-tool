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
            
            PermittedErrors.ClearAll();
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
        [EditorResolution(750, 1334)]
        public IEnumerator TestMakeScreenShotAndCompare()
        {
            yield return Interact.MakeScreenShotReference("reference");
            yield return Interact.MakeScreenshotAndCompare("test", "reference");
        }
        
        [UnityTest]
        [EditorResolution(750, 1334)]
        public IEnumerator TestMakeScreenshotDontFail()
        {
            string expectedWarning = "Screenshot comparing fail was ignored: "+ 
                                   "can't find reference screen shot with path: ReferenceScreenshots/resolution_750_1334/TestMakeScreenshotDontFail/reference to compare it with screen shot: test";
            var waiter = AsyncWait.StartWaitingForLog(expectedWarning,
                LogType.Warning);
            yield return Interact.MakeScreenshotAndCompare("test", "reference", 0.9f,
                true);
            yield return waiter;
            
            string expectedError = "Screenshot equals failed: \n"+ 
                                     "can't find reference screen shot with path: ReferenceScreenshots/resolution_750_1334/TestMakeScreenshotDontFail/reference to compare it with screen shot: test";
            PermittedErrors.Add(expectedError);
            waiter = AsyncWait.StartWaitingForLog(expectedError, LogType.Error);
            Interact.FailIfScreenShotsNotEquals();
            yield return waiter;
        }


//        public class TestClass : ScreenShotTests
//        {
//            [UnityTest]
//            public IEnumerator Test()
//            {
//                yield return null;
//            }   
//        }
      
    }
}