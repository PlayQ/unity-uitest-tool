using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using System.Runtime;
using PlayQ.UITestTools.WaitResults;
using UnityEngine;
using UnityEngine.UI;

namespace PlayQ.UITestTools
{
    /// <summary>
    /// Contains methods which allow checking the state of game components and objects on the scene
    /// </summary>
    public static partial class Check
    {
        #region Text

        /// <summary>
        /// Checks that `GameObject` by given path has `Text` component attached and its variable text is equal to expected text
        /// </summary>
        /// <param name="path">Path to `GameObject` in hierarchy</param>
        /// <param name="expectedText">Expected text</param>
        [ShowInEditor(typeof(CheckTextEquals), "Text Equals")]
        public static void TextEquals(string path, string expectedText)
        {
            var go = UITestUtils.FindAnyGameObject(path);
            Assert.IsNotNull(go, "InputEquals: Object " + path + " is not exist");
            TextEquals(go, expectedText);
        }

        /// <summary>
        /// Checks that `GameObject` by given path has `Text` component attached and its variable text is not equal to expected text
        /// </summary>
        /// <param name="path">Path to `GameObject` in hierarchy</param>
        /// <param name="expectedText">Expected text</param>
        [ShowInEditor(typeof(CheckTextEquals), "Text Not Equals")]
        public static void TextNotEquals(string path, string expectedText)
        {
            var go = UITestUtils.FindAnyGameObject(path);
            Assert.IsNotNull(go, "InputEquals: Object " + path + " is not exist");
            TextNotEquals(go, expectedText);
        }

        private class CheckTextEquals : ShowHelperBase
        {
            public override bool IsAvailable(GameObject go)
            {
                if (!go)
                {
                    return false;
                }
                return go.GetComponent<Text>() != null;
            }

            public override AbstractGenerator CreateGenerator(GameObject go)
            {
                return VoidMethod.Path(go)
                    .String(go.GetComponent<Text>().text);
            }
        }

        /// <summary>
        /// Checks that `GameObject` has `Text` component attached and its variable text is equal to expected text
        /// </summary>
        /// <param name="go">`GameObject` with `Text` component</param>
        /// <param name="expectedText">Expected text</param>
        public static void TextEquals(GameObject go, string expectedText)
        {
            var t = go.GetComponent<Text>();
            if (t == null)
            {
                Assert.Fail("TextEquals: Object " + go.name + " has no Text attached");
            }
            if (t.text != expectedText)
            {
                Assert.Fail("TextEquals: Label " + go.name + " actual text: " + t.text + ", expected " + expectedText +
                            ", text dont't match");
            }
        }

        /// <summary>
        /// Checks that `GameObject` has `Text` component attached and its variable text is not equal to expected text
        /// </summary>
        /// <param name="go">`GameObject` with `Text` component</param>
        /// <param name="expectedText">Expected text</param>
        public static void TextNotEquals(GameObject go, string expectedText)
        {
            var t = go.GetComponent<Text>();
            if (t == null)
            {
                Assert.Fail("TextNotEquals: Object " + go.name + " has no Text attached");
            }
            if (t.text == expectedText)
            {
                Assert.Fail("TextNotEquals: Label " + go.name + " actual text: " + expectedText +
                            " text matches but must not");
            }
        }

        #endregion

        #region Input
        
        /// <summary>
        /// Checks that `GameObject` has `InputField` component attached and its variable text is equal to expected text
        /// </summary>
        /// <param name="go">`GameObject` with `InputField` component</param>
        /// <param name="expectedText">Expected text</param>
        public static void InputEquals(GameObject go, string expectedText)
        {
            var t = UITestUtils.FindComponentInParents<InputField>(go);
            if (t == null)
            {
                Assert.Fail("InputEquals: Object " + go.name + " has no InputField attached");
            }
            if (t.text != expectedText)
            {
                Assert.Fail("InputEquals: InputField " + go.name + " actual text: " + t.text + ", expected " +
                            expectedText + ", text dont't match");
            }
        }

