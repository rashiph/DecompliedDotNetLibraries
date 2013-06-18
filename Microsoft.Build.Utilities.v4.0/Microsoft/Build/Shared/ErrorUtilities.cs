namespace Microsoft.Build.Shared
{
    using System;
    using System.IO;
    using System.Runtime;

    internal static class ErrorUtilities
    {
        private static readonly bool throwExceptions = string.IsNullOrEmpty(Environment.GetEnvironmentVariable("MSBUILDDONOTTHROWINTERNAL"));

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        internal static void ThrowArgument(string resourceName, params object[] args)
        {
            ThrowArgument(null, resourceName, args);
        }

        private static void ThrowArgument(Exception innerException, string resourceName, params object[] args)
        {
            if (throwExceptions)
            {
                throw new ArgumentException(ResourceUtilities.FormatResourceString(resourceName, args), innerException);
            }
        }

        internal static void ThrowArgumentOutOfRange(string parameterName)
        {
            if (throwExceptions)
            {
                throw new ArgumentOutOfRangeException(parameterName);
            }
        }

        internal static void ThrowIfTypeDoesNotImplementToString(object param)
        {
        }

        internal static void ThrowInternalError(string message, params object[] args)
        {
            if (throwExceptions)
            {
                throw new InternalErrorException(ResourceUtilities.FormatString(message, args));
            }
        }

        internal static void ThrowInternalError(string message, Exception innerException, params object[] args)
        {
            if (throwExceptions)
            {
                throw new InternalErrorException(ResourceUtilities.FormatString(message, args), innerException);
            }
        }

        internal static void ThrowInternalErrorUnreachable()
        {
            if (throwExceptions)
            {
                throw new InternalErrorException("Unreachable?");
            }
        }

        internal static void ThrowInvalidOperation(string resourceName, params object[] args)
        {
            if (throwExceptions)
            {
                throw new InvalidOperationException(ResourceUtilities.FormatResourceString(resourceName, args));
            }
        }

        internal static void VerifyThrow(bool condition, string unformattedMessage)
        {
            if (!condition)
            {
                ThrowInternalError(unformattedMessage, null, null);
            }
        }

        internal static void VerifyThrow(bool condition, string unformattedMessage, object arg0)
        {
            if (!condition)
            {
                ThrowInternalError(unformattedMessage, new object[] { arg0 });
            }
        }

        internal static void VerifyThrow(bool condition, string unformattedMessage, object arg0, object arg1)
        {
            if (!condition)
            {
                ThrowInternalError(unformattedMessage, new object[] { arg0, arg1 });
            }
        }

        internal static void VerifyThrow(bool condition, string unformattedMessage, object arg0, object arg1, object arg2)
        {
            if (!condition)
            {
                ThrowInternalError(unformattedMessage, new object[] { arg0, arg1, arg2 });
            }
        }

        internal static void VerifyThrow(bool condition, string unformattedMessage, object arg0, object arg1, object arg2, object arg3)
        {
            if (!condition)
            {
                ThrowInternalError(unformattedMessage, new object[] { arg0, arg1, arg2, arg3 });
            }
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        internal static void VerifyThrowArgument(bool condition, string resourceName)
        {
            VerifyThrowArgument(condition, null, resourceName);
        }

        internal static void VerifyThrowArgument(bool condition, Exception innerException, string resourceName)
        {
            if (!condition)
            {
                ThrowArgument(innerException, resourceName, null);
            }
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        internal static void VerifyThrowArgument(bool condition, string resourceName, object arg0)
        {
            VerifyThrowArgument(condition, null, resourceName, arg0);
        }

        internal static void VerifyThrowArgument(bool condition, Exception innerException, string resourceName, object arg0)
        {
            if (!condition)
            {
                ThrowArgument(innerException, resourceName, new object[] { arg0 });
            }
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        internal static void VerifyThrowArgument(bool condition, string resourceName, object arg0, object arg1)
        {
            VerifyThrowArgument(condition, null, resourceName, arg0, arg1);
        }

        internal static void VerifyThrowArgument(bool condition, Exception innerException, string resourceName, object arg0, object arg1)
        {
            if (!condition)
            {
                ThrowArgument(innerException, resourceName, new object[] { arg0, arg1 });
            }
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        internal static void VerifyThrowArgument(bool condition, string resourceName, object arg0, object arg1, object arg2)
        {
            VerifyThrowArgument(condition, null, resourceName, arg0, arg1, arg2);
        }

        internal static void VerifyThrowArgument(bool condition, Exception innerException, string resourceName, object arg0, object arg1, object arg2)
        {
            if (!condition)
            {
                ThrowArgument(innerException, resourceName, new object[] { arg0, arg1, arg2 });
            }
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        internal static void VerifyThrowArgument(bool condition, string resourceName, object arg0, object arg1, object arg2, object arg3)
        {
            VerifyThrowArgument(condition, null, resourceName, arg0, arg1, arg2, arg3);
        }

        internal static void VerifyThrowArgument(bool condition, Exception innerException, string resourceName, object arg0, object arg1, object arg2, object arg3)
        {
            if (!condition)
            {
                ThrowArgument(innerException, resourceName, new object[] { arg0, arg1, arg2, arg3 });
            }
        }

        internal static void VerifyThrowArgumentArraysSameLength(Array parameter1, Array parameter2, string parameter1Name, string parameter2Name)
        {
            VerifyThrowArgumentNull(parameter1, parameter1Name);
            VerifyThrowArgumentNull(parameter2, parameter2Name);
            if ((parameter1.Length != parameter2.Length) && throwExceptions)
            {
                throw new ArgumentException(ResourceUtilities.FormatResourceString("Shared.ParametersMustHaveTheSameLength", new object[] { parameter1Name, parameter2Name }));
            }
        }

        internal static void VerifyThrowArgumentLength(string parameter, string parameterName)
        {
            VerifyThrowArgumentNull(parameter, parameterName);
            if ((parameter.Length == 0) && throwExceptions)
            {
                throw new ArgumentException(ResourceUtilities.FormatResourceString("Shared.ParameterCannotHaveZeroLength", new object[] { parameterName }));
            }
        }

        internal static void VerifyThrowArgumentLengthIfNotNull(string parameter, string parameterName)
        {
            if (((parameter != null) && (parameter.Length == 0)) && throwExceptions)
            {
                throw new ArgumentException(ResourceUtilities.FormatResourceString("Shared.ParameterCannotHaveZeroLength", new object[] { parameterName }));
            }
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        internal static void VerifyThrowArgumentNull(object parameter, string parameterName)
        {
            VerifyThrowArgumentNull(parameter, parameterName, "Shared.ParameterCannotBeNull");
        }

        internal static void VerifyThrowArgumentNull(object parameter, string parameterName, string resourceName)
        {
            if ((parameter == null) && throwExceptions)
            {
                throw new ArgumentNullException(ResourceUtilities.FormatResourceString(resourceName, new object[] { parameterName }), null);
            }
        }

        internal static void VerifyThrowArgumentOutOfRange(bool condition, string parameterName)
        {
            if (!condition)
            {
                ThrowArgumentOutOfRange(parameterName);
            }
        }

        internal static void VerifyThrowInternalLength(string parameterValue, string parameterName)
        {
            VerifyThrowInternalNull(parameterValue, parameterName);
            if (parameterValue.Length == 0)
            {
                ThrowInternalError("{0} unexpectedly empty", new object[] { parameterName });
            }
        }

        internal static void VerifyThrowInternalNull(object parameter, string parameterName)
        {
            if (parameter == null)
            {
                ThrowInternalError("{0} unexpectedly null", new object[] { parameterName });
            }
        }

        internal static void VerifyThrowInternalRooted(string value)
        {
            if (!Path.IsPathRooted(value))
            {
                ThrowInternalError("{0} unexpectedly not a rooted path", new object[] { value });
            }
        }

        internal static void VerifyThrowInvalidOperation(bool condition, string resourceName)
        {
            if (!condition)
            {
                ThrowInvalidOperation(resourceName, null);
            }
        }

        internal static void VerifyThrowInvalidOperation(bool condition, string resourceName, object arg0)
        {
            if (!condition)
            {
                ThrowInvalidOperation(resourceName, new object[] { arg0 });
            }
        }

        internal static void VerifyThrowInvalidOperation(bool condition, string resourceName, object arg0, object arg1)
        {
            if (!condition)
            {
                ThrowInvalidOperation(resourceName, new object[] { arg0, arg1 });
            }
        }

        internal static void VerifyThrowInvalidOperation(bool condition, string resourceName, object arg0, object arg1, object arg2)
        {
            if (!condition)
            {
                ThrowInvalidOperation(resourceName, new object[] { arg0, arg1, arg2 });
            }
        }
    }
}

