namespace System.Web.UI
{
    using System;

    public interface IUserControlTypeResolutionService
    {
        Type GetType(string tagPrefix, string tagName);
    }
}

