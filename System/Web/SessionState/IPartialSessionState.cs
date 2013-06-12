namespace System.Web.SessionState
{
    using System.Collections.Generic;

    public interface IPartialSessionState
    {
        IList<string> PartialSessionStateKeys { get; }
    }
}

