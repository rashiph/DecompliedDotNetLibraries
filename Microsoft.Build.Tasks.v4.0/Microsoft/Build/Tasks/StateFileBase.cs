namespace Microsoft.Build.Tasks
{
    using Microsoft.Build.Shared;
    using Microsoft.Build.Utilities;
    using System;
    using System.IO;
    using System.Runtime.Serialization.Formatters.Binary;

    [Serializable]
    internal class StateFileBase
    {
        internal StateFileBase()
        {
        }

        internal static void DeleteFile(string stateFile, TaskLoggingHelper log)
        {
            try
            {
                if (((stateFile != null) && (stateFile.Length > 0)) && File.Exists(stateFile))
                {
                    File.Delete(stateFile);
                }
            }
            catch (Exception exception)
            {
                if (Microsoft.Build.Shared.ExceptionHandling.NotExpectedException(exception))
                {
                    throw;
                }
                log.LogWarningWithCodeFromResources("General.CouldNotDeleteStateFile", new object[] { stateFile, exception.Message });
            }
        }

        internal static StateFileBase DeserializeCache(string stateFile, TaskLoggingHelper log, Type requiredReturnType)
        {
            StateFileBase o = null;
            try
            {
                if (((stateFile == null) || (stateFile.Length <= 0)) || !File.Exists(stateFile))
                {
                    return o;
                }
                using (FileStream stream = new FileStream(stateFile, FileMode.Open))
                {
                    object obj2 = new BinaryFormatter().Deserialize(stream);
                    o = obj2 as StateFileBase;
                    if ((o == null) && (obj2 != null))
                    {
                        log.LogMessageFromResources("General.CouldNotReadStateFileMessage", new object[] { stateFile, log.FormatResourceString("General.IncompatibleStateFileType", new object[0]) });
                    }
                    if ((o != null) && !requiredReturnType.IsInstanceOfType(o))
                    {
                        log.LogWarningWithCodeFromResources("General.CouldNotReadStateFile", new object[] { stateFile, log.FormatResourceString("General.IncompatibleStateFileType", new object[0]) });
                        o = null;
                    }
                }
            }
            catch (Exception exception)
            {
                log.LogWarningWithCodeFromResources("General.CouldNotReadStateFile", new object[] { stateFile, exception.Message });
            }
            return o;
        }

        internal virtual void SerializeCache(string stateFile, TaskLoggingHelper log)
        {
            try
            {
                if ((stateFile != null) && (stateFile.Length > 0))
                {
                    if (File.Exists(stateFile))
                    {
                        File.Delete(stateFile);
                    }
                    using (FileStream stream = new FileStream(stateFile, FileMode.CreateNew))
                    {
                        new BinaryFormatter().Serialize(stream, this);
                    }
                }
            }
            catch (Exception exception)
            {
                if (Microsoft.Build.Shared.ExceptionHandling.NotExpectedException(exception))
                {
                    throw;
                }
                log.LogWarningWithCodeFromResources("General.CouldNotWriteStateFile", new object[] { stateFile, exception.Message });
            }
        }
    }
}