        /// <summary>
        /// Checks that `GameObject` has `InputField` component attached and its variable text is not equal to expected text
        /// </summary>
        /// <param name="go">`GameObject` with `InputField` component</param>
        /// <param name="expectedText">Expected text</param>
        public static void InputNotEquals(GameObject go, string expectedText)
        {
            var t = go.GetComponent<Text>();
            if (t == null)
            {
                Assert.Fail("InputNotEquals: Object " + go.name + " has no Text attached");
            }
            if (t.text == expectedText)
            {
                Assert.Fail("InputNotEquals: InputField " + go.name + " actual text: " + expectedText +
                            " text matches but must not");
            }
        }

        /// <summary>
        /// Checks that `GameObject` by given path has `InputField` component attached and its variable text is equal to expected text
        /// </summary>
        /// <param name="path">Path to `GameObject` in hierarchy</param>
        /// <param name="expectedText">Expected text</param>
        [ShowInEditor(typeof(CheckInputTextEquals), "Input Text Equals")]
        public static void InputEquals(string path, string expectedText)
        {
            var go = UITestUtils.FindAnyGameObject(path);
            Assert.AreEqual(null, go, "InputEquals: Object " + path + " is not exist");
            InputEquals(go, expectedText);
        }

        /// <summary>
        /// Checks that `GameObject` by given path has `InputField` component attached and its variable text is not equal to expected text
        /// </summary>
        /// <param name="path">Path to `GameObject` in hierarchy</param>
        /// <param name="expectedText">Expected text</param>
        [ShowInEditor(typeof(CheckInputTextEquals), "Input Text Not Equals")]
        public static void InputNotEquals(string path, string expectedText)
        {
            var go = UITestUtils.FindAnyGameObject(path);
            Assert.IsNotNull(go, "InputEquals: Object " + path + " is not exist");
            InputNotEquals(go, expectedText);
        }

        private class CheckInputTextEquals : ShowHelperBase
        {
            public override AbstractGenerator CreateGenerator(GameObject go)
            {
                return VoidMethod
                    .Path(go).String(go.GetComponent<InputField>().text);
            }

            public override bool IsAvailable(GameObject go)
            {
                if (!go)
                {
                    return false;
                }
                return go.GetComponent<InputField>() != null;
            }
        }

        #endregion

        #region EnableDisable

        private class IsEnableGenerator : ShowHelperBase
        {
            public override AbstractGenerator CreateGenerator(GameObject go)
            {
                return VoidMethod.Path(go);
            }

            public override bool IsAvailable(GameObject go)
            {
                return go != null;
            }
        }

        /// <summary>
        /// Checks that `Gameobject` by given path is present on scene and active in hierarchy
        /// </summary>
        /// <param name="path">Path to `GameObject` in hierarchy</param>
        [ShowInEditor(typeof(IsEnableGenerator), "Is Enable")]
        public static void IsEnable(string path)
        {
            var go = IsExist(path);

            if (!go.gameObject.activeInHierarchy)
            {
                Assert.Fail("IsEnable: with path " + path + " Game Object disabled");
            }
        }

        /// <summary>
        /// Checks that `Gameobject` by given path is present on scene, contains component `T` and is active in hierarchy
        /// </summary>
        /// <param name="path">Path to `GameObject` in hierarchy</param>
        /// <typeparam name="T">`Type` of object</typeparam>
        public static void IsEnable<T>(string path) where T : Component
        {
            var go = IsExist<T>(path);

            if (!go.gameObject.activeInHierarchy)
            {
                Assert.Fail("IsEnable<" + typeof(T) + ">: with path " + path + " Game Object disabled");
            }
        }

