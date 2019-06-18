using System;
using UnityEngine;
using UnityEngine.UI;

namespace PlayQ.UITestTools
{
    /// <summary>
    /// Contains methods which allow waiting until the game components and objects on the scene reach the certain state asynchronously
    /// </summary>
    public static partial class AsyncWait
    {
#region LogWaiting
        /// <summary>
        /// Starts the waiting for the log
        /// </summary>
        /// <returns>Abstract async waiter</returns>
        /// <param name="message">Expected log message</param>
        /// <param name="expectedErrorMessageCouldBeSubstring">Is expected log is substring of error log</param>
        /// <param name="timeout">Timeout (optional, default = 10)</param>
        [ShowInEditor(typeof(StartWaitingForLogClass), "Async Wait/Start Waiting For Log", false)]
        public static AbstractAsyncWaiter StartWaitingForLog(string message,
            LogType logType = LogType.Log,
            bool expectedErrorMessageCouldBeSubstring = false,
            float timeout = 10)
        {
            return new LogWaiter(message, false, expectedErrorMessageCouldBeSubstring, logType, timeout);
        }
        
        /// <summary>
        /// Starts the waiting for the log
        /// </summary>
        /// <returns>Abstract async waiter</returns>
        /// <param name="message">Expected log message</param>
        /// <param name="isRegExp">Is expected log a regular expression</param>
        /// <param name="timeout">Timeout (optional, default = 10)</param>
        [ShowInEditor(typeof(StartWaitingForLogRegExpClass), "Async Wait/Start Waiting For Log RegExp", false)]
        public static AbstractAsyncWaiter StartWaitingForLogRegExp(string message, LogType logType = LogType.Log,
            float timeout = 10)
        {
            return new LogWaiter(message, true, false, logType, timeout);
        }        

