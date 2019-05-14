using System.IO;
using UnityEngine;

namespace PlayQ.UITestTools
{
    public class PreloaderTimeTracker
    {
        private float starTime;
        private bool done;
        
        public void Start()
        {
            starTime = Time.time;
        }

        public void Done()
        {
            if (!done)
            {
                PlayModeTestRunner.SavePreloadingTime(Time.time - starTime);
                done = true;
            }
        }
    }
}