using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

namespace PlayQ.UITestTools
{
    public static partial class Check
    {
#region Text
        [ShowInEditor(typeof(CheckTextEquals), "Text equals")]
        public static void TextEquals(string path, string expectedText)
        {
            CheckTextEquals.CheckEquals(path, expectedText);
        }
        
        [ShowInEditor(typeof(CheckTextEquals), "Text not equals")]
        public static void TextNotEquals(string path, string expectedText)
        {
            CheckTextEquals.CheckNotEquals(path, expectedText);
        }

        private static class CheckTextEquals
        {
            public static void CheckEquals(string path, string expectedText)
            {
                var go = UITestUtils.FindAnyGameObject(path);
                Assert.AreEqual(null, go, "InputEquals: Object " + path + " is not exist");
                TextEquals(go, expectedText);
            }

            public static void CheckNotEquals(string path, string expectedText)
            {
                var go = UITestUtils.FindAnyGameObject(path);
                Assert.AreEqual(null, go, "InputEquals: Object " + path + " is not exist");
                TextNotEquals(go, expectedText);
            }

            public static bool IsAvailable(GameObject go)
            {
                return go.GetComponent<Text>() != null;
            }

            public static List<object> GetDefautParams(GameObject go)
            {
                List<object> result = new List<object>();
                var labelText = go.GetComponent<Text>();
                result.Add(labelText.text);
                return result;
            }
        }

        public static void TextEquals(GameObject go, string expectedText)
        {
            var t = go.GetComponent<Text>();
            if (t == null)
            {
                Assert.Fail("TextEquals: Object " + go.name + " has no Text attached");
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
                Assert.Fail("TextNotEquals: Object " + go.name + " has no Text attached");
            }
            if (t.text == expectedText)
            {
                Assert.Fail("TextNotEquals: Label " + go.name + " actual text: " + expectedText + " text matches but must not");
            }
        }
#endregion

#region Input

        public static void InputEquals(GameObject go, string expectedText)
        {
            var t = UITestUtils.FindComponentInParents<InputField>(go);
            if (t == null)
            {
                Assert.Fail("InputEquals: Object " + go.name + " has no InputField attached");
            }
            if (t.text != expectedText)
            {
                Assert.Fail("InputEquals: InputField " + go.name + " actual text: " + t.text + ", expected "+expectedText+", text dont't match");
            }
        }
       
        public static void InputNotEquals(GameObject go, string expectedText)
        {
            var t = go.GetComponent<Text>();
            if (t == null)
            {
                Assert.Fail("InputNotEquals: Object " + go.name + " has no Text attached");
            }
            if (t.text == expectedText)
            {
                Assert.Fail("InputNotEquals: InputField " + go.name + " actual text: " + expectedText + " text matches but must not");
            }
        }
       
        [ShowInEditor(typeof(CheckInputTextEquals), "Input text equals")]
        public static void InputEquals(string path, string expectedText)
        {
            CheckInputTextEquals.CheckEquals(path, expectedText);
        }
        
        [ShowInEditor(typeof(CheckInputTextEquals), "Input text not equals")]
        public static void InputNotEquals(string path, string expectedText)
        {
            CheckInputTextEquals.CheckNotEquals(path, expectedText);
        }
        
        private static class CheckInputTextEquals
        {
            public static void CheckEquals(string path, string expectedText)
            {
                var go = UITestUtils.FindAnyGameObject(path);
                Assert.AreEqual(null, go, "InputEquals: Object " + path + " is not exist");
                InputEquals(go, expectedText);
            }
            
            public static void CheckNotEquals(string path, string expectedText)
            {
                var go = UITestUtils.FindAnyGameObject(path);
                Assert.AreEqual(null, go, "InputEquals: Object " + path + " is not exist");
                InputNotEquals(go, expectedText);
            }

            public static bool IsAvailable(GameObject go)
            {
                return go.GetComponent<InputField>() != null;
            }

