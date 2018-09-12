using System;
using System.Text.RegularExpressions;
using UnityEngine;

namespace PlayQ.UITestTools
{
    public static partial class AsyncWait
    {
        [ShowInEditor(typeof(StartWaitingForLogClass), "Async Wait/Start Waiting For Log", false)]
        public static AbstractAsyncWaiter StartWaitingForLog(string message, bool isRegExp,
            float timeout = 10)
        {
            return new LogWaiter(message, isRegExp, timeout);
        }

        [ShowInEditor(typeof(StartWaitingForUnityAnimationClass), "Async Wait/Start Waiting For Unity Animation", false)]
        public static AbstractAsyncWaiter StartWaitingForUnityAnimation(string path, string animationName, float timeout = 10)
        {
            return new UnityAnimationStartWaiter(path, animationName, timeout);
        }

        [ShowInEditor(typeof(StopWaitingForLogClass), "Async Wait/Stop Waiting For Log", false)]
        public static void StopWaitingForGameLog()
        {
            throw new Exception("StopWaitingForGameLog method is only for code generation, don't use it");
        }

        private class UnityAnimationStartWaiter : AbstractAsyncWaiter
        {
            private string animationName;
            private string path;
            private int debug;

            protected override string errorMessage
            {
                get
                {
                    string reason = "";
                    switch (debug)
                    {
                        case 1:
                            reason = "no enabled gameobject on scene";
                            break;
                        case 2:
                            reason = "no Animation component on gameobject";
                            break;
                        case 3:
                            reason = "no expected clip playing in Animation component";
                            break;
                    }

                    return "cant wait animation for gameobject path: "
                           + path + " animation clip name: " + animationName
                           + " " + reason;
                }
            }

            public UnityAnimationStartWaiter(string path, string animationName, float timeout)
            {
                debug = 0;
                this.path = path;
                this.animationName = animationName;
                StartWait(timeout);
            }

            protected override bool Check()
            {
                var go = UITestUtils.FindEnabledGameObjectByPath(path);
                if (go)
                {
                    var animation = go.GetComponent<Animation>();
                    if (animation)
                    {
                        if (animation.IsPlaying(animationName))
                        {
                            return true;
                        }
                        else
                        {
                            debug = 3;
                        }
                    }
                    else
                    {
                        debug = 2;
                    }
                }
                else
                {
                    debug = 1;
                }

                return false;
            }
        }

        private class StartWaitingForUnityAnimationClass : ShowHelperBase
        {

            public override AbstractGenerator CreateGenerator(GameObject go)
            {
                var clipName = "no_animation";
                var anim = go.GetComponent<Animation>();
                if (anim)
                {
                    foreach (AnimationState state in anim)
                    {
                        var isPlaying = anim.IsPlaying(state.clip.name);
                        if (isPlaying)
                        {
                            clipName = state.clip.name;
                            break;
                        }
                    }
                }

                return
                    new CreateWaitVariable<UnityAnimationStartWaiter>()
                        .Append(new MethodName())
                        .Path(go)
                        .String(clipName)
                        .Float(10);
            }

            public override bool IsAvailable(GameObject go)
            {
                if (go)
                {
                    var animation = go.GetComponent<Animation>();
                    return animation != null;
                }
                return false;
            }
        }
        public class LogWaiter : AbstractAsyncWaiter
        {
            protected override string errorMessage
            {
                get { return "can't wait for log: " + message; }
            }

            private string message;
            private bool isComplete;
            private IStringComparator stringComparator;

            public LogWaiter(string message, bool isRegExp, float timeout)
            {
                Application.logMessageReceived += ApplicationOnLogMessageReceived;
                OnCompleteCallback += OnComplete;
                stringComparator = UITestUtils.GetStringComparator(message, isRegExp);
                this.message = message;
                StartWait(timeout);
            }

            private void OnComplete()
            {
                OnCompleteCallback -= OnComplete;
                Application.logMessageReceived -= ApplicationOnLogMessageReceived;
            }

            private void ApplicationOnLogMessageReceived(string condition, 
                string stackTrace, LogType logType)
            {
                if (logType == LogType.Error)
                {
                    return;
                }

                isComplete = stringComparator.TextEquals(condition);

                if (isComplete)
                {
                    Application.logMessageReceived -= ApplicationOnLogMessageReceived;
                }
            }
                

            protected override bool Check()
            {
                return isComplete;
            }
        }

        private class StartWaitingForLogClass : ShowHelperBase
        {
            public override AbstractGenerator CreateGenerator(GameObject go)
            {
                return
                    new CreateWaitVariable<LogWaiter>()
                        .Append(new MethodName())
                        .String("empty log")
                        .Bool(false)
                        .Float(10);
            }
        }
        
        private class StopWaitingForLogClass : ShowHelperBase
        {
            public override AbstractGenerator CreateGenerator(GameObject go)
            {
                return
                    new WaitForVariable<LogWaiter>();
            }
        }
        
    }
}