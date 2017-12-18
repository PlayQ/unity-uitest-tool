using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using NUnit.Framework;
using System.Linq;
using System.Text;
using NUnit.Framework.Constraints;
using Object = UnityEngine.Object;

namespace PlayQ.UITestTools
{
    public abstract class UITestBase
    {
        protected EventSystem FindCurrentEventSystem()
        {
            return GameObject.FindObjectOfType<EventSystem>();
        }
        
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
            if (!Application.isMobilePlatform)
            {
                name = new StringBuilder().Append(Application.persistentDataPath).Append(Path.DirectorySeparatorChar)
                    .Append(name).Append('-').Append(DateTime.UtcNow.ToString(NUnitLogger.FILE_DATE_FORMAT))
                    .Append(".png").ToString();
            }
            else
            {
                name = new StringBuilder().Append(name).Append('-')
                    .Append(DateTime.UtcNow.ToString(NUnitLogger.FILE_DATE_FORMAT))
                    .Append(".png").ToString();
            }
            Application.CaptureScreenshot(name);
        }


        protected IEnumerator WaitFrame(int count = 1)
        {
            while (count > 0)
            {
                yield return null;
                count--;
            }
        }

        protected GameObject FindObjectByPixels(float x, float y)
        {
            var pointerData = new PointerEventData(EventSystem.current);
            var resultsData = new List<RaycastResult>();
            pointerData.position = new Vector2(x ,y);
            
            EventSystem.current.RaycastAll(pointerData, resultsData);

            return resultsData.Count > 0 ? resultsData[0].gameObject : null;
        }

        protected GameObject FindObjectByPercents(float x, float y)
        {
            return FindObjectByPixels(Screen.width * x, Screen.height * y);
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
            for (int i = SceneManager.sceneCount -1; i >=0; i--)
            {
                Scene scene = SceneManager.GetSceneAt(i);
                if (!scene.name.StartsWith("InitTestScene"))
                {
                    //AsyncOperation ao =  UnityEditor.SceneManagement.EditorSceneManager.UnloadSceneAsync(scene);
                    SceneManager.UnloadScene(scene);
                }
            }
        }

        #region Interactions

        protected void ClickPixels(float x, float y)
        {
            GameObject go = FindObjectByPixels(x, y);
            if (go == null)
            {
                Assert.Fail("Cannot click to pixels [" + x + ";" + y + "], couse there are no objects.");
            }
            Click(go);
        }

        protected void ClickPercents(float x, float y)
        {
            GameObject go = FindObjectByPercents(x, y);
            if (go == null)
            {
                Assert.Fail("Cannot click to percents [" + x + ";" + y +
                            "], couse there are no objects.");
            }
            Click(go);
        }



        
        protected IEnumerator DragPixels(Vector2 from, Vector2 to, float time = 1)
        {
            var go = FindObjectByPixels(from.x, from.y);
            if (go == null)
            {
                Assert.Fail("Cannot grag object from pixels [" + from.x + ";" + from.y +
                            "], couse there are no objects.");
            }
            yield return DragPixels(go, from, to, time);
        }
      
        

        protected IEnumerator DragPercents(Vector2 from, Vector2 to, float time = 1)
        {
            var startPixel = new Vector2(Screen.width * from.x, Screen.height * from.y);
            var endPixel = new Vector2(Screen.width * to.x, Screen.height * to.y);
            yield return DragPixels(startPixel, endPixel, time);
        }



        protected IEnumerator DragPixels(GameObject go, Vector2 to, float time = 1)
        {
            var rectTransform = go.transform as RectTransform;
            if (rectTransform == null)
            {
                Assert.Fail("Can't find rect transform on object \"{0}\"", go.name);
            }
            yield return DragPixels(go, null, to, time);
        }

        protected IEnumerator DragPercents(GameObject go, Vector2 to, float time = 1)
        {
            yield return DragPixels(go, new Vector2(Screen.width * to.x, Screen.height * to.y), time);
        }