        /// <summary>
        /// Searches for `Gameobject` with component `T` and checks that it is present on scene and active in hierarchy
        /// </summary>
        /// <typeparam name="T">`Type` of object</typeparam>
        public static void IsEnable<T>() where T : Component
        {
            var go = IsExist<T>();

            if (!go.gameObject.activeInHierarchy)
            {
                Assert.Fail("IsEnable<" + typeof(T) + ">: Game Object disabled");
            }
        }

        /// <summary>
        /// Checks that `GameObject` is active in hierarchy
        /// </summary>
        /// <param name="go">`GameObject`</param>
        public static void IsEnable(GameObject go)
        {
            if (!go.activeInHierarchy)
            {
                Assert.Fail("IsEnable by object instance: with path " + UITestUtils.GetGameObjectFullPath(go) +
                            " Game Object disabled");
            }
        }

        private class IsDisableGenerator : ShowHelperBase
        {
            public override AbstractGenerator CreateGenerator(GameObject go)
            {
                return VoidMethod.Path(go);
            }

            public override bool IsAvailable(GameObject go)
            {
                return go != null;
            }
        }

        /// <summary>
        /// Checks that `GameObject` by given path is present on scene and is not active in hierarchy
        /// </summary>
        /// <param name="path">Path to `GameObject` in hierarchy</param>
        [ShowInEditor(typeof(IsDisableGenerator), "Is Disable")]
        public static void IsDisable(string path)
        {
            var go = IsExist(path);

            if (go.gameObject.activeInHierarchy)
            {
                Assert.Fail("IsDisable: with path " + path + " Game Object enabled");
            }
        }

        /// <summary>
        /// Checks that `GameObject` by given path is present on scene, contains component `T` and is not active in hierarchy
        /// </summary>
        /// <param name="path">Path to `GameObject` in hierarchy</param>
        /// <typeparam name="T">`Type` of object</typeparam>
        public static void IsDisable<T>(string path) where T : Component
        {
            IsExist<T>(path);
            var go = UITestUtils.FindAnyGameObject<T>(path);

            if (go.gameObject.activeInHierarchy)
            {
                Assert.Fail("IsDisable<" + typeof(T) + ">: with path " + path + " Game Object enabled");
            }
        }

        /// <summary>
        /// Searches for `Gameobject` with component `T` and checks that it is present on scene and is not active in hierarchy
        /// </summary>
        /// <typeparam name="T">`Type` of object</typeparam>
        public static void IsDisable<T>() where T : Component
        {
            var go = IsExist<T>();

            if (go.gameObject.activeInHierarchy)
            {
                Assert.Fail("IsDisable<" + typeof(T) + ">: Game Object enabled");
            }
        }

        /// <summary>
        /// Checks that `GameObject` is not active in hierarchy
        /// </summary>
        /// <param name="go">`GameObject`</param>
        public static void IsDisable(GameObject go)
        {
            if (go.activeInHierarchy)
            {
                Assert.Fail("IsDisable by object instance: Game Object " + UITestUtils.GetGameObjectFullPath(go) +
                            " enabled");
            }
        }

        /// <summary>
        /// Checks that `Gameobject` by given path is present on scene and its active in hierarchy flag equals to state variable
        /// </summary>
        /// <param name="path">Path to `GameObject` in hierarchy</param>
        /// <param name="state">Enable state (optional)</param>
        [ShowInEditor(typeof(CheckEnabledState), "Check Enabled State")]
        public static void CheckEnabled(string path, bool state = true)
        {
            var go = IsExist(path);
            Assert.AreEqual(state, go.gameObject.activeInHierarchy, "CheckEnabled: object with path " + path + " is " +
                                                                    (go.gameObject.activeInHierarchy
                                                                        ? "enabled"
                                                                        : "disabled") +
                                                                    " but expected: " +
                                                                    (state ? "enabled" : "disabled"));
        }

        private class CheckEnabledState : ShowHelperBase
        {
            public override AbstractGenerator CreateGenerator(GameObject go)
            {
                return VoidMethod.Path(go).Bool(go.activeInHierarchy);
            }