            public static List<object> GetDefautParams(GameObject go)
            {
                List<object> result = new List<object>();
                var labelText = go.GetComponent<InputField>();
                result.Add(labelText.text);
                return result;              
            }
        }
#endregion

#region EnableDisable
        
        
        
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
                Assert.Fail("IsEnable by object instance: with path " + UITestUtils.GetGameObjectFullPath(go) + " Game Object disabled");
            }
        }
        
        [ShowInEditor(typeof(CheckAverageFPSClass), "check awerage FPS", false)]
        public static void CheckAverageFPS(float targetFPS)
        {
            CheckAverageFPSClass.CheckAverageFPS(targetFPS);
        }

        private static class CheckAverageFPSClass
        {
            public static void CheckAverageFPS(float targetFPS)
            {
                var awerageFps = FPSCounter.AverageFPS;
                Assert.GreaterOrEqual(awerageFps, targetFPS);
            }
            
            public static List<object> GetDefautParams(GameObject go)
            {
                List<object> result = new List<object>();
                result.Add(30f);
                return result;
            } 
        }
        
        [ShowInEditor(typeof(CheckMinFPSClass), "Check min FPS", false)]
        public static void CheckMinFPS(float targetFPS)
        {
            CheckMinFPSClass.CheckMinFPS(targetFPS);
        }

        private static class CheckMinFPSClass
        {
            public static void CheckMinFPS(float targetFPS)
            {
                var minFps = FPSCounter.MixFPS;
                Assert.GreaterOrEqual(minFps, targetFPS);
            }
            
            public static List<object> GetDefautParams(GameObject go)
            {
                List<object> result = new List<object>();
                result.Add(20f);
                return result;
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
            var go = UITestUtils.FindAnyGameObject<T>(path);
             
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
                Assert.Fail("IsDisable by object instance: Game Object " + UITestUtils.GetGameObjectFullPath(go) + " enabled");
            }
        }

        [ShowInEditor(typeof(CheckEnabledState), "Check enabled state")]
        public static void CheckEnabled(string path, bool state = true)
        {
            CheckEnabledState.Check(path, state);
        }
        
        private static class CheckEnabledState
        {
            public static void Check(string path, bool state)
            {
                var go = IsExist(path);
                Assert.AreEqual(state, go.gameObject.activeInHierarchy, "IsEnable: object with path "+path+" is "+
                                                                           (go.gameObject.activeInHierarchy?"enabled":"disabled") +
                                                                           " but expected: "+(state?"enabled":"disabled"));
            }

            public static List<object> GetDefautParams(GameObject go)
            {
                List<object> result = new List<object>();
                result.Add(go.activeInHierarchy);
                return result;
            }
        }
        

#endregion

#region ExistNotExtis
        [ShowInEditor("Is Exist")]
        public static GameObject IsExist(string path)
        {
            var go = UITestUtils.FindAnyGameObject(path);
            
            if (go == null)
            {
                Assert.Fail("IsExist: Object with path " + path + " does not exist.");
            }
            return go;
        }

        public static GameObject IsExist<T>(string path) where T : Component
        {
            var go = UITestUtils.FindAnyGameObject<T>(path);
            
            if (go == null)
            {
                Assert.Fail("IsExist<"+typeof(T)+">: Object with path " + path + " does not exist.");
            }

            return go.gameObject;
        }

        public static GameObject IsExist<T>() where T : Component
        {
            var go = UITestUtils.FindAnyGameObject<T>();
            
            if (go == null)
            {
                Assert.Fail("IsExist<"+typeof(T)+">: Object does not exist.");
            }

            return go.gameObject;
        }
        
        [ShowInEditor("Is not Exist")]
        public static void IsNotExist(string path)
        {
            var go = UITestUtils.FindAnyGameObject(path);
            
            if (go != null)
            {
                Assert.Fail("IsNotExist: Object with path " + path + " exists.");
            }
        }

        public static void IsNotExist<T>(string path) where T : Component
        {
            var go = UITestUtils.FindAnyGameObject<T>(path);
            
            if (go != null)
            {
                Assert.Fail("IsNotExist<"+typeof(T)+">: Object with path " + path + " exists.");
            }
        }

        public static void IsNotExist<T>() where T : Component
        {
            var go = UITestUtils.FindAnyGameObject<T>();
            
            if (go != null)
            {
                Assert.Fail("IsNotExist<"+typeof(T)+">: Object exists.");
            }
        }
