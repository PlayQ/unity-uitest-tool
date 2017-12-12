using UnityEngine;
using System.Collections;
using System;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using NUnit.Framework;
using System.Linq;
using Object = UnityEngine.Object;

namespace PlayQ.UITestTools
{
    public abstract class UITestBase
    {
        protected void LoadSceneForSetUp(string sceneName)
        {
            SceneManager.LoadScene(sceneName, LoadSceneMode.Additive);
        }

        private IEnumerator WaitFor(Func<bool> condition, float timeout, string testInfo)
        {
            float time = 0;
            while (!condition())
            {
                time += Time.unscaledDeltaTime;
                if (time > timeout)
                {
                    throw new Exception("Operation timed out: " + testInfo);
                }
                yield return null;
            }
        }

        [TearDown]
        protected void Clean()
        {
            int sceneCound = SceneManager.sceneCount;
            var loadedScenes = new Scene[sceneCound];

            for (int i = 0; i < sceneCound; i++)
            {
                loadedScenes[i] = SceneManager.GetSceneAt(i);
            }

            foreach (var scene in loadedScenes)
            {
                if (!scene.name.StartsWith("InitTestScene"))
                {
                    //AsyncOperation ao =  UnityEditor.SceneManagement.EditorSceneManager.UnloadSceneAsync(scene);
                    SceneManager.UnloadScene(scene);
                }
            }
        }

        #region Interactions

        protected void Click(GameObject go)
        {
            if (!go.activeInHierarchy)
            {
                Assert.Fail("Trying to click to " + go.name + " but it disabled");
            }
            ExecuteEvents.Execute(go, new PointerEventData(EventSystem.current), ExecuteEvents.pointerClickHandler);
        }

        protected void Click(string path)
        {
            GameObject go = GameObject.Find(path);
            if (go == null)
            {
                Assert.Fail("Trying to click to " + path + " but it doesn't exist");
            }
            Click(go);
        }
        #endregion
        
        #region Waits
        //todo investigate
        protected IEnumerator WaitForAnimationPlaying(string objectName, string param, float waitTimeout = 2f)
        {
            yield return WaitFor(() =>
            {
                GameObject gameObject = GameObject.Find(objectName);
                return gameObject.GetComponent<Animation>().IsPlaying(param);
                
            }, waitTimeout, "WaitForAnimationPlaying " + objectName + "  animating param " + param);
        }

        protected IEnumerator WaitForObject<T>(float waitTimeout = 2f) where T : Component
        {
            yield return WaitFor(() =>
            {
                var obj = Object.FindObjectOfType(typeof(T));
                return obj != null;
                    
            }, waitTimeout, "WaitForObject<"+typeof(T)+">");
        }

        protected IEnumerator WaitForObject(string path, float waitTimeout = 2f)
        {
            yield return WaitFor(() =>
            {
                var o = GameObject.Find(path);
                return o != null;
                    
            }, waitTimeout, "WaitForObject path: " + path);
        }

        protected IEnumerator WaitForDestroy<T>(float waitTimeout = 2f) where T : Component
        {
            yield return WaitFor(() =>
            {
                var obj = Object.FindObjectOfType(typeof(T)) as T;
                return obj == null;
                    
            }, waitTimeout, "WaitForDestroy<"+typeof(T)+">");
        }

        protected IEnumerator WaitForDestroy(string path, float waitTimeout = 2f)
        {
            yield return WaitFor(() =>
            {
                var gameObj = GameObject.Find(path);
                return gameObj == null;
                    
            }, waitTimeout, "WaitForDestroy path: " + path );
        }
        
        
        protected IEnumerator WaitForDestroy(GameObject gameObject, float waitTimeout = 2f)
        {
            yield return WaitFor(() =>
            {
                return gameObject == null;
                    
            }, waitTimeout, "WaitForDestroy "+ (gameObject != null ? gameObject.GetType().ToString() : ""));
        }

        protected IEnumerator WaitForCondition(Func<bool> func, float waitTimeout = 2f)
        {
            yield return WaitFor(func, waitTimeout, "WaitForCondition");
        }
        
        protected IEnumerator ButtonAccessible(GameObject button, float waitTimeout = 2f)
        {
            yield return WaitFor(() =>
            {
                return button != null && button.GetComponent<Button>() != null;
                
            }, waitTimeout, "ButtonAccessible " + button.GetType());
        }
        
        protected IEnumerator WaitObjectEnabled(string path, float waitTimeout = 2f)
        {
            yield return WaitFor(() =>
            {
                var obj = GameObject.Find(path); //finds only enabled gameobjects
                return obj != null;

            }, waitTimeout, "WaitObjectEnabled path: " + path);
        }
        
        protected IEnumerator WaitObjectEnabled<T>(float waitTimeout = 2f) where T : Component
        {
            yield return WaitFor(() =>
            {
                var obj = Object.FindObjectOfType<T>();
                return obj != null && obj.gameObject.activeInHierarchy;

            }, waitTimeout, "WaitObjectEnabled<"+typeof(T)+">");
        }
        
        
        protected IEnumerator WaitObjectDisabled<T>(float waitTimeout = 2f) where T : Component
        {
            yield return WaitFor(() =>
            {
                var obj = Object.FindObjectOfType<T>();
                return obj != null && !obj.gameObject.activeInHierarchy;

            }, waitTimeout, "WaitObjectDisabled<"+typeof(T)+">");
        }
        
        protected IEnumerator WaitObjectDisabled(string path, float waitTimeout = 2f)
        {
            yield return WaitFor(() =>
            {
                var obj = GameObject.Find(path); //finds only enabled gameobjects
                return obj == null;

            }, waitTimeout, "WaitObjectDisabled path: " + path);
        }

        
        protected IEnumerator LoadScene(string sceneName, float waitTimeout = 2f)
        {
            SceneManager.LoadScene(sceneName, LoadSceneMode.Additive);
            
            yield return WaitFor(() =>
            {
                int sceneCount = SceneManager.sceneCount;
                if (sceneCount == 0)
                {
                    return false;
                }

                var loadedScenes = new Scene[sceneCount];
                for (int i = 0; i < sceneCount; i++)
                {
                    loadedScenes[i] = SceneManager.GetSceneAt(i);
                }

                return loadedScenes.Any(scene =>
                {
                    return scene.name == sceneName;
                });
            }, waitTimeout, "LoadScene name: " + sceneName);
        }

        #endregion
    }


    public static class Check
    {
        public static void TextEquals(string path, string expectedText)
        {
            var go = GameObject.Find(path);
            if (go == null)
            {
                Assert.Fail("Label object " + path + " does not exist or disabled");
            }
            
            var t = go.GetComponent<Text>();
            if (t == null)
            {
                Assert.Fail("Label object " + path + " has no Text attached");
            }
            if (t.text != expectedText)
            {
                Assert.Fail("Label " + path + "\n text expected: " + expectedText + ",\n actual: " + t.text);
            }
        }

    }
}