            public override bool IsAvailable(GameObject go)
            {
                return go != null;
            }
        }

        #endregion

        #region ExistNotExtis

        private class IsExistsGenerator : ShowHelperBase
        {
            public override AbstractGenerator CreateGenerator(GameObject go)
            {
                return VoidMethod.Path(go);
            }

            public override bool IsAvailable(GameObject go)
            {
                return go != null;
            }
        }
        
        /// <summary>
        /// Checks that `GameObject` by given path is present on scene
        /// </summary>
        /// <param name="path">Path to `GameObject` in hierarchy</param>
        /// <returns>`GameObject`</returns>
        [ShowInEditor(typeof(IsExistsGenerator), "Is Exist")]
        public static GameObject IsExist(string path)
        {
            var go = UITestUtils.FindAnyGameObject(path);

            if (go == null)
            {
                Assert.Fail("IsExist: Object with path " + path + " does not exist.");
            }
            return go;
        }

        /// <summary>
        /// Checks that `GameObject` by given path is present on scene and contains component `T`
        /// </summary>
        /// <param name="path">Path to `GameObject` in hierarchy</param>
        /// <typeparam name="T">`Type` of object</typeparam>
        /// <returns>`GameObject`</returns>
        public static GameObject IsExist<T>(string path) where T : Component
        {
            var go = UITestUtils.FindAnyGameObject<T>(path);

            if (go == null)
            {
                Assert.Fail("IsExist<" + typeof(T) + ">: Object with path " + path + " does not exist.");
            }

            return go.gameObject;
        }

        /// <summary>
        /// Searches for `GameObject` with component `T` on scene
        /// </summary>
        /// <typeparam name="T">`Type` of object</typeparam>
        /// <returns>`GameObject`</returns>
        public static GameObject IsExist<T>() where T : Component
        {
            var go = UITestUtils.FindAnyGameObject<T>();

            if (go == null)
            {
                Assert.Fail("IsExist<" + typeof(T) + ">: Object does not exist.");
            }

            return go.gameObject;
        }

        private class DoesNotExistsGenerator : ShowHelperBase
        {
            public override AbstractGenerator CreateGenerator(GameObject go)
            {
                return VoidMethod.Path(go);
            }

            public override bool IsAvailable(GameObject go)
            {
                return go != null;
            }
        }

        /// <summary>
        /// Checks that `GameObject` by given path is not present on scene
        /// </summary>
        /// <param name="path">Path to `GameObject` in hierarchy</param>
        [ShowInEditor(typeof(DoesNotExistsGenerator), "Does Not Exist")]
        public static void DoesNotExist(string path)
        {
            var go = UITestUtils.FindAnyGameObject(path);

            if (go != null)
            {
                Assert.Fail("DoesNotExist: Object with path " + path + " exists.");
            }
        }

        /// <summary>
        /// Checks that `GameObject` by given path is not present on scene or is not active
        /// </summary>
        /// <param name="path">Path to `GameObject` in hierarchy</param>
        /// <param name="timeout">Timeout (optional)</param>
        /// <param name="ignoreTimeScale">Should time scale be ignored or not (optional)</param>
        [ShowInEditor(typeof(DoesNotExistOrDisabledClass), "Does Not Exist Or Disabled")]
        public static IEnumerator DoesNotExistOrDisabled(string path, float timeout = 2, bool ignoreTimeScale = false)
        {
            var go = UITestUtils.FindAnyGameObject(path);

            yield return Wait.WaitFor(() =>
            {
                if (go == null || !go.activeInHierarchy)
                {
                    return new WaitSuccess();
                }
                return new WaitFailed("DoesNotExistOrDisabled: object with path: " + path + " exists or enabled after " + timeout + " second(s)");
            }, timeout, ignoreTimeScale: ignoreTimeScale);
        }

        private class DoesNotExistOrDisabledClass : ShowHelperBase
        {
            public override AbstractGenerator CreateGenerator(GameObject go)
            {
                return IEnumeratorMethod.Path(go).Float(1f).Bool(false);
            }

