using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace PlayQ.UITestTools
{
    /// <summary>
    /// Contains methods that are needed for multiple action methods from all classes and facilitates the test actions writing
    /// </summary>
    public static class UITestUtils
    {
        /// <summary>
        /// Finds `EventSystem` on the scene
        /// </summary>
        public static EventSystem FindCurrentEventSystem()
        {
            return GameObject.FindObjectOfType<EventSystem>();
        }
        
        /// <summary>
        /// Loads scene by given name
        /// </summary>
        /// <param name="sceneName">Scene name</param>
        public static void LoadSceneForSetUp(string sceneName)
        {
            SceneManager.LoadScene(sceneName, LoadSceneMode.Additive);
        }
        
        /// <summary>
        /// Returns `Selectable` object from `GameObject` or its parent
        /// </summary>
        /// <param name="go">`GameObject` to find selectable object</param>
        /// <param name="handlePosition">Pointer position</param>
        public static GameObject GetScrollElement(GameObject go, ref Vector2 handlePosition)
        {
            var scrollBar = go.GetComponent<Scrollbar>();
            if (scrollBar == null)
            {
                scrollBar = go.GetComponentInParent<Scrollbar>();
            }

            if (scrollBar != null && scrollBar.targetGraphic != null)
            {
                handlePosition = CenterPointOfObject(scrollBar.targetGraphic.rectTransform);
                return scrollBar.targetGraphic.gameObject;
            }

            var scrollRect = go.GetComponent<ScrollRect>();
            if (scrollRect == null)
            {
                scrollRect = go.GetComponentInParent<ScrollRect>();
            }
            if (scrollRect != null)
            {
                handlePosition = CenterPointOfObject(go.transform as RectTransform);
                return scrollRect.gameObject;
            }

            var slider = go.GetComponent<Slider>();
            if (slider == null)
            {
                slider = go.GetComponentInParent<Slider>();
            }
            if (slider != null)
            {
                handlePosition = CenterPointOfObject(go.transform as RectTransform);
                return slider.gameObject;
            }

            return null;
        }
        
        /// <summary>
        /// Prints console log with a list of paths to all GameObjects on scene
        /// </summary>
        public static string LogHierarchy()
        {
            var result = new StringBuilder();
            result.Append("Scene hierarchy:");
            
            var objects = Resources.FindObjectsOfTypeAll<GameObject>().Where(obj=>!obj.transform.parent);
            foreach (var obj in objects)
            {
                result.Append("\n");
                result.Append(obj.name);
                result.Append(obj.activeInHierarchy ? "  active" : "  inActive");
                LogHierarchyRecursive(obj, result, obj.name);
            }

            return result.ToString();
        }

        private static void LogHierarchyRecursive(GameObject go, StringBuilder result, string basePath)
        {
            for (int i = 0; i < go.transform.childCount; i++)
            {
                var child = go.transform.GetChild(i);
                var fullPath = "     " + basePath + "/" + child.name;
                result.Append("\n");
                result.Append(fullPath);
                result.Append(child.gameObject.activeInHierarchy ? "  active" : "  inActive");
                LogHierarchyRecursive(child.gameObject, result, fullPath);
            }
        }

        /// <summary>
        /// Uses `UnityEngine.EventSystems.EventSystem` class to raycast by given coordinates to find GameObject that was clicked
        /// </summary>
        /// <param name="x">X position in pixels</param>
        /// <param name="y">Y position in pixels</param>
        /// <param name="ignoreNames">set of names of object, that are ignored (optional)</param>
        /// <returns>GameObjects under coords or null</returns>
        public static GameObject FindObjectByPixels(float x, float y, HashSet<string> ignoreNames = null)
        {
            var pointerData = new PointerEventData(EventSystem.current);
            var resultsData = new List<RaycastResult>();
            pointerData.position = new Vector2(x, y);
            
            EventSystem.current.RaycastAll(pointerData, resultsData);

            if (resultsData.Count > 0 && ignoreNames != null && ignoreNames.Count > 0)
            {
                resultsData = resultsData.Where(raycastResult => !ignoreNames.Contains(raycastResult.gameObject.name)).ToList();
            }
             
            return resultsData.Count > 0 ? resultsData[0].gameObject : null;
        }
        
        /// <summary>
        /// Calculates pixels coordinates from percent coordinates, then Uses `UnityEngine.EventSystems.EventSystem` class to raycast by resulting coordinates to find `GameObject` that was clicked
        /// </summary>
        /// <param name="x">X position in percents</param>
        /// <param name="y">Y position in percents</param>
        /// <returns>GameObjects under coords or null</returns>
        public static GameObject FindObjectByPercents(float x, float y)
        {
            return FindObjectByPixels(Screen.width * x, Screen.height * y);
        }
        
        
        public static GameObject FindUIObjectWhichOverlayGivenObject(GameObject overlayedGameObject)
        {
            var rtr = overlayedGameObject.GetComponent<RectTransform>();
            if (!rtr)
            {
                Debug.LogError("rect transform is null for Gameobject: " +
                               UITestUtils.GetGameObjectFullPath(overlayedGameObject));
            }
            var point = UITestUtils.CenterPointOfObject(rtr);
            
            GameObject gameObjectUnderClick = UITestUtils.FindObjectByPixels(point.x, point.y);
            if (gameObjectUnderClick == null || gameObjectUnderClick == overlayedGameObject)
            {
                return null;
            }
            
            GameObject parent = null;
            if (gameObjectUnderClick.transform.parent)
            {
                parent = gameObjectUnderClick.transform.parent.gameObject;    
            }

            while (parent)
            {
                if(parent == overlayedGameObject)
                {
                    return null;
                } 
                if (parent.transform.parent)
                {
                    parent = parent.transform.parent.gameObject;    
                }
                else
                {
                    parent = null;
                }
            }

            return gameObjectUnderClick;
        }
        
        /// <summary>
        /// Checks if given `GameObject` has `TComponent` component attached to it. If not - performs recursive check on its parent
        /// </summary>
        /// <param name="go">Parent `GameObject`</param>
        /// <typeparam name="TComponent">Component Type</typeparam>
        /// <returns>`Component` or null.</returns>
        public static TComponent FindGameObjectWithComponentInParents<TComponent>(GameObject go) where TComponent : Component 
        {
            var parent = go.transform;
            while (parent != null)
            {
                var component = parent.GetComponent<TComponent>();
                if (component != null)
                {
                    return component;
                }
                parent = parent.parent;
            }
            return null;
        }

        /// <summary>
        /// Checks if given GameObject has `TComponent` component attached to it. If not - performs recursive check on its parent
        /// </summary>
        /// <param name="go">Parent `GameObject`</param>
        /// <typeparam name="TComponent">Component Type</typeparam>
        /// <returns>`TComponent` instance, or null.</returns>
        public static TComponent FindComponentInParents<TComponent>(GameObject go)
        {
            TComponent component = default(TComponent);
            var parent = go.transform;
            while (parent != null)
            {
                component = parent.GetComponent<TComponent>();
                if (component != null)
                {
                    return component;
                }
                parent = parent.parent;
            }
            return default(TComponent);
        }

        /// <summary>
        /// Finds enabled `GameObject` by Path in hierarchy
        /// </summary>
        /// <param name="path">Path to `GameObject` in hierarchy</param>
        /// <returns>Enabled `GameObject` or null.</returns>
        public static GameObject FindEnabledGameObjectByPath(string path)
        {
            var chars = path.ToCharArray();
            var parts = new List<string>();
            var sb = new StringBuilder();
            for (int i = 0; i < chars.Length; i++)
            {
                if (chars[i] == '/' && i-1>0 && chars[i-1] != '%')
                {
                    parts.Add(sb.ToString());   
                    sb = new StringBuilder();
                }
                else
                {
                    sb.Append(chars[i]);
                }

                if (i == chars.Length - 1)
                {
                    parts.Add(sb.ToString());
                }
            }
            
           
            GameObject target = null;
            for (int i = 0; i < parts.Count; i++)
            {
                var name = DecodeString(parts[i]);
                if (i==0)
                {
                    target = GameObject.Find(name);
                }
                else
                {
                    if (!target)
                    {
                        return null;
                    }
                    
                    GameObject newTarget = null; 
                    for (int chiuldInd = 0; chiuldInd < target.transform.childCount; chiuldInd++)
                    {
                        var child = target.transform.GetChild(chiuldInd);
                        if (child.gameObject.activeInHierarchy && child.name == name)
                        {
                            newTarget = child.gameObject;
                            break;
                        }
                    }

                    target = newTarget;
                }
            }
            return target;
        }

        /// <summary>
        /// Encodes path string for Test Tools format (`/` => `%/`, `%` => `%%`)
        /// </summary>
        /// <param name="text">String to encode</param>
        /// <returns>Encoded strings</returns>
        private static string EncodeString(string text)
        {
            var chars = text.ToCharArray();
            var adjustedName = new StringBuilder();
            foreach (var symbol in chars)
            {
                if (symbol == '%')
                {
                    adjustedName.Append('%');
                    adjustedName.Append('%');
                }
                else if (symbol == '/')
                {
                    adjustedName.Append('%');
                    adjustedName.Append('/');
                }
                else
                {
                    adjustedName.Append(symbol);
                }
            }

            return adjustedName.ToString();
        }
        
        /// <summary>
        /// Decodes path in Test Tools format to simple path (`%/` => `/`, `%%` => `%`)
        /// </summary>
        /// <param name="text">String to encode</param>
        /// <returns>Encoded strings</returns>
        private static string DecodeString(string text)
        {
            var chars = text.ToCharArray();
            var adjustedName = new StringBuilder();
            for (int i=0; i < chars.Length; i++)
            {
                if (chars[i] == '%' && i+1 < chars.Length)
                {
                    adjustedName.Append(chars[i+1]);
                    i++;
                }
                else
                {
                    adjustedName.Append(chars[i]);
                }
            }
            return adjustedName.ToString();
        }
        
        /// <summary>
        /// Calculates and retuns full path to `GameObject`
        /// </summary>
        /// <param name="gameObject">`GameObject`</param>
        /// <returns>Full path to `GameObject`</returns>
        /// <exception cref="NullReferenceException"></exception>
        public static string GetGameObjectFullPath(GameObject gameObject)
        {
            if (gameObject == null)
            {
                throw new NullReferenceException("given object is null!");
            }
            StringBuilder sb = new StringBuilder();
            while (gameObject)
            {
                var adjustedName = EncodeString(gameObject.name);   
                sb.Insert(0, adjustedName);
                if (gameObject.transform.parent != null)
                {
                    sb.Insert(0, '/');
                    gameObject = gameObject.transform.parent.gameObject;
                }
                else
                {
                    break;
                }
            }
            return sb.ToString();
        }

        /// <summary>
        /// Searches for GameObject that has component of the given type attached and matches the given path in hierarchy
        /// </summary>
        /// <param name="path">Path to `GameObject` in hierarchy</param>
        /// <typeparam name="T">Type of `GameObject`</typeparam>
        /// <returns>active and non-active `GameObjects` or null</returns>
        public static GameObject FindAnyGameObject<T>(String path) where T : Component
        {
            if (String.IsNullOrEmpty(path))
            {
                return null;
            }

            var objects = Resources.FindObjectsOfTypeAll<T>();

            if (objects == null || objects.Length == 0)
            {
                return null;
            }

            var objectsOnScene = objects.Where(obj => obj.gameObject.scene.isLoaded);
            if (!objectsOnScene.Any())
            {
                return null;
            }

            var result = objectsOnScene.FirstOrDefault(obj =>
            {
                var fullPath = GetGameObjectFullPath(obj.gameObject);
                var resultComparsion = fullPath.EndsWith(path, StringComparison.Ordinal);
                return resultComparsion;
            });

            if (result == null)
            {
                return null;
            }

            return result.gameObject;
        }

        /// <summary>
        /// Searches for GameObject by path in hierarchy
        /// </summary>
        /// <param name="path">Path to `GameObject` in hierarchy</param>
        /// <returns>active and non-active `GameObjects` or null</returns>
        public static GameObject FindAnyGameObject(string path)
        {
            var objectsOnScene = Resources.FindObjectsOfTypeAll<GameObject>();

            if (objectsOnScene.Length == 0)
            {
                return null;
            }

            for (int i = 0; i < objectsOnScene.Length; i++)
            {
                if (objectsOnScene[i].scene.isLoaded && GetGameObjectFullPath(objectsOnScene[i]).EndsWith(path))
                {
                    return objectsOnScene[i];
                }
            }
            return null;
        }

        /// <summary>
        /// Searches for `GameObject` that has component of `T` attached to it
        /// </summary>
        /// <typeparam name="T">Type of `GameObject`</typeparam>
        /// <returns>active and non-active `GameObjects` or null</returns>
        public static T FindAnyGameObject<T>() where T : Component
        {
            var objects = Resources.FindObjectsOfTypeAll<T>();

            if (objects == null || objects.Length == 0)
            {
                return null;
            }

            var objectsOnScene = objects.Where(obj => obj.gameObject.scene.isLoaded);
            return objectsOnScene.FirstOrDefault();
        }

        /// <summary>
        /// Return center point of the given `RectTransform`
        /// </summary>
        /// <param name="transform">`RectTransform` component instance</param>
        /// <returns>Center point of given `RectTransform`</returns>
        public static Vector2 CenterPointOfObject(RectTransform transform)
        {
            Vector2[] points = ScreenVerticesOfObject(transform);
            Vector2 middlePoint = new Vector2();
            foreach (var point in points)
            {
                middlePoint.x += point.x;
                middlePoint.y += point.y;
            }

            middlePoint.x /= points.Length;
            middlePoint.y /= points.Length;
            return middlePoint;
        }

        /// <summary>
        /// Returns array of coordinates of screen rectangle of the given `RectTransform`
        /// </summary>
        /// <param name="transform">`RectTransform` component instance</param>
        /// <returns>Array of coords of screen rectangle of given `RectTransform`</returns>
        public static Vector2[] ScreenVerticesOfObject(RectTransform transform)
        {
            Vector3[] worldCoreners = new Vector3[4];
            transform.GetWorldCorners(worldCoreners);

            var anyCamera = FindAnyGameObject<Camera>();
            if (anyCamera != null)
            {
                Canvas canvas = null;
                Transform root = transform;
                while (root)
                {
                    var newCanvas = root.GetComponent<Canvas>();
                    if (newCanvas != null)
                    {
                        canvas = newCanvas;
                    }
                    root = root.parent;
                }

                if (canvas != null)
                {
                    switch (canvas.renderMode)
                    {
                        case RenderMode.ScreenSpaceCamera:
                            anyCamera = canvas.worldCamera ?? Camera.main;
                            break;
                        case RenderMode.WorldSpace:
                            anyCamera = Camera.main;
                            break;
                        case RenderMode.ScreenSpaceOverlay:
                            anyCamera = null;
                            break;
                    }

                    var screenCorners = worldCoreners
                        .Select(worldPoint => RectTransformUtility.WorldToScreenPoint(anyCamera, worldPoint)).ToArray();

                    return screenCorners;
                }
            }
            return null;
        }

        /// <summary>
        /// Transform screen percents to screen pixels
        /// </summary>
        /// <param name="percents">Screen percents</param>
        /// <returns>Screen pixels</returns>
        public static Vector2 PercentsToPixels(Vector2 percents)
        {
            return new Vector2(percents.x * Screen.width, percents.y * Screen.height);
        }

        /// <summary>
        /// Transform screen percents to screen pixels
        /// </summary>
        /// <returns>Screen percents</returns>
        /// <param name="x">X percents position in screen</param>
        /// <param name="y">Y percents position in screen</param>
        /// <returns>Screen pixels</returns>
        public static Vector2 PercentsToPixels(float x, float y)
        {
            return new Vector2(x * Screen.width, y * Screen.height);
        }

        /// <summary>
        /// Transform screen percents to screen pixels
        /// </summary>
        /// <param name="x">X percents position in screen</param>
        /// <returns>Screen pixels</returns>
        public static float WidthPercentsToPixels(float x)
        {
            return x * Screen.width;
        }

        /// <summary>
        /// Transform screen percents to screen pixels
        /// </summary>
        /// <param name="y">Y percents position in screen</param>
        /// <returns>Screen pixels</returns>
        public static float HeightPercentsToPixels(float y)
        {
            return y * Screen.height;
        }

        /// <summary>
        /// Gets the string comparator by specified text and regex option
        /// </summary>
        /// <returns>The string comparator</returns>
        /// <param name="text">Text</param>
        /// <param name="useRegEx">Is the specified text a regular expression</param>
        public static IStringComparator GetStringComparator(string text, bool useRegEx)
        {
            return StringComparatorFactory.Build(text, useRegEx);
        }
    }


    public interface IStringComparator
    {
        bool TextEquals(string text);
    }

    public class DefaultStringComparator : IStringComparator
    {
        private string text;

        public DefaultStringComparator(string text)
        {
            this.text = text;
        }

        public bool TextEquals(string text)
        {
            return this.text == text;
        }
    }

    public class RegexStringComparator : IStringComparator
    {
        private Regex regex;

        public RegexStringComparator(string text)
        {
            regex = new Regex(text);
        }

        public bool TextEquals(string text)
        {
            return regex.IsMatch(text);
        }
    }

    public static class StringComparatorFactory
    {
        public static IStringComparator Build(string text, bool useRegex)
        {
            return useRegex
                ? (IStringComparator)new RegexStringComparator(text)
                : (IStringComparator)new DefaultStringComparator(text);
        }
    }
}