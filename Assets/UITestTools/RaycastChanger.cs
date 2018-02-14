using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

namespace PlayQ.UITestTools
{
    public static partial class RaycastChanger
    {
        private static List<Graphic> unityGraphic = new List<Graphic>();
        [RaycastChanger]
        public static void ChangeForUnityText(bool isEnable)
        {
            if (isEnable && unityGraphic.Count > 0)
            {
                return;
            }
            if (isEnable)
            {
                var graphic = Resources.FindObjectsOfTypeAll<Graphic>();
                foreach (var text in graphic)
                {
                    if (!text.raycastTarget && text.gameObject.scene.isLoaded)
                    {
                        text.raycastTarget = true;
                        unityGraphic.Add(text);
                    }
                }
            }
            else
            {
                foreach (var graphic in unityGraphic)
                {
                    if (graphic)
                    {
                        graphic.raycastTarget = false;    
                    }
                }
                unityGraphic.Clear();
            }
        }
    }
}