        /// <summary>
        /// Used only for generation waiting code for StartWaitingForLog 
        /// </summary>
        [ShowInEditor(typeof(StopWaitingForLogClass), "Async Wait/Stop Waiting For Log", false)]
        public static void StopWaitingForGameLog()
        {
            throw new Exception("StopWaitingForGameLog method is only for code generation, don't use it");
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
            private LogType logType;

            public LogWaiter(string message, bool isRegExp,  bool expectedErrorMessageCouldBeSubstring,
                LogType logType, float timeout)
            {
                this.logType = logType;
                Application.logMessageReceived += ApplicationOnLogMessageReceived;
                OnCompleteCallback += OnComplete;
                stringComparator = UITestUtils.GetStringComparator(message, isRegExp, expectedErrorMessageCouldBeSubstring);
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
                if (logType != this.logType)
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
                        .Enum(LogType.Error)
                        .Bool(false)
                        .Float(10);
            }
        }
        private class StartWaitingForLogRegExpClass : ShowHelperBase
        {
            public override AbstractGenerator CreateGenerator(GameObject go)
            {
                return
                    new CreateWaitVariable<LogWaiter>()
                        .Append(new MethodName())
                        .String("empty log")
                        .Enum(LogType.Error)
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
        
#endregion
        
#region WaitingForSourceImage
        [ShowInEditor(typeof(StartWaitingForSourceImageClass), "Async Wait/Start Waiting For Source Image", false)]
        public static AbstractAsyncWaiter StartWaitingForSourceImage(string path, string animationName, float timeout = 10)
        {
            return new SourceImageWaiter(path, animationName, timeout);
        }
        
        [ShowInEditor(typeof(StopWaitingForSourceImageClass), "Async Wait/Stop Waiting For Source Image", false)]
        public static void StopWaitingForSourceImage()
        {
            throw new Exception("StopWaitingForSourceImageClass method is only for code generation, don't use it");
        }
        
        private class StopWaitingForSourceImageClass : ShowHelperBase
        {
            public override AbstractGenerator CreateGenerator(GameObject go)
            {
                return
                    new WaitForVariable<SourceImageWaiter>();
            }
        }

        private class SourceImageWaiter : AbstractAsyncWaiter
        {
            private string sourceName;
            private string path;
            private string reason;

            protected override string errorMessage
            {
                get { return reason; }
            }

            public SourceImageWaiter(string path, string sourceName, float timeout)
            {
                reason = string.Empty;
                this.path = path;
                this.sourceName = sourceName;
                StartWait(timeout);
            }

            protected override bool Check()
            {
                var go = UITestUtils.FindEnabledGameObjectByPath(path);
                var image = UITestUtils.FindGameObjectWithComponentInParents<Image>(go);
                var sr = UITestUtils.FindGameObjectWithComponentInParents<SpriteRenderer>(go);

                if (image != null)
                {
                    if (image.mainTexture == null)
                    {
                        reason = "SourceImage: Image " +
                                 UITestUtils.GetGameObjectFullPath(go) +
                                 " has no texture.";
                        return false;
                    }
                    if (image.mainTexture.name != sourceName)
                    {
                        reason = "SourceImage: Image " +
                                 UITestUtils.GetGameObjectFullPath(go) +
                                 " has texture: " + image.mainTexture.name +
                                 " - but expected: " +
                                 sourceName;
                        return false;
                    }
                    return true;
                }
                if (sr != null)
                {
                    if (sr.sprite == null)
                    {
                        reason = "SourceImage: SpriteRenderer " +
                                 UITestUtils.GetGameObjectFullPath(go) +
                                 " has no sprite.";
                        return false;
                    }
                    if (sr.sprite.name != sourceName)
                    {
                        reason = "SourceImage: SpriteRenderer " +
                                 UITestUtils.GetGameObjectFullPath(go) +
                                 " has sprite: " + sr.sprite.name +
                                 " - but expected: " +
                                 sourceName;
                        return false;
                    }
                    return true;
                }
                reason = "SourceImage: Game object "
                         + UITestUtils.GetGameObjectFullPath(go) +
                         " has no Image and SpriteRenderer component.";
                return false;
            }
        }

        private class StartWaitingForSourceImageClass : ShowHelperBase
        {
            public override AbstractGenerator CreateGenerator(GameObject go)
            {
                var image = UITestUtils.FindGameObjectWithComponentInParents<Image>(go);
                var sr = UITestUtils.FindGameObjectWithComponentInParents<SpriteRenderer>(go);

                if (image != null)
                {
                    if (image.mainTexture!= null)
                    {
                        return
                            new CreateWaitVariable<UnityAnimationStartWaiter>()
                                .Append(new MethodName())
                                .Path(go)
                                .String(image.mainTexture.name)
                                .Float(10);
                    }
                    return
                        new CreateWaitVariable<UnityAnimationStartWaiter>()
                            .Append(new MethodName())
                            .Path(go)
                            .String(String.Empty)
                            .Float(10);
                }
                if (sr != null)
                {
                    if (sr.sprite != null)
                    {
                        return
                            new CreateWaitVariable<UnityAnimationStartWaiter>()
                                .Append(new MethodName())
                                .Path(go)
                                .String(sr.sprite.name)
                                .Float(10);
                    }
                    return
                        new CreateWaitVariable<UnityAnimationStartWaiter>()
                            .Append(new MethodName())
                            .Path(go)
                            .String(String.Empty)
                            .Float(10);
                }
                throw new ArgumentException();
            }

            public override bool IsAvailable(GameObject go)
            {
                if (!go)
                {
                    return false;
                }
                var image = UITestUtils.FindGameObjectWithComponentInParents<Image>(go);
                var sr = UITestUtils.FindGameObjectWithComponentInParents<SpriteRenderer>(go);
                return image || sr;
            }
        }
#endregion

#region Animation
        /// <summary>
        /// Starts the waiting for unity animation to complete
        /// </summary>
        /// <returns>Abstract async waiter</returns>
        /// <param name="path">Path to `GameObject` in hierarchy</param>
        /// <param name="animationName">Animation name</param>
        /// <param name="timeout">Timeout (optional, default = 10)</param>
        [ShowInEditor(typeof(StartWaitingForUnityAnimationClass), "Async Wait/Start Waiting For Unity Animation", false)]
        public static AbstractAsyncWaiter StartWaitingForUnityAnimation(string path, string animationName, float timeout = 10)
        {
            return new UnityAnimationStartWaiter(path, animationName, timeout);
        }

        /// <summary>
        /// Used only for generation waiting code for StartWaitingForUnityAnimation 
        /// </summary>
        [ShowInEditor(typeof(StopWaitingForAnimationClass), "Async Wait/Stop Waiting For Unity Animation", false)]
        public static void StopWaitingForUnityAnimation()
        {
            throw new Exception("StopWaitingForUnityAnimation method is only for code generation, don't use it");
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
        
        private class StopWaitingForAnimationClass : ShowHelperBase
        {
            public override AbstractGenerator CreateGenerator(GameObject go)
            {
                return
                    new WaitForVariable<UnityAnimationStartWaiter>();
            }
        }
#endregion
        
    }
}