#endregion
        
#region Toggle

        public static void CheckToggle(GameObject go, bool expectedIsOn)
        {
            var toggleGO = UITestUtils.FindGameObjectWithComponentInParents<Toggle>(go);
            var toggle = toggleGO.GetComponent<Toggle>();
            
            if (toggle == null)
            {
                Assert.Fail("CheckToggle: Game object "
                            + UITestUtils.GetGameObjectFullPath(go) +
                            " has no Toggle component.");
            }
            if (toggle.isOn != expectedIsOn)
            {
                Assert.Fail("CheckToggle: Toggle "+
                            UITestUtils.GetGameObjectFullPath(go) +
                            " is "+(toggle.isOn ? "On" : "Off")+" - but expected  "+
                            " is "+(expectedIsOn ? "On" : "Off"));    
            }
        }
     
        private static class CheckToggleState
        {
            public static void Check(string path, bool state)
            {
                var go = IsExist(path);
                Toggle toggle = go.GetComponent<Toggle>();
                Assert.AreEqual(null, toggle, "CheckToggle: Game object " + path + " has no Toggle component.");
                Assert.AreNotEqual(state, toggle.isOn, "CheckToggle: Toggle "+ path +
                            " is "+(toggle.isOn ? "On" : "Off")+" - but expected  "+
                            " is "+(state ? "On" : "Off"));
            }

            public static bool IsAvailable(GameObject go)
            {
                var toggleGo = UITestUtils.FindGameObjectWithComponentInParents<Toggle>(go);
                return toggleGo && toggleGo.activeInHierarchy;
            }

            public static List<object> GetDefautParams(GameObject go)
            {
                List<object> result = new List<object>();
                var toggleGo = UITestUtils.FindComponentInParents<Toggle>(go);
                result.Add(toggleGo.isOn);
                return result;
            }
        }

        [ShowInEditor(typeof(CheckToggleState), "check toggle state")]
        public static void CheckToggle(string path, bool state)
        {
            CheckToggleState.Check(path, state);
        }
        
#endregion
 
        
#region animation
        
        [ShowInEditor(typeof(AnimatorStateStartedClass), "Animator state started", false)]
        public static IEnumerator AnimatorStateStarted(string path, string stateName, float timeout)
        {
            yield return AnimatorStateStartedClass.AnimatorStateStarted(path, stateName, timeout);
        }

        private static class AnimatorStateStartedClass
        {
            private static Func<Animator, int, string> GetCurrentStateName;
            private static Func<TThis, TArg0, TReturn> BuildFastOpenMemberDelegate<TThis, TArg0, TReturn>(string methodName)
            {
                var method = typeof(TThis).GetMethod(
                    methodName,
                    BindingFlags.Instance | BindingFlags.NonPublic,
                    null,
                    CallingConventions.Any,
                    new[] { typeof(TArg0) },
                    null);

                if (method == null)
                {
                    throw new ArgumentException("Can't find method " + typeof(TThis).FullName + "." + methodName + "(" + typeof(TArg0).FullName + ")");
                } 
                else if (method.ReturnType != typeof(TReturn))
                {
                    throw new ArgumentException("Expected " + typeof(TThis).FullName + "." + methodName + "(" + typeof(TArg0).FullName + ") to have return type of string but was " + method.ReturnType.FullName);
                }
                return (Func<TThis, TArg0, TReturn>)Delegate.CreateDelegate(typeof(Func<TThis, TArg0, TReturn>), method);
            }
            
            public static IEnumerator AnimatorStateStarted(string path, string stateName, float timeout)
            {
                var stateNames = stateName.Split(';');
                yield return Wait.ObjectEnabled(path, timeout);
                var animator = GameObject.Find(path).GetComponent<Animator>();

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
                                return true;
                            }
                        }
                    }
                    return false;
                }, timeout, "AnimatorStateStarted failed for path: " + path + "  and state name: "+stateName);
            } 
           

            public static List<object> GetDefautParams(GameObject go)
            {
                List<object> result = new List<object>{"object path", "animator state name", 2f};
                return result;
            }
        }
#endregion
    }
}