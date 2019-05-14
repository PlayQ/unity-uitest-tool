using System;
using UnityEngine;

namespace PlayQ.UITestTools
{
    public static class TestStep
    {
        public static event Action OnTestStepUpdated;
        public static void Step(string step)
        {
            currentStep = step;
            CurrentIndex++;
            Debug.Log("Step " + CurrentIndex + " (" + step + ") started.");
            if (OnTestStepUpdated != null)
            {
                OnTestStepUpdated();
            }
        }

        public static void Reset()
        {
            currentStep = null;
            CurrentIndex = 0;
            if (OnTestStepUpdated != null)
            {
                OnTestStepUpdated();
            }
        }

        public static int CurrentIndex { get; private set; }

        private static string currentStep;

        public static CurrentStep GetCurrentStep(int stepsCount)
        {
            if (currentStep == null)
            {
                return null;
            }

            if (stepsCount <= 0)
            {
                stepsCount = 1;
            }
            return new CurrentStep(currentStep, CurrentIndex, Math.Min((float) CurrentIndex / stepsCount, 1));
        }

        public class CurrentStep
        {
            public readonly string Text;
            public readonly float Progress;
            public readonly int Index;

            public override string ToString()
            {
                return "Step " + Index + " - " + Text;
            }

            public CurrentStep(string text, int index, float percent)
            {
                Index = index;
                Text = text;
                Progress = percent;
            }
        }
    }
}