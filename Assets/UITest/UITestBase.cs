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

        protected void MakeScreenShot(string name)
        {
            throw new NotImplementedException();
        }

        protected T FindObject<T>() where T : Object
        {
            return Object.FindObjectOfType<T>();
        }

        protected T FindObject<T>(string name) where T : Object
        {
            return Object.FindObjectsOfType<T>().First(x => x.name == name);
        }

        protected GameObject FindObject(string name = null)
        {
            return GameObject.Find(name);
        }

        protected GameObject FindObjectByPixels(Vector2 pixels)
        {
            throw new NotImplementedException();
        }

        protected GameObject FindObjectByPercents(Vector2 percents)
        {
            throw new NotImplementedException();
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
        private void Clean()
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

        protected void ClickPixels(Vector2 pixels)
        {
            GameObject go = FindObjectByPixels(pixels);
            if (go == null)
            {
                Assert.Fail("Cannot click to pixels [" + pixels.x + ";" + pixels.y + "], couse there are no objects.");
            }
            Click(go);
        }

        protected void ClickPercents(Vector2 percents)
        {
            GameObject go = FindObjectByPercents(percents);
            if (go == null)
            {
                Assert.Fail("Cannot click to percents [" + percents.x + ";" + percents.y +
                            "], couse there are no objects.");
            }
            Click(go);
        }

        protected void DragPixels(Vector2 start, Vector2 end, float time = 1)
        {
            GameObject go = FindObjectByPixels(start);
            if (go == null)
            {
                Assert.Fail("Cannot grag object from pixels [" + start.x + ";" + start.y +
                            "], couse there are no objects.");
            }
            DragPixels(go, end - start, time);
        }

        protected void DragPercents(Vector2 start, Vector2 end, float time = 1)
        {
            GameObject go = FindObjectByPercents(start);
            if (go == null)
            {
                Assert.Fail("Cannot grag object from percents [" + start.x + ";" + start.y +
                            "], couse there are no objects.");
            }
            DragPercents(go, end - start, time);
        }

        protected void DragPixels(GameObject go, Vector2 direction, float time = 1)
        {
            throw new NotImplementedException();
        }

        protected void DragPixels(string path, Vector2 direction, float time = 1)
        {
            GameObject go = FindObject(path);
            if (go == null)
            {
                Assert.Fail("Cannot grag object " + path + ", couse there are not exist.");
            }
            DragPixels(go, direction, time);
        }

        protected void DragPercents(GameObject go, Vector2 direction, float time = 1)
        {
            throw new NotImplementedException();
        }

        protected void DragPercents(string path, Vector2 direction, float time = 1)
        {
            GameObject go = FindObject(path);
            if (go == null)
            {
                Assert.Fail("Cannot grag object " + path + ", couse there are not exist.");
            }
            DragPercents(go, direction, time);
        }

        protected void SetText(GameObject go, string text)
        {
            InputField inputField = go.GetComponent<InputField>();
            if (inputField == null)
            {
                Assert.Fail("Cannot set text to object " + go.name + ", couse his have not InputField component.");
            }
            inputField.text = text;
        }

        protected void SetText(string path, string text)
        {
            GameObject go = FindObject(path);
            if (go == null)
            {
                Assert.Fail("Cannot set text to object " + path + ", couse there are not exist.");
            }
            SetText(go, text);
        }

        protected void AppendText(GameObject go, string text)
        {
            InputField inputField = go.GetComponent<InputField>();
            if (inputField == null)
            {
                Assert.Fail("Cannot set text to object " + go.name + ", couse his have not InputField component.");
            }
            inputField.text += text;
        }

        protected void AppendText(string path, string text)
        {
            GameObject go = FindObject(path);
            if (go == null)
            {
                Assert.Fail("Cannot set text to object " + path + ", couse there are not exist.");
            }
            AppendText(go, text);
        }

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

            }, waitTimeout, "WaitForObject<" + typeof(T) + ">");
        }

        protected IEnumerator WaitForObject<T>(string name, float waitTimeout = 2f) where T : Component
        {
            yield return WaitFor(() =>
            {
                var obj = Object.FindObjectOfType(typeof(T));
                return obj != null;

            }, waitTimeout, "WaitForObject<" + typeof(T) + ">");
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

            }, waitTimeout, "WaitForDestroy<" + typeof(T) + ">");
        }

        protected IEnumerator WaitForDestroy(string path, float waitTimeout = 2f)
        {
            yield return WaitFor(() =>
            {
                var gameObj = GameObject.Find(path);
                return gameObj == null;

            }, waitTimeout, "WaitForDestroy path: " + path);
        }


        protected IEnumerator WaitForDestroy(GameObject gameObject, float waitTimeout = 2f)
        {
            yield return WaitFor(() =>
            {
                return gameObject == null;

            }, waitTimeout, "WaitForDestroy " + gameObject.GetType());
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

            }, waitTimeout, "WaitObjectEnabled<" + typeof(T) + ">");
        }


        protected IEnumerator WaitObjectDisabled<T>(float waitTimeout = 2f) where T : Component
        {
            yield return WaitFor(() =>
            {
                var obj = Object.FindObjectOfType<T>();
                return obj != null && !obj.gameObject.activeInHierarchy;

            }, waitTimeout, "WaitObjectDisabled<" + typeof(T) + ">");
        }

        protected IEnumerator WaitObjectDisabled(string path, float waitTimeout = 2f)
        {
            yield return WaitFor(() =>
            {
                var obj = GameObject.Find(path); //finds only enabled gameobjects
                return obj == null;

            }, waitTimeout, "WaitObjectDisabled path: " + path);
        }

        protected IEnumerator WaitForSeconds(float seconds)
        {
            yield return new WaitForSeconds(seconds);
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
            TextEquals(go, expectedText);
        }

        public static void TextEquals(GameObject go, string expectedText)
        {
            var t = go.GetComponent<Text>();
            if (t == null)
            {
                Assert.Fail("Label object " + go.name + " has no Text attached");
            }
            if (t.text != expectedText)
            {
                Assert.Fail("Label " + go.name + "\n text expected: " + expectedText + ",\n actual: " + t.text);
            }
        }

        public static void TextNotEquals(GameObject go, string expectedText)
        {
            var t = go.GetComponent<Text>();
            if (t == null)
            {
                Assert.Fail("Label object " + go.name + " has no Text attached");
            }
            if (t.text == expectedText)
            {
                Assert.Fail("Label " + go.name + "\n text expected not: " + expectedText + ",\n but it is.");
            }
        }

        public static void TextNotEquals(string path, string expectedText)
        {
            var go = GameObject.Find(path);
            if (go == null)
            {
                Assert.Fail("Label object " + path + " does not exist or disabled");
            }
            TextNotEquals(go, expectedText);
        }

        public static void IsEnable(string path)
        {
            var go = GameObject.Find(path);
            if (go == null)
            {
                Assert.Fail("Game Object " + path + " does not exist or disabled");
            }
        }

        public static void IsEnable<T>(string name) where T : Component
        {
            T go = Object.FindObjectsOfType<T>().First(x => x.name == name);
            if (go == null || go.gameObject.activeInHierarchy)
            {
                Assert.Fail("Game Object " + name + " does not exist or disabled");
            }
        }

        public static void IsEnable<T>() where T : Component
        {
            var go = Object.FindObjectOfType<T>();
            if (go == null || !go.gameObject.activeInHierarchy)
            {
                Assert.Fail("Game Object does not exist or disabled");
            }
        }

        public static void IsEnable(GameObject go)
        {
            if (!go.activeInHierarchy)
            {
                Assert.Fail("Game Object " + go.name + " does not exist or disabled");
            }
        }

        public static void IsDisable(string name)
        {
            var go = Object.FindObjectsOfType<GameObject>().First(x => x.name == name);
            if (go == null)
            {
                Assert.Fail("Game Object " + name + " does not exist");
            }
            if (go.activeInHierarchy)
            {
                Assert.Fail("Game Object " + name + " is enable");
            }
        }

        public static void IsDisable<T>(string name) where T : Component
        {
            var go = Object.FindObjectsOfType<T>().First(x => x.name == name);
            if (go == null)
            {
                Assert.Fail("Game Object " + name + " does not exist");
            }
            if (go.gameObject.activeInHierarchy)
            {
                Assert.Fail("Game Object " + name + " is enable");
            }
        }

        public static void IsDisable<T>() where T : Component
        {
            var go = Object.FindObjectOfType<T>();
            if (go == null)
            {
                Assert.Fail("Game Object does not exist");
            }
            if (go.gameObject.activeInHierarchy)
            {
                Assert.Fail("Game Object " + go.name + " is enable");
            }
        }

        public static void IsDisable(GameObject go)
        {
            if (go.gameObject.activeInHierarchy)
            {
                Assert.Fail("Game Object " + go.name + " is enable");
            }
        }

