using System;
using System.Linq;
using System.Text;
using UnityEngine;

namespace PlayQ.UITestTools
{
    public static class UITestTools
    {
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
            
            
            var objectsOnScene = objects.Where(obj => obj.gameObject.scene.isLoaded).ToArray();
            if (objectsOnScene.Length == 0)
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

            
            var objectsOnScene = objects.Where(obj => obj.gameObject.scene.isLoaded).ToArray();
            if (objectsOnScene.Length == 0)
            {
                return null;
            }

            return objectsOnScene[0];
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

                    var screenCorners = worldCoreners.Select(worldPoint =>
                    {
                        return RectTransformUtility.WorldToScreenPoint(anyCamera, worldPoint);

                    }).ToArray();

                    return screenCorners;
                }
            }
            return null;
        }
    }
}