using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace PlayQ.UITestTools
{
    [Serializable]
    public class UITestUserFlowRecorderController
    {
        private Vector2 startMousePos;
        public bool IsRecording { get; private set; }
        private bool isClickMode = true;
        public bool IsAssertationMode { get; private set; } 
        public bool IsForceRaycastEnabled { get; private set; }
        public bool IsTimeScaleZero { get; private set; }
        public string GeneratedCode;
        private GameObject screenBlocker;
        private UserFlowModel userFlowModel = new UserFlowModel();

        private List<UserActionInfo> fakeUserActions;
        public List<UserActionInfo> UserActions
        {
            get
            {
                if (userFlowModel == null)
                {
                    if (fakeUserActions == null)
                    {
                        fakeUserActions = new List<UserActionInfo>();   
                    }
                    return fakeUserActions;
                }
                else
                {
                    return userFlowModel.UserActions;
                }
            }
        }

        private bool previousEditorPaused;
        private bool previousEditorStateIsPlaying;
        public bool Update()
        {
            if (!EditorApplication.isPlaying)
            {
                if (previousEditorStateIsPlaying && IsRecording)
                {
                    StartOrStopRecording();
                    IsForceRaycastEnabled = false;
                    userFlowModel.ForceEnableRaycast(IsForceRaycastEnabled);
                    previousEditorStateIsPlaying = EditorApplication.isPlaying;
                    IsTimeScaleZero = false;
                    return true;
                }
                return false;
            }
            previousEditorStateIsPlaying = EditorApplication.isPlaying;

            if (Input.GetMouseButtonDown(0))
            {
                startMousePos = Input.mousePosition;
            }
            
            if (Input.GetMouseButtonUp(0))
            {
                Vector2 currentPos = Input.mousePosition;
                if (isClickMode)
                {
                    HandleClick(currentPos);
                }
                else
                {
                    HandleDrag(startMousePos, currentPos);
                }
                return true;
            }

            if (previousEditorPaused != EditorApplication.isPaused)
            {
                previousEditorPaused = EditorApplication.isPaused;
                return true;
            }
            previousEditorPaused = EditorApplication.isPaused;
            return false;
        }

        private void HandleDrag(Vector2 startPos, Vector2 endPos)
        {

        }

        private void HandleClick(Vector2 pos)
        {
            userFlowModel.HandleClick(pos);
        }


        private void HideScreenBlocker()
        {
            if (screenBlocker)
            {
                screenBlocker.SetActive(false);
            }
        }

        public void CreateNoGameobjectAction()
        {
            userFlowModel.CreateNoGameobjectAction();
        }
        
        public void EnableOrDisableForceRaycast()
        {
            IsForceRaycastEnabled = !IsForceRaycastEnabled;
            userFlowModel.ForceEnableRaycast(IsForceRaycastEnabled);
        }
        
        public void EnableOrDisableTimescalePause()
        {
            IsTimeScaleZero = !IsTimeScaleZero;
            Time.timeScale = IsTimeScaleZero ? 0 : 1;
        }

        private void AddScreenBlocker()
        {
            if (!screenBlocker)
            {
                screenBlocker = new GameObject();
                screenBlocker.name = UserFlowModel.UI_TEST_SCREEN_BLOCKER;
                var canvas = screenBlocker.AddComponent<Canvas>();
                screenBlocker.AddComponent<CanvasScaler>();
                var image = screenBlocker.AddComponent<Image>();
                screenBlocker.AddComponent<GraphicRaycaster>();

                image.color = new Color(0, 0, 0, 0);
                canvas.sortingOrder = 9999;
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            }
            screenBlocker.SetActive(true);
        }

        public void ApplyAction(UserActionInfo action)
        {
            if (userFlowModel != null)
            {
                userFlowModel.ApplyAction(action);   
            }           
        }
        
        public void GenerateCode(bool isGenerateDebugStrings)
        {
            GeneratedCode = userFlowModel.GeneratedCode(isGenerateDebugStrings);

            CopyToBuffer();
        }

        public void CopyToBuffer()
        {
            if (!String.IsNullOrEmpty(GeneratedCode))
            {
                EditorGUIUtility.systemCopyBuffer = GeneratedCode;
            }
        }

        //todo create 2 methods
        public void StartOrStopRecording()
        {
            IsRecording = !IsRecording;
            if (IsRecording)
            {
                userFlowModel.FetchAssertationMethods();
            }
            else
            {
                IsAssertationMode = false;
            }
        }

        public void ChagneAssertation()
        {
            IsAssertationMode = !IsAssertationMode;
            if (IsAssertationMode)
            {
                AddScreenBlocker();
            }
            else
            {
                HideScreenBlocker();
                IsForceRaycastEnabled = false;
                userFlowModel.ForceEnableRaycast(IsForceRaycastEnabled);
            }
        }

        public void Clean()
        {
            if (userFlowModel != null)
            {
                userFlowModel.CleanFlow();
                GeneratedCode = null;
            }
        }
    }

}