using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace PlayQ.UITestTools
{
    public class TestCoroutiner : MonoBehaviour
    {
        private abstract class CoroutinerExecutorBase
        {
            public bool IsNotCompleted { get; protected set; }
            public abstract YieldInstruction MoveNext();
            public abstract bool IsCreatedFrom(object corutine);
            public int YieldInstructionIndex { get; protected set; }

            protected CoroutinerExecutorBase()
            {
                IsNotCompleted = true;
            }
        }

        private class CoroutineException : Exception
        {
            public CoroutineException(Exception e) : base("", e)
            {
            }
        }
        
        private class IEnumeratorExecutor: CoroutinerExecutorBase
        {
            private IEnumerator coroutine;
            private CoroutinerExecutorBase inner;
            private YieldInstruction lastInstruction;
            
            public IEnumeratorExecutor(IEnumerator coroutine)
            {
                this.coroutine = coroutine;
            }

            public override YieldInstruction MoveNext()
            {
                if (inner != null && inner.IsNotCompleted)
                {
                    YieldInstruction result;
                    try
                    {
                        result = inner.MoveNext();
                    }
                    catch (Exception e)
                    {
                        if (e is CoroutineException)
                        {
                            throw e;
                        }
                        throw new CoroutineException(e);
                    }
                    if (result != null)
                    {
                        return result;
                    }
                    if (inner.IsNotCompleted)
                    {
                        return null;
                    }
                }
                YieldInstruction yieldInstruction = null;

                IsNotCompleted = coroutine.MoveNext();
                YieldInstructionIndex++;
                
                if (CheckCurrent(ref yieldInstruction))
                {
                    return yieldInstruction;
                }
                return null;
            }

            private bool CheckCurrent(ref YieldInstruction refYieldInstruction)
            {
                if (coroutine.Current != null)
                {
                    YieldInstruction yieldInstruction = coroutine.Current as YieldInstruction;
                    if (yieldInstruction != null)
                    {
                        if (lastInstruction != yieldInstruction)
                        {
                            lastInstruction = yieldInstruction;
                            refYieldInstruction = yieldInstruction;
                            return true;
                        }
                    }
                    else
                    {
                        if (inner == null || !inner.IsCreatedFrom(coroutine.Current))
                        {
                            IEnumerator iEnumerator = coroutine.Current as IEnumerator;
                            IEnumerable iEnumerable = coroutine.Current as IEnumerable;
                            if (iEnumerator != null)
                            {
                                inner = new IEnumeratorExecutor(iEnumerator);
                            }
                            if (iEnumerable != null)
                            {
                                inner = new IEnumerableExecutor(iEnumerable);
                            }
                            if (inner == null)
                            {
                                Debug.LogError(coroutine.Current.GetType());
                            }
                            refYieldInstruction = inner.MoveNext();
                            return true;
                        }
                    }
                }
                return false;
            }

            public override bool IsCreatedFrom(object corutine)
            {
                return corutine == this.coroutine;
            }
        }
        
        private class IEnumerableExecutor: CoroutinerExecutorBase
        {
            private IEnumerable corutine;
            private List<IEnumeratorExecutor> executors;
            
            public IEnumerableExecutor(IEnumerable corutine)
            {
                this.corutine = corutine;
                executors = new List<IEnumeratorExecutor>();
                foreach (IEnumerator yieldInstruction in corutine)
                {
                    executors.Add(new IEnumeratorExecutor(yieldInstruction));
                }
            }

            public override YieldInstruction MoveNext()
            {
                if (executors.Count == 0)
                {
                    IsNotCompleted = false;
                    return null;
                }
                YieldInstructionIndex++;
                var result = executors[0].MoveNext();
                if (!executors[0].IsNotCompleted)
                {
                    executors.RemoveAt(0);
                }
                return result;
            }

            public override bool IsCreatedFrom(object corutine)
            {
                return corutine == this.corutine;
            }
        }
        
        protected void StopTestCoroutine()
        {
            enumeratorExecutor = null;
            if (onDone != null)
            {
                onDone();
                onDone = null;
            }
        }

        private CoroutinerExecutorBase enumeratorExecutor;
        private string className;
        private string methodName;
        private Action onDone;
        
        protected void StartTestCoroutine(IEnumerable coroutine, string className, string methodName, Action onDone)
        {
            SetTestCoroutineParams(className, methodName, onDone);
            enumeratorExecutor = new IEnumerableExecutor(coroutine);
        }
        protected void StartTestCoroutine(IEnumerator coroutine, string className, string methodName, Action onDone)
        {
            SetTestCoroutineParams(className, methodName, onDone);
            enumeratorExecutor = new IEnumeratorExecutor(coroutine);
        }
        private void SetTestCoroutineParams(string className, string methodName, Action onDone)
        {
            this.className = className;
            this.methodName = methodName;
            this.onDone = onDone;
        }

        private bool processingYieldInstruction;
        private IEnumerator ProcessYieldInstruction(YieldInstruction yieldInstruction)
        {
            processingYieldInstruction = true;
            yield return yieldInstruction;
            processingYieldInstruction = false;
        }
        
        protected virtual void Update()
        {
            if (!processingYieldInstruction)
            {
                if (enumeratorExecutor != null)
                {
                    if (enumeratorExecutor.IsNotCompleted)
                    {
                        YieldInstruction yieldInstruction = null;
                        try
                        {
                            yieldInstruction = enumeratorExecutor.MoveNext();
                        }
                        catch (Exception ex)
                        {
                            if (ex is CoroutineException)
                            {
                                string result = ClassFileFinder.FindStackTrace(className, methodName, enumeratorExecutor.YieldInstructionIndex);
                                throw new Exception(result, ex.InnerException);
                            }
                            throw ex;
                        }
                        if (yieldInstruction != null)
                        {
                            StartCoroutine(ProcessYieldInstruction(yieldInstruction));
                        }
                    }
                    else
                    {
                        StopTestCoroutine();
                    }
                }
            }
        }
    }
}