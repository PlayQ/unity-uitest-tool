namespace PlayQ.UITestTools
{
    public static class RunTestsMode
    {
        public enum RunTestsModeEnum
        {
            All,
            Smoke,
            DoubleClick,
            Selected
        }

       
        

        public static void SelectOnlySmokeTests()
        {
            Mode = RunTestsModeEnum.Smoke;
        }

        public static RunTestsModeEnum Mode
        {
            get { return PlayModeTestRunner.SerializedTests.TestRunMode; }
            private set
            {
                PlayModeTestRunner.SerializedTests.TestRunMode = value;
            }
        }
        
        public static void SelectOnlySelectedTests()
        {
            Mode = RunTestsModeEnum.Selected;
        }
        
        public static int RunTestsGivenAmountOfTimes
        {
            get
            {
                return PlayModeTestRunner.SerializedTests.RepeatTestsNTimes < 1 ? 
                    1 : PlayModeTestRunner.SerializedTests.RepeatTestsNTimes;
            }
            set
            {
                PlayModeTestRunner.SerializedTests.RepeatTestsNTimes =  value < 1 ? 1 : value;
            }
        }

        public static string SpecificTestOrClassName
        {
            get
            {
                return PlayModeTestRunner.SerializedTests.SpecificTestOrClassName;
            }
            private set
            {
                PlayModeTestRunner.SerializedTests.SpecificTestOrClassName =  value;
            }
        }

        public static PlayModeTestRunner.SpecificTestType RunSpecificTestType
        {
            get
            {
                return PlayModeTestRunner.SerializedTests.RunSpecificTestType;
            }
            private set
            {
                PlayModeTestRunner.SerializedTests.RunSpecificTestType = value;
            }
        }
        
        
        public static void RunTestByDoubleClick(PlayModeTestRunner.SpecificTestType type, string testFullName)
        {
            RunSpecificTestType = type;
            SpecificTestOrClassName = testFullName;
            Mode = RunTestsModeEnum.DoubleClick;
        }

        public static void SelectAllTests()
        {
            Mode = RunTestsModeEnum.All;
        }
    }
}