            public override bool IsAvailable(GameObject go)
            {
                return go != null;
            }
        }

        /// <summary>
        /// Checks that `GameObject` by given path with component `T` is not present on scene
        /// </summary>
        /// <param name="path">Path to `GameObject` in hierarchy</param>
        /// <typeparam name="T">`Type` of object</typeparam>
        public static void DoesNotExist<T>(string path) where T : Component
        {
            var go = UITestUtils.FindAnyGameObject<T>(path);

            if (go != null)
            {
                Assert.Fail("DoesNotExist<" + typeof(T) + ">: Object with path " + path + " exists.");
            }
        }

        /// <summary>
        /// Checks that no `GameObject` with component `T` is present on scene
        /// </summary>
        /// <typeparam name="T">`Type` of object</typeparam>
        public static void DoesNotExist<T>() where T : Component
        {
            var go = UITestUtils.FindAnyGameObject<T>();

            if (go != null)
            {
                Assert.Fail("DoesNotExist<" + typeof(T) + ">: Object exists.");
            }
        }

        #endregion

        #region Toggle

        /// <summary>
        /// Checks that `GameObject` has `Toggle` component and its `isOn` value equals to expected
        /// </summary>
        /// <param name="go">`GameObject` with `Toggle` component</param>
        /// <param name="expectedIsOn">Expected value of the toggle</param>
        public static void CheckToggle(GameObject go, bool expectedIsOn)
        {
            var toggle = UITestUtils.FindGameObjectWithComponentInParents<Toggle>(go);

            if (toggle == null)
            {
                Assert.Fail("CheckToggle: Game object "
                            + UITestUtils.GetGameObjectFullPath(go) +
                            " has no Toggle component.");
            }
            if (toggle.isOn != expectedIsOn)
            {
                Assert.Fail("CheckToggle: Toggle " +
                            UITestUtils.GetGameObjectFullPath(go) +
                            " is " + (toggle.isOn ? "On" : "Off") + " - but expected  " +
                            " is " + (expectedIsOn ? "On" : "Off"));
            }
        }

        private class CheckToggleState : ShowHelperBase
        {
            public override AbstractGenerator CreateGenerator(GameObject go)
            {
                return VoidMethod.Path(go).Bool(UITestUtils.FindComponentInParents<Toggle>(go).isOn);
            }

            public override bool IsAvailable(GameObject go)
            {
                if (!go)
                {
                    return false;
                }
                var toggleGo = UITestUtils.FindGameObjectWithComponentInParents<Toggle>(go);
                return toggleGo && toggleGo.gameObject.activeInHierarchy;
            }
        }

        /// <summary>
        /// Checks that `GameObject` by given path has `Toggle` component and its `isOn` value equals to expected
        /// </summary>
        /// <param name="path">Path to `GameObject` in hierarchy</param>
        /// <param name="state">Toggle state</param>
        [ShowInEditor(typeof(CheckToggleState), "Check Toggle State")]
        public static void Toggle(string path, bool state)
        {
            var go = IsExist(path);
            Toggle toggle = go.GetComponent<Toggle>();
            Assert.IsNotNull(toggle, "CheckToggle: Game object " + path + " has no Toggle component.");
            Assert.AreNotEqual(state, toggle.isOn, "CheckToggle: Toggle " + path +
                                                   " is " + (toggle.isOn ? "On" : "Off") + " - but expected  " +
                                                   " is " + (state ? "On" : "Off"));
        }

        #endregion

        #region Animation

        /// <summary>
        /// Seraches for `GameObject` by given path with `Animator` component. During a given `timeOut` waits for an animation state with specific name to become active
        /// </summary>
        /// <param name="path">Path to `GameObject` in hierarchy</param>
        /// <param name="stateName">`Animator` state name</param>
        /// <param name="timeout">Timeout</param>
        [ShowInEditor(typeof(AnimatorStateStartedClass), "Animator State Started", false)]
        public static IEnumerator AnimatorStateStarted(string path, string stateName, float timeout)
        {
            return AnimatorStateStartedClass.AnimatorStateStarted(path, stateName, timeout);
        }

