using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace PlayQ.UITestTools
{
    public static partial class Interact
    {
        private static string GenerateScreenshotName(string name)
        {
            name = new StringBuilder()
                .Append(name)
                .Append('-')
                .Append(DateTime.UtcNow.ToString(PlayModeLogger.FILE_DATE_FORMAT))
                .Append(".png").ToString();
            if (!Application.isMobilePlatform)
            {
                string path = new StringBuilder()
                    .Append(Application.persistentDataPath)
                    .Append(Path.DirectorySeparatorChar)
                    .Append("Screenshots")
                    .Append(Path.DirectorySeparatorChar).ToString();
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                name = path + name;
            }
            else
            {
                string path = new StringBuilder()
                    .Append("Screenshots")
                    .Append(Path.DirectorySeparatorChar).ToString();
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                name = path + name;
            }

            return name;
        }
        
        [ShowInEditor(typeof(MakeScreenshot), "Make screenshot", false)]
        public static void MakeScreenShot(string name)
        {
            name = GenerateScreenshotName(name);
            Application.CaptureScreenshot(name);
        }
        
        private static class MakeScreenshot
        {
            public static List<object> GetDefautParams(GameObject go)
            {
                List<object> result = new List<object>();
                result.Add("default_screenshot_name");
                return result;
            }
        }

        
        [ShowInEditor("reset screenshot failed flag", false)]
        public static void ResetScreenshotFailFlag()
        {
            MakeScreenshotAndCompareClass.TestFailed = false;
        }
        
        [ShowInEditor("fail test if screenshot failed", false)]
        public static void FailIfScreenShotsNotEquals()
        {
            if (MakeScreenshotAndCompareClass.TestFailed)
            {
                MakeScreenshotAndCompareClass.TestFailed = false;
                Assert.Fail("Screenshot equals failed. Specific screenshot names were logged as warnings.");   
            }
        }
        
        
        [ShowInEditor(typeof(MakeScreenshotAndCompareClass), "Make screenshot and compare", false)]
        public static IEnumerator MakeScreenshotAndCompare(string screenShotName, string referenceName, Vector2 targetResolution, float treshold = 1, bool dontFail = false)
        {
            var failMessageInfo = "screenshot " + screenShotName + " doesn't match reference " + referenceName;
            var name = GenerateScreenshotName(screenShotName);
            var pathToScreenShot = name;
            if (Application.isMobilePlatform)
            {
                pathToScreenShot = Application.persistentDataPath + Path.DirectorySeparatorChar + name;
            }
            Application.CaptureScreenshot(name);

            yield return Wait.Frame(5);

            var bytes = File.ReadAllBytes(pathToScreenShot);
            var screenshotTexture = new Texture2D((int)targetResolution.x,  (int)targetResolution.y, TextureFormat.ARGB32, false);
            screenshotTexture.LoadImage(bytes);
            screenshotTexture.Apply();            
            var referenceTex = Resources.Load<Texture2D>(referenceName);
            
            var pixelsRef = referenceTex.GetPixels32();
            var screenShot = screenshotTexture.GetPixels32();

            if (dontFail)
            {
                if (pixelsRef.Length != screenShot.Length)
                {
                    MakeScreenshotAndCompareClass.TestFailed = true;
                    Debug.LogWarning(failMessageInfo);
                    yield break;
                }
            }
            Assert.AreEqual(pixelsRef.Length, screenShot.Length, failMessageInfo);
            var matchedPisels = 0f;
            for (int i = 0; i < pixelsRef.Length; i++)
            {
                if (pixelsRef[i].a == screenShot[i].a &&
                    pixelsRef[i].r == screenShot[i].r &&
                    pixelsRef[i].g == screenShot[i].g &&
                    pixelsRef[i].b == screenShot[i].b )
                {
                    matchedPisels++;
                }
            }
            var actualTreshold = matchedPisels / pixelsRef.Length;

            if (dontFail)
            {
                if (actualTreshold < treshold)
                {
                    Debug.LogWarning(failMessageInfo);
                    MakeScreenshotAndCompareClass.TestFailed = true;
                    yield break;
                }
            }
            Assert.GreaterOrEqual(actualTreshold, treshold, failMessageInfo);    
            GameObject.DestroyImmediate(screenshotTexture, true);
            GameObject.DestroyImmediate(referenceTex, true);
            
            File.Delete(pathToScreenShot);
        }
        
        private static class MakeScreenshotAndCompareClass
        {
            public static bool TestFailed;
            public static List<object> GetDefautParams(GameObject go)
            {
                List<object> result = new List<object>();
                result.Add("default_screenshot_name");
                result.Add("default_reference_name");
                result.Add(1f);
                result.Add(false);
                return result;
            }
        }

        [ShowInEditor(typeof(ResetFPSClass), "FPS/Reset FPS", false)]
        public static void ResetFPS()
        {
            ResetFPSClass.ResetFPS();
        }

        public static void ClearFPSMEtrics()
        {
            if (File.Exists(SaveFPSClass.FPSMettricsFileFullPath))
            {
                File.Delete(SaveFPSClass.FPSMettricsFileFullPath);
            }
        }
        
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
            var buildVersion = SaveFPSClass.GetArg("-buildNumber");
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
        
        public static class SaveFPSClass
        {
            public const string MetricsFileName = "metrics.json"; 
            public static string FPSMettricsFolder
            {
                get
                {
                    return Application.persistentDataPath +
                           Path.DirectorySeparatorChar + "FPS_metrics";
                }
            }
            public static string FPSMettricsFileFullPath
            {
                get
                {
                    return FPSMettricsFolder + Path.DirectorySeparatorChar + MetricsFileName;
                }
            }
            
            public static List<object> GetDefautParams(GameObject go)
            {
                List<object> result = new List<object>();
                result.Add("default_tag");
                return result;
            }
            
            public static string GetArg(string name)
            {
                var args = System.Environment.GetCommandLineArgs();
                for (int i = 0; i < args.Length; i++)
                {
                    if (args[i] == name && args.Length > i + 1)
                    {
                        return args[i + 1];
                    }
                }
                return null;
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
        }

        
        [ShowInEditor(typeof(SetTimescaleClass), "Set timescale", false)]
        public static void SetTimescale(float scale)
        {
            Time.timeScale = scale;
        }

        private static class SetTimescaleClass
        {
            public static List<object> GetDefautParams(GameObject go)
            {
                List<object> result = new List<object>();
                result.Add(1f);
                return result;
            } 
        }
        
        private static class ResetFPSClass
        {
            public static void ResetFPS()
            {
                FPSCounter.Reset();
            }
        }

        public static GameObject GetScrollElement(GameObject go, ref Vector2 handlePosition)
        {
            var selectable = go.GetComponent<Selectable>();
            if (selectable == null)
            {
                var parentSelectable = go.GetComponentInParent<Selectable>();
                if (parentSelectable != null)
                {
                    handlePosition = parentSelectable.targetGraphic.gameObject == go
                        ? UITestUtils.CenterPointOfObject(parentSelectable.targetGraphic.rectTransform)
                        : UITestUtils.CenterPointOfObject(go.transform as RectTransform);

                    return parentSelectable.gameObject;
                }
            }
            else
            {
                handlePosition = UITestUtils.CenterPointOfObject(selectable.targetGraphic.rectTransform);
                return selectable.gameObject;
            }

            var scrollRect = go.GetComponent<ScrollRect>();
            if (scrollRect == null)
            {
                var parentScrollRect = go.GetComponentInParent<ScrollRect>();
                if (parentScrollRect != null)
                {
                    handlePosition = parentScrollRect.content.gameObject == go ? 
                        UITestUtils.CenterPointOfObject(parentScrollRect.content) :
                        UITestUtils.CenterPointOfObject(go.transform as RectTransform);
                    return parentScrollRect.gameObject;
                }
            }
            else
            {
                handlePosition = UITestUtils.CenterPointOfObject(scrollRect.content);
                return scrollRect.gameObject;
            }
            return null;
        }

#region Click
        [ShowInEditor(typeof(ButtonClick), "Button click")]
        public static void Click(string path)
        {
            ButtonClick.Click(path);
        }
        
        public static void Click(GameObject go)
        {
            ButtonClick.Click(go);
        } 
        
        [ShowInEditor(typeof(ButtonWaitDelayAndClick), "Wait, delay and click")]
        public static IEnumerator WaitDelayAndClick(string path, float delay, float timeOut)
        {
            yield return Wait.ObjectEnabledInstantiatedAndDelay(path, delay, timeOut);
            ButtonClick.Click(path);
        }
        
        [ShowInEditor(typeof(ButtonWaitDelayAndClick), "Wait, delay, click if possible")]
        public static IEnumerator WaitDelayAndClickIfPossible(string path, float delay, float timeOut)
        {
            yield return Wait.ObjectEnabledInstantiatedAndDelayIfPossible(path, delay, timeOut);
            ButtonClick.ClickIfPossible(path);
        }
        
        public static class ButtonClick
        {
            public static void ClickIfPossible(string path)
            {
                GameObject go = GameObject.Find(path);
                if (go != null)
                {
                    Click(go);
                }
            }
            
            public static void Click(string path)
            {
                GameObject go = GameObject.Find(path);
                if (go == null)
                {
                    Assert.Fail("Trying to click to " + path + " but it doesn't exist");
                }
                Click(go);
            }

            public static void Click(GameObject go)
            {
                var clicable = go.GetComponent<IPointerClickHandler>();
            
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
            
                ExecuteEvents.Execute(go, new PointerEventData(EventSystem.current), ExecuteEvents.pointerClickHandler);
            }

            public static bool IsAvailable(GameObject go)
            {
                var clicable = go.GetComponent<IPointerClickHandler>();
                return clicable != null && go.activeInHierarchy;
            }
        }
          
        public static class ButtonWaitDelayAndClick
        {
            public static bool IsAvailable(GameObject go)
            {
                var clicable = go.GetComponent<IPointerClickHandler>();
                return clicable != null && go.activeInHierarchy;
            }

            public static List<object> GetDefautParams(GameObject go)
            {
                List<object> result = new List<object>();
                result.Add((float) 2);
                result.Add((float) 30);
                return result;            
            }
        }
        
        private static class ButtonClickPixels
        {
            public static void Click(int x, int y)
            {
                GameObject go = UITestUtils.FindObjectByPixels(x, y);
                if (go == null)
                {
                    Assert.Fail("Cannot click to pixels [" + x + ";" + y + "], couse there are no objects.");
                }
                Interact.Click(go);
            }

            public static bool IsAvailable(GameObject go)
            {
                var clicable = go.GetComponent<IPointerClickHandler>();
                return clicable != null && go.activeInHierarchy;
            }

            public static List<object> GetDefautParams(GameObject go)
            {
                List<object> result = new List<object>();
                result.Add((int) Input.mousePosition.x);
                result.Add((int) Input.mousePosition.y);                
                return result;            
            }
        }
      
        [ShowInEditor(typeof(ButtonClickPixels), "Click pixels", false)]
        public static void ClickPixels(int x, int y)
        {
            ButtonClickPixels.Click(x, y);   
        }
 
        private static class ButtonClickPercent
        {
            public static void Click(int x, int y)
            {
                GameObject go = UITestUtils.FindObjectByPixels(x, y);
                if (go == null)
                {
                    Assert.Fail("Cannot click to pixels [" + x + ";" + y + "], couse there are no objects.");
                }
                Interact.Click(go);
            }

            public static bool IsAvailable(GameObject go)
            {
                var clicable = go.GetComponent<IPointerClickHandler>();
                return clicable != null && go.activeInHierarchy;
            }

            public static List<object> GetDefautParams(GameObject go)
            {
                List<object> result = new List<object>();
                result.Add(Input.mousePosition.x/Screen.width);
                result.Add(Input.mousePosition.y/Screen.height);
                return result;            
            }
        }
        
        [ShowInEditor(typeof(ButtonClickPercent), "Click percents", false)]
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
#endregion

#region Drag
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

        public static IEnumerator DragPercents(Vector2 from, Vector2 to, float time = 1)
        {
            var startPixel = new Vector2(Screen.width * from.x, Screen.height * from.y);
            var endPixel = new Vector2(Screen.width * to.x, Screen.height * to.y);
            yield return DragPixels(startPixel, endPixel, time);
        }

        public static IEnumerator DragPixels(GameObject go, Vector2 to, float time = 1)
        {
            var rectTransform = go.transform as RectTransform;
            if (rectTransform == null)
            {
                Assert.Fail("Can't find rect transform on object \"{0}\"", go.name);
            }
            yield return DragPixels(go, null, to, time);
        }

        public static IEnumerator DragPercents(GameObject go, Vector2 to, float time = 1)
        {
            yield return DragPixels(go, new Vector2(Screen.width * to.x, Screen.height * to.y), time);
        }

        public static IEnumerator DragPixels(GameObject go, Vector2? from, Vector2 to, float time = 1)
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
        
        public static IEnumerator DragPixels(string path, Vector2 to, float time = 1)
        {
            GameObject go = UITestUtils.FindAnyGameObject(path);
            if (go == null)
            {
                Assert.Fail("Cannot grag object " + path + ", couse there are not exist.");
            }
            yield return DragPixels(go, to, time);
        }

        public static IEnumerator DragPercents(string path, Vector2 to, float time = 1)
        {
            yield return DragPixels(path, new Vector2(Screen.width * to.x, Screen.height * to.y), time);
        }

#endregion

#region Text
        private static class SetTextClass
        {
            public static bool IsAvailable(GameObject go)
            {
                return go.GetComponent<InputField>() != null;
            }

            public static List<object> GetDefautParams(GameObject go)
            {
                List<object> result = new List<object>();
                result.Add(go.GetComponent<InputField>().text);
                return result;            
            }
        }
      
        public static void SetText(GameObject go, string text)
        {
            InputField inputField = go.GetComponent<InputField>();
            if (inputField == null)
            {
                Assert.Fail("Cannot set text to object " + go.name + ", couse his have not InputField component.");
            }
            inputField.text = text;
        }

        [ShowInEditor(typeof(SetTextClass), "Set text")]
        public static void SetText(string path, string text)
        {
            GameObject go = UITestUtils.FindAnyGameObject(path);
            
            SetText(go, text);
        }

        public static void AppendText(GameObject go, string text)
        {
            InputField inputField = go.GetComponent<InputField>();
            if (inputField == null)
            {
                Assert.Fail("Cannot set text to object " + go.name + ", couse his have not InputField component.");
            }
            inputField.text += text;
        }

        [ShowInEditor(typeof(SetTextClass), "Set text")]
        public static void AppendText(string path, string text)
        {
            GameObject go = Check.IsExist(path);
            AppendText(go, text);
        }
#endregion
    }
}