namespace System.Configuration
{
    using System;

    [AttributeUsage(AttributeTargets.Property)]
    public sealed class ApplicationScopedSettingAttribute : SettingAttribute
    {
    }
}

