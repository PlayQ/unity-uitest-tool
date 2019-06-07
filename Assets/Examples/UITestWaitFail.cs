﻿using System.Collections;
 using UnityEngine;
 using UnityEngine.SceneManagement;
 using Object = UnityEngine.Object;

namespace PlayQ.UITestTools.Tests
{
    public class UITestWaitFail
    {
        [TearDown]
        public void TearsDown()
        {
            PermittedErrors.Clear();
        }
        
        [SetUp]
        public IEnumerator LoadSceneSetUp()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;

            sceneLoaded = false;

            SceneManager.LoadScene("1", LoadSceneMode.Additive);

            while (!sceneLoaded)
            {
                yield return null;
            }
        }

        void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            sceneLoaded = true;

            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private bool sceneLoaded;

        [TearDown]
        public IEnumerator UnloadSceneTearDown()
        {
            SceneManager.sceneUnloaded += OnSceneUnloaded;

            sceneUnloaded = false;

            SceneManager.UnloadSceneAsync("1");

            while (!sceneUnloaded)
            {
                yield return null;
            }
        }

        void OnSceneUnloaded(Scene scene)
        {
            sceneUnloaded = true;

            SceneManager.sceneUnloaded -= OnSceneUnloaded;
        }

        private bool sceneUnloaded;


        [UnityTest]
        public IEnumerator WaitForObjectByName()
        {
            var testObject = UITestUtils.FindAnyGameObject<TestObject>().gameObject;
            Object.DestroyImmediate(testObject);

            var exceptionMessage = "Exception: Operation timed out: Wait failed, reason: WaitForObject path: container"; 
            PermittedErrors.AddPermittedError(exceptionMessage);
            var waiter = AsyncWait.StartWaitingForLog(exceptionMessage, LogType.Exception);
            
            yield return Wait.ObjectInstantiated("container");

            yield return waiter;
        }

        [UnityTest]
        public IEnumerator WaitForObjectByComponent()
        {
            var testObject = UITestUtils.FindAnyGameObject<TestObject>();
            Object.DestroyImmediate(testObject);

            var exceptionMessage = "Exception: Operation timed out: Wait failed, reason: WaitForObject<PlayQ.UITestTools.Tests.TestObject>"; 
            PermittedErrors.AddPermittedError(exceptionMessage);
            var waiter = AsyncWait.StartWaitingForLog(exceptionMessage, LogType.Exception);
            
            yield return Wait.ObjectInstantiated<TestObject>();
            yield return waiter;
        }

        [UnityTest]
        public IEnumerator WaitForObjectByComponentAndName()
        {
            var testObject = UITestUtils.FindAnyGameObject<TestObject>().gameObject;
            Object.DestroyImmediate(testObject);
            
            var exceptionMessage = "Exception: Operation timed out: Wait failed, reason: WaitForObject<PlayQ.UITestTools.Tests.TestObject>"; 
            PermittedErrors.AddPermittedError(exceptionMessage);
            var waiter = AsyncWait.StartWaitingForLog(exceptionMessage, LogType.Exception);

            yield return Wait.ObjectInstantiated<TestObject>("container");
            yield return waiter;
        }

        [UnityTest]
        public IEnumerator WaitForObjectDestractionByComponent()
        {
            var exceptionMessage = "Exception: Operation timed out: Wait failed, reason: WaitForDestroy<PlayQ.UITestTools.Tests.TestObject>"; 
            PermittedErrors.AddPermittedError(exceptionMessage);
            var waiter = AsyncWait.StartWaitingForLog(exceptionMessage, LogType.Exception);
            
            yield return Wait.ObjectDestroyed<TestObject>(1);
            yield return waiter;
        }

        [UnityTest]
        public IEnumerator WaitForObjectDestractionByInstance()
        {
            var objectInstance = UITestUtils.FindAnyGameObject<TestObject>().gameObject;

            var exceptionMessage = "Exception: Operation timed out: Wait failed, reason: WaitForDestroy UnityEngine.GameObject"; 
            PermittedErrors.AddPermittedError(exceptionMessage);
            var waiter = AsyncWait.StartWaitingForLog(exceptionMessage, LogType.Exception);
            
            yield return Wait.ObjectDestroyed(objectInstance);
            yield return waiter;
        }

        [UnityTest]
        public IEnumerator WaitForObjectDestractionByName()
        {
            var exceptionMessage = "Exception: Operation timed out: Wait failed, reason: WaitForDestroy path: container"; 
            PermittedErrors.AddPermittedError(exceptionMessage);
            var waiter = AsyncWait.StartWaitingForLog(exceptionMessage, LogType.Exception);
            
            yield return Wait.ObjectDestroyed("container");
            yield return waiter;
        }

        [UnityTest]
        public IEnumerator WaitForObjectDisabledByName()
        {
            var exceptionMessage = "Exception: Operation timed out: Wait failed, reason: WaitObjectDisabled path: Object_enabled_at_start"; 
            PermittedErrors.AddPermittedError(exceptionMessage);
            var waiter = AsyncWait.StartWaitingForLog(exceptionMessage, LogType.Exception);
            
            yield return Wait.ObjectDisabled("Object_enabled_at_start");
            yield return waiter;

        }


        [UnityTest]
        public IEnumerator WaitForObjectDisabledByComponent()
        {
            var exceptionMessage = "Exception: Operation timed out: Wait failed, reason: WaitObjectDisabled<PlayQ.UITestTools.Tests.TestObject>"; 
            PermittedErrors.AddPermittedError(exceptionMessage);
            var waiter = AsyncWait.StartWaitingForLog(exceptionMessage, LogType.Exception);
            
            yield return Wait.ObjectDisabled<TestObject>();
            yield return waiter;
        }


        [UnityTest]
        public IEnumerator WaitForObjectEnabledByComponent()
        {
            var exceptionMessage = "Exception: Operation timed out: Wait failed, reason: WaitObjectEnabled<PlayQ.UITestTools.Tests.TestObject>"; 
            PermittedErrors.AddPermittedError(exceptionMessage);
            var waiter = AsyncWait.StartWaitingForLog(exceptionMessage, LogType.Exception);
            
            var testObject = UITestUtils.FindAnyGameObject<TestObject>().gameObject;
            testObject.SetActive(false);
            //check for enabled by component type
            yield return Wait.ObjectEnabled<TestObject>();
            yield return waiter;
        }

        [UnityTest]
        public IEnumerator WaitForObjectEnabledByName()
        {
            var exceptionMessage = "Exception: Operation timed out: Wait failed, reason: WaitObjectEnabled path: container"; 
            PermittedErrors.AddPermittedError(exceptionMessage);
            var waiter = AsyncWait.StartWaitingForLog(exceptionMessage, LogType.Exception);
            
            var testObject = UITestUtils.FindAnyGameObject<TestObject>().gameObject;
            testObject.SetActive(false);

            //check for enabled by path in hierarchy
            yield return Wait.ObjectEnabled("container");
            yield return waiter;
        }

        [UnityTest]
        public IEnumerator WaitForButtonAccesible()
        {
            var exceptionMessage = "Exception: Operation timed out: Wait failed, reason: ButtonAccessible UnityEngine.GameObject"; 
            PermittedErrors.AddPermittedError(exceptionMessage);
            var waiter = AsyncWait.StartWaitingForLog(exceptionMessage, LogType.Exception);
            
            var button = UITestUtils.FindAnyGameObject("container");
            yield return Wait.ButtonAccessible(button);
            yield return waiter;
        }

    }

}