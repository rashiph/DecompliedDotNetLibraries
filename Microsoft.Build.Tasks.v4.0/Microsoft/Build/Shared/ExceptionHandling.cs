namespace Microsoft.Build.Shared
{
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Security;
    using System.Threading;
    using System.Xml;
    using System.Xml.Schema;

    internal static class ExceptionHandling
    {
        private static string dumpFileName;

        [MethodImpl(MethodImplOptions.Synchronized)]
        internal static void DumpExceptionToFile(Exception ex)
        {
            if (dumpFileName == null)
            {
                dumpFileName = Path.Combine(Path.GetTempPath(), "MSBuild_" + Guid.NewGuid().ToString());
                using (StreamWriter writer = new StreamWriter(dumpFileName, true))
                {
                    writer.WriteLine("UNHANDLED EXCEPTIONS FROM PROCESS {0}:", Process.GetCurrentProcess().Id);
                    writer.WriteLine("=====================");
                }
            }
            using (StreamWriter writer2 = new StreamWriter(dumpFileName, true))
            {
                writer2.WriteLine(DateTime.Now.ToString("G", CultureInfo.CurrentCulture));
                writer2.WriteLine(ex.ToString());
                writer2.WriteLine("===================");
            }
        }

        internal static bool IsCriticalException(Exception e)
        {
            return (((e is StackOverflowException) || (e is OutOfMemoryException)) || (((e is ThreadAbortException) || (e is AccessViolationException)) || (e is Microsoft.Build.Shared.InternalErrorException)));
        }

        internal static bool IsIoRelatedException(Exception e)
        {
            return !NotExpectedException(e);
        }

        internal static bool NotExpectedException(Exception e)
        {
            return ((((!(e is UnauthorizedAccessException) && !(e is PathTooLongException)) && (!(e is DirectoryNotFoundException) && !(e is NotSupportedException))) && (!(e is ArgumentException) || (e is ArgumentNullException))) && (!(e is SecurityException) && !(e is IOException)));
        }

        internal static bool NotExpectedFunctionException(Exception e)
        {
            return ((!(e is InvalidCastException) && !(e is ArgumentNullException)) && ((!(e is FormatException) && !(e is InvalidOperationException)) && NotExpectedReflectionException(e)));
        }

        internal static bool NotExpectedIoOrXmlException(Exception e)
        {
            return ((!(e is XmlSyntaxException) && !(e is XmlException)) && (!(e is XmlSchemaException) && NotExpectedException(e)));
        }

        internal static bool NotExpectedReflectionException(Exception e)
        {
            return ((((!(e is TypeLoadException) && !(e is MethodAccessException)) && (!(e is MissingMethodException) && !(e is MemberAccessException))) && ((!(e is BadImageFormatException) && !(e is ReflectionTypeLoadException)) && (!(e is CustomAttributeFormatException) && !(e is TargetParameterCountException)))) && (((!(e is InvalidCastException) && !(e is AmbiguousMatchException)) && (!(e is InvalidFilterCriteriaException) && !(e is TargetException))) && (!(e is MissingFieldException) && NotExpectedException(e))));
        }

        internal static bool NotExpectedRegistryException(Exception e)
        {
            return ((!(e is SecurityException) && !(e is UnauthorizedAccessException)) && ((!(e is IOException) && !(e is ObjectDisposedException)) && !(e is ArgumentException)));
        }

        internal static void UnhandledExceptionHandler(object sender, UnhandledExceptionEventArgs e)
        {
            Exception exceptionObject = (Exception) e.ExceptionObject;
            DumpExceptionToFile(exceptionObject);
        }
    }
}

