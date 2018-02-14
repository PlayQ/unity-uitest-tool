using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;

namespace PlayQ.UITestTools
{
    public static class UITestUtils
    {
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
        
        public static GameObject FindObjectByPercents(float x, float y)
        {
            return FindObjectByPixels(Screen.width * x, Screen.height * y);
        }
        
        public static GameObject FindGameObjectWithComponentInParents<TComponent>(GameObject go)
        {
            TComponent component = default(TComponent);

            var parent = go.transform;
            while (parent != null)
            {
                component = parent.GetComponent<TComponent>();
                if (component != null)
                {
                    return parent.gameObject;
                }
                parent = parent.parent;
            }

            return null;
        }

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

        public static string GetGameObjectFullPath(GameObject gameObject)
        {
            if (gameObject == null)
            {
                throw new NullReferenceException("given object is null!");
            }
            StringBuilder sb = new StringBuilder();
            while (gameObject)
            {
                sb.Insert(0, gameObject.name);
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

        public static Vector2[] ScreenVerticesOfObject(RectTransform transform)
        {
            Vector3[] worldCoreners = new Vector3[4];
            transform.GetWorldCorners(worldCoreners);

            var anyCamera = FindAnyGameObject<Camera>();
            if (anyCamera != null)
            {
                Canvas canvas = null;
                var root = transform;
                while (root)
                {
                    canvas = root.GetComponent<Canvas>();
                    root = root.parent as RectTransform;
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

        public static Vector2 PercentsToPixels(Vector2 percents)
        {
            return new Vector2(percents.x * Screen.width, percents.y * Screen.height);
        }

        public static Vector2 PercentsToPixels(float x, float y)
        {
            return new Vector2(x * Screen.width, y * Screen.height);
        }

        public static float WidthPercentsToPixels(float x)
        {
            return x * Screen.width;
        }

        public static float HeightPercentsToPixels(float y)
        {
            return y * Screen.height;
        }
    }
}