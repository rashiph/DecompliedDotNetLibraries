namespace Microsoft.Build.Framework
{
    using System;

    public interface IGeneratedTask : ITask
    {
        object GetPropertyValue(TaskPropertyInfo property);
        void SetPropertyValue(TaskPropertyInfo property, object value);
    }
}