        protected IEnumerator DragPixels(GameObject go, Vector2? from, Vector2 to, float time = 1)
        {
            if (!go.activeInHierarchy)
            {
                Assert.Fail("Trying to click to " + go.name + " but it disabled");
            }

            var fromPos = new Vector2();
            var scrollElement = GetScrollElement(go, ref fromPos);

            if (scrollElement == null)
            {
                Assert.Fail("Can't find draggable object for \"{0}\"", go.name);
            }

            if (from != null)
            {
                fromPos = from.Value;
            }

            var initialize = new PointerEventData(EventSystem.current);
            ExecuteEvents.Execute(scrollElement, initialize, ExecuteEvents.initializePotentialDrag);

            var currentTime = 0f;
            while (currentTime <= time)
            {
                yield return null;
                var targetPos = Vector2.Lerp(fromPos, to, currentTime / time);
                var drag = new PointerEventData(EventSystem.current)
                {
                    button = PointerEventData.InputButton.Left,
                    position = targetPos
                };
                ExecuteEvents.Execute(scrollElement, drag, ExecuteEvents.dragHandler);
                currentTime += Time.deltaTime;
            }

            var finalDrag = new PointerEventData(EventSystem.current)
            {
                button = PointerEventData.InputButton.Left,
                position = to
            };
            ExecuteEvents.Execute(scrollElement, finalDrag, ExecuteEvents.dragHandler);
        }

        private GameObject GetScrollElement(GameObject go, ref Vector2 handlePosition)
        {
            var selectable = go.GetComponent<Selectable>();
            if (selectable == null)
            {
                var parentSelectable = go.GetComponentInParent<Selectable>();
                if (parentSelectable != null)
                {
                    handlePosition = parentSelectable.targetGraphic.gameObject == go
                        ? UITestTools.CenterPointOfObject(parentSelectable.targetGraphic.rectTransform)
                        : UITestTools.CenterPointOfObject(go.transform as RectTransform);

                    return parentSelectable.gameObject;
                }
            }
            else
            {
                handlePosition = UITestTools.CenterPointOfObject(selectable.targetGraphic.rectTransform);
                return selectable.gameObject;
            }

            var scrollRect = go.GetComponent<ScrollRect>();
            if (scrollRect == null)
            {
                var parentScrollRect = go.GetComponentInParent<ScrollRect>();
                if (parentScrollRect != null)
                {
                    handlePosition = parentScrollRect.content.gameObject == go ? 
                        UITestTools.CenterPointOfObject(parentScrollRect.content) :
                        UITestTools.CenterPointOfObject(go.transform as RectTransform);
                    return parentScrollRect.gameObject;
                }
            }
            else
            {
                handlePosition = UITestTools.CenterPointOfObject(scrollRect.content);
                return scrollRect.gameObject;
            }
            return null;
        }

        protected IEnumerator DragPixels(string path, Vector2 to, float time = 1)
        {
            GameObject go = UITestTools.FindAnyGameObject(path);
            if (go == null)
            {
                Assert.Fail("Cannot grag object " + path + ", couse there are not exist.");
            }
            yield return DragPixels(go, to, time);
        }

