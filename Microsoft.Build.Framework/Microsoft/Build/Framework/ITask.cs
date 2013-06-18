namespace Microsoft.Build.Framework
{
    using System;

    public interface ITask
    {
        bool Execute();

        IBuildEngine BuildEngine { get; set; }

        ITaskHost HostObject { get; set; }
    }
}