        private class AnimatorStateStartedClass : ShowHelperBase
        {
            // todo: move logic from class
            private static Func<Animator, int, string> GetCurrentStateName;

            private static Func<TThis, TArg0, TReturn> BuildFastOpenMemberDelegate<TThis, TArg0, TReturn>(
                string methodName)
            {
                var method = typeof(TThis).GetMethod(
                    methodName,
                    BindingFlags.Instance | BindingFlags.NonPublic,
                    null,
                    CallingConventions.Any,
                    new[] {typeof(TArg0)},
                    null);

                if (method == null)
                {
                    throw new ArgumentException("Can't find method " + typeof(TThis).FullName + "." + methodName + "(" +
                                                typeof(TArg0).FullName + ")");
                }
                else if (method.ReturnType != typeof(TReturn))
                {
                    throw new ArgumentException("Expected " + typeof(TThis).FullName + "." + methodName + "(" +
                                                typeof(TArg0).FullName + ") to have return type of string but was " +
                                                method.ReturnType.FullName);
                }
                return (Func<TThis, TArg0, TReturn>) Delegate.CreateDelegate(typeof(Func<TThis, TArg0, TReturn>),
                    method);
            }

            public static IEnumerator AnimatorStateStarted(string path, string stateName, float timeout, bool ignoreTimeScale = false)
            {
                var stateNames = stateName.Split(';');
                var obj = UITestUtils.FindEnabledGameObjectByPath(path);
                if (obj == null || !obj.activeInHierarchy)
                {
                    yield return Wait.ObjectEnabled(path, timeout);
                }

                var animator = UITestUtils.FindEnabledGameObjectByPath(path).GetComponent<Animator>();

                if (GetCurrentStateName == null)
                {
                    GetCurrentStateName = BuildFastOpenMemberDelegate<Animator, int, string>("GetCurrentStateName");
                }

                yield return Wait.WaitFor(() =>
                {
                    if (animator.enabled)
                    {
                        for (int i = 0; i < animator.layerCount; i++)
                        {
                            var name = GetCurrentStateName(animator, i);
                            if (stateNames.Any(x => name.EndsWith(x)))
                            {
                                return new WaitSuccess();
                            }
                        }
                    }
                    return new WaitFailed("AnimatorStateStarted failed for path: " + path + "  and state name: " +
                                          stateName);
                }, timeout, ignoreTimeScale: ignoreTimeScale);
            }

            public override AbstractGenerator CreateGenerator(GameObject go)
            {
                return IEnumeratorMethod.Path(go).String("animator state name").Float(2f);
            }

            public override bool IsAvailable(GameObject go)
            {
                if (!go)
                {
                    return false;
                }
                var animator = go.GetComponent<Animator>();
                return animator != null;
            }
        }

        #endregion

        #region Fps

        /// <summary>
        /// Checks average fps since the last time user called `Interact.ResetFPS()` method or since the game started. If average fps is less than `targetFPS` value, test fails
        /// </summary>
        /// <param name="targetFPS">Minimum acceptable value of average fps</param>
        [ShowInEditor(typeof(CheckAverageFPSClass), "FPS/Check Average FPS", false)]
        public static void AverageFPS(float targetFPS)
        {
            var awerageFps = FPSCounter.AverageFPS;
            Assert.GreaterOrEqual(awerageFps, targetFPS);
        }

        private class CheckAverageFPSClass : ShowHelperBase
        {
            public override AbstractGenerator CreateGenerator(GameObject go)
            {
                return VoidMethod.Float(30f);
            }
        }

