namespace System.Web.UI
{
    using System;

    public interface IValidator
    {
        void Validate();

        string ErrorMessage { get; set; }

        bool IsValid { get; set; }
    }
}

