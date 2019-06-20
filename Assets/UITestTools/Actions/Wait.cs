using System;
using System.Collections;
using System.Linq;
using PlayQ.UITestTools.WaitResults;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace PlayQ.UITestTools
{
    /// <summary>
    /// Contains methods which allow waiting until the game components and objects on the scene reach the certain state
    /// </summary>
    public static partial class Wait
    {
        /// <summary>
        /// Awaits for 'GameObject' to become present on scene or fails after specified timeout
        /// </summary>
        /// <param name="path">Path to 'GameObject' in hierarchy</param>
        /// <param name="timeout">Timeout (optional, default = 2)</param>
        /// <param name="ignoreTimeScale">Should time scale be ignored or not (optional, default = false)</param>
        [ShowInEditor(typeof(WaitForObjectInstantiated), "Wait/For Object Instantiated")]
        public static IEnumerator ObjectInstantiated(string path, float timeout = 5, bool ignoreTimeScale = false)
        {
            yield return WaitFor(() =>
            {
                var go = UITestUtils.FindAnyGameObject(path);
                if (go)
                {
                    return new WaitSuccess();
                }
                return new WaitFailed("WaitForObject path: " + path);
            }, timeout, ignoreTimeScale: ignoreTimeScale);
        }

        private class WaitForObjectInstantiated : ShowHelperBase
        {
            public override AbstractGenerator CreateGenerator(GameObject go)
            {
                return IEnumeratorMethod.Path(go).Float(20f).Bool(false);
            }

            public override bool IsAvailable(GameObject go)
            {
                return go != null;
            }
        }

        /// <summary>
        /// Awaits for 'GameObject' to become enabled and interactible if it is a button
        /// </summary>
        /// <param name="path">Path to 'GameObject' in hierarchy</param>
        /// <param name="timeout">Timeout (optional, default = 2)</param>
        /// <param name="dontFail">Whether the test should fail upon exceeding timeout (optional, default = false)</param>
        /// <param name="ignoreTimeScale">Should time scale be ignored or not (optional, default = false)</param>
        [ShowInEditor(typeof(ObjectEnableAndInteractibleIfButtonClass), "Wait/For Object Enabled And Interactible If Button")]
        public static IEnumerator ObjectEnableAndInteractibleIfButton(string path, float timeout = 2f, bool dontFail = false, bool ignoreTimeScale = false)
        {
            yield return WaitFor(() =>
            {
                var go = UITestUtils.FindEnabledGameObjectByPath(path); //finds only enabled gameobjects
                if (go && go.activeInHierarchy)
                {
                    string overlay = IsOverlayed(go);
                    if (overlay != null)
                    {
                        return new WaitFailed("gameobject object " + path + " isOverlayed by " +
                                              overlay);
                    }

                    if (IsClickable(go))
                    {
                        return new WaitSuccess();
                    }
                    return new WaitFailed("gameobject "+path+" button is not interactable");
                }
                return new WaitFailed("gameobject "+path+" is not " + (go == null ? "present on scene":"active in hierarchy"));
            }, timeout, dontFail, ignoreTimeScale: ignoreTimeScale);
        }

        private static string IsOverlayed(GameObject go)
        {
            var objectWhichOverlays = UITestUtils
                .FindUIObjectWhichOverlayGivenObject(go);
            if (objectWhichOverlays)
            {
                return UITestUtils.GetGameObjectFullPath(objectWhichOverlays);
            }
            else
            {
                return null;
            }
                
        }
        private static bool IsClickable(GameObject go)
        {
            var selectables = go.GetComponents<Selectable>();
            if (selectables != null && selectables.Length > 0)
            {
                var allInteractable = selectables.All(selectable => selectable.interactable);
                if (allInteractable)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return true;
            }
        }
        
        /// <summary>
        /// Awaits for 'GameObject' to become enabled and interactible if it is a button
        /// </summary>
        /// <param name="go">'GameObject' of button</param>
        /// <param name="timeout">Timeout (optional, default = 2)</param>
        /// <param name="dontFail">Whether the test should fail upon exceeding timeout (optional, default = false)</param>
        /// <param name="ignoreTimeScale">Should time scale be ignored or not (optional, default = false)</param>
        public static IEnumerator ObjectEnableAndInteractibleIfButton(GameObject go, float timeout = 2f,
            bool dontFail = false, bool ignoreTimeScale = false)
        {
            string path = UITestUtils.GetGameObjectFullPath(go);
            yield return WaitFor(() =>
            {
                if (go.activeInHierarchy)
                {
                    string overlay = IsOverlayed(go);
                    if (overlay != null)
                    {
                        return new WaitFailed("gameobject object " + path + " isOverlayed by " +
                                              overlay);
                    }

                    if (IsClickable(go))
                    {
                        return new WaitSuccess();
                    }
                    return new WaitFailed("gameobject "+path+" button is not interactable");
                }

                return new WaitFailed("gameobject " + path + " is not " +
                                      (go == null ? "present on scene" : "active in hierarchy"));
            }, timeout, dontFail, ignoreTimeScale: ignoreTimeScale);
        }

        private class ObjectEnableAndInteractibleIfButtonClass : ShowHelperBase
        {
            public override AbstractGenerator CreateGenerator(GameObject go)
            {
                return IEnumeratorMethod.Path(go).Float(2f).Bool(false).Bool(false);
            }

            public override bool IsAvailable(GameObject go)
            {
                return go != null;
            }
        }

        /// <summary>
        /// Awaits for 'GameObject' to become present on scene and active in hierarchy. Then waits during given amount of time and returns after that. Method fails after exceeding the given timeout
        /// </summary>
        /// <param name="path">Path to 'GameObject' in hierarchy</param>
        /// <param name="delay">Amount of time to delay</param>
        /// <param name="timeout">Timeout (optional, default = 2)</param>
        /// <param name="dontFail">If true, method will not generate exception after timeout (optional, default = true)</param>
        /// <param name="ignoreTimeScale">Should time scale be ignored or not (optional, default = false)</param>
        [ShowInEditor(typeof(WaitObjectEnabledWithDelay), "Wait/For Object Enabled With Delay")]
        public static IEnumerator ObjectEnabledInstantiatedAndDelay(string path, float delay = 1, float timeout = 5, bool dontFail = true, bool ignoreTimeScale = false)
        {
            yield return WaitFor(() =>
            {
                var obj = UITestUtils.FindEnabledGameObjectByPath(path); //finds only enabled gameobjects
                if (obj != null && obj.activeInHierarchy)
                {
                    return new WaitSuccess();
                }
                return new WaitFailed("WaitObjectEnabled path: " + path);

            }, timeout, dontFail, ignoreTimeScale: ignoreTimeScale);

            yield return new WaitForSeconds(delay);
        }

        private class WaitObjectEnabledWithDelay : ShowHelperBase
        {
            public override AbstractGenerator CreateGenerator(GameObject go)
            {
                return IEnumeratorMethod.Path(go).Float(1f).Float(20f).Bool(true).Bool(false);
            }

            public override bool IsAvailable(GameObject go)
            {
                return go != null;
            }
        }

        private class WaitForSecondClass : ShowHelperBase
        {
            public override AbstractGenerator CreateGenerator(GameObject go)
            {
                return IEnumeratorMethod.Float(1f).Bool(false);
            }
        }
        
        private class WaitForFrameClass : ShowHelperBase
        {
            public override AbstractGenerator CreateGenerator(GameObject go)
            {
                return IEnumeratorMethod.Int(1);
            }
        }

        /// <summary>
        /// Waits for given amount of frames, then return
        /// </summary>
        /// <param name="count">Amount of frames to wait (optional, default = 1)</param>
        [ShowInEditor(typeof(WaitForFrameClass), "Wait/For Frame", false)]
        public static IEnumerator Frame(int count = 1)
        {
            while (count > 0)
            {
                yield return null;
                count--;
            }
        }

        /// <summary>
        /// Waits until given predicate returns true or fails after specified timeout
        /// </summary>
        /// <param name="condition">Predicate that return true, if its condition is successfuly fulfilled</param>
        /// <param name="timeout">Timeout</param>
        /// <param name="testInfo"> This label would be passed to logs if method fails</param>
        /// <param name="dontFail">If true, method will not generate exception after timeout (optional, default = false)</param>
        /// <param name="ignoreTimeScale">Should time scale be ignored or not (optional, default = false)</param>
        /// <exception cref="Exception"></exception>
        public static IEnumerator WaitFor(Func<WaitResult> condition, float timeout, bool dontFail = false, bool ignoreTimeScale = false)
        {
            float time = 0;
            while (true)
            {
                WaitResult waitResult = condition();
                time += ignoreTimeScale
                    ? Time.unscaledDeltaTime
                    : Time.deltaTime;

                if (waitResult is WaitFailed && time > timeout)
                {
                    if (dontFail)
                    {
                        yield break;
                    }
                    else
                    {
                        throw new Exception("Operation timed out: " + waitResult);
                    }
                }

                if (waitResult is WaitSuccess)
                {
                    yield break;
                }
                yield return null;
            }
        }


        /// <summary>
        /// Waits until given predicate returns true or fails after specified timeout
        /// </summary>
        /// <param name="condition">Predicate that return true, if its condition is successfuly fulfilled</param>
        /// <param name="timeout">Timeout</param>
        /// <param name="testInfo"> This label would be passed to logs if method fails</param>
        /// <param name="dontFail">If true, method will not generate exception after timeout (optional, default = false)</param>
        /// <param name="ignoreTimeScale">Should time scale be ignored or not (optional, default = false)</param>
        /// <exception cref="Exception"></exception>
        public static IEnumerator WaitFor(Func<bool> condition, float timeout, string message = "No message", bool ignoreTimeScale = false)
        {
            float time = 0;
            while (true)
            {
                if (condition())
                {
                    yield break;
                }
                time += ignoreTimeScale
                    ? Time.unscaledDeltaTime
                    : Time.deltaTime;

                if (time > timeout)
                {
                    throw new Exception("Operation WaitFor timed out: " + message);
                }
                yield return null;
            }
        }

        /// <summary>
        /// Waits until 'GameObject' with component 'T' becomes present on scene or fails after specified timeout
        /// </summary>
        /// <param name="timeout">Timeout (optional, default = 2)</param>
        /// <param name="ignoreTimeScale">Should time scale be ignored or not (optional, default = false)</param>
        /// <typeparam name="T">Type of component</typeparam>
        public static IEnumerator ObjectInstantiated<T>(float timeout = 2f, bool ignoreTimeScale = false) where T : Component
        {
            yield return WaitFor(() =>
            {
                var obj = UITestUtils.FindAnyGameObject<T>();
                if (obj != null)
                {
                    return new WaitSuccess();
                }
                return new WaitFailed("WaitForObject<" + typeof(T) + ">");
            }, timeout, ignoreTimeScale: ignoreTimeScale);
        }

        /// <summary>
        /// Waits until 'GameObject' with component 'T' becomes present on scene or fails after specified timeout
        /// </summary>
        /// <param name="path">Path to 'GameObject' in hierarchy</param>
        /// <param name="timeout">Timeout (optional, default = 2)</param>
        /// <param name="ignoreTimeScale">Should time scale be ignored or not (optional, default = false)</param>
        /// <typeparam name="T">Type of component</typeparam>
        public static IEnumerator ObjectInstantiated<T>(string path, float timeout = 2f, bool ignoreTimeScale = false) where T : Component
        {
            yield return WaitFor(() =>
            {
                var obj = UITestUtils.FindAnyGameObject<T>(path);
                if (obj != null)
                {
                    return new WaitSuccess();
                }
                return new WaitFailed("WaitForObject<" + typeof(T) + ">");
            }, timeout, ignoreTimeScale: ignoreTimeScale);
        }

        /// <summary>
        /// Waits until 'GameObject' with component 'T' disappears from scene or fails after specified timeout
        /// </summary>
        /// <param name="timeout">Timeout (optional, default = 2)</param>
        /// <param name="ignoreTimeScale">Should time scale be ignored or not (optional, default = false)</param>
        /// <typeparam name="T">Type of component</typeparam>
        public static IEnumerator ObjectDestroyed<T>(float timeout = 2f, bool ignoreTimeScale = false) where T : Component
        {
            yield return WaitFor(() =>
            {
                var obj = UITestUtils.FindAnyGameObject<T>();
                if (obj == null)
                {
                    return new WaitSuccess();
                }
                return new WaitFailed("WaitForDestroy<" + typeof(T) + ">");

            }, timeout, ignoreTimeScale: ignoreTimeScale);
        }

        private class WaitForObjectEnableOrDestroyed : ShowHelperBase
        {
            public override AbstractGenerator CreateGenerator(GameObject go)
            {
                return IEnumeratorMethod.Path(go).Float(20f).Bool(false);
            }

            public override bool IsAvailable(GameObject go)
            {
                return go != null;
            }
        }

        /// <summary>
        /// Waits until 'GameObject' by given path disappears from scene or fails after specified timeout
        /// </summary>
        /// <param name="path">Path to `GameObject` in hierarchy</param>
        /// <param name="timeout">Timeout (optional, default = 2)</param>
        /// <param name="ignoreTimeScale">Should time scale be ignored or not (optional, default = false)</param>
        [ShowInEditor(typeof(WaitForObjectEnableOrDestroyed), "Wait/For Object Destroy")]
        public static IEnumerator ObjectDestroyed(string path, float timeout = 2f, bool ignoreTimeScale = false)
        {
            yield return WaitFor(() =>
            {
                var gameObj = UITestUtils.FindAnyGameObject(path);
                if (gameObj == null)
                {
                    return new WaitSuccess();
                }
                return new WaitFailed("WaitForDestroy path: " + path);

            }, timeout, ignoreTimeScale: ignoreTimeScale);
        }

        /// <summary>
        /// Waits until 'GameObject' by given path disappears from scene or fails after specified timeout
        /// </summary>
        /// <param name="gameObject">`GameObject` who should be destroyed</param>
        /// <param name="timeout">Timeout (optional, default = 2)</param>
        /// <param name="ignoreTimeScale">Should time scale be ignored or not (optional, default = false)</param>
        public static IEnumerator ObjectDestroyed(GameObject gameObject, float timeout = 2f, bool ignoreTimeScale = false)
        {
            yield return WaitFor(() =>
            {
                if (gameObject == null)
                {
                    return new WaitSuccess();
                }
                return new WaitFailed("WaitForDestroy "+ (gameObject != null ? gameObject.GetType().ToString() : ""));
            }, timeout, ignoreTimeScale: ignoreTimeScale);
        }

        /// <summary>
        /// Waits until given 'GameObject' obtains component 'UnityEngine.UI.Button' or fails after specified timeout
        /// </summary>
        /// <param name="button">'GameObject' who should be start accessible</param>
        /// <param name="timeout">Timeout (optional, default = 2)</param>
        /// <param name="ignoreTimeScale">Should time scale be ignored or not (optional, default = false)</param>
        public static IEnumerator ButtonAccessible(GameObject button, float timeout = 2f, bool ignoreTimeScale = false)
        {
            yield return WaitFor(() =>
            {
                if (button != null && button.GetComponent<Button>() != null)
                {
                    return new WaitSuccess();
                }
                return new WaitFailed("ButtonAccessible " + button.GetType());
            }, timeout, ignoreTimeScale: ignoreTimeScale);
        }

        /// <summary>
        /// Waits until 'GameObject' by given path becomes present on scene and active in hierarchy or fails after specified timeout
        /// </summary>
        /// <param name="path">Path to 'GameObject' in hierarchy</param>
        /// <param name="timeout">Timeout (optional, default = 2)</param>
        /// <param name="ignoreTimeScale">Should time scale be ignored or not (optional, default = false)</param>
        [ShowInEditor(typeof(WaitForObjectEnableOrDestroyed), "Wait/For Object Enabled")]
        public static IEnumerator ObjectEnabled(string path, float timeout = 5, bool ignoreTimeScale = false)
        {
            yield return WaitFor(() =>
            {
                var obj = UITestUtils.FindEnabledGameObjectByPath(path);
                if (obj != null && obj.activeInHierarchy)
                {
                    return new WaitSuccess();
                }

                return new WaitFailed("WaitObjectEnabled path: " + path);
            }, timeout, ignoreTimeScale: ignoreTimeScale);
        }

        /// <summary>
        /// Waits until 'GameObject' with component 'T' becomes present on scene and active in hierarchy or fails after specified timeout
        /// </summary>
        /// <param name="timeout">Timeout (optional, default = 2)</param>
        /// <param name="ignoreTimeScale">Should time scale be ignored or not (optional, default = false)</param>
        /// <typeparam name="T">Type of component</typeparam>
        public static IEnumerator ObjectEnabled<T>(float timeout = 2f, bool ignoreTimeScale = false) where T : Component
        {
            yield return WaitFor(() =>
            {
                var obj = Object.FindObjectOfType<T>();
                if (obj != null)
                {
                    return new WaitSuccess();
                }
                return new WaitFailed("WaitObjectEnabled<" + typeof(T) + ">");

            }, timeout, ignoreTimeScale: ignoreTimeScale);
        }

        /// <summary>
        /// Waits until 'GameObject' with component 'T' becomes present on scene and disabled in hierarchy or fails after specified timeout
        /// </summary>
        /// <param name="timeout">Timeout (optional, default = 2)</param>
        /// <param name="ignoreTimeScale">Should time scale be ignored or not (optional, default = false)</param>
        /// <typeparam name="T">Type of component</typeparam>
        public static IEnumerator ObjectDisabled<T>(float timeout = 2f, bool ignoreTimeScale = false) where T : Component
        {
            yield return WaitFor(() =>
            {
                var obj = UITestUtils.FindAnyGameObject<T>();
                if (obj != null && !obj.gameObject.activeInHierarchy)
                {
                    return new WaitSuccess();
                }
                return new WaitFailed("WaitObjectDisabled<" + typeof(T) + ">");

            }, timeout, ignoreTimeScale: ignoreTimeScale);
        }

        /// <summary>
        /// Waits until 'GameObject' by given path becomes present on scene and disabled in hierarchy or fails after specified timeout
        /// </summary>
        /// <param name="path">Path to 'GameObject' in hierarchy</param>
        /// <param name="timeout">Timeout (optional, default = 2)</param>
        /// <param name="ignoreTimeScale">Should time scale be ignored or not (optional, default = false)</param>
        [ShowInEditor(typeof(WaitForObjectEnableOrDestroyed), "Wait/For Object Disabled")]
        public static IEnumerator ObjectDisabled(string path, float timeout = 5, bool ignoreTimeScale = false)
        {
            yield return WaitFor(() =>
            {
                var obj = UITestUtils.FindAnyGameObject(path);
                if (obj != null && !obj.gameObject.activeInHierarchy)
                {
                    return new WaitSuccess();
                }
                return new WaitFailed("WaitObjectDisabled path: " + path);

            }, timeout, ignoreTimeScale: ignoreTimeScale);
        }

        /// <summary>
        /// Waits until 'GameObject' by given path disappears from scene or becomes disabled in hierarchy or fails after specified timeout
        /// </summary>
        /// <param name="path">Path to 'GameObject' in hierarchy</param>
        /// <param name="timeout">Timeout (optional, default = 2)</param>
        /// <param name="ignoreTimeScale">Should time scale be ignored or not (optional, default = false)</param>
        [ShowInEditor(typeof(WaitForObjectEnableOrDestroyed), "Wait/For Object Disabled Or Not Exist")]
        public static IEnumerator ObjectIsDisabledOrDoesNotExist(string path, float timeout = 5, bool ignoreTimeScale = false)
        {
            yield return WaitFor(() =>
                {
                    var obj = UITestUtils.FindAnyGameObject(path);
                    if (!obj)
                    {
                        return new WaitSuccess();
                    }

                    if (!obj.gameObject.activeInHierarchy)
                    {
                        return new WaitSuccess();
                    }
                    return new WaitFailed("WaitObjectDisabled path: " + path);

                }, timeout, ignoreTimeScale: ignoreTimeScale);
        }

        /// <summary>
        /// Waits for specified amount of seconds
        /// </summary>
        /// <param name="seconds">Count of seconds to wait</param>
        /// <param name="ignoreTimescale">Should time scale be ignored or not (optional, default = false)</param>
        [ShowInEditor(typeof(WaitForSecondClass), "Wait/For Second", false)]
        public static IEnumerator Seconds(float seconds, bool ignoreTimescale = false)
        {
            if (ignoreTimescale)
            {
                yield return new WaitForSecondsRealtime(seconds);
            }
            else
            {
                yield return new WaitForSeconds(seconds);
            }
        }

        /// <summary>
        /// Waits until scene with given name is loaded or fails after specified timeout
        /// </summary>
        /// <param name="sceneName">Name of scene to load</param>
        /// <param name="timeout">Timeout (optional, default = 2)</param>
        /// <param name="ignoreTimeScale">Should time scale be ignored or not (optional, default = false)</param>
        //todo add ShowInEditor 
        public static IEnumerator SceneLeaded(string sceneName, float timeout = 2f, bool ignoreTimeScale = false)
        {
            yield return WaitFor(() =>
            {
                int sceneCount = SceneManager.sceneCount;
                if (sceneCount == 0)
                {
                    return new WaitFailed("sceneName: "+sceneName+" scenes count 0");
                }

                var loadedScenes = new Scene[sceneCount];
                for (int i = 0; i < sceneCount; i++)
                {
                    loadedScenes[i] = SceneManager.GetSceneAt(i);
                }

                if (loadedScenes.Any(scene => scene.name == sceneName))
                {
                    return new WaitSuccess();
                }
                return new WaitFailed("WhaitSceneLeaded name: " + sceneName);
            }, timeout, ignoreTimeScale: ignoreTimeScale);
        }

        /// <summary>
        /// Waits until animation is completed
        /// </summary>
        /// <param name="path">Path to 'GameObject' in hierarchy</param>
        /// <param name="animationName">Animation name</param>
        /// <param name="timeout">Timeout (optional, default = 10)</param>
        /// <param name="ignoreTimeScale">Should time scale be ignored or not (optional, default = false)</param>
        [ShowInEditor(typeof(WaitingForAnimationCompleteClass), "Wait/Animation Completed", false)]
        public static IEnumerator AnimationCompleted(string path, string animationName, float timeout = 10, bool ignoreTimeScale = false)
        {
            float currentTime = Time.time;
            yield return AsyncWait.StartWaitingForUnityAnimation(path, animationName, timeout);
            float restTime = timeout - (Time.time - currentTime);
            yield return WaitFor(() =>
            {
                var go = UITestUtils.FindEnabledGameObjectByPath(path);
                if (go == null)
                {
                    return new WaitFailed("WaitingForUnityAnimationCompleted: Object not found");
                }
                var animation = go.GetComponent<Animation>();
                if (animation != null)
                {
                    if (animation.IsPlaying(animationName))
                    {
                        return new WaitFailed("WaitingForUnityAnimationCompleted: Animation is played");
                    }
                    else
                    {
                        return new WaitSuccess();
                    }
                }
                else
                {
                    return new WaitFailed("WaitingForUnityAnimationCompleted: Animator not found");
                }
            }, restTime, ignoreTimeScale: ignoreTimeScale);
        }
        public static IEnumerator WaitForObjectScaleChanged(GameObject element, float timeout = 10f)
        {
            yield return WaitFor(() =>
            {
                if (element.transform.localScale != Vector3.one)
                {
                    return new WaitSuccess();
                }
                
                return new WaitFailed("WaitsForElementScaleChanged: " + element.GetType());
            }, timeout);
        }

        private class WaitingForAnimationCompleteClass : ShowHelperBase
        {
            public override AbstractGenerator CreateGenerator(GameObject go)
            {
                var clipName = "no_animation";
                var anim = go.GetComponent<Animation>();
                if (anim)
                {
                    foreach (AnimationState state in anim)
                    {
                        var isPlaying = anim.IsPlaying(state.clip.name);
                        if (isPlaying)
                        {
                            clipName = state.clip.name;
                            break;
                        }
                    }
                }

                return IEnumeratorMethod
                    .Append(new MethodName())
                    .Path(go)
                    .String(clipName)
                    .Float(10)
                    .Bool(false);
            }
            public override bool IsAvailable(GameObject go)
            {
                if (go)
                {
                    var animation = go.GetComponent<Animation>();
                    return animation != null;
                }
                return false;
            }
        }
    }
}