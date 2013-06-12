namespace System.ComponentModel.Design
{
    using System;

    public interface IDesignerOptionService
    {
        object GetOptionValue(string pageName, string valueName);
        void SetOptionValue(string pageName, string valueName, object value);
    }
}

