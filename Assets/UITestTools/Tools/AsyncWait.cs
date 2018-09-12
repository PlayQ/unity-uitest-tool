using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using PlayQ.UITestTools.WaitResults;
using UnityEngine;

namespace PlayQ.UITestTools
{
    public static class WaitVariablesContainer
    {
        public static bool SilentMode;
        public static Dictionary<Type, int> currentWaitingVariablesByType = new Dictionary<Type, int>();
        public static Dictionary<Type, int> maxAmountOfCreatedWaitingVariablesByType = new Dictionary<Type, int>();

        public static void Clear()
        {
            if (currentWaitingVariablesByType != null)
            {
                currentWaitingVariablesByType.Clear();
            }
            if (maxAmountOfCreatedWaitingVariablesByType != null)
            {
                maxAmountOfCreatedWaitingVariablesByType.Clear();
            }
        }
    }
    
    public abstract class AbstractAsyncWaiter : CustomYieldInstruction
    {
        protected class InnerCoroutine : CustomYieldInstruction
        {
            private readonly IEnumerator enumerator;
            public bool IsDone { get; private set; }
            private Action OnCompleteCallback;

            public InnerCoroutine(IEnumerator enumerator, Action OnCompleteCallback)
            {
                this.OnCompleteCallback = OnCompleteCallback;
                this.enumerator = enumerator;
            }
                
            public override bool keepWaiting
            {
                get
                {
                    bool result;
                    Exception exception = null;
                    try
                    {
                        result = enumerator.MoveNext();
                    }
                    catch (Exception ex)
                    {
                        exception = ex;
                        result = false;
                    }
                   
                    IsDone = !result;
                    if (IsDone)
                    {
                        if (OnCompleteCallback != null)
                        {
                            OnCompleteCallback();
                        }
                    }

                    if (exception != null)
                    {
                        throw exception;
                    }
                    return result;
                }
            }
        }
        protected InnerCoroutine innerCroutine;

        protected virtual string errorMessage
        {
            get { return "error"; }
        }
        public virtual void StopAndCheck()
        {
            if (IsDone)
            {
                return;
            }
                
            CoroutineProvider.Mono.StopCoroutine(innerCroutine);
            var result = Check();
                
            if (!result)
            {
                Assert.Fail(errorMessage);
            }
        }

        protected virtual void StartWait(float timeout, bool ignoreTimeScale = false)
        {
            var iEnumerator = Wait.WaitFor(()=>
            {
                if (Check())
                {
                    return new WaitSuccess();
                }
                return new WaitFailed(errorMessage);
            }, timeout, ignoreTimeScale: ignoreTimeScale);
                
            innerCroutine = new InnerCoroutine(iEnumerator, OnCompleteCallback);
            CoroutineProvider.Mono.StartCoroutine(innerCroutine);
        }

        protected Action OnCompleteCallback;
        
        public override bool keepWaiting
        {
            get { return !IsDone; }
        }
        
        public bool IsDone
        {
            get { return innerCroutine != null && innerCroutine.IsDone; }
        }

        protected abstract bool Check();

    }
    
    public class WaitForVariable<T> : AbstractGenerator where T : AbstractAsyncWaiter
    {
        protected override string GenerateFromMethodInfo(MethodInfo methodInfo)
        {
            int current; 
            var result = WaitVariablesContainer
                .currentWaitingVariablesByType.TryGetValue(typeof(T), out current);
            if (!result)
            {
                if (!WaitVariablesContainer.SilentMode)
                {
                    Debug.LogError("You are trying generate yield return for varible of type: " +
                                   typeof(T).FullName + " but variable has not " +
                                   "beed created before in the test code");
                }
                return "";
            }
            
            if (WaitVariablesContainer.currentWaitingVariablesByType[typeof(T)] > 0)
            {
                WaitVariablesContainer.currentWaitingVariablesByType[typeof(T)]--;
            }
            else
            {
                if (!WaitVariablesContainer.SilentMode)
                {
                    Debug.LogError("You are trying generate yield return for varible of type: " +
                                   typeof(T).FullName + " but you have already done it before " +
                                   " in test code. Yield return instruction will be skipped");
                }
                return "";
            }
            var name = new StringBuilder(typeof(T).Name);
            var firstLetter = name[0].ToString().ToLower();
            name[0] = firstLetter[0];

            name.Insert(0, "yield return ");
            name.Append(current);
            name.Append(";");
            return name.ToString();
        }

        protected override AbstractGenerator MemberwiseClone()
        {
            return new WaitForVariable<T>();
        }
    }

    public class CreateWaitVariable<T> : AbstractGenerator where T : AbstractAsyncWaiter
    {
        protected override string GenerateFromMethodInfo(MethodInfo methodInfo)
        {
            int currentWaitingVariables;
            WaitVariablesContainer.
                currentWaitingVariablesByType.TryGetValue(typeof(T), 
                out currentWaitingVariables);
            
            currentWaitingVariables++;
            WaitVariablesContainer.
                currentWaitingVariablesByType[typeof(T)] = currentWaitingVariables;
            
            int maxAmountOfCreatedWaitingVariables;
            WaitVariablesContainer.
                maxAmountOfCreatedWaitingVariablesByType.TryGetValue(typeof(T),
                out maxAmountOfCreatedWaitingVariables);
            
            var name = new StringBuilder(typeof(T).Name);
            var firstLetter = name[0].ToString().ToLower();
            name[0] = firstLetter[0];
            
            var isNewVariableName = currentWaitingVariables > maxAmountOfCreatedWaitingVariables;
            if (isNewVariableName)
            {
                WaitVariablesContainer.
                    maxAmountOfCreatedWaitingVariablesByType[typeof(T)] = currentWaitingVariables;
                name.Insert(0, "var ");
            }
            
            name.Append(currentWaitingVariables);
            name.Append(" = ");
            return name.ToString();
        }

        protected override AbstractGenerator MemberwiseClone()
        {
            return new CreateWaitVariable<T>();
        }
    }
    
    public class StopAndCheckWaitVariable<T> : AbstractGenerator where T : AbstractAsyncWaiter
    {
        protected override string GenerateFromMethodInfo(MethodInfo methodInfo)
        {
            int current; 
            var result = WaitVariablesContainer
                .currentWaitingVariablesByType.TryGetValue(typeof(T), out current);
            if (!result)
            {
                if (!WaitVariablesContainer.SilentMode)
                {
                    Debug.LogError("You are trying generate call of method 'StopAndCheck()' for" +
                                   " varible of type: " + typeof(T).FullName +
                                   " but variable has not beed created before in" +
                                   " the test code. Call will be skipped");
                }
                return "";
            }

            if (WaitVariablesContainer.currentWaitingVariablesByType[typeof(T)] > 0)
            {
                WaitVariablesContainer.currentWaitingVariablesByType[typeof(T)]--;
            }
            else
            {
                if (!WaitVariablesContainer.SilentMode)
                {
                    Debug.LogError("You are trying generate call of method 'StopAndCheck()' for" +
                                   " varible of type: " + typeof(T).FullName +
                                   " but you already have called this method before in the test code." +
                                   " Call will be skipped");
                }
                return "";
            }
            
                
            var name = new StringBuilder(typeof(T).Name);
            var firstLetter = name[0].ToString().ToLower();
            name[0] = firstLetter[0];
            name.Append(current);
            name.Append(".StopAndCheck();");
            return name.ToString();
        }

        protected override AbstractGenerator MemberwiseClone()
        {
            return new StopAndCheckWaitVariable<T>();
        }
    }

}