using UnityEngine;
using System.Collections;
using System;
using System.IO;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using NUnit.Framework;
using System.Linq;
using System.Threading;

public class UITest
{
    //const float WaitTimeout = 2;

    protected void LoadSceneForSetUp(string sceneName)
    {
        SceneManager.LoadScene(sceneName, LoadSceneMode.Additive);
    }

    [TearDown]
    public void Clean()
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

    protected IEnumerator WaitForAnimationPlaying(string objectName, string param, float waitTimeout = 2f)
    {
        var condition = new ObjectAnimationPlaying(objectName, param, waitTimeout);
        yield return WaitForInternal(condition, Environment.StackTrace);
    }


    protected IEnumerator WaitForObjectAppeared<T>(float waitTimeout = 2f) where T : Component
    {
        var condition = new ObjectAppeared<T>(waitTimeout);
        yield return WaitForInternal(condition, Environment.StackTrace);
    }

    protected IEnumerator WaitForObjectAppeared(string path, float waitTimeout = 2f)
    {
        var condition = new ObjectAppeared(path, waitTimeout);
        yield return WaitForInternal(condition, Environment.StackTrace);
    }

    protected IEnumerator WaitForObjectDisappeared<T>(float waitTimeout = 2f) where T : Component
    {
        var condition = new ObjectDisappeared<T>(waitTimeout);
        yield return WaitForInternal(condition, Environment.StackTrace);
    }

    protected IEnumerator WaitForObjectDisappeared(string path, float waitTimeout = 2f)
    {
        var condition = new ObjectDisappeared(path, waitTimeout);
        yield return WaitForInternal(condition, Environment.StackTrace);
    }
    
    protected IEnumerator WaitForCondition(Func<bool> func, float waitTimeout = 2f)
    {
        var condition = new BoolCondition(func, waitTimeout);
        yield return WaitForInternal(condition, Environment.StackTrace);
    }
    


    protected IEnumerator WaitFor(Condition condition)
    {
        yield return WaitForInternal(condition, Environment.StackTrace);
    }
                
    protected IEnumerator LoadScene(string name, float waitTimeout = 2f)
    {
        SceneManager.LoadScene(name, LoadSceneMode.Additive);
        yield return WaitFor(new SceneLoaded(name, waitTimeout));
    }


    protected IEnumerator AssertLabel(string id, string text, float waitTimeout = 2f)
    {
        yield return WaitFor(new LabelTextAppeared(id, text, waitTimeout));
    }

    protected IEnumerator WaitAndPress(string buttonName, float waitTimeout = 2f)
    {
        var buttonAppeared = new ObjectAppeared(buttonName, waitTimeout);
        yield return WaitFor(buttonAppeared);
        yield return WaitAndPress(buttonAppeared.o);
    }

    protected IEnumerator WaitAndPress(GameObject o, float waitTimeout = 2f)
    {
        yield return WaitFor(new ButtonAccessible(o, waitTimeout));
        Debug.Log("Button pressed: " + o);
        ExecuteEvents.Execute(o, new PointerEventData(EventSystem.current), ExecuteEvents.pointerClickHandler);
        yield return null;
    }

    IEnumerator WaitForInternal(Condition condition, string stackTrace)
    {
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

   
                
   
                
    protected abstract class Condition
    {
        public float WaitTimeout { get; private set; }

        public Condition(float waitTimeout)
        {
            WaitTimeout = waitTimeout;
        }

        public abstract bool Satisfied();
    }

    protected class LabelTextAppeared : Condition
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

    protected class SceneLoaded : Condition
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
            if(sceneCount == 0)
            {
                return false;
            }

            var loadedScenes = new Scene[sceneCount];
            for(int i=0; i<sceneCount; i++)
            {
                loadedScenes[i] = SceneManager.GetSceneAt(i);
            }


            return loadedScenes.Any(scene =>
            {
                return scene.name == sceneName;
            });
        }
    }

    protected class ObjectAnimationPlaying : Condition
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

    protected class ObjectAppeared<T> : Condition where T : Component
    {
        public ObjectAppeared(float waitTimeout = 2f) : base(waitTimeout)
        {
        }

        public override bool Satisfied()
        {
            var obj = GameObject.FindObjectOfType(typeof (T)) as T;
            return obj != null && obj.gameObject.activeInHierarchy;
        }
    }

    protected class ObjectDisappeared<T> : Condition where T : Component
    {

        public ObjectDisappeared(float waitTimeout = 2f) : base(waitTimeout)
        {
        }

        public override bool Satisfied()
        {
            var obj = GameObject.FindObjectOfType(typeof(T)) as T;
            return obj == null || !obj.gameObject.activeInHierarchy;
        }
    }

    protected class ObjectAppeared : Condition
    {
        protected string path;
        public GameObject o;

        public ObjectAppeared(string path, float waitTimeout = 2f)
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

    protected class ObjectDisappeared : Condition
    {
        protected string path;
        public GameObject o;

        public ObjectDisappeared(string path, float waitTimeout = 2f) : base(waitTimeout)
        {
            this.path = path;
        }

        public override bool Satisfied()
        {
            o = GameObject.Find(path);
            return o != null && o.activeInHierarchy;
        }

    }

    protected class BoolCondition : Condition
    {
        private Func<bool> _getter;

        public BoolCondition(Func<bool> getter, float waitTimeout = 2f) : base(waitTimeout)
        {
            _getter = getter;
        }

        public override bool Satisfied()
        {
            if (_getter == null) return false;
            return _getter();
        }

        public override string ToString()
        {
            return "BoolCondition(" + _getter + ")";
        }
    }

    protected class ButtonAccessible : Condition
    {
        GameObject button;

        public ButtonAccessible(GameObject button, float waitTimeout = 2f) : base(waitTimeout)
        {
            this.button = button;
        }

        public override bool Satisfied()
        {
            return GetAccessibilityMessage() == null;
        }

        public override string ToString()
        {
            return GetAccessibilityMessage() ?? "Button " + button.name + " is accessible";
        }

        string GetAccessibilityMessage()
        {
            if (button == null)
                return "Button " + button + " not found";
            if (button.GetComponent<Button>() == null)
                return "GameObject " + button + " does not have a Button component attached";
            return null;
        }
    }
}
