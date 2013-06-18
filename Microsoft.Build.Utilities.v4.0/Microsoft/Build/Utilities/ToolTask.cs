namespace Microsoft.Build.Utilities
{
    using Microsoft.Build.Framework;
    using Microsoft.Build.Shared;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.IO;
    using System.Resources;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Threading;

    public abstract class ToolTask : Task, ICancelableTask, ITask
    {
        private List<KeyValuePair<object, object>> environmentVariablePairs;
        private static char[] equalsSplitter = new char[] { '=' };
        private object eventCloseLock;
        private bool eventsDisposed;
        private int exitCode;
        private TaskLoggingHelper logPrivate;
        private TaskLoggingHelper logShared;
        private bool logStandardErrorAsError;
        private Queue standardErrorData;
        private ManualResetEvent standardErrorDataAvailable;
        private string standardErrorImportance;
        private MessageImportance standardErrorImportanceToUse;
        private Queue standardOutputData;
        private ManualResetEvent standardOutputDataAvailable;
        private string standardOutputImportance;
        private MessageImportance standardOutputImportanceToUse;
        private string temporaryBatchFile;
        private bool terminatedTool;
        private int timeout;
        private string toolExe;
        private ManualResetEvent toolExited;
        private string toolPath;
        private ManualResetEvent toolTimeoutExpired;
        private Timer toolTimer;

        protected ToolTask()
        {
            this.timeout = -1;
            this.eventCloseLock = new object();
            this.standardOutputImportanceToUse = MessageImportance.Low;
            this.standardErrorImportanceToUse = MessageImportance.Normal;
            this.logPrivate = new TaskLoggingHelper(this);
            this.logPrivate.TaskResources = AssemblyResources.PrimaryResources;
            this.logPrivate.HelpKeywordPrefix = "MSBuild.";
            this.logShared = new TaskLoggingHelper(this);
            this.logShared.TaskResources = AssemblyResources.SharedResources;
            this.logShared.HelpKeywordPrefix = "MSBuild.";
            this.TaskProcessTerminationTimeout = 0x1388;
            this.ToolCanceled = new ManualResetEvent(false);
        }

        protected ToolTask(ResourceManager taskResources) : this()
        {
            base.TaskResources = taskResources;
        }

        protected ToolTask(ResourceManager taskResources, string helpKeywordPrefix) : this(taskResources)
        {
            base.HelpKeywordPrefix = helpKeywordPrefix;
        }

        private bool AssignStandardStreamLoggingImportance()
        {
            if ((this.standardErrorImportance == null) || (this.standardErrorImportance.Length == 0))
            {
                this.standardErrorImportanceToUse = this.StandardErrorLoggingImportance;
            }
            else
            {
                try
                {
                    this.standardErrorImportanceToUse = (MessageImportance) Enum.Parse(typeof(MessageImportance), this.standardErrorImportance, true);
                }
                catch (ArgumentException)
                {
                    base.Log.LogErrorWithCodeFromResources("Message.InvalidImportance", new object[] { this.standardErrorImportance });
                    return false;
                }
            }
            if ((this.standardOutputImportance == null) || (this.standardOutputImportance.Length == 0))
            {
                this.standardOutputImportanceToUse = this.StandardOutputLoggingImportance;
            }
            else
            {
                try
                {
                    this.standardOutputImportanceToUse = (MessageImportance) Enum.Parse(typeof(MessageImportance), this.standardOutputImportance, true);
                }
                catch (ArgumentException)
                {
                    base.Log.LogErrorWithCodeFromResources("Message.InvalidImportance", new object[] { this.standardOutputImportance });
                    return false;
                }
            }
            return true;
        }

        protected virtual bool CallHostObjectToExecute()
        {
            return false;
        }

        public virtual void Cancel()
        {
            this.ToolCanceled.Set();
        }

        private string ComputePathToTool()
        {
            string str;
            if (this.UseCommandProcessor)
            {
                return this.ToolExe;
            }
            if ((this.ToolPath != null) && (this.ToolPath.Length > 0))
            {
                str = Path.Combine(this.ToolPath, this.ToolExe);
            }
            else
            {
                str = this.GenerateFullPathToTool();
                if ((str != null) && !string.IsNullOrEmpty(this.toolExe))
                {
                    str = Path.Combine(Path.GetDirectoryName(str), this.ToolExe);
                }
            }
            if (str != null)
            {
                if (Path.GetFileName(str).Length != str.Length)
                {
                    if (!File.Exists(str))
                    {
                        this.LogPrivate.LogErrorWithCodeFromResources("ToolTask.ToolExecutableNotFound", new object[] { str });
                        return null;
                    }
                    return str;
                }
                string str3 = NativeMethodsShared.FindOnPath(str);
                if (str3 != null)
                {
                    str = str3;
                }
            }
            return str;
        }

        protected void DeleteTempFile(string fileName)
        {
            try
            {
                File.Delete(fileName);
            }
            catch (Exception exception)
            {
                if (ExceptionHandling.NotExpectedException(exception))
                {
                    throw;
                }
                this.LogShared.LogWarningWithCodeFromResources("Shared.FailedDeletingTempFile", new object[] { fileName, exception.Message });
            }
        }

        public override bool Execute()
        {
            bool flag2;
            if (!this.ValidateParameters())
            {
                return false;
            }
            if (this.EnvironmentVariables != null)
            {
                this.environmentVariablePairs = new List<KeyValuePair<object, object>>(this.EnvironmentVariables.Length);
                foreach (string str in this.EnvironmentVariables)
                {
                    string[] strArray = str.Split(equalsSplitter, 2);
                    if ((strArray.Length == 1) || ((strArray.Length == 2) && (strArray[0].Length == 0)))
                    {
                        this.LogPrivate.LogErrorWithCodeFromResources("ToolTask.InvalidEnvironmentParameter", new object[] { strArray[0] });
                        return false;
                    }
                    this.environmentVariablePairs.Add(new KeyValuePair<object, object>(strArray[0], strArray[1]));
                }
            }
            if (!this.AssignStandardStreamLoggingImportance())
            {
                return false;
            }
            try
            {
                if (this.SkipTaskExecution())
                {
                    return true;
                }
                string contents = this.GenerateCommandLineCommands();
                string message = contents;
                string responseFileCommands = this.GenerateResponseFileCommands();
                if (this.UseCommandProcessor)
                {
                    this.ToolExe = "cmd.exe";
                    this.temporaryBatchFile = FileUtilities.GetTemporaryFile(".cmd");
                    File.AppendAllText(this.temporaryBatchFile, contents, Encoding.ASCII);
                    string temporaryBatchFile = this.temporaryBatchFile;
                    if (temporaryBatchFile.Contains("&") && !temporaryBatchFile.Contains("^&"))
                    {
                        temporaryBatchFile = NativeMethodsShared.GetShortFilePath(temporaryBatchFile).Replace("&", "^&");
                    }
                    contents = "/C \"" + temporaryBatchFile + "\"";
                    if (this.EchoOff)
                    {
                        contents = "/Q " + contents;
                    }
                }
                if ((contents == null) || (contents.Length == 0))
                {
                    contents = string.Empty;
                }
                else
                {
                    contents = " " + contents;
                }
                HostObjectInitializationStatus status = this.InitializeHostObject();
                switch (status)
                {
                    case HostObjectInitializationStatus.NoActionReturnSuccess:
                        return true;

                    case HostObjectInitializationStatus.NoActionReturnFailure:
                        this.exitCode = 1;
                        return this.HandleTaskExecutionErrors();

                    default:
                    {
                        string pathToTool = this.ComputePathToTool();
                        if (pathToTool == null)
                        {
                            return false;
                        }
                        bool alreadyLoggedEnvironmentHeader = false;
                        StringDictionary environmentOverride = this.EnvironmentOverride;
                        if (environmentOverride != null)
                        {
                            foreach (DictionaryEntry entry in environmentOverride)
                            {
                                alreadyLoggedEnvironmentHeader = this.LogEnvironmentVariable(alreadyLoggedEnvironmentHeader, (string) entry.Key, (string) entry.Value);
                            }
                        }
                        if (this.environmentVariablePairs != null)
                        {
                            foreach (KeyValuePair<object, object> pair in this.environmentVariablePairs)
                            {
                                alreadyLoggedEnvironmentHeader = this.LogEnvironmentVariable(alreadyLoggedEnvironmentHeader, (string) pair.Key, (string) pair.Value);
                            }
                        }
                        if (this.UseCommandProcessor)
                        {
                            this.LogToolCommand(pathToTool + contents);
                            this.LogToolCommand(message);
                        }
                        else
                        {
                            this.LogToolCommand(pathToTool + contents + " " + responseFileCommands);
                        }
                        this.exitCode = 0;
                        if (status == HostObjectInitializationStatus.UseHostObjectToExecute)
                        {
                            try
                            {
                                if (!this.CallHostObjectToExecute())
                                {
                                    this.exitCode = 1;
                                }
                                break;
                            }
                            catch (Exception exception)
                            {
                                this.LogPrivate.LogErrorFromException(exception);
                                return false;
                            }
                        }
                        ErrorUtilities.VerifyThrow(status == HostObjectInitializationStatus.UseAlternateToolToExecute, "Invalid return status");
                        this.exitCode = this.ExecuteTool(pathToTool, responseFileCommands, contents);
                        break;
                    }
                }
                if (this.terminatedTool)
                {
                    return false;
                }
                if (this.exitCode != 0)
                {
                    return this.HandleTaskExecutionErrors();
                }
                flag2 = true;
            }
            catch (ArgumentException exception2)
            {
                if (!this.terminatedTool)
                {
                    this.LogPrivate.LogErrorWithCodeFromResources("General.InvalidToolSwitch", new object[] { this.ToolExe, this.GetErrorMessageWithDiagnosticsCheck(exception2) });
                }
                flag2 = false;
            }
            catch (Win32Exception exception3)
            {
                if (!this.terminatedTool)
                {
                    this.LogPrivate.LogErrorWithCodeFromResources("ToolTask.CouldNotStartToolExecutable", new object[] { this.ToolExe, this.GetErrorMessageWithDiagnosticsCheck(exception3) });
                }
                flag2 = false;
            }
            catch (IOException exception4)
            {
                if (!this.terminatedTool)
                {
                    this.LogPrivate.LogErrorWithCodeFromResources("ToolTask.CouldNotStartToolExecutable", new object[] { this.ToolExe, this.GetErrorMessageWithDiagnosticsCheck(exception4) });
                }
                flag2 = false;
            }
            catch (UnauthorizedAccessException exception5)
            {
                if (!this.terminatedTool)
                {
                    this.LogPrivate.LogErrorWithCodeFromResources("ToolTask.CouldNotStartToolExecutable", new object[] { this.ToolExe, this.GetErrorMessageWithDiagnosticsCheck(exception5) });
                }
                flag2 = false;
            }
            finally
            {
                if ((this.temporaryBatchFile != null) && File.Exists(this.temporaryBatchFile))
                {
                    File.Delete(this.temporaryBatchFile);
                }
            }
            return flag2;
        }

        protected virtual int ExecuteTool(string pathToTool, string responseFileCommands, string commandLineCommands)
        {
            if (!this.UseCommandProcessor)
            {
                this.LogPathToTool(this.ToolExe, pathToTool);
            }
            string fileName = null;
            Process proc = null;
            this.standardErrorData = new Queue();
            this.standardOutputData = new Queue();
            this.standardErrorDataAvailable = new ManualResetEvent(false);
            this.standardOutputDataAvailable = new ManualResetEvent(false);
            this.toolExited = new ManualResetEvent(false);
            this.toolTimeoutExpired = new ManualResetEvent(false);
            this.eventsDisposed = false;
            try
            {
                string str2;
                fileName = this.GetTemporaryResponseFile(responseFileCommands, out str2);
                proc = new Process {
                    StartInfo = this.GetProcessStartInfo(pathToTool, commandLineCommands, str2),
                    EnableRaisingEvents = true
                };
                proc.Exited += new EventHandler(this.ReceiveExitNotification);
                proc.ErrorDataReceived += new DataReceivedEventHandler(this.ReceiveStandardErrorData);
                proc.OutputDataReceived += new DataReceivedEventHandler(this.ReceiveStandardOutputData);
                this.exitCode = -1;
                proc.Start();
                proc.StandardInput.Close();
                proc.BeginErrorReadLine();
                proc.BeginOutputReadLine();
                this.toolTimer = new Timer(new TimerCallback(this.ReceiveTimeoutNotification));
                this.toolTimer.Change(this.Timeout, -1);
                this.HandleToolNotifications(proc);
            }
            finally
            {
                if (fileName != null)
                {
                    this.DeleteTempFile(fileName);
                }
                if (proc != null)
                {
                    try
                    {
                        this.exitCode = proc.ExitCode;
                    }
                    catch (InvalidOperationException)
                    {
                    }
                    proc.Close();
                    proc = null;
                }
                if ((this.exitCode == 0) && this.HasLoggedErrors)
                {
                    this.exitCode = -1;
                }
                lock (this.eventCloseLock)
                {
                    this.eventsDisposed = true;
                    this.standardErrorDataAvailable.Close();
                    this.standardOutputDataAvailable.Close();
                    this.toolExited.Close();
                    this.toolTimeoutExpired.Close();
                    if (this.toolTimer != null)
                    {
                        this.toolTimer.Dispose();
                    }
                }
            }
            return this.exitCode;
        }

        protected virtual string GenerateCommandLineCommands()
        {
            return string.Empty;
        }

        protected abstract string GenerateFullPathToTool();
        protected virtual string GenerateResponseFileCommands()
        {
            return string.Empty;
        }

        private string GetErrorMessageWithDiagnosticsCheck(Exception e)
        {
            if (Environment.GetEnvironmentVariable("MSBuildDiagnostics") != null)
            {
                return e.ToString();
            }
            return e.Message;
        }

        protected ProcessStartInfo GetProcessStartInfo(string pathToTool, string commandLineCommands, string responseFileSwitch)
        {
            string arguments = commandLineCommands;
            if (!this.UseCommandProcessor && !string.IsNullOrEmpty(responseFileSwitch))
            {
                arguments = arguments + " " + responseFileSwitch;
            }
            if (arguments.Length > 0x7d00)
            {
                this.LogPrivate.LogWarningWithCodeFromResources("ToolTask.CommandTooLong", new object[] { base.GetType().Name });
            }
            ProcessStartInfo info = new ProcessStartInfo(pathToTool, arguments) {
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                StandardErrorEncoding = this.StandardErrorEncoding,
                StandardOutputEncoding = this.StandardOutputEncoding,
                RedirectStandardInput = true
            };
            string workingDirectory = this.GetWorkingDirectory();
            if (workingDirectory != null)
            {
                info.WorkingDirectory = workingDirectory;
            }
            StringDictionary environmentOverride = this.EnvironmentOverride;
            if (environmentOverride != null)
            {
                foreach (DictionaryEntry entry in environmentOverride)
                {
                    info.EnvironmentVariables[(string) entry.Key] = (string) entry.Value;
                }
            }
            if (this.environmentVariablePairs != null)
            {
                foreach (KeyValuePair<object, object> pair in this.environmentVariablePairs)
                {
                    info.EnvironmentVariables[(string) pair.Key] = (string) pair.Value;
                }
            }
            return info;
        }

        protected virtual string GetResponseFileSwitch(string responseFilePath)
        {
            return ("@\"" + responseFilePath + "\"");
        }

        private string GetTemporaryResponseFile(string responseFileCommands, out string responseFileSwitch)
        {
            string path = null;
            responseFileSwitch = null;
            if (!string.IsNullOrEmpty(responseFileCommands))
            {
                path = FileUtilities.GetTemporaryFile(".rsp");
                using (StreamWriter writer = new StreamWriter(path, false, this.ResponseFileEncoding))
                {
                    writer.Write(responseFileCommands);
                }
                responseFileSwitch = this.GetResponseFileSwitch(path);
            }
            return path;
        }

        protected virtual string GetWorkingDirectory()
        {
            return null;
        }

        protected virtual bool HandleTaskExecutionErrors()
        {
            if (this.HasLoggedErrors)
            {
                this.LogPrivate.LogMessageFromResources(MessageImportance.Low, "General.ToolCommandFailedNoErrorCode", new object[] { this.exitCode });
            }
            else
            {
                this.LogPrivate.LogErrorWithCodeFromResources("ToolTask.ToolCommandFailed", new object[] { this.ToolExe, this.exitCode });
            }
            return false;
        }

        private void HandleToolNotifications(Process proc)
        {
            WaitHandle[] waitHandles = new WaitHandle[] { this.toolTimeoutExpired, this.ToolCanceled, this.standardErrorDataAvailable, this.standardOutputDataAvailable, this.toolExited };
            bool flag = true;
            if (this.YieldDuringToolExecution)
            {
                base.BuildEngine3.Yield();
            }
            try
            {
                while (flag)
                {
                    int num = WaitHandle.WaitAny(waitHandles);
                    switch (num)
                    {
                        case 0:
                        case 1:
                        {
                            this.TerminateToolProcess(proc, num == 1);
                            this.terminatedTool = true;
                            flag = false;
                            continue;
                        }
                        case 2:
                        {
                            this.LogMessagesFromStandardError();
                            this.LogMessagesFromStandardOutput();
                            continue;
                        }
                        case 3:
                        {
                            this.LogMessagesFromStandardOutput();
                            continue;
                        }
                        case 4:
                        {
                            this.WaitForProcessExit(proc);
                            this.LogMessagesFromStandardError();
                            this.LogMessagesFromStandardOutput();
                            flag = false;
                            continue;
                        }
                    }
                    ErrorUtilities.VerifyThrow(false, "Unknown tool notification.");
                }
            }
            finally
            {
                if (this.YieldDuringToolExecution)
                {
                    base.BuildEngine3.Reacquire();
                }
            }
        }

        protected virtual HostObjectInitializationStatus InitializeHostObject()
        {
            return HostObjectInitializationStatus.UseAlternateToolToExecute;
        }

        private void KillToolProcessOnTimeout(Process proc, bool isBeingCancelled)
        {
            if (!proc.HasExited)
            {
                if (!isBeingCancelled)
                {
                    ErrorUtilities.VerifyThrow(this.Timeout != -1, "A time-out value must have been specified or the task must be cancelled.");
                    this.LogShared.LogWarningWithCodeFromResources("Shared.KillingProcess", new object[] { this.Timeout });
                }
                else
                {
                    this.LogShared.LogWarningWithCodeFromResources("Shared.KillingProcessByCancellation", new object[0]);
                }
                try
                {
                    NativeMethodsShared.KillTree(proc.Id);
                }
                catch (InvalidOperationException)
                {
                }
                int milliseconds = 0x1388;
                string environmentVariable = Environment.GetEnvironmentVariable("MSBUILDTOOLTASKCANCELPROCESSWAITTIMEOUT");
                if (environmentVariable != null)
                {
                    int result = 0;
                    if (int.TryParse(environmentVariable, out result) && (result >= 0))
                    {
                        milliseconds = result;
                    }
                }
                proc.WaitForExit(milliseconds);
            }
        }

        private bool LogEnvironmentVariable(bool alreadyLoggedEnvironmentHeader, string key, string value)
        {
            if (!alreadyLoggedEnvironmentHeader)
            {
                this.LogPrivate.LogMessageFromResources(MessageImportance.Low, "ToolTask.EnvironmentVariableHeader", new object[0]);
                alreadyLoggedEnvironmentHeader = true;
            }
            base.Log.LogMessage(MessageImportance.Low, "  {0}={1}", new object[] { key, value });
            return alreadyLoggedEnvironmentHeader;
        }

        protected virtual void LogEventsFromTextOutput(string singleLine, MessageImportance messageImportance)
        {
            base.Log.LogMessageFromText(singleLine, messageImportance);
        }

        private void LogMessagesFromStandardError()
        {
            this.LogMessagesFromStandardErrorOrOutput(this.standardErrorData, this.standardErrorDataAvailable, this.standardErrorImportanceToUse, StandardOutputOrErrorQueueType.StandardError);
        }

        private void LogMessagesFromStandardErrorOrOutput(Queue dataQueue, ManualResetEvent dataAvailableSignal, MessageImportance messageImportance, StandardOutputOrErrorQueueType queueType)
        {
            ErrorUtilities.VerifyThrow(dataQueue != null, "The data queue must be available.");
            lock (dataQueue.SyncRoot)
            {
                while (dataQueue.Count > 0)
                {
                    string singleLine = dataQueue.Dequeue() as string;
                    if (!this.LogStandardErrorAsError || (queueType == StandardOutputOrErrorQueueType.StandardOutput))
                    {
                        this.LogEventsFromTextOutput(singleLine, messageImportance);
                    }
                    else if (this.LogStandardErrorAsError && (queueType == StandardOutputOrErrorQueueType.StandardError))
                    {
                        base.Log.LogError(singleLine, new object[0]);
                    }
                }
                ErrorUtilities.VerifyThrow(dataAvailableSignal != null, "The signalling event must be available.");
                dataAvailableSignal.Reset();
            }
        }

        private void LogMessagesFromStandardOutput()
        {
            this.LogMessagesFromStandardErrorOrOutput(this.standardOutputData, this.standardOutputDataAvailable, this.standardOutputImportanceToUse, StandardOutputOrErrorQueueType.StandardOutput);
        }

        protected virtual void LogPathToTool(string toolName, string pathToTool)
        {
        }

        protected virtual void LogToolCommand(string message)
        {
            this.LogPrivate.LogCommandLine(MessageImportance.High, message);
        }

        private void ReceiveExitNotification(object sender, EventArgs e)
        {
            ErrorUtilities.VerifyThrow(this.toolExited != null, "The signalling event for tool exit must be available.");
            lock (this.eventCloseLock)
            {
                if (!this.eventsDisposed)
                {
                    this.toolExited.Set();
                }
            }
        }

        private void ReceiveStandardErrorData(object sender, DataReceivedEventArgs e)
        {
            this.ReceiveStandardErrorOrOutputData(e, this.standardErrorData, this.standardErrorDataAvailable);
        }

        private void ReceiveStandardErrorOrOutputData(DataReceivedEventArgs e, Queue dataQueue, ManualResetEvent dataAvailableSignal)
        {
            if (e.Data != null)
            {
                ErrorUtilities.VerifyThrow(dataQueue != null, "The data queue must be available.");
                lock (dataQueue.SyncRoot)
                {
                    dataQueue.Enqueue(e.Data);
                    ErrorUtilities.VerifyThrow(dataAvailableSignal != null, "The signalling event must be available.");
                    lock (this.eventCloseLock)
                    {
                        if (!this.eventsDisposed)
                        {
                            dataAvailableSignal.Set();
                        }
                    }
                }
            }
        }

        private void ReceiveStandardOutputData(object sender, DataReceivedEventArgs e)
        {
            this.ReceiveStandardErrorOrOutputData(e, this.standardOutputData, this.standardOutputDataAvailable);
        }

        private void ReceiveTimeoutNotification(object unused)
        {
            ErrorUtilities.VerifyThrow(this.toolTimeoutExpired != null, "The signalling event for tool time-out must be available.");
            lock (this.eventCloseLock)
            {
                if (!this.eventsDisposed)
                {
                    this.toolTimeoutExpired.Set();
                }
            }
        }

        protected virtual bool SkipTaskExecution()
        {
            return false;
        }

        private void TerminateToolProcess(Process proc, bool isBeingCancelled)
        {
            if ((proc != null) && !proc.HasExited)
            {
                if (isBeingCancelled)
                {
                    try
                    {
                        proc.CancelOutputRead();
                        proc.CancelErrorRead();
                    }
                    catch (InvalidOperationException)
                    {
                    }
                }
                this.KillToolProcessOnTimeout(proc, isBeingCancelled);
            }
        }

        protected internal virtual bool ValidateParameters()
        {
            return true;
        }

        private void WaitForProcessExit(Process proc)
        {
            proc.WaitForExit();
            while (!proc.HasExited)
            {
                Thread.Sleep(50);
            }
        }

        public bool EchoOff
        {
            [CompilerGenerated, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.<EchoOff>k__BackingField;
            }
            [CompilerGenerated, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.<EchoOff>k__BackingField = value;
            }
        }

        [Obsolete("Use EnvironmentVariables property")]
        protected virtual StringDictionary EnvironmentOverride
        {
            get
            {
                return null;
            }
        }

        public string[] EnvironmentVariables
        {
            [CompilerGenerated, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.<EnvironmentVariables>k__BackingField;
            }
            [CompilerGenerated, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.<EnvironmentVariables>k__BackingField = value;
            }
        }

        [Output]
        public int ExitCode
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.exitCode;
            }
        }

        protected virtual bool HasLoggedErrors
        {
            get
            {
                if (!base.Log.HasLoggedErrors && !this.LogPrivate.HasLoggedErrors)
                {
                    return this.LogShared.HasLoggedErrors;
                }
                return true;
            }
        }

        private TaskLoggingHelper LogPrivate
        {
            get
            {
                return this.logPrivate;
            }
        }

        private TaskLoggingHelper LogShared
        {
            get
            {
                return this.logShared;
            }
        }

        public bool LogStandardErrorAsError
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.logStandardErrorAsError;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.logStandardErrorAsError = value;
            }
        }

        protected virtual Encoding ResponseFileEncoding
        {
            get
            {
                return Encoding.UTF8;
            }
        }

        protected virtual Encoding StandardErrorEncoding
        {
            get
            {
                return EncodingUtilities.CurrentSystemOemEncoding;
            }
        }

        public string StandardErrorImportance
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.standardErrorImportance;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.standardErrorImportance = value;
            }
        }

        protected MessageImportance StandardErrorImportanceToUse
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.standardErrorImportanceToUse;
            }
        }

        protected virtual MessageImportance StandardErrorLoggingImportance
        {
            get
            {
                return MessageImportance.Normal;
            }
        }

        protected virtual Encoding StandardOutputEncoding
        {
            get
            {
                return EncodingUtilities.CurrentSystemOemEncoding;
            }
        }

        public string StandardOutputImportance
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.standardOutputImportance;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.standardOutputImportance = value;
            }
        }

        protected MessageImportance StandardOutputImportanceToUse
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.standardOutputImportanceToUse;
            }
        }

        protected virtual MessageImportance StandardOutputLoggingImportance
        {
            get
            {
                return MessageImportance.Low;
            }
        }

        protected int TaskProcessTerminationTimeout
        {
            [CompilerGenerated, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.<TaskProcessTerminationTimeout>k__BackingField;
            }
            [CompilerGenerated, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.<TaskProcessTerminationTimeout>k__BackingField = value;
            }
        }

        public virtual int Timeout
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.timeout;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.timeout = value;
            }
        }

        protected ManualResetEvent ToolCanceled
        {
            [CompilerGenerated, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.<ToolCanceled>k__BackingField;
            }
            [CompilerGenerated]
            private set
            {
                this.<ToolCanceled>k__BackingField = value;
            }
        }

        public virtual string ToolExe
        {
            get
            {
                if (!string.IsNullOrEmpty(this.toolExe))
                {
                    return this.toolExe;
                }
                return this.ToolName;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.toolExe = value;
            }
        }

        protected abstract string ToolName { get; }

        public string ToolPath
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.toolPath;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.toolPath = value;
            }
        }

        public bool UseCommandProcessor
        {
            [CompilerGenerated, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.<UseCommandProcessor>k__BackingField;
            }
            [CompilerGenerated, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.<UseCommandProcessor>k__BackingField = value;
            }
        }

        public bool YieldDuringToolExecution
        {
            [CompilerGenerated, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.<YieldDuringToolExecution>k__BackingField;
            }
            [CompilerGenerated, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.<YieldDuringToolExecution>k__BackingField = value;
            }
        }

        private enum StandardOutputOrErrorQueueType
        {
            StandardError,
            StandardOutput
        }
    }
}

