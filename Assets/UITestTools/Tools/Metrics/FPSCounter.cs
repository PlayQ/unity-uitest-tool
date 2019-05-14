using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace PlayQ.UITestTools
{
    public class FPSCounter
    {
        private float framesPassed;
        private float secondsPassed;
        private float minDeltaTime = float.MaxValue;
        private float maxDeltaTime = 0f;

        private const string METRICS_FILE_NAME = "fps.json"; 

        private static string FPSMettricsFileFullPath
        {
            get
            {
                return TestMetrics.PathTo(METRICS_FILE_NAME);
            }
        }
        
        private class FPSData
        {
            public int Min;
            public int Avg;
            public int Max;
        }

        private class FPSMetrics
        {
            public string Build;
            public Dictionary<string, FPSData> Fps;
        }
        
        public int AverageFPS
        {
            get
            {
                if (secondsPassed == 0)
                {
                    return 0;
                }
                return (int) Math.Round(framesPassed / secondsPassed);
            }
        }

        public int MaxFPS
        {
            get
            {
                if (minDeltaTime == 0)
                {
                    return 0;
                }
                return (int) Math.Round(1f / minDeltaTime);
            }
        }

        public int MixFPS
        {
            get
            {
                if (maxDeltaTime == 0)
                {
                    return 0;
                }
                return (int) Math.Round(1f / maxDeltaTime);
            }
        }

        private Coroutine updateCoroutine;
        public void Reset()
        {
            if (updateCoroutine == null)
            {
                updateCoroutine = CoroutineProvider.Mono.StartCoroutine(Update());    
            }

            framesPassed = 0;
            secondsPassed = 0;
            minDeltaTime = float.MaxValue;
            maxDeltaTime = 0;
        }

        private IEnumerator Update()
        {
            while (true)
            {
                yield return null;

                framesPassed++;
                secondsPassed += Time.unscaledDeltaTime;
                if (maxDeltaTime < Time.unscaledDeltaTime)
                {
                    maxDeltaTime = Time.unscaledDeltaTime;
                }
                if (minDeltaTime > Time.unscaledDeltaTime)
                {
                    minDeltaTime = Time.unscaledDeltaTime;
                }
            }
        }


        public void ClearFPSMetrics()
        {
            if (File.Exists(FPSMettricsFileFullPath))
            {
                File.Delete(FPSMettricsFileFullPath);
            }
        }

        public void SaveFPS(string tag)
        {
            FPSMetrics metrics = new FPSMetrics();
            string textData;
            if (File.Exists(FPSMettricsFileFullPath))
            {
                try
                {
                    textData = File.ReadAllText(FPSMettricsFileFullPath);
                    metrics = JObject.Parse(textData).ToObject<FPSMetrics>();

                }
                catch (Exception ex)
                {
                    Debug.LogWarning("file with fps metrics exists but can't be read " + ex.Message);
                }
            }
            
            metrics.Build = Application.version;
            var buildVersion = ConsoleArgumentHelper.GetArg("-buildNumber");
            if (!string.IsNullOrEmpty(buildVersion))
            {
                metrics.Build = metrics.Build + ":" + buildVersion;
            }

            if (metrics.Fps == null)
            {
                metrics.Fps = new Dictionary<string, FPSData>();
            }

            if (metrics.Fps.ContainsKey(tag))
            {
                tag = tag + "_" +Time.realtimeSinceStartup;
            }
            
            metrics.Fps.Add(tag, new FPSData
            {
                Min = MixFPS,
                Avg = AverageFPS,
                Max = MaxFPS
            });

            TestMetrics.CheckAndCreateFolder();
            
            textData = JObject.FromObject(metrics).ToString();
            File.WriteAllText(FPSMettricsFileFullPath, textData);
        }
    }
}