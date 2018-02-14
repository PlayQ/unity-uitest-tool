using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace PlayQ.UITestTools
{
    public static partial class Wait
    {
        [ShowInEditor(typeof(WaitForObjectInstantiated), "Wait/ for object instantiated")]
        public static IEnumerator ObjectInstantiated(string path, float waitTimeout = 2f)
        {
            yield return WaitForObjectInstantiated.Wait(path, waitTimeout);
        }
        
        private static class WaitForObjectInstantiated
        {
            public static IEnumerator Wait(string path, float waitTimeout = 2f)
            {
                yield return WaitFor(() =>
                {
                    var o = UITestUtils.FindAnyGameObject(path);
                    return o != null;

                }, waitTimeout, "WaitForObject path: " + path);
            }

            public static List<object> GetDefautParams(GameObject go)
            {
                List<object> result = new List<object>();
                result.Add(20f);
                return result;              
            }
        }
        
        [ShowInEditor(typeof(WaitObjectEnabledWithDelay), "Wait/ for object enabled with delay")]
        public static IEnumerator ObjectEnabledInstantiatedAndDelay(string path, float delay, float waitTimeout = 2f)
        {
            yield return WaitObjectEnabledWithDelay.Wait(path, delay, waitTimeout);
        } 
        
        
        public static IEnumerator ObjectEnabledInstantiatedAndDelayIfPossible(string path, float delay, float waitTimeout = 2f)
        {
            yield return WaitObjectEnabledWithDelay.WaitIfPossible(path, delay, waitTimeout);
        }
        
        public static class WaitObjectEnabledWithDelay
        {
            public static IEnumerator WaitIfPossible(string path, float delay, float waitTimeout = 2f)
            {
                yield return WaitFor(() =>
                {
                    var obj = GameObject.Find(path);
                    return obj != null && obj.activeInHierarchy;

                }, waitTimeout, "WaitObjectEnabled path: " + path, true);

                yield return new WaitForSeconds(delay);
                
            }
            
            public static IEnumerator Wait(string path, float delay, float waitTimeout = 2f)
            {
                yield return WaitFor(() =>
                {
                    var obj = GameObject.Find(path); //finds only enabled gameobjects
                    return obj != null && obj.activeInHierarchy;

                }, waitTimeout, "WaitObjectEnabled path: " + path);

                yield return new WaitForSeconds(delay);
                
            }

            public static List<object> GetDefautParams(GameObject go)
            {
                List<object> result = new List<object>();
                result.Add(1f);
                result.Add(20f);
                return result;              
            }
        }
        
        
        private static class WaitForSecondClass
        {
            public static List<object> GetDefautParams(GameObject go)
            {
                List<object> result = new List<object>();
                result.Add(1f);
                return result;              
            }
        }
        
        private static class WaitForFrameClass
        {
            public static List<object> GetDefautParams(GameObject go)
            {
                List<object> result = new List<object>();
                result.Add(1);
                return result;              
            }
        }
        
        [ShowInEditor(typeof(WaitForFrameClass), "Wait/ for frame", false)]
        public static IEnumerator Frame(int count = 1)
        {
            while (count > 0)
            {
                yield return null;
                count--;
            }
        }

        public static IEnumerator WaitFor(Func<bool> condition, float timeout, string testInfo, bool dontFail = false)
        {
            float time = 0;
            while (!condition())
            {
                time += Time.unscaledDeltaTime;
                if (time > timeout)
                {
                    if (dontFail)
                    {
                        yield break;
                    }
                    else
                    {
                        throw new Exception("Operation timed out: " + testInfo);   
                    }
                }
                yield return null;
            }
        }

        public static IEnumerator ObjectInstantiated<T>(float waitTimeout = 2f) where T : Component
        {
            yield return WaitFor(() =>
            {
                var obj = UITestUtils.FindAnyGameObject<T>();
                return obj != null;

            }, waitTimeout, "WaitForObject<" + typeof(T) + ">");
        }

        public static IEnumerator ObjectInstantiated<T>(string path, float waitTimeout = 2f) where T : Component
        {
            yield return WaitFor(() =>
            {
                var obj = UITestUtils.FindAnyGameObject<T>(path);
                return obj != null;
            }, waitTimeout, "WaitForObject<" + typeof(T) + ">");
        }
        
        public static IEnumerator ObjectDestroy<T>(float waitTimeout = 2f) where T : Component
        {
            yield return WaitFor(() =>
            {
                var obj = UITestUtils.FindAnyGameObject<T>();
                return obj == null;

            }, waitTimeout, "WaitForDestroy<" + typeof(T) + ">");
        }

        private static class WaitForObjectEnableOrDestroy
        {
            public static List<object> GetDefautParams(GameObject go)
            {
                List<object> result = new List<object>();
                result.Add(20f);
                return result;
            }
        }
        
        [ShowInEditor(typeof(WaitForObjectEnableOrDestroy), "Wait/ for object destroy")]
        public static IEnumerator ObjectDestroy(string path, float waitTimeout = 2f)
        {
            yield return WaitFor(() =>
            {
                var gameObj = UITestUtils.FindAnyGameObject(path);
                return gameObj == null;

            }, waitTimeout, "WaitForDestroy path: " + path);
        }

        public static IEnumerator ObjectDestroy(GameObject gameObject, float waitTimeout = 2f)
        {
            yield return WaitFor(() =>
            {
                return gameObject == null;
            }, waitTimeout, "WaitForDestroy "+ (gameObject != null ? gameObject.GetType().ToString() : ""));
        }

        public static IEnumerator Condition(Func<bool> func, float waitTimeout = 2f)
        {
            yield return WaitFor(func, waitTimeout, "WaitForCondition");
        }

        public static IEnumerator ButtonAccessible(GameObject button, float waitTimeout = 2f)
        {
            yield return WaitFor(() =>
            {
                return button != null && button.GetComponent<Button>() != null;

            }, waitTimeout, "ButtonAccessible " + button.GetType());
        }

        [ShowInEditor(typeof(WaitForObjectEnableOrDestroy), "Wait/ for object enabled")]
        public static IEnumerator ObjectEnabled(string path, float waitTimeout = 2f)
        {
            yield return WaitFor(() =>
            {
                var obj = GameObject.Find(path);
                return obj != null && obj.activeInHierarchy;

            }, waitTimeout, "WaitObjectEnabled path: " + path);
        }
        
        public static IEnumerator ObjectEnabled<T>(float waitTimeout = 2f) where T : Component
        {
            yield return WaitFor(() =>
            {
                var obj = Object.FindObjectOfType<T>();
                return obj != null;

            }, waitTimeout, "WaitObjectEnabled<" + typeof(T) + ">");
        }

        public static IEnumerator ObjectDisabled<T>(float waitTimeout = 2f) where T : Component
        {
            yield return WaitFor(() =>
            {
                var obj = UITestUtils.FindAnyGameObject<T>();
                return obj != null && !obj.gameObject.activeInHierarchy;

            }, waitTimeout, "WaitObjectDisabled<" + typeof(T) + ">");
        }
        
        [ShowInEditor(typeof(WaitForObjectEnableOrDestroy), "Wait/ for object disabled")]
        public static IEnumerator ObjectDisabled(string path, float waitTimeout = 2f)
        {
            yield return WaitFor(() =>
            {
                var obj = UITestUtils.FindAnyGameObject(path);
                return obj != null && !obj.gameObject.activeInHierarchy;

            }, waitTimeout, "WaitObjectDisabled path: " + path);
        }

        [ShowInEditor(typeof(WaitForSecondClass), "Wait/ for second", false)]
        public static IEnumerator Seconds(float seconds)
        {
            yield return new WaitForSeconds(seconds);
        }
        
        public static IEnumerator SceneLoaded(string sceneName, float waitTimeout = 2f)
        {
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

                return loadedScenes.Any(scene => scene.name == sceneName);
            }, waitTimeout, "WhaitSceneLeaded name: " + sceneName);
        }
    }
}