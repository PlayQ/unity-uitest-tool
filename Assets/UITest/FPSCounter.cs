using System;
using System.Collections;
using UnityEngine;

namespace PlayQ.UITestTools
{
    public static class FPSCounter
    {
        private static FPSCounterMono mono;
        private static long framesPassed;
        private static float secondsPassed;
        private static float minDeltaTime = float.MaxValue;
        private static float maxDeltaTime = 0f;

        public static float AverageFPS
        {
            get
            {
                if (secondsPassed == 0)
                {
                    return 0;
                }
                return (float) Math.Round(framesPassed / secondsPassed);
            }
        }

        public static float MaxFPS
        {
            get
            {
                if (minDeltaTime == 0)
                {
                    return 0;
                }
                return (float) Math.Round(1 / minDeltaTime);
            }
        }

        public static float MixFPS
        {
            get
            {
                if (maxDeltaTime == 0)
                {
                    return 0;
                }
                return (float) Math.Round(1 / maxDeltaTime);
            }
        }

        public static void Reset()
        {
            if (!mono)
            {
                mono = new GameObject().AddComponent<FPSCounterMono>();
                mono.StartCoroutine(Update());
                mono.gameObject.name = "FPS_COUNTER_HELPER";
                GameObject.DontDestroyOnLoad(mono);
            }

            framesPassed = 0;
            secondsPassed = 0;
            minDeltaTime = float.MaxValue;
            maxDeltaTime = 0;
        }

        private static IEnumerator Update()
        {
            while (true)
            {
                yield return null;

                framesPassed++;
                secondsPassed += Time.deltaTime;
                if (maxDeltaTime < Time.deltaTime)
                {
                    maxDeltaTime = Time.deltaTime;
                }
                if (minDeltaTime > Time.deltaTime)
                {
                    minDeltaTime = Time.deltaTime;
                }
            }
        }

        private class FPSCounterMono : MonoBehaviour
        {}
    }
}