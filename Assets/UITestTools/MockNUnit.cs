using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace PlayQ.UITestTools
{
    [AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public class TimeoutAttribute: Attribute
    {
        
        private int timeout;

        /// <summary>
        /// Construct a TimeoutAttribute given a time in milliseconds
        /// </summary>
        /// <param name="timeout">The timeout value in milliseconds</param>
        public TimeoutAttribute(int timeout)
        {
            this.timeout = timeout;
        }

        public TimeoutProperties Properties
        {
            get { return new TimeoutProperties(timeout); }
        }

        public class TimeoutProperties
        {
            private int timeout;
            public TimeoutProperties(int timeout)
            {
                this.timeout = timeout;
            }

            public int Get(string key)
            {
                return timeout;
            }
            
        }
    }
    
    [AttributeUsage(AttributeTargets.Method)]
    public class UnityTestAttribute : Attribute
    {
    }
    
    [AttributeUsage(AttributeTargets.Method)]
    public class TestAttribute : Attribute
    {
    }
    
    [AttributeUsage(AttributeTargets.Method)]
    public class TearDownAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class SetUpAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class OneTimeTearDownAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class OneTimeSetUpAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public class IgnoreAttribute : Attribute
    {
        public readonly string Reason;
        public IgnoreAttribute(string reason)
        {
            Reason = reason;
        }
    }
    
    
    public static class Assert
    {
        
        /// <summary>
        ///   <para>A float comparer used by Assertions.Assert performing approximate comparison.</para>
        /// </summary>
        private class FloatComparer : IEqualityComparer<float>
        {
            /// <summary>
            ///   <para>Default instance of a comparer class with deafult error epsilon and absolute error check.</para>
            /// </summary>
            public static readonly FloatComparer s_ComparerWithDefaultTolerance = new FloatComparer(1E-05f);

            private readonly float m_Error;
            private readonly bool m_Relative;

            /// <summary>
            ///   <para>Default epsilon used by the comparer.</para>
            /// </summary>
            public const float kEpsilon = 1E-05f;

            /// <summary>
            ///   <para>Creates an instance of the comparer.</para>
            /// </summary>
            /// <param name="relative">Should a relative check be used when comparing values? By default, an absolute check will be used.</param>
            /// <param name="error">Allowed comparison error. By default, the FloatComparer.kEpsilon is used.</param>
            public FloatComparer()
                : this(1E-05f, false)
            {
            }

            /// <summary>
            ///   <para>Creates an instance of the comparer.</para>
            /// </summary>
            /// <param name="relative">Should a relative check be used when comparing values? By default, an absolute check will be used.</param>
            /// <param name="error">Allowed comparison error. By default, the FloatComparer.kEpsilon is used.</param>
            public FloatComparer(bool relative)
                : this(1E-05f, relative)
            {
            }

            /// <summary>
            ///   <para>Creates an instance of the comparer.</para>
            /// </summary>
            /// <param name="relative">Should a relative check be used when comparing values? By default, an absolute check will be used.</param>
            /// <param name="error">Allowed comparison error. By default, the FloatComparer.kEpsilon is used.</param>
            public FloatComparer(float error)
                : this(error, false)
            {
            }

            /// <summary>
            ///   <para>Creates an instance of the comparer.</para>
            /// </summary>
            /// <param name="relative">Should a relative check be used when comparing values? By default, an absolute check will be used.</param>
            /// <param name="error">Allowed comparison error. By default, the FloatComparer.kEpsilon is used.</param>
            public FloatComparer(float error, bool relative)
            {
                this.m_Error = error;
                this.m_Relative = relative;
            }

            public bool Equals(float a, float b)
            {
                return !this.m_Relative
                    ? FloatComparer.AreEqual(a, b, this.m_Error)
                    : FloatComparer.AreEqualRelative(a, b, this.m_Error);
            }

            public int GetHashCode(float obj)
            {
                return this.GetHashCode();
            }

            /// <summary>
            ///   <para>Performs equality check with absolute error check.</para>
            /// </summary>
            /// <param name="expected">Expected value.</param>
            /// <param name="actual">Actual value.</param>
            /// <param name="error">Comparison error.</param>
            /// <returns>
            ///   <para>Result of the comparison.</para>
            /// </returns>
            public static bool AreEqual(float expected, float actual, float error)
            {
                return (double) Math.Abs(actual - expected) <= (double) error;
            }

            /// <summary>
            ///   <para>Performs equality check with relative error check.</para>
            /// </summary>
            /// <param name="expected">Expected value.</param>
            /// <param name="actual">Actual value.</param>
            /// <param name="error">Comparison error.</param>
            /// <returns>
            ///   <para>Result of the comparison.</para>
            /// </returns>
            public static bool AreEqualRelative(float expected, float actual, float error)
            {
                if ((double) expected == (double) actual)
                    return true;
                float num1 = Math.Abs(expected);
                float num2 = Math.Abs(actual);
                return (double) Math.Abs((float) (((double) actual - (double) expected) /
                                                  ((double) num1 <= (double) num2 ? (double) num2 : (double) num1))) <=
                       (double) error;
            }
        }

        /// <summary>
        ///   <para>An exception that is thrown on a failure. Assertions.Assert._raiseExceptions needs to be set to true.</para>
        /// </summary>
        public class AssertionException : Exception
        {
            private string m_UserMessage;

            public AssertionException(string message, string userMessage)
                : base(message)
            {
                this.m_UserMessage = userMessage;
            }

            public override string Message
            {
                get
                {
                    string str = base.Message;
                    if (!string.IsNullOrEmpty(this.m_UserMessage))
                        str = str + (object) '\n' + this.m_UserMessage;
                    return str;
                }
            }
        }

        private class AssertionMessageUtil
        {
            private const string k_Expected = "Expected:";
            private const string k_AssertionFailed = "Assertion failed.";

            public static string GetMessage(string failureMessage)
            {
                return string.Format("{0} {1}", (object) "Assertion failed.", (object) failureMessage);
            }

            public static string GetMessage(string failureMessage, string expected)
            {
                return AssertionMessageUtil.GetMessage(
                    string.Format("{0}{1}{2} {3}", failureMessage, "", "Expected:", expected));
            }

            public static string GetEqualityMessage(object actual, object expected, bool expectEqual)
            {
                return AssertionMessageUtil.GetMessage(
                    string.Format("Values are {0}equal.", (!expectEqual ? "" : "not ")),
                    string.Format("{0} {2} {1}", actual, expected ?? "null", (!expectEqual ? "!=" : "==")));
            }

            public static string NullFailureMessage(object value, bool expectNull)
            {
                return AssertionMessageUtil.GetMessage(string.Format("Value was {0}Null", (!expectNull ? "" : "not ")),
                    string.Format("Value was {0}Null", (!expectNull ? "not " : "")));
            }

            public static string BooleanFailureMessage(bool expected)
            {
                return AssertionMessageUtil.GetMessage("Value was " + !expected, expected.ToString());
            }
        }

        /// <summary>
        ///   <para>Should an exception be thrown on a failure.</para>
        /// </summary>
        public static bool raiseExceptions = true;

        public static void Fail()
        {
            Fail(string.Empty, string.Empty);
        }

        public static void Fail(string message)
        {
            Fail(message, string.Empty);
        }

        public static void Fail(string message, string userMessage)
        {
            if (Debugger.IsAttached)
                throw new AssertionException(message, userMessage);
            if (raiseExceptions)
                throw new AssertionException(message, userMessage);
            if (message == null)
                message = "Assertion has failed\n";
            if (userMessage != null)
                message = userMessage + (object) '\n' + message;

            UnityEngine.Debug.LogAssertion((object) message);
        }

        /// <summary>
        ///   <para>Asserts that the condition is true.</para>
        /// </summary>
        /// <param name="condition"></param>
        /// <param name="message"></param>
        public static void IsTrue(bool condition)
        {
            Assert.IsTrue(condition, (string) null);
        }

        /// <summary>
        ///   <para>Asserts that the condition is true.</para>
        /// </summary>
        /// <param name="condition"></param>
        /// <param name="message"></param>
        public static void IsTrue(bool condition, string message)
        {
            if (condition)
                return;
            Assert.Fail(AssertionMessageUtil.BooleanFailureMessage(true), message);
        }

        /// <summary>
        ///   <para>Asserts that the condition is false.</para>
        /// </summary>
        /// <param name="condition"></param>
        /// <param name="message"></param>
        public static void IsFalse(bool condition)
        {
            Assert.IsFalse(condition, (string) null);
        }

        /// <summary>
        ///   <para>Asserts that the condition is false.</para>
        /// </summary>
        /// <param name="condition"></param>
        /// <param name="message"></param>
        public static void IsFalse(bool condition, string message)
        {
            if (!condition)
                return;
            Assert.Fail(AssertionMessageUtil.BooleanFailureMessage(false), message);
        }

        /// <summary>
        ///         <para>Asserts that the values are approximately equal. An absolute error check is used for approximate equality check (|a-b| &lt; tolerance). Default tolerance is 0.00001f.
        /// 
        /// Note: Every time you call the method with tolerance specified, a new instance of Assertions.Comparers.FloatComparer is created. For performance reasons you might want to instance your own comparer and pass it to the AreEqual method. If the tolerance is not specifies, a default comparer is used and the issue does not occur.</para>
        ///       </summary>
        /// <param name="tolerance">Tolerance of approximation.</param>
        /// <param name="expected"></param>
        /// <param name="actual"></param>
        /// <param name="message"></param>
        public static void AreApproximatelyEqual(float expected, float actual)
        {
            Assert.AreEqual<float>(expected, actual, (string) null,
                (IEqualityComparer<float>) FloatComparer.s_ComparerWithDefaultTolerance);
        }

        /// <summary>
        ///         <para>Asserts that the values are approximately equal. An absolute error check is used for approximate equality check (|a-b| &lt; tolerance). Default tolerance is 0.00001f.
        /// 
        /// Note: Every time you call the method with tolerance specified, a new instance of Assertions.Comparers.FloatComparer is created. For performance reasons you might want to instance your own comparer and pass it to the AreEqual method. If the tolerance is not specifies, a default comparer is used and the issue does not occur.</para>
        ///       </summary>
        /// <param name="tolerance">Tolerance of approximation.</param>
        /// <param name="expected"></param>
        /// <param name="actual"></param>
        /// <param name="message"></param>
        public static void AreApproximatelyEqual(float expected, float actual, string message)
        {
            Assert.AreEqual<float>(expected, actual, message,
                (IEqualityComparer<float>) FloatComparer.s_ComparerWithDefaultTolerance);
        }

        /// <summary>
        ///         <para>Asserts that the values are approximately equal. An absolute error check is used for approximate equality check (|a-b| &lt; tolerance). Default tolerance is 0.00001f.
        /// 
        /// Note: Every time you call the method with tolerance specified, a new instance of Assertions.Comparers.FloatComparer is created. For performance reasons you might want to instance your own comparer and pass it to the AreEqual method. If the tolerance is not specifies, a default comparer is used and the issue does not occur.</para>
        ///       </summary>
        /// <param name="tolerance">Tolerance of approximation.</param>
        /// <param name="expected"></param>
        /// <param name="actual"></param>
        /// <param name="message"></param>
        public static void AreEqual(float expected, float actual, float tolerance)
        {
            Assert.AreApproximatelyEqual(expected, actual, tolerance, (string)null);
        }

        /// <summary>
        ///         <para>Asserts that the values are approximately equal. An absolute error check is used for approximate equality check (|a-b| &lt; tolerance). Default tolerance is 0.00001f.
        /// 
        /// Note: Every time you call the method with tolerance specified, a new instance of Assertions.Comparers.FloatComparer is created. For performance reasons you might want to instance your own comparer and pass it to the AreEqual method. If the tolerance is not specifies, a default comparer is used and the issue does not occur.</para>
        ///       </summary>
        /// <param name="tolerance">Tolerance of approximation.</param>
        /// <param name="expected"></param>
        /// <param name="actual"></param>
        /// <param name="message"></param>
        public static void AreApproximatelyEqual(float expected, float actual, float tolerance)
        {
            Assert.AreApproximatelyEqual(expected, actual, tolerance, (string) null);
        }

        /// <summary>
        ///         <para>Asserts that the values are approximately equal. An absolute error check is used for approximate equality check (|a-b| &lt; tolerance). Default tolerance is 0.00001f.
        /// 
        /// Note: Every time you call the method with tolerance specified, a new instance of Assertions.Comparers.FloatComparer is created. For performance reasons you might want to instance your own comparer and pass it to the AreEqual method. If the tolerance is not specifies, a default comparer is used and the issue does not occur.</para>
        ///       </summary>
        /// <param name="tolerance">Tolerance of approximation.</param>
        /// <param name="expected"></param>
        /// <param name="actual"></param>
        /// <param name="message"></param>
        public static void AreApproximatelyEqual(float expected, float actual, float tolerance, string message)
        {
            Assert.AreEqual<float>(expected, actual, message,
                (IEqualityComparer<float>) new FloatComparer(tolerance));
        }

        /// <summary>
        ///   <para>Asserts that the values are approximately not equal. An absolute error check is used for approximate equality check (|a-b| &lt; tolerance). Default tolerance is 0.00001f.</para>
        /// </summary>
        /// <param name="tolerance">Tolerance of approximation.</param>
        /// <param name="expected"></param>
        /// <param name="actual"></param>
        /// <param name="message"></param>
        public static void AreNotApproximatelyEqual(float expected, float actual)
        {
            Assert.AreNotEqual<float>(expected, actual, (string) null,
                (IEqualityComparer<float>) FloatComparer.s_ComparerWithDefaultTolerance);
        }

        /// <summary>
        ///   <para>Asserts that the values are approximately not equal. An absolute error check is used for approximate equality check (|a-b| &lt; tolerance). Default tolerance is 0.00001f.</para>
        /// </summary>
        /// <param name="tolerance">Tolerance of approximation.</param>
        /// <param name="expected"></param>
        /// <param name="actual"></param>
        /// <param name="message"></param>
        public static void AreNotApproximatelyEqual(float expected, float actual, string message)
        {
            Assert.AreNotEqual<float>(expected, actual, message,
                (IEqualityComparer<float>) FloatComparer.s_ComparerWithDefaultTolerance);
        }

        /// <summary>
        ///   <para>Asserts that the values are approximately not equal. An absolute error check is used for approximate equality check (|a-b| &lt; tolerance). Default tolerance is 0.00001f.</para>
        /// </summary>
        /// <param name="tolerance">Tolerance of approximation.</param>
        /// <param name="expected"></param>
        /// <param name="actual"></param>
        /// <param name="message"></param>
        public static void AreNotApproximatelyEqual(float expected, float actual, float tolerance)
        {
            Assert.AreNotApproximatelyEqual(expected, actual, tolerance, (string) null);
        }

        /// <summary>
        ///   <para>Asserts that the values are approximately not equal. An absolute error check is used for approximate equality check (|a-b| &lt; tolerance). Default tolerance is 0.00001f.</para>
        /// </summary>
        /// <param name="tolerance">Tolerance of approximation.</param>
        /// <param name="expected"></param>
        /// <param name="actual"></param>
        /// <param name="message"></param>
        public static void AreNotApproximatelyEqual(float expected, float actual, float tolerance, string message)
        {
            Assert.AreNotEqual<float>(expected, actual, message,
                (IEqualityComparer<float>) new FloatComparer(tolerance));
        }

        [Conditional("UNITY_ASSERTIONS")]
        public static void AreEqual<T>(T expected, T actual)
        {
            Assert.AreEqual<T>(expected, actual, (string) null);
        }

        public static void AreEqual<T>(T expected, T actual, string message)
        {
            Assert.AreEqual<T>(expected, actual, message, (IEqualityComparer<T>) EqualityComparer<T>.Default);
        }

        public static void AreEqual<T>(T expected, T actual, string message, IEqualityComparer<T> comparer)
        {
            if (typeof(UnityEngine.Object).IsAssignableFrom(typeof(T)))
            {
                Assert.AreEqual((object) expected as UnityEngine.Object, (object) actual as UnityEngine.Object,
                    message);
            }
            else
            {
                if (comparer.Equals(actual, expected))
                    return;
                Assert.Fail(AssertionMessageUtil.GetEqualityMessage((object) actual, (object) expected, true),
                    message);
            }
        }

        public static void AreEqual(UnityEngine.Object expected, UnityEngine.Object actual, string message)
        {
            if (!(actual != expected))
                return;
            Assert.Fail(AssertionMessageUtil.GetEqualityMessage((object) actual, (object) expected, true), message);
        }

        public static void AreNotEqual<T>(T expected, T actual)
        {
            Assert.AreNotEqual<T>(expected, actual, (string) null);
        }

        public static void AreNotEqual<T>(T expected, T actual, string message)
        {
            Assert.AreNotEqual<T>(expected, actual, message, (IEqualityComparer<T>) EqualityComparer<T>.Default);
        }

        public static void AreNotEqual<T>(T expected, T actual, string message, IEqualityComparer<T> comparer)
        {
            if (typeof(UnityEngine.Object).IsAssignableFrom(typeof(T)))
            {
                Assert.AreNotEqual((object) expected as UnityEngine.Object, (object) actual as UnityEngine.Object,
                    message);
            }
            else
            {
                if (!comparer.Equals(actual, expected))
                    return;
                Assert.Fail(AssertionMessageUtil.GetEqualityMessage((object) actual, (object) expected, false),
                    message);
            }
        }

        public static void AreNotEqual(UnityEngine.Object expected, UnityEngine.Object actual, string message)
        {
            if (!(actual == expected))
                return;
            Assert.Fail(AssertionMessageUtil.GetEqualityMessage((object) actual, (object) expected, false), message);
        }

        public static void IsNull<T>(T value) where T : class
        {
            Assert.IsNull<T>(value, (string) null);
        }

        public static void IsNull<T>(T value, string message) where T : class
        {
            if (typeof(UnityEngine.Object).IsAssignableFrom(typeof(T)))
            {
                Assert.IsNull((object) value as UnityEngine.Object, message);
            }
            else
            {
                if ((object) value == null)
                    return;
                Assert.Fail(AssertionMessageUtil.NullFailureMessage((object) value, true), message);
            }
        }

        public static void IsNull(UnityEngine.Object value, string message)
        {
            if (!(value != (UnityEngine.Object) null))
                return;
            Assert.Fail(AssertionMessageUtil.NullFailureMessage((object) value, true), message);
        }

        public static void IsNotNull<T>(T value) where T : class
        {
            Assert.IsNotNull<T>(value, (string) null);
        }

        public static void IsNotNull<T>(T value, string message) where T : class
        {
            if (typeof(UnityEngine.Object).IsAssignableFrom(typeof(T)))
            {
                Assert.IsNotNull((object) value as UnityEngine.Object, message);
            }
            else
            {
                if ((object) value != null)
                    return;
                Assert.Fail(AssertionMessageUtil.NullFailureMessage((object) value, false), message);
            }
        }

        public static void IsNotNull(UnityEngine.Object value, string message)
        {
            if (!(value == (UnityEngine.Object) null))
                return;
            Assert.Fail(AssertionMessageUtil.NullFailureMessage((object) value, false), message);
        }

        public static void GreaterOrEqual(float current, float expected)
        {
            if (current < expected)
            {
                throw new Exception("GreaterOrEqual fail, expected: " + expected + ", current: " + current);
            }
        }

        public static void LessOrEquals(float current, float expected, string message)
        {
            if (current > expected)
            {
                throw new Exception("LessOrEquals fail, expected: " + expected + ", current: " + current +
                                    ", message: " + message);
            }
        }

        public static void Less(float current, float expected, string message)
        {
            if (current >= expected)
            {
                throw new Exception("Less fail, expected: " + expected + ", current: " + current +
                                    ", message: " + message);
            }
        }


        public static void Greater(float current, float expected, string message)
        {
            if (current <= expected)
            {
                throw new Exception("Greater fail, expected: " + expected + ", current: " + current +
                                    ", message: " + message);
            }
        }

        public static void GreaterOrEqual(float current, float expected, string message)
        {
            if (current < expected)
            {
                throw new Exception("GreaterOrEqual fail, expected: " + expected + ", current: " + current +
                                    ", message: " + message);
            }
        }
    }
}