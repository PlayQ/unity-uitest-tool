using System;
using UnityEngine;

namespace PlayQ.UITestTools
{
    public static class TestStep
    {
        public static void Step(string step)
        {
            currentStep = new CurrentStep(step, (float) current/count);
            current++;
            if (current > count)
            {
                throw new ArgumentException("Current step (" + current + ") more than count (" + count + ")");                
            }
            Debug.Log("Step " + current + " (" + step + ") started.");
        }

        public static void SetStepsCount(uint count)
        {
            if (count == 0)
            {
                throw new ArgumentException("Count should be grater then Zero.");
            }
            currentStep = null;
            current = 0;
            TestStep.count = count;
        }

        private static uint current = 0;
        private static uint count = 0;

        private static CurrentStep currentStep;
        
        public static CurrentStep GetCurrentStep()
        {
            return currentStep;
        }
        
        public class CurrentStep
        {
            public readonly string Text;
            public readonly float Progress;

            public CurrentStep(string text, float percent)
            {
                Text = text;
                Progress = percent;
            }
        }
    }
}