using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using JetBrains.Annotations;
using Newtonsoft.Json.Linq;
using PlayQ.UITestTools.WaitResults;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace PlayQ.UITestTools
{
    /// <summary>
    /// Contains methods which allow interacting with game components and objects on the scene
    /// </summary>
    public static partial class Interact
    {
        private static string GenerateScreenshotDirectoryPath()
        {
            var pathBuilder = new StringBuilder();

            if (!Application.isMobilePlatform)
            {
                pathBuilder
                    .Append(Application.persistentDataPath)
                    .Append(Path.DirectorySeparatorChar);
            }

            pathBuilder.Append("Screenshots");
            var result = pathBuilder.ToString();
            return result;
        }
        
        /// <summary>
        /// Makes a screenshot. Saves it to persistant data folder
        /// </summary>
        /// <param name="name">Name of screenshot</param>
        [ShowInEditor(typeof(MakeScreenshot), "Screenshot/Make Screenshot", false)]
        public static IEnumerator MakeScreenShot(string name)
        {
            var screenshotPath = GenerateScreenshotDirectoryPath();
            if (!Directory.Exists(screenshotPath))
            {
                Directory.CreateDirectory(screenshotPath);
            }

            name = TestScreenshotTools.GenerateScreenshotName(name);
            var fullPath = screenshotPath + Path.DirectorySeparatorChar + name;
            
            PlayModeTestRunner.IsTestUIEnabled = false;
            yield return null;
            Application.CaptureScreenshot(fullPath);
            yield return Wait.Frame(5);
            PlayModeTestRunner.IsTestUIEnabled = true;
        }

        private class MakeScreenshot : ShowHelperBase
        {
            public override AbstractGenerator CreateGenerator(GameObject go)
            {
                return new MethodName().String("default_screenshot_name");
            }
        }

        /// <summary>
        /// Resets FPS counter
        /// FPS counter stores average fps, minimum and maximum FPS values since the last moment this method was called
        /// </summary>
        [ShowInEditor(typeof(ResetFPSClass), "FPS/Reset FPS", false)]
        public static void ResetFPS()
        {
            FPSCounter.Reset();
        }

        private class ResetFPSClass : ShowHelperBase
        {
            public override AbstractGenerator CreateGenerator(GameObject go)
            {
                return VoidMethod;
            }

            public override bool IsAvailable(GameObject go)
            {
                return true;
            }
        }
        
        /// <summary>
        /// Clears FPS data from the hard drive
        /// </summary>
        public static void ClearFPSMetrics()
        {
            if (File.Exists(SaveFPSClass.FPSMettricsFileFullPath))
            {
                File.Delete(SaveFPSClass.FPSMettricsFileFullPath);
            }
        }
        
        /// <summary>
        /// Stores FPS data on the hard drive
        /// </summary>
        /// <param name="tag">Measure discription</param>
        [ShowInEditor(typeof(SaveFPSClass), "FPS/Save FPS", false)]
        public static void SaveFPS(string tag)
        {
            string textData = null;
            if (File.Exists(SaveFPSClass.FPSMettricsFileFullPath))
            {
                try
                {
                    textData = File.ReadAllText(SaveFPSClass.FPSMettricsFileFullPath);
                }
                catch (Exception ex)
                {
                    Debug.LogWarning("file with fps metrics exists but can't be read " + ex.Message);
                }
            }

            var  metrics = new SaveFPSClass.FPSMetrics();
            if (!string.IsNullOrEmpty(textData))
            {
                metrics = JObject.Parse(textData).ToObject<SaveFPSClass.FPSMetrics>();
                if (metrics == null)
                {
                    metrics = new SaveFPSClass.FPSMetrics();
                }
            }
            metrics.Build = Application.version;
            var buildVersion = ConsoleArgumentHelper.GetArg("-buildNumber");
            if (!string.IsNullOrEmpty(buildVersion))
            {
                metrics.Build = metrics.Build + ":" + buildVersion;
            }

            if (metrics.Fps == null)
            {
                metrics.Fps = new Dictionary<string, SaveFPSClass.FPSData>();
            }

            if (metrics.Fps.ContainsKey(tag))
            {
                Debug.LogWarning("Tag " + tag + " is already exists in FPS metrics file!");
                tag = tag + "_" +Time.realtimeSinceStartup;
            }
            metrics.Fps.Add(tag, new SaveFPSClass.FPSData
            {
                Min = FPSCounter.MixFPS,
                Avg = FPSCounter.AverageFPS,
                Max = FPSCounter.MaxFPS
            });

            textData = JObject.FromObject(metrics).ToString();

            if (!Directory.Exists(SaveFPSClass.FPSMettricsFolder))
            {
                Directory.CreateDirectory(SaveFPSClass.FPSMettricsFolder);
            }
            
            File.WriteAllText(SaveFPSClass.FPSMettricsFileFullPath, textData);
        }
        
        public class SaveFPSClass : ShowHelperBase
        {
            private const string METRICS_FILE_NAME = "fps.json"; 
            public static string FPSMettricsFolder
            {
                get
                {
                    return Application.persistentDataPath +
                           Path.DirectorySeparatorChar + "Metrics";
                }
            }
            public static string FPSMettricsFileFullPath
            {
                get
                {
                    return FPSMettricsFolder + Path.DirectorySeparatorChar + METRICS_FILE_NAME;
                }
            }

            public class FPSData
            {
                public float Min;
                public float Avg;
                public float Max;
            }
            public class FPSMetrics
            {
                public string Build;
                public Dictionary<string, FPSData> Fps;
            }

            public override AbstractGenerator CreateGenerator(GameObject go)
            {
                return VoidMethod.String("default_tag");
            }
        }

        /// <summary>
        /// Sets timescale
        /// </summary>
        /// <param name="scale">New timescale</param>
        [ShowInEditor(typeof(SetTimescaleClass), "Timescale", false)]
        public static void SetTimescale(float scale)
        {
            Time.timeScale = scale;
        }

        private class SetTimescaleClass : ShowHelperBase
        {
            public override AbstractGenerator CreateGenerator(GameObject go)
            {
                return VoidMethod.Float(1);
            }

            public override bool IsAvailable(GameObject go)
            {
                return true;
            }
        }
        

        
#region Click
        /// <summary>
        /// Emulates LMB click on `Unity UI` element by given path
        /// </summary>
        /// <param name="path">Path to GameObject in hierarchy</param>
        [ShowInEditor(typeof(ButtonClick), "Button Click")]
        public static void Click(string path)
        {
            GameObject go = UITestUtils.FindEnabledGameObjectByPath(path);
            if (go == null)
            {
                Assert.Fail("Trying to click to " + path + " but it doesn't exist");
            }
            Click(go);
        }
        
        private class ButtonClick : ShowHelperBase
        {
            public override AbstractGenerator CreateGenerator(GameObject go)
            {
                return VoidMethod.Path(go);
            }

            public override bool IsAvailable(GameObject go)
            {
                if (!go)
                {
                    return false;
                }
                var clicable = go.GetComponent<IPointerClickHandler>();
                return clicable != null && go.activeInHierarchy;
            }
        }
        
        /// <summary>
        /// Emulates LMB click on `Unity UI GameObject`
        /// </summary>
        /// <param name="go">GameObject to click</param>
        public static void Click(GameObject go)
        {
            var clicables = go.GetComponents<IPointerClickHandler>();

            foreach (var clicable in clicables)
            {
                if (clicable == null)
                {
                    Assert.Fail("Trying to click on object " + UITestUtils.GetGameObjectFullPath(go) +
                                " but it or his parents has no IPointerClickHandler component ");
                }

                if (!go.activeInHierarchy)
                {
                    Assert.Fail("Trying to click on " + UITestUtils.GetGameObjectFullPath(go) +
                                " but it disabled. Original gameobject path: " +
                                UITestUtils.GetGameObjectFullPath(go));
                }

                clicable.OnPointerClick(new PointerEventData(EventSystem.current));
            }
        } 
        
        /// <summary>
        /// Waits until `GameObject` by given path is present on scene and active in hierarchy then emulates LMB click after the specified delay. Fails if exceeds the given timeout
        /// </summary>
        /// <param name="path">Path to GameObject in hierarchy</param>
        /// <param name="delay">Amount of time to delay</param>
        /// <param name="timeout">Timeout</param>
        [ShowInEditor(typeof(ButtonWaitDelayAndClick), "Wait, Delay and Click", true)]
        public static IEnumerator WaitDelayAndClick(string path, float delay, float timeout)
        {
            yield return Wait.ObjectEnableAndInteractibleIfButton(path, timeout);
            if (delay > 0)
            {
                yield return new WaitForSeconds(delay);
            }
            Click(path);
        }

        /// <summary>
        /// Obsolete. Use WaitDelayAndClick instead
        /// Waits for one second, then waits until `GameObject` by given path is present on scene and active in hierarchy, then emulates LMB click and finally waits for a specified delay. Fails if exceeds the given timeout
        /// </summary>
        /// <param name="path">Path to GameObject in hierarchy</param>
        /// <param name="delay">Amount of time to delay</param>
        /// <param name="timeout">Timeout</param>
        //todo make it by desing
        [Obsolete]
        [ShowInEditor(typeof(ObsoleteButtonWaitClickAndDelay), "Obsolete Wait, Click And Delay")]
        public static IEnumerator ObsoleteWaitClickAndDelay(string path, float delay, float timeout)
        {
            yield return new WaitForSeconds(1);
            yield return Wait.ObjectEnableAndInteractibleIfButton(path, timeout);
            
            Click(path);

            if (delay > 0)
            {
                yield return new WaitForSeconds(delay);
            }
        }

        /// <summary>
        /// Obsolete. Use WaitDelayAndClick instead
        /// Waits for one second, then waits until `GameObject` by given path is present on scene and active in hierarchy, then emulates LMB click and finally waits for a specified delay
        /// </summary>
        /// <param name="path">Path to GameObject in hierarchy</param>
        /// <param name="delay">Amount of time to delay</param>
        /// <param name="timeout">Timeout</param>
        //todo remove it
        [Obsolete]
        [ShowInEditor(typeof(ObsoleteButtonWaitClickAndDelay), "Obsolete Wait, Click And Delay if Possible")]
        public static IEnumerator ObsoleteWaitClickAndDelayIfPossible(string path, float delay, float timeout)
        {
            yield return new WaitForSeconds(1);
            yield return Wait.ObjectEnableAndInteractibleIfButton(path, timeout, true);
            ObsoleteButtonWaitClickAndDelay.ClickIfPossible(path);
            yield return new WaitForSeconds(delay);
        }
        
        [Obsolete]
        private class ObsoleteButtonWaitClickAndDelay : ShowHelperBase
        {
            public override AbstractGenerator CreateGenerator(GameObject go)
            {
                return IEnumeratorMethod.Path(go).Float(2).Float(30);
            }

            public override bool IsAvailable(GameObject go)
            {
                if (go != null)
                {
                    var clicable = go.GetComponent<IPointerClickHandler>();
                    return clicable != null && go.activeInHierarchy;
                }
                return false;
            }
            
            public static void ClickIfPossible(string path)
            {
                GameObject go = UITestUtils.FindEnabledGameObjectByPath(path);
                var isOverlayed = UITestUtils.FindUIObjectWhichOverlayGivenObject(go) != null;
                if (isOverlayed)
                {
                    return;
                }
                if (go != null && go.activeInHierarchy)
                {
                    Click(go);
                }
            }
        }
        
     
        
        //todo make private
        public class ButtonWaitDelayAndClick : ShowHelperBase
        {
            public override AbstractGenerator CreateGenerator(GameObject go)
            {
                return IEnumeratorMethod.Path(go).Float(1).Float(20);
            }

            public override bool IsAvailable(GameObject go)
            {
                if (!go)
                {
                    return false;
                }
                var clicable = go.GetComponent<IPointerClickHandler>();
                return clicable != null && go.activeInHierarchy;
            }
        }
        
      
        /// <summary>
        /// Uses `UnityEngine.EventSystems.EventSystem` class to raycast UI by specified coords to find `GameObject` and perform LMB click on it
        /// </summary>
        /// <param name="x">X position in screen pixels</param>
        /// <param name="y">Y position in screen pixels</param>
        [ShowInEditor(typeof(ButtonClickPixels), "Click Pixels", false)]
        public static void ClickPixels(int x, int y)
        {
            GameObject go = UITestUtils.FindObjectByPixels(x, y);
            if (go == null)
            {
                Assert.Fail("Cannot click to pixels [" + x + ";" + y + "], couse there are no objects.");
            }
            Interact.Click(go);
        }
 
        private class ButtonClickPixels : ShowHelperBase
        {
            public override AbstractGenerator CreateGenerator(GameObject go)
            {
                return VoidMethod
                    .Int((int) Input.mousePosition.x)
                    .Int((int) Input.mousePosition.y);
            }

            public override bool IsAvailable(GameObject go)
            {
                if (!go)
                {
                    return false;
                }
                var clicable = go.GetComponent<IPointerClickHandler>();
                return clicable != null && go.activeInHierarchy;
            }
        }
        
        
        /// <summary>
        /// Finds screen space pixel coordinates by given screen size percents and uses `UnityEngine.EventSystems.EventSystem` class to raycast by resulting coordinates to find `GameObject` and perform LMB click on it
        /// </summary>
        /// <param name="x">X position in screen percents</param>
        /// <param name="y">Y position in screen percents</param>
        [ShowInEditor(typeof(ButtonClickPercent), "Click Percents", false)]
        public static void ClickPercents(float x, float y)
        {
            GameObject go = UITestUtils.FindObjectByPercents(x, y);
            if (go == null)
            {
                Assert.Fail("Cannot click to percents [" + x + ";" + y +
                            "], couse there are no objects.");
            }
            Click(go);
        }     
        
        private class ButtonClickPercent : ShowHelperBase
        {
            public override AbstractGenerator CreateGenerator(GameObject go)
            {
                return VoidMethod.Float(Input.mousePosition.x / Screen.width)
                    .Float(Input.mousePosition.y / Screen.height);
            }

            public override bool IsAvailable(GameObject go)
            {
                if (!go)
                {
                    return false;
                }
                var clicable = go.GetComponent<IPointerClickHandler>();
                return clicable != null && go.activeInHierarchy;
            }
        }
#endregion

#region Drag
        /// <summary>
        /// Uses `UnityEngine.EventSystems.EventSystem` class to raycast by given coordinates to find `GameObject` and perform drag on it
        /// </summary>
        /// <param name="from">Start position in pixels</param>
        /// <param name="to">Finish position in pixels</param>
        /// <param name="time">Drag Time (optional)</param>
        public static IEnumerator DragPixels(Vector2 from, Vector2 to, float time = 1)
        {
            var go = UITestUtils.FindObjectByPixels(from.x, from.y);
            if (go == null)
            {
                Assert.Fail("Cannot grag object from pixels [" + from.x + ";" + from.y +
                            "], couse there are no objects.");
            }
            yield return DragPixels(go, from, to, time);
        }

        /// <summary>
        /// Uses `UnityEngine.EventSystems.EventSystem` class to raycast by given coordinates to find `GameObject` and perform drag on it
        /// </summary>
        /// <param name="from">Start position in percents</param>
        /// <param name="to">Finish position in percents</param>
        /// <param name="time">Drag Time (optional)</param>
        public static IEnumerator DragPercents(Vector2 from, Vector2 to, float time = 1)
        {
            var startPixel = new Vector2(Screen.width * from.x, Screen.height * from.y);
            var endPixel = new Vector2(Screen.width * to.x, Screen.height * to.y);
            yield return DragPixels(startPixel, endPixel, time);
        }

        /// <summary>
        /// Perform drag on `GameObject`
        /// </summary>
        /// <param name="go">`GameObject` to drag</param>
        /// <param name="to">Finish position in pixels</param>
        /// <param name="time">Drag Time (optional)</param>
        public static IEnumerator DragPixels(GameObject go, Vector2 to, float time = 1)
        {
            var rectTransform = go.transform as RectTransform;
            if (rectTransform == null)
            {
                Assert.Fail("Can't find rect transform on object \"" + go.name + "\"");
            }
            yield return DragPixels(go, null, to, time);
        }

        /// <summary>
        /// Perform drag on `GameObject`
        /// </summary>
        /// <param name="go">`GameObject` to drag</param>
        /// <param name="to">Finish position in percents</param>
        /// <param name="time">Drag Time (optional)</param>
        public static IEnumerator DragPercents(GameObject go, Vector2 to, float time = 1)
        {
            yield return DragPixels(go, new Vector2(Screen.width * to.x, Screen.height * to.y), time);
        }

        /// <summary>
        /// Perform drag on `GameObject`
        /// </summary>
        /// <param name="go">`GameObject` to drag</param>
        /// <param name="from">Start position in percents</param>
        /// <param name="to">Finish position in percents</param>
        /// <param name="time">Drag Time (optional)</param>
        public static IEnumerator DragPixels(GameObject go, Vector2? from, Vector2 to, float time = 1)
        {
            if (!go.activeInHierarchy)
            {
                Assert.Fail("Trying to click to " + go.name + " but it disabled");
            }

            var fromPos = new Vector2();
            var scrollElement = UITestUtils.GetScrollElement(go, ref fromPos);

            if (scrollElement == null)
            {
                Assert.Fail("Can't find draggable object for \"" + go.name + "\"");
            }

            if (from != null)
            {
                fromPos = from.Value;
            }

            var raycaster = go.GetComponentInParent<GraphicRaycaster>();

            if (!raycaster)
            {
                Assert.Fail("Can't find GraphicRaycaster object for \"" + go.name + "\"");
            }

            var initialize = new PointerEventData(EventSystem.current)
            {
                position = fromPos,
                button = PointerEventData.InputButton.Left,
                pointerPressRaycast = new RaycastResult
                {
                    module = raycaster
                }
            };

            ExecuteEvents.Execute(scrollElement, initialize, ExecuteEvents.beginDragHandler);
            
            var currentTime = 0f;

            while (currentTime <= time)
            {
                yield return null;

                var targetPos = Vector2.Lerp(fromPos, to, currentTime / time);
                var drag = new PointerEventData(EventSystem.current)
                {
                    button = PointerEventData.InputButton.Left,
                    position = targetPos,
                    pointerPressRaycast = new RaycastResult
                    {
                        module = raycaster    
                    }
                };

                ExecuteEvents.Execute(scrollElement, drag, ExecuteEvents.dragHandler);
                currentTime += Time.deltaTime;
            }

            var finalDrag = new PointerEventData(EventSystem.current)
            {
                button = PointerEventData.InputButton.Left,
                position = to,
                pointerPressRaycast = new RaycastResult
                {
                    module = raycaster    
                }
            };
            ExecuteEvents.Execute(scrollElement, finalDrag, ExecuteEvents.dragHandler);
           
            ExecuteEvents.Execute(scrollElement, finalDrag, ExecuteEvents.endDragHandler);
        }

        /// <summary>
        /// Perform drag on `GameObject` by path
        /// </summary>
        /// <param name="path">Path to GameObject in hierarchy</param>
        /// <param name="to">Finish position in pixels</param>
        /// <param name="time">Drag Time (optional)</param>
        public static IEnumerator DragPixels(string path, Vector2 to, float time = 1)
        {
            GameObject go = UITestUtils.FindAnyGameObject(path);
            if (go == null)
            {
                Assert.Fail("Cannot grag object " + path + ", couse there are not exist.");
            }
            yield return DragPixels(go, to, time);
        }

        /// <summary>
        /// Perform drag on `GameObject` by path
        /// </summary>
        /// <param name="path">Path to `GameObject` in hierarchy</param>
        /// <param name="to">Finish position in percents</param>
        /// <param name="time">Drag Time (optional)</param>
        public static IEnumerator DragPercents(string path, Vector2 to, float time = 1)
        {
            yield return DragPixels(path, new Vector2(Screen.width * to.x, Screen.height * to.y), time);
        }

        /// <summary>
        /// Perform scroll on `GameObject` by path
        /// </summary>
        /// <param name="path">Path to `GameObject` in hierarchy</param>
        /// <param name="horizontalPosition">Horizontal position</param>
        /// <param name="verticalPosition">Vertical position</param>
        /// <param name="animationDuration">Animation duration (optional)</param>
        /// <param name="timeout">Timeout (optional)</param>
        /// <param name="ignoreTimeScale">Should time scale be ignored or not (optional)</param>
        [ShowInEditor(typeof(ScrollToPositionClass), "Scroll To Position")]
        public static IEnumerator ScrollToPosition(string path,
            float horizontalPosition,
            float verticalPosition, 
            float animationDuration = 1f,
            float timeout = 2f,
            bool ignoreTimeScale = false)
        {
            var go = UITestUtils.FindEnabledGameObjectByPath(path);
            ScrollRect scrollRect = null;
            yield return Wait.WaitFor(() =>
                {
                    scrollRect = go.GetComponentInParent<ScrollRect>();
                    if (scrollRect != null)
                    {
                        return new WaitSuccess();
                    }
                    return new WaitFailed("can't find scroll rect durin timeout for object: " + path);
                }, timeout, ignoreTimeScale: ignoreTimeScale);
            
            var currentTime = 0f;
            var currentPos = scrollRect.normalizedPosition;
            var newPos = new Vector2(horizontalPosition, verticalPosition);
            
            while (currentTime < animationDuration)
            {
                var targetPos = Vector2.Lerp(currentPos, newPos, currentTime / animationDuration);
                scrollRect.normalizedPosition = targetPos;
                currentTime += Time.deltaTime;
                yield return null;
            }
            scrollRect.normalizedPosition = newPos;
        }

        private class ScrollToPositionClass : ShowHelperBase
        {
            public override AbstractGenerator CreateGenerator(GameObject go)
            {
                var scrollRect = go.GetComponent<ScrollRect>();
                return IEnumeratorMethod.Path(go).Float(scrollRect.normalizedPosition.x)
                                        .Float(scrollRect.normalizedPosition.y).Float(1).Float(2).Bool(false);
            }

            public override bool IsAvailable(GameObject go)
            {
                if (go == null)
                {
                    return false;
                }
                var scrollRect = go.GetComponent<ScrollRect>();
                return scrollRect != null;
            } 
        }

        /// <summary>
        /// Obtains drag percents of `GameObject` by path
        /// </summary>
        /// <param name="path">Path to `GameObject` in hierarchy</param>
        /// <param name="fromPercentX">Min percent of drag at dimension X</param>
        /// <param name="fromPercentY">Min percent of drag at dimension Y</param>
        /// <param name="toPercentX">Max percent of drag at dimension X</param>
        /// <param name="toPercentY">Max percent of drag at dimension Y</param>
        /// <param name="time">Time (optional)</param>
        [ShowInEditor(typeof(DragPercentsClass), "Drag percents")]
        public static IEnumerator DragPercents(string path,
            float fromPercentX, float fromPercentY,
            float toPercentX, float toPercentY, float time = 1)
        {
            var go = UITestUtils.FindEnabledGameObjectByPath(path);
            var anyCamera = GameObject.FindObjectOfType<Camera>();
            yield return DragPixels(go, 
                new Vector2(anyCamera.pixelWidth * fromPercentX,
                    anyCamera.pixelHeight * fromPercentY),
                new Vector2(anyCamera.pixelWidth * toPercentX,
                    anyCamera.pixelHeight * toPercentY), time);
        }

        private class DragPercentsClass : ShowHelperBase
        {
            public override AbstractGenerator CreateGenerator(GameObject go)
            {
                var pos = UITestUtils.CenterPointOfObject(go.GetComponent<RectTransform>());
                var anyCamera = GameObject.FindObjectOfType<Camera>();
                pos.x = pos.x / anyCamera.pixelWidth;
                pos.y = pos.y / anyCamera.pixelHeight;
                return VoidMethod.Path(go).Float(pos.x).Float(pos.y).Float(pos.x).Float(pos.y).Float(1);
            }

            public override bool IsAvailable(GameObject go)
            {
                if (go == null)
                {
                    return false;
                }
                var fromPos = new Vector2();
                var scrollElement = UITestUtils.GetScrollElement(go, ref fromPos);
                return scrollElement != null;
            }
        }

#endregion

#region Text      
        /// <summary>
        /// Finds `GameObject` by path, checks if `GameObject` has `Text` component attached, then sets text variable of `Text` to given value
        /// </summary>
        /// <param name="go">`GameObject` with `Text` component</param>
        /// <param name="text">`Text` to set</param>
        public static void SetText(GameObject go, string text)
        {
            InputField inputField = go.GetComponent<InputField>();
            if (inputField == null)
            {
                Assert.Fail("Cannot set text to object " + go.name + ", couse his have not InputField component.");
            }
            inputField.text = text;
        }

        /// <summary>
        /// Finds `GameObject` by path, checks if `GameObject` has `Text` component attached, then set text variable of `Text` to given value
        /// </summary>
        /// <param name="path">Path to `GameObject` in hierarchy</param>
        /// <param name="text">`Text` to set</param>
        [ShowInEditor(typeof(SetTextClass), "Set Text")]
        public static void SetText(string path, string text)
        {
            GameObject go = UITestUtils.FindAnyGameObject(path);
            
            SetText(go, text);
        }
        private class SetTextClass : ShowHelperBase
        {
            public override AbstractGenerator CreateGenerator(GameObject go)
            {
                return VoidMethod.String(go.GetComponent<InputField>().text);
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

        /// <summary>
        /// Checks if `GameObject` has `InputField` component attached, then appends given text to text variable of `InputField`
        /// </summary>
        /// <param name="go">`GameObject` with `Text` component</param>
        /// <param name="text">`Text` to set</param>
        public static void AppendText(GameObject go, string text)
        {
            InputField inputField = go.GetComponent<InputField>();
            if (inputField == null)
            {
                Assert.Fail("Cannot set text to object " + go.name + ", couse his have not InputField component.");
            }
            inputField.text += text;
        }

        /// <summary>
        /// Appends text to `GameObject` by path with `InputField` component attached
        /// </summary>
        /// <param name="path">Path to `GameObject` in hierarchy</param>
        /// <param name="text">`Text` to set</param>
        [ShowInEditor(typeof(AppendTextClass), "Set text")]
        public static void AppendText(string path, string text)
        {
            GameObject go = Check.IsExist(path);
            AppendText(go, text);
        }
        
        private class AppendTextClass : ShowHelperBase
        {
            public override AbstractGenerator CreateGenerator(GameObject go)
            {
                return VoidMethod.String(go.GetComponent<InputField>().text);
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
    }
}