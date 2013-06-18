namespace System.Web.SessionState
{
    using System;
    using System.Runtime.InteropServices;
    using System.Web;

    public interface ISessionIDManager
    {
        string CreateSessionID(HttpContext context);
        string GetSessionID(HttpContext context);
        void Initialize();
        bool InitializeRequest(HttpContext context, bool suppressAutoDetectRedirect, out bool supportSessionIDReissue);
        void RemoveSessionID(HttpContext context);
        void SaveSessionID(HttpContext context, string id, out bool redirected, out bool cookieAdded);
        bool Validate(string id);
    }
}