        protected IEnumerator DragPercents(string path, Vector2 to, float time = 1)
        {
            yield return DragPixels(path, new Vector2(Screen.width * to.x, Screen.height * to.y), time);
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
            GameObject go = UITestTools.FindAnyGameObject(path);
            
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
            GameObject go = Check.IsExist(path);
            
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

//        protected IEnumerator WaitForAnimationPlaying(string objectName, string param, float waitTimeout = 2f)
//        {
//            yield return WaitFor(() =>
//            {
//                GameObject gameObject = GameObject.Find(objectName);
//                return gameObject.GetComponent<Animation>().IsPlaying(param);
//
//            }, waitTimeout, "WaitForAnimationPlaying " + objectName + "  animating param " + param);
//        }

        protected IEnumerator WaitForObject<T>(float waitTimeout = 2f) where T : Component
        {
            yield return WaitFor(() =>
            {
                var obj = UITestTools.FindAnyGameObject<T>();
                return obj != null;

            }, waitTimeout, "WaitForObject<" + typeof(T) + ">");
        }

        protected IEnumerator WaitForObject<T>(string path, float waitTimeout = 2f) where T : Component
        {
            yield return WaitFor(() =>
            {
                var obj = UITestTools.FindAnyGameObject<T>(path);
                return obj != null;

            }, waitTimeout, "WaitForObject<" + typeof(T) + ">");
        }

        protected IEnumerator WaitForObject(string path, float waitTimeout = 2f)
        {
            yield return WaitFor(() =>
            {
                var o = UITestTools.FindAnyGameObject(path);
                return o != null;

            }, waitTimeout, "WaitForObject path: " + path);
        }

        protected IEnumerator WaitForDestroy<T>(float waitTimeout = 2f) where T : Component
        {
            yield return WaitFor(() =>
            {
                var obj = UITestTools.FindAnyGameObject<T>();
                return obj == null;

            }, waitTimeout, "WaitForDestroy<" + typeof(T) + ">");
        }

        protected IEnumerator WaitForDestroy(string path, float waitTimeout = 2f)
        {
            yield return WaitFor(() =>
            {
                var gameObj = UITestTools.FindAnyGameObject(path);
                return gameObj == null;

            }, waitTimeout, "WaitForDestroy path: " + path);
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
                return obj != null;

            }, waitTimeout, "WaitObjectEnabled<" + typeof(T) + ">");
        }


        protected IEnumerator WaitObjectDisabled<T>(float waitTimeout = 2f) where T : Component
        {
            yield return WaitFor(() =>
            {
                var obj = UITestTools.FindAnyGameObject<T>();
                return obj != null && !obj.gameObject.activeInHierarchy;

            }, waitTimeout, "WaitObjectDisabled<" + typeof(T) + ">");
        }

        protected IEnumerator WaitObjectDisabled(string path, float waitTimeout = 2f)
        {
            yield return WaitFor(() =>
            {
                var obj = UITestTools.FindAnyGameObject(path);
                return obj != null && !obj.gameObject.activeInHierarchy;

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
            var go = IsExist(path);
            
            TextEquals(go, expectedText);
        }

        public static void TextEquals(GameObject go, string expectedText)
        {
            var t = go.GetComponent<Text>();
            if (t == null)
            {
                Assert.Fail("TextEquals: Label object " + go.name + " has no Text attached");
            }
            if (t.text != expectedText)
            {
                Assert.Fail("TextEquals: Label " + go.name + " actual text: " + t.text + ", expected "+expectedText+", text dont't match");
            }
        }

        public static void TextNotEquals(GameObject go, string expectedText)
        {
            var t = go.GetComponent<Text>();
            if (t == null)
            {
                Assert.Fail("TextNotEquals: Label object " + go.name + " has no Text attached");
            }
            if (t.text == expectedText)
            {
                Assert.Fail("TextNotEquals: Label " + go.name + " actual text: " + expectedText + " text matches but must not");
            }
        }

        public static void TextNotEquals(string path, string expectedText)
        {
            var go = IsExist(path);
            
            TextNotEquals(go, expectedText);
        }

        public static void IsEnable(string path)
        {
            var go = IsExist(path);
            
            if(!go.gameObject.activeInHierarchy)
            {
                Assert.Fail("IsEnable: with path "+path+" Game Object disabled");
            }
        }

        public static void IsEnable<T>(string path) where T : Component
        {
            var go = IsExist<T>(path);

            if(!go.gameObject.activeInHierarchy)
            {
                Assert.Fail("IsEnable<"+typeof(T)+">: with path "+path+" Game Object disabled");
            }
        }

        public static void IsEnable<T>() where T : Component
        {
            var go = IsExist<T>();
            
            if (!go.gameObject.activeInHierarchy)
            {
                Assert.Fail("IsEnable<"+typeof(T)+">: Game Object disabled");
            }
        }

        public static void IsEnable(GameObject go)
        {
            if (!go.activeInHierarchy)
            {
                Assert.Fail("IsEnable by object instance: with path " + UITestTools.GetGameObjectFullPath(go) + " Game Object disabled");
            }
        }

        public static void IsDisable(string path)
        {
            var go = IsExist(path);
             
            if(go.gameObject.activeInHierarchy)
            {
                Assert.Fail("IsDisable: with path "+path+" Game Object enabled");
            }
        }

        public static void IsDisable<T>(string path) where T : Component
        {
            IsExist<T>(path);
            var go = UITestTools.FindAnyGameObject<T>(path);
             
            if(go.gameObject.activeInHierarchy)
            {
                Assert.Fail("IsDisable<"+typeof(T)+">: with path "+path+" Game Object enabled");
            }
        }

        public static void IsDisable<T>() where T : Component
        {
            var go = IsExist<T>();
            
            if(go.gameObject.activeInHierarchy)
            {
                Assert.Fail("IsDisable<"+typeof(T)+">: Game Object enabled");
            }
        }

        public static void IsDisable(GameObject go)
        {
            if (go.activeInHierarchy)
            {
                Assert.Fail("IsDisable by object instance: Game Object " + UITestTools.GetGameObjectFullPath(go) + " enabled");
            }
        }

        public static GameObject IsExist(string path)
        {
            var go = UITestTools.FindAnyGameObject(path);
            
            if (go == null)
            {
                Assert.Fail("IsExist: Object with path " + path + " does not exist.");
            }
            
            return go;
        }

        public static GameObject IsExist<T>(string path) where T : Component
        {
            var go = UITestTools.FindAnyGameObject<T>(path);
            
            if (go == null)
            {
                Assert.Fail("IsExist<"+typeof(T)+">: Object with path " + path + " does not exist.");
            }

            return go.gameObject;
        }

        public static GameObject IsExist<T>() where T : Component
        {
            var go = UITestTools.FindAnyGameObject<T>();
            
            if (go == null)
            {
                Assert.Fail("IsExist<"+typeof(T)+">: Object does not exist.");
            }

            return go.gameObject;
        }
        
        public static void IsNotExist(string path)
        {
            var go = UITestTools.FindAnyGameObject(path);
            
            if (go != null)
            {
                Assert.Fail("IsNotExist: Object with path " + path + " exists.");
            }
        }

        public static void IsNotExist<T>(string path) where T : Component
        {
            var go = UITestTools.FindAnyGameObject<T>(path);
            
            if (go != null)
            {
                Assert.Fail("IsNotExist<"+typeof(T)+">: Object with path " + path + " exists.");
            }
        }

        public static void IsNotExist<T>() where T : Component
        {
            var go = UITestTools.FindAnyGameObject<T>();
            
            if (go != null)
            {
                Assert.Fail("IsNotExist<"+typeof(T)+">: Object exists.");
            }
        }

        public static void IsToggleOn(string path)
        {
            var go = IsExist(path);
           
            Toggle toggle = go.GetComponent<Toggle>();
            if (toggle == null)
            {
                Assert.Fail("IsToggleOn: Game object " + path + " has no Toggle component.");
            }
            if (!toggle.isOn)
            {
                Assert.Fail("IsToggleOn: Toggle " + path + " is OFF.");
            }
        }
        public static void IsToggleOff(string path)
        {
            var go = IsExist(path);
            
            Toggle toggle = go.GetComponent<Toggle>();
            if (toggle == null)
            {
                Assert.Fail("IsToggleOff: Game object " + path + " has no Toggle component.");
            }
            if (toggle.isOn)
            {
                Assert.Fail("IsToggleOn: Toggle " + path + " is ON.");
            }
        }
    }
}