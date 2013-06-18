namespace System.Web.Services.Description
{
    using System;

    internal class DelegateInfo
    {
        internal string handlerArgs;
        internal string handlerType;

        internal DelegateInfo(string handlerType, string handlerArgs)
        {
            this.handlerType = handlerType;
            this.handlerArgs = handlerArgs;
        }
    }
}