        /// <summary>
        /// Checks minimum fps since the last time user called `Interact.ResetFPS()` method or since the game started. If minimum fps is less than `targetFPS` value, test fails
        /// </summary>
        /// <param name="targetFPS">Minimum acceptable value of minimum fps</param>
        [ShowInEditor(typeof(CheckMinFPSClass), "FPS/Check Min FPS", false)]
        public static void MinFPS(float targetFPS)
        {
            CheckMinFPSClass.CheckMinFPS(targetFPS);
        }

        private class CheckMinFPSClass : ShowHelperBase
        {
            public static void CheckMinFPS(float targetFPS)
            {
                var minFps = FPSCounter.MixFPS;
                Assert.GreaterOrEqual(minFps, targetFPS);
            }

            public override AbstractGenerator CreateGenerator(GameObject go)
            {
                return VoidMethod.Float(20f);
            }

            public override bool IsAvailable(GameObject go)
            {
                return true;
            }
        }

        #endregion

        #region Image

        public static void SourceImage(GameObject go, string sourceName)
        {
            var image = UITestUtils.FindGameObjectWithComponentInParents<Image>(go);
            var sr = UITestUtils.FindGameObjectWithComponentInParents<SpriteRenderer>(go);

            if (image != null)
            {
                Assert.IsNotNull(image.sprite, "SourceImage: Image " +
                                                 UITestUtils.GetGameObjectFullPath(go) +
                                                 " has no sprite.");
                Assert.AreEqual(image.sprite.name, sourceName, "SourceImage: Image " +
                                                                    UITestUtils.GetGameObjectFullPath(go) +
                                                                    " has sprite: " + image.sprite.name +
                                                                    " - but expected: " +
                                                                    sourceName);
                return;
            }
            if (sr != null)
            {
                Assert.IsNotNull(sr.sprite, "SourceImage: SpriteRenderer " +
                                         UITestUtils.GetGameObjectFullPath(go) +
                                         " has no sprite.");
                Assert.AreEqual(sr.sprite.name, sourceName, "SourceImage: SpriteRenderer " +
                                                            UITestUtils.GetGameObjectFullPath(go) +
                                                            " has sprite: " + sr.sprite.name +
                                                            " - but expected: " +
                                                            sourceName);
                return;
            }
            Assert.Fail("SourceImage: Game object "
                        + UITestUtils.GetGameObjectFullPath(go) +
                        " has no Image and SpriteRenderer component.");
        }

        private class CheckSourceImage : ShowHelperBase
        {
            public override AbstractGenerator CreateGenerator(GameObject go)
            {
                var image = UITestUtils.FindGameObjectWithComponentInParents<Image>(go);
                var sr = UITestUtils.FindGameObjectWithComponentInParents<SpriteRenderer>(go);

                if (image != null)
                {
                    if (image.sprite!= null)
                    {
                        return VoidMethod.Path(go).String(image.sprite.name);
                    }
                    return VoidMethod.Path(go).String(string.Empty);
                }
                if (sr != null)
                {
                    if (sr.sprite != null)
                    {
                        return VoidMethod.Path(go).String(sr.sprite.name);
                    }
                    return VoidMethod.Path(go).String(string.Empty);
                }
                throw new ArgumentException();
            }

            public override bool IsAvailable(GameObject go)
            {
                if (!go)
                {
                    return false;
                }
                var image = UITestUtils.FindGameObjectWithComponentInParents<Image>(go);
                var sr = UITestUtils.FindGameObjectWithComponentInParents<SpriteRenderer>(go);
                return image || sr;
            }
        }

        /// <summary>
        /// Seraches for `GameObject` by given path and checks the source image
        /// </summary>
        /// <param name="path">Path to `GameObject` in hierarchy</param>
        /// <param name="sourceName">Source image name</param>
        [ShowInEditor(typeof(CheckSourceImage), "Check/Source Image")]
        public static void SourceImage(string path, string sourceName)
        {
            var go = IsExist(path);
            SourceImage(go, sourceName);
        }

        #endregion
    }
}