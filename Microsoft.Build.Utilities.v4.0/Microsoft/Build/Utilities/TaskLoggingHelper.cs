namespace Microsoft.Build.Utilities
{
    using Microsoft.Build.Framework;
    using Microsoft.Build.Shared;
    using System;
    using System.Globalization;
    using System.IO;
    using System.Resources;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.Text;

    public class TaskLoggingHelper : MarshalByRefObject
    {
        private IBuildEngine buildEngine;
        private bool hasLoggedErrors;
        private string helpKeywordPrefix;
        private ITask taskInstance;
        private string taskName;
        private string taskNameUpperCase;
        private ResourceManager taskResources;

        public TaskLoggingHelper(ITask taskInstance)
        {
            ErrorUtilities.VerifyThrowArgumentNull(taskInstance, "taskInstance");
            this.taskInstance = taskInstance;
            this.taskName = taskInstance.GetType().Name;
        }

        public TaskLoggingHelper(IBuildEngine buildEngine, string taskName)
        {
            ErrorUtilities.VerifyThrowArgumentNull(buildEngine, "buildEngine");
            ErrorUtilities.VerifyThrowArgumentLength(taskName, "taskName");
            this.taskName = taskName;
            this.buildEngine = buildEngine;
        }

        public string ExtractMessageCode(string message, out string messageWithoutCodePrefix)
        {
            string str;
            ErrorUtilities.VerifyThrowArgumentNull(message, "message");
            messageWithoutCodePrefix = ResourceUtilities.ExtractMessageCode(false, message, out str);
            return str;
        }

        public virtual string FormatResourceString(string resourceName, params object[] args)
        {
            ErrorUtilities.VerifyThrowArgumentNull(resourceName, "resourceName");
            ErrorUtilities.VerifyThrowInvalidOperation(this.TaskResources != null, "Shared.TaskResourcesNotRegistered", this.TaskName);
            string unformatted = this.TaskResources.GetString(resourceName, CultureInfo.CurrentUICulture);
            ErrorUtilities.VerifyThrowArgument(unformatted != null, "Shared.TaskResourceNotFound", resourceName, this.TaskName);
            return this.FormatString(unformatted, args);
        }

        public virtual string FormatString(string unformatted, params object[] args)
        {
            ErrorUtilities.VerifyThrowArgumentNull(unformatted, "unformatted");
            return ResourceUtilities.FormatString(unformatted, args);
        }

        public virtual string GetResourceMessage(string resourceName)
        {
            return this.FormatResourceString(resourceName, null);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public void LogCommandLine(string commandLine)
        {
            this.LogCommandLine(MessageImportance.Low, commandLine);
        }

        public void LogCommandLine(MessageImportance importance, string commandLine)
        {
            ErrorUtilities.VerifyThrowArgumentNull(commandLine, "commandLine");
            TaskCommandLineEventArgs e = new TaskCommandLineEventArgs(commandLine, this.TaskName, importance);
            if (this.BuildEngine == null)
            {
                ErrorUtilities.ThrowInvalidOperation("LoggingBeforeTaskInitialization", new object[] { e.Message });
            }
            this.BuildEngine.LogMessageEvent(e);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public void LogError(string message, params object[] messageArgs)
        {
            this.LogError(null, null, null, null, 0, 0, 0, 0, message, messageArgs);
        }

        public void LogError(string subcategory, string errorCode, string helpKeyword, string file, int lineNumber, int columnNumber, int endLineNumber, int endColumnNumber, string message, params object[] messageArgs)
        {
            ErrorUtilities.VerifyThrowArgumentNull(message, "message");
            ErrorUtilities.VerifyThrowInvalidOperation(this.BuildEngine != null, "LoggingBeforeTaskInitialization", message);
            bool flag = (string.IsNullOrEmpty(file) && (lineNumber == 0)) && (columnNumber == 0);
            BuildErrorEventArgs e = new BuildErrorEventArgs(subcategory, errorCode, flag ? this.BuildEngine.ProjectFileOfTaskNode : file, flag ? this.BuildEngine.LineNumberOfTaskNode : lineNumber, flag ? this.BuildEngine.ColumnNumberOfTaskNode : columnNumber, endLineNumber, endColumnNumber, message, helpKeyword, this.TaskName, DateTime.UtcNow, messageArgs);
            this.BuildEngine.LogErrorEvent(e);
            this.hasLoggedErrors = true;
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public void LogErrorFromException(Exception exception)
        {
            this.LogErrorFromException(exception, false);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public void LogErrorFromException(Exception exception, bool showStackTrace)
        {
            this.LogErrorFromException(exception, showStackTrace, false, null);
        }

        public void LogErrorFromException(Exception exception, bool showStackTrace, bool showDetail, string file)
        {
            ErrorUtilities.VerifyThrowArgumentNull(exception, "exception");
            string message = null;
            if (!showDetail && (Environment.GetEnvironmentVariable("MSBUILDDIAGNOSTICS") == null))
            {
                message = exception.Message;
                if (showStackTrace)
                {
                    message = message + Environment.NewLine + exception.StackTrace;
                }
            }
            else
            {
                StringBuilder builder = new StringBuilder(200);
                do
                {
                    builder.Append(exception.GetType().Name);
                    builder.Append(": ");
                    builder.AppendLine(exception.Message);
                    if (showStackTrace)
                    {
                        builder.AppendLine(exception.StackTrace);
                    }
                    exception = exception.InnerException;
                }
                while (exception != null);
                message = builder.ToString();
            }
            this.LogError(null, null, null, file, 0, 0, 0, 0, message, new object[0]);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public void LogErrorFromResources(string messageResourceName, params object[] messageArgs)
        {
            this.LogErrorFromResources(null, null, null, null, 0, 0, 0, 0, messageResourceName, messageArgs);
        }

        public void LogErrorFromResources(string subcategoryResourceName, string errorCode, string helpKeyword, string file, int lineNumber, int columnNumber, int endLineNumber, int endColumnNumber, string messageResourceName, params object[] messageArgs)
        {
            ErrorUtilities.VerifyThrowArgumentNull(messageResourceName, "messageResourceName");
            string subcategory = null;
            if (subcategoryResourceName != null)
            {
                subcategory = this.FormatResourceString(subcategoryResourceName, new object[0]);
            }
            this.LogError(subcategory, errorCode, helpKeyword, file, lineNumber, columnNumber, endLineNumber, endColumnNumber, this.FormatResourceString(messageResourceName, messageArgs), new object[0]);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public void LogErrorWithCodeFromResources(string messageResourceName, params object[] messageArgs)
        {
            this.LogErrorWithCodeFromResources(null, null, 0, 0, 0, 0, messageResourceName, messageArgs);
        }

        public void LogErrorWithCodeFromResources(string subcategoryResourceName, string file, int lineNumber, int columnNumber, int endLineNumber, int endColumnNumber, string messageResourceName, params object[] messageArgs)
        {
            string str2;
            ErrorUtilities.VerifyThrowArgumentNull(messageResourceName, "messageResourceName");
            string subcategory = null;
            if (subcategoryResourceName != null)
            {
                subcategory = this.FormatResourceString(subcategoryResourceName, new object[0]);
            }
            string message = ResourceUtilities.ExtractMessageCode(false, this.FormatResourceString(messageResourceName, messageArgs), out str2);
            string helpKeyword = null;
            if (this.HelpKeywordPrefix != null)
            {
                helpKeyword = this.HelpKeywordPrefix + messageResourceName;
            }
            this.LogError(subcategory, str2, helpKeyword, file, lineNumber, columnNumber, endLineNumber, endColumnNumber, message, new object[0]);
        }

        public void LogExternalProjectFinished(string message, string helpKeyword, string projectFile, bool succeeded)
        {
            ExternalProjectFinishedEventArgs e = new ExternalProjectFinishedEventArgs(message, helpKeyword, this.TaskName, projectFile, succeeded);
            this.BuildEngine.LogCustomEvent(e);
        }

        public void LogExternalProjectStarted(string message, string helpKeyword, string projectFile, string targetNames)
        {
            ExternalProjectStartedEventArgs e = new ExternalProjectStartedEventArgs(message, helpKeyword, this.TaskName, projectFile, targetNames);
            this.BuildEngine.LogCustomEvent(e);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public void LogMessage(string message, params object[] messageArgs)
        {
            this.LogMessage(MessageImportance.Normal, message, messageArgs);
        }

        public void LogMessage(MessageImportance importance, string message, params object[] messageArgs)
        {
            ErrorUtilities.VerifyThrowArgumentNull(message, "message");
            BuildMessageEventArgs e = new BuildMessageEventArgs(message, null, this.TaskName, importance, DateTime.UtcNow, messageArgs);
            if (this.BuildEngine == null)
            {
                ErrorUtilities.ThrowInvalidOperation("LoggingBeforeTaskInitialization", new object[] { e.Message });
            }
            this.BuildEngine.LogMessageEvent(e);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public void LogMessageFromResources(string messageResourceName, params object[] messageArgs)
        {
            this.LogMessageFromResources(MessageImportance.Normal, messageResourceName, messageArgs);
        }

        public void LogMessageFromResources(MessageImportance importance, string messageResourceName, params object[] messageArgs)
        {
            ErrorUtilities.VerifyThrowArgumentNull(messageResourceName, "messageResourceName");
            this.LogMessage(importance, this.FormatResourceString(messageResourceName, messageArgs), new object[0]);
        }

        public bool LogMessageFromText(string lineOfText, MessageImportance messageImportance)
        {
            ErrorUtilities.VerifyThrowArgumentNull(lineOfText, "lineOfText");
            bool flag = false;
            CanonicalError.Parts parts = CanonicalError.Parse(lineOfText);
            if (parts == null)
            {
                this.LogMessage(messageImportance, lineOfText, new object[0]);
                return flag;
            }
            string origin = parts.origin;
            if ((origin == null) || (origin.Length == 0))
            {
                origin = this.TaskNameUpperCase;
            }
            switch (parts.category)
            {
                case CanonicalError.Parts.Category.Warning:
                    this.LogWarning(parts.subcategory, parts.code, null, origin, parts.line, parts.column, parts.endLine, parts.endColumn, parts.text, new object[0]);
                    return flag;

                case CanonicalError.Parts.Category.Error:
                    this.LogError(parts.subcategory, parts.code, null, origin, parts.line, parts.column, parts.endLine, parts.endColumn, parts.text, new object[0]);
                    return true;
            }
            ErrorUtilities.VerifyThrow(false, "Impossible canonical part.");
            return flag;
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public bool LogMessagesFromFile(string fileName)
        {
            return this.LogMessagesFromFile(fileName, MessageImportance.Low);
        }

        public bool LogMessagesFromFile(string fileName, MessageImportance messageImportance)
        {
            ErrorUtilities.VerifyThrowArgumentNull(fileName, "fileName");
            using (StreamReader reader = new StreamReader(fileName, Encoding.Default))
            {
                return this.LogMessagesFromStream(reader, messageImportance);
            }
        }

        public bool LogMessagesFromStream(TextReader stream, MessageImportance messageImportance)
        {
            string str;
            ErrorUtilities.VerifyThrowArgumentNull(stream, "stream");
            bool flag = false;
            while ((str = stream.ReadLine()) != null)
            {
                flag |= this.LogMessageFromText(str, messageImportance);
            }
            return flag;
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public void LogWarning(string message, params object[] messageArgs)
        {
            this.LogWarning(null, null, null, null, 0, 0, 0, 0, message, messageArgs);
        }

        public void LogWarning(string subcategory, string warningCode, string helpKeyword, string file, int lineNumber, int columnNumber, int endLineNumber, int endColumnNumber, string message, params object[] messageArgs)
        {
            ErrorUtilities.VerifyThrowArgumentNull(message, "message");
            ErrorUtilities.VerifyThrowInvalidOperation(this.BuildEngine != null, "LoggingBeforeTaskInitialization", message);
            bool flag = (string.IsNullOrEmpty(file) && (lineNumber == 0)) && (columnNumber == 0);
            BuildWarningEventArgs e = new BuildWarningEventArgs(subcategory, warningCode, flag ? this.BuildEngine.ProjectFileOfTaskNode : file, flag ? this.BuildEngine.LineNumberOfTaskNode : lineNumber, flag ? this.BuildEngine.ColumnNumberOfTaskNode : columnNumber, endLineNumber, endColumnNumber, message, helpKeyword, this.TaskName, DateTime.UtcNow, messageArgs);
            this.BuildEngine.LogWarningEvent(e);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public void LogWarningFromException(Exception exception)
        {
            this.LogWarningFromException(exception, false);
        }

        public void LogWarningFromException(Exception exception, bool showStackTrace)
        {
            ErrorUtilities.VerifyThrowArgumentNull(exception, "exception");
            string message = exception.Message;
            if (showStackTrace)
            {
                message = message + Environment.NewLine + exception.StackTrace;
            }
            this.LogWarning(message, new object[0]);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public void LogWarningFromResources(string messageResourceName, params object[] messageArgs)
        {
            this.LogWarningFromResources(null, null, null, null, 0, 0, 0, 0, messageResourceName, messageArgs);
        }

        public void LogWarningFromResources(string subcategoryResourceName, string warningCode, string helpKeyword, string file, int lineNumber, int columnNumber, int endLineNumber, int endColumnNumber, string messageResourceName, params object[] messageArgs)
        {
            ErrorUtilities.VerifyThrowArgumentNull(messageResourceName, "messageResourceName");
            string subcategory = null;
            if (subcategoryResourceName != null)
            {
                subcategory = this.FormatResourceString(subcategoryResourceName, new object[0]);
            }
            this.LogWarning(subcategory, warningCode, helpKeyword, file, lineNumber, columnNumber, endLineNumber, endColumnNumber, this.FormatResourceString(messageResourceName, messageArgs), new object[0]);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public void LogWarningWithCodeFromResources(string messageResourceName, params object[] messageArgs)
        {
            this.LogWarningWithCodeFromResources(null, null, 0, 0, 0, 0, messageResourceName, messageArgs);
        }

        public void LogWarningWithCodeFromResources(string subcategoryResourceName, string file, int lineNumber, int columnNumber, int endLineNumber, int endColumnNumber, string messageResourceName, params object[] messageArgs)
        {
            string str2;
            ErrorUtilities.VerifyThrowArgumentNull(messageResourceName, "messageResourceName");
            string subcategory = null;
            if (subcategoryResourceName != null)
            {
                subcategory = this.FormatResourceString(subcategoryResourceName, new object[0]);
            }
            string message = ResourceUtilities.ExtractMessageCode(false, this.FormatResourceString(messageResourceName, messageArgs), out str2);
            string helpKeyword = null;
            if (this.HelpKeywordPrefix != null)
            {
                helpKeyword = this.HelpKeywordPrefix + messageResourceName;
            }
            this.LogWarning(subcategory, str2, helpKeyword, file, lineNumber, columnNumber, endLineNumber, endColumnNumber, message, new object[0]);
        }

        protected IBuildEngine BuildEngine
        {
            get
            {
                if (this.taskInstance != null)
                {
                    return this.taskInstance.BuildEngine;
                }
                return this.buildEngine;
            }
        }

        public bool HasLoggedErrors
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.hasLoggedErrors;
            }
        }

        public string HelpKeywordPrefix
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.helpKeywordPrefix;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.helpKeywordPrefix = value;
            }
        }

        protected string TaskName
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.taskName;
            }
        }

        private string TaskNameUpperCase
        {
            get
            {
                if (this.taskNameUpperCase == null)
                {
                    this.taskNameUpperCase = this.TaskName.ToUpper(CultureInfo.CurrentCulture);
                }
                return this.taskNameUpperCase;
            }
        }

        public ResourceManager TaskResources
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.taskResources;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.taskResources = value;
            }
        }
    }
}

