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

        private IEnumerator WaitFor(WaitCondition condition)
        {
            string stackTrace = Environment.StackTrace;
            float time = 0;
            while (!condition.Satisfied())
            {
                time += Time.unscaledDeltaTime;
                if (time > condition.WaitTimeout)
                {
                    throw new Exception("Operation timed out: " + condition + "\n" + stackTrace);
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

        protected void Click(GameObject go, float waitTimeout = 2f)
        {
            if (!go.activeInHierarchy)
            {
                Assert.Fail("Trying to click to " + go.name + " but it disabled");
            }
            ExecuteEvents.Execute(go, new PointerEventData(EventSystem.current), ExecuteEvents.pointerClickHandler);
        }

        protected void Click(string path, float waitTimeout = 2f)
        {
            GameObject go = GameObject.Find(path);
            if (go == null)
            {
                Assert.Fail("Trying to click to " + path + " but it doesn't exist");
            }
            Click(go, waitTimeout);
        }


        #endregion
        
        #region Waits
        //todo investigate
        protected IEnumerator WaitForAnimationPlaying(string objectName, string param, float waitTimeout = 2f)
        {
            var condition = new ObjectAnimationPlaying(objectName, param, waitTimeout);
            yield return WaitFor(condition);
        }

        protected IEnumerator WaitForObject<T>(float waitTimeout = 2f) where T : Component
        {
            var condition = new ObjectAppeared<T>(waitTimeout);
            yield return WaitFor(condition);
        }

        protected IEnumerator WaitForObject(string path, float waitTimeout = 2f)
        {
            var condition = new ObjectAppeared(path, waitTimeout);
            yield return WaitFor(condition);
        }

        protected IEnumerator WaitForDestroy<T>(float waitTimeout = 2f) where T : Component
        {
            var condition = new ObjectDestroyed<T>(waitTimeout);
            yield return WaitFor(condition);
        }

        protected IEnumerator WaitForDestroy(string path, float waitTimeout = 2f)
        {
            var condition = new ObjectDestroyed(path, waitTimeout);
            yield return WaitFor(condition);
        }
        
        //todo implement
        protected IEnumerator WaitForDestroy(GameObject gameObject, float waitTimeout = 2f)
        {
            throw new NotImplementedException();
        }

        protected IEnumerator WaitForCondition(Func<bool> func, float waitTimeout = 2f)
        {
            var condition = new BoolCondition(func, waitTimeout);
            yield return WaitFor(condition);
        }

        protected IEnumerator LoadScene(string name, float waitTimeout = 2f)
        {
            SceneManager.LoadScene(name, LoadSceneMode.Additive);
            yield return WaitFor(new SceneLoaded(name, waitTimeout));
        }

        #endregion
        
        #region WaitConditions
        private abstract class WaitCondition
        {
            public float WaitTimeout { get; private set; }

            public WaitCondition(float waitTimeout)
            {
                WaitTimeout = waitTimeout;
            }

            public abstract bool Satisfied();
        }
        private class SceneLoaded : WaitCondition
        {
            private string sceneName;

            public SceneLoaded(string sceneName, float waitTimeout = 2f)
                : base(waitTimeout)
            {
                this.sceneName = sceneName;
            }

            public override bool Satisfied()
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
            }
        }
        private class ObjectAnimationPlaying : WaitCondition
        {
            string objectName;
            string param;

            public ObjectAnimationPlaying(string objectName, string param, float waitTimeout = 2f)
                : base(waitTimeout)
            {
                this.objectName = objectName;
                this.param = param;
            }

            public override bool Satisfied()
            {
                GameObject gameObject = GameObject.Find(objectName);
                return gameObject.GetComponent<Animation>().IsPlaying(param);
            }
        }
        private class ObjectAppeared : WaitCondition
        {
            private string path;
            public ObjectAppeared(string path, float waitTimeout = 2f)
                : base(waitTimeout)
            {
                this.path = path;
            }

            public override bool Satisfied()
            {
                var o = GameObject.Find(path);
                return o != null;
            }
        }
        private class ObjectAppeared<T> : WaitCondition where T : Component
        {
            public ObjectAppeared(float waitTimeout = 2f) : base(waitTimeout)
            {
            }

            public override bool Satisfied()
            {
                var obj = Object.FindObjectOfType(typeof(T)) as T;
                return obj != null;
            }
        }
        private class ObjectDestroyed : WaitCondition
        {
            private string path;
            private GameObject o;

            public ObjectDestroyed(string path, float waitTimeout = 2f) : base(waitTimeout)
            {
                this.path = path;
            }

            public override bool Satisfied()
            {
                o = GameObject.Find(path);
                return o == null;
            }
        }
        private class ObjectDestroyed<T> : WaitCondition where T : Component
        {

            public ObjectDestroyed(float waitTimeout = 2f) : base(waitTimeout)
            {
            }

            public override bool Satisfied()
            {
                var obj = Object.FindObjectOfType(typeof(T)) as T;
                return obj == null;
            }
        }
        private class ObjectEnabled : WaitCondition
        {
            private string path;
            private GameObject o;

            public ObjectEnabled(string path, float waitTimeout = 2f)
                : base(waitTimeout)
            {
                this.path = path;
            }

            public override bool Satisfied()
            {
                o = GameObject.Find(path);
                return o != null && o.activeInHierarchy;
            }
        }
        private class ObjectEnabled<T> : WaitCondition where T : Component
        {
            public ObjectEnabled(float waitTimeout = 2f)
                : base(waitTimeout)
            {
            }

            public override bool Satisfied()
            {
                var o = Object.FindObjectOfType(typeof(T)) as T;
                return o != null && o.gameObject.activeInHierarchy;
            }
        }
        private class ObjectDisabled : WaitCondition
        {
            private string path;
            private GameObject o;

            public ObjectDisabled(string path, float waitTimeout = 2f) : base(waitTimeout)
            {
                this.path = path;
            }

            public override bool Satisfied()
            {
                o = GameObject.Find(path);
                return o != null && !o.activeInHierarchy;
            }

        }
        private class ObjectDisabled<T> : WaitCondition where T : Component
        {
            public ObjectDisabled(float waitTimeout = 2f) : base(waitTimeout)
            {
            }

            public override bool Satisfied()
            {
                var o = Object.FindObjectOfType(typeof(T)) as T;
                return o != null && !o.gameObject.activeInHierarchy;
            }

        }
        private class BoolCondition : WaitCondition
        {
            private Func<bool> getter;

            public BoolCondition(Func<bool> getter, float waitTimeout = 2f) : base(waitTimeout)
            {
                this.getter = getter;
            }

            public override bool Satisfied()
            {
                if (getter == null) return false;
                return getter();
            }
        }
        private class ButtonAccessible : WaitCondition
        {
            private GameObject button;

            public ButtonAccessible(GameObject button, float waitTimeout = 2f) : base(waitTimeout)
            {
                this.button = button;
            }

            public override bool Satisfied()
            {
                return button != null && button.GetComponent<Button>() != null;
            }
        }
        #endregion
    }


    public static class Check
    {
        /*
        private class LabelTextAppeared : Condition
        {
            string labalName;
            string expectedText;

            public LabelTextAppeared(string labalName, string expectedText, float waitTimeout = 2f)
                : base(waitTimeout)
            {
                this.labalName = labalName;
                this.expectedText = expectedText;
            }

            public override bool Satisfied()
            {
                var go = GameObject.Find(labalName);
                if (go == null)
                {
                    Assert.Fail("Label object " + labalName + " does not exist");
                }
                if (!go.activeInHierarchy) Assert.Fail("Label object " + labalName + " is inactive");
                var t = go.GetComponent<Text>();
                if (t == null)
                {
                    Assert.Fail("Label object " + labalName + " has no Text attached");
                }
                if (t.text != expectedText)
                {
                    Assert.Fail("Label " + labalName + "\n text expected: " + expectedText + ",\n actual: " + t.text);
                }
                return true;
            }

        }
*/
        public static void TextEquals(string path, string text)
        {
            throw new NotImplementedException();
            //yield return WaitFor(new LabelTextAppeared(id, text, waitTimeout));
        }

    }
}