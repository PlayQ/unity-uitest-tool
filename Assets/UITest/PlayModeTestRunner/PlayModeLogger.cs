﻿using System;
using UnityEngine;

namespace PlayQ.UITestTools
{
    public class PlayModeLogger
    {
        public const string TC_DATE_FORMAT = "yyyy-MM-ddThh:mm:ss.fff";
        public const string FILE_DATE_FORMAT = "yyyy-MM-ddThh.mm.ss.fff";

        private const string DELIMETER =
            "===================================================================================";

        public void IgnoreLog(string methodName)
        {
            Debug.Log(DELIMETER);
            Debug.LogFormat("\"{0}\" ignored.", methodName);
            Debug.LogFormat("##teamcity[testIgnored timestamp='{0}' name='{1}']",
                DateTime.UtcNow.ToString(TC_DATE_FORMAT), methodName);
            Debug.Log(DELIMETER);
        }

        public void StartLog(string methodName)
        {
            Debug.Log(DELIMETER);
            Debug.LogFormat("\"{0}\" setup", methodName);
            Debug.LogFormat("##teamcity[testStarted timestamp='{0}' name='{1}']",
                DateTime.UtcNow.ToString(TC_DATE_FORMAT), methodName);
        }

        public void SuccessLog(string methodName)
        {
            Debug.LogFormat("##teamcity[testFinished timestamp='{0}' name='{1}']",
                DateTime.UtcNow.ToString(TC_DATE_FORMAT), methodName);
            Debug.LogFormat("\"{0}\" success", methodName);
            Debug.Log(DELIMETER);
        }

        public void FailLog(string methodName)
        {
            Debug.LogFormat("##teamcity[testFailed timestamp='{0}' name='{1}']",
                DateTime.UtcNow.ToString(TC_DATE_FORMAT),
                methodName);
            Debug.LogFormat("\"{0}\" fail", methodName);
            Debug.Log(DELIMETER);
        }
    }
}