//todo change all names to paths
        public static void IsExist(string name)
        {
            var go = Object.FindObjectsOfType<GameObject>().First(x => x.name == name);
            if (go == null)
            {
                Assert.Fail("Object " + name + " does not exist.");
            }
        }

        public static void IsExist<T>(string name) where T : Component
        {
            var go = Object.FindObjectsOfType<T>().First(x => x.name == name);
            if (go == null)
            {
                Assert.Fail("Object " + name + " does not exist.");
            }
        }

        public static void IsExist<T>() where T : Component
        {
            var go = Object.FindObjectOfType<T>();
            if (go == null)
            {
                Assert.Fail("Object does not exist.");
            }
        }

        public static void IsNotExist(string name)
        {
            var go = Object.FindObjectsOfType<GameObject>().First(x => x.name == name);
            if (go != null)
            {
                Assert.Fail("Object " + name + " is exist.");
            }
        }

        public static void IsNotExist<T>(string name) where T : Component
        {
            var go = Object.FindObjectsOfType<T>().First(x => x.name == name);
            if (go != null)
            {
                Assert.Fail("Object " + name + " is exist.");
            }
        }

        public static void IsNotExist<T>() where T : Component
        {
            var go = Object.FindObjectOfType<T>();
            if (go != null)
            {
                Assert.Fail("Object is exist.");
            }
        }

        public static void IsToggle(string path)
        {
            var go = GameObject.Find(path);
            if (go == null)
            {
                Assert.Fail("Toggle " + path + " not exist.");
            }
            Toggle toggle = go.GetComponent<Toggle>();
            if (toggle == null)
            {
                Assert.Fail("Game object " + path + " has no Toggle component.");
            }
            if (!toggle.isOn)
            {
                Assert.Fail("Toggle " + path + " is disabled.");
            }
        }
        public static void IsNotToggle(string path)
        {
            var go = GameObject.Find(path);
            if (go == null)
            {
                Assert.Fail("Toggle " + path + " not exist.");
            }
            Toggle toggle = go.GetComponent<Toggle>();
            if (toggle == null)
            {
                Assert.Fail("Game object " + path + " has no Toggle component.");
            }
            if (toggle.isOn)
            {
                Assert.Fail("Toggle " + path + " is enabled.");
            }
        }
    }
}