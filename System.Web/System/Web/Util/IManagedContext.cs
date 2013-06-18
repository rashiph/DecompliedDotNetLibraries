namespace System.Web.Util
{
    using System;
    using System.Runtime.InteropServices;

    [ComImport, Guid("a1cca730-0e36-4870-aa7d-ca39c211f99d"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IManagedContext
    {
        [return: MarshalAs(UnmanagedType.I4)]
        int Context_IsPresent();
        void Application_Lock();
        void Application_UnLock();
        [return: MarshalAs(UnmanagedType.BStr)]
        string Application_GetContentsNames();
        [return: MarshalAs(UnmanagedType.BStr)]
        string Application_GetStaticNames();
        object Application_GetContentsObject([In, MarshalAs(UnmanagedType.LPWStr)] string name);
        void Application_SetContentsObject([In, MarshalAs(UnmanagedType.LPWStr)] string name, [In] object obj);
        void Application_RemoveContentsObject([In, MarshalAs(UnmanagedType.LPWStr)] string name);
        void Application_RemoveAllContentsObjects();
        object Application_GetStaticObject([In, MarshalAs(UnmanagedType.LPWStr)] string name);
        [return: MarshalAs(UnmanagedType.BStr)]
        string Request_GetAsString([In, MarshalAs(UnmanagedType.I4)] int what);
        [return: MarshalAs(UnmanagedType.BStr)]
        string Request_GetCookiesAsString();
        [return: MarshalAs(UnmanagedType.I4)]
        int Request_GetTotalBytes();
        [return: MarshalAs(UnmanagedType.I4)]
        int Request_BinaryRead([Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex=1)] byte[] bytes, int size);
        [return: MarshalAs(UnmanagedType.BStr)]
        string Response_GetCookiesAsString();
        void Response_AddCookie([In, MarshalAs(UnmanagedType.LPWStr)] string name);
        void Response_SetCookieText([In, MarshalAs(UnmanagedType.LPWStr)] string name, [In, MarshalAs(UnmanagedType.LPWStr)] string text);
        void Response_SetCookieSubValue([In, MarshalAs(UnmanagedType.LPWStr)] string name, [In, MarshalAs(UnmanagedType.LPWStr)] string key, [In, MarshalAs(UnmanagedType.LPWStr)] string value);
        void Response_SetCookieExpires([In, MarshalAs(UnmanagedType.LPWStr)] string name, [In, MarshalAs(UnmanagedType.R8)] double dtExpires);
        void Response_SetCookieDomain([In, MarshalAs(UnmanagedType.LPWStr)] string name, [In, MarshalAs(UnmanagedType.LPWStr)] string domain);
        void Response_SetCookiePath([In, MarshalAs(UnmanagedType.LPWStr)] string name, [In, MarshalAs(UnmanagedType.LPWStr)] string path);
        void Response_SetCookieSecure([In, MarshalAs(UnmanagedType.LPWStr)] string name, [In, MarshalAs(UnmanagedType.I4)] int secure);
        void Response_Write([In, MarshalAs(UnmanagedType.LPWStr)] string text);
        void Response_BinaryWrite([In, MarshalAs(UnmanagedType.LPArray, SizeParamIndex=1)] byte[] bytes, int size);
        void Response_Redirect([In, MarshalAs(UnmanagedType.LPWStr)] string url);
        void Response_AddHeader([In, MarshalAs(UnmanagedType.LPWStr)] string name, [In, MarshalAs(UnmanagedType.LPWStr)] string value);
        void Response_Pics([In, MarshalAs(UnmanagedType.LPWStr)] string value);
        void Response_Clear();
        void Response_Flush();
        void Response_End();
        void Response_AppendToLog([In, MarshalAs(UnmanagedType.LPWStr)] string entry);
        [return: MarshalAs(UnmanagedType.BStr)]
        string Response_GetContentType();
        void Response_SetContentType([In, MarshalAs(UnmanagedType.LPWStr)] string contentType);
        [return: MarshalAs(UnmanagedType.BStr)]
        string Response_GetCharSet();
        void Response_SetCharSet([In, MarshalAs(UnmanagedType.LPWStr)] string charSet);
        [return: MarshalAs(UnmanagedType.BStr)]
        string Response_GetCacheControl();
        void Response_SetCacheControl([In, MarshalAs(UnmanagedType.LPWStr)] string cacheControl);
        [return: MarshalAs(UnmanagedType.BStr)]
        string Response_GetStatus();
        void Response_SetStatus([In, MarshalAs(UnmanagedType.LPWStr)] string status);
        [return: MarshalAs(UnmanagedType.I4)]
        int Response_GetExpiresMinutes();
        void Response_SetExpiresMinutes([In, MarshalAs(UnmanagedType.I4)] int expiresMinutes);
        [return: MarshalAs(UnmanagedType.R8)]
        double Response_GetExpiresAbsolute();
        void Response_SetExpiresAbsolute([In, MarshalAs(UnmanagedType.R8)] double dtExpires);
        [return: MarshalAs(UnmanagedType.I4)]
        int Response_GetIsBuffering();
        void Response_SetIsBuffering([In, MarshalAs(UnmanagedType.I4)] int isBuffering);
        [return: MarshalAs(UnmanagedType.I4)]
        int Response_IsClientConnected();
        [return: MarshalAs(UnmanagedType.Interface)]
        object Server_CreateObject([In, MarshalAs(UnmanagedType.LPWStr)] string progId);
        [return: MarshalAs(UnmanagedType.BStr)]
        string Server_MapPath([In, MarshalAs(UnmanagedType.LPWStr)] string logicalPath);
        [return: MarshalAs(UnmanagedType.BStr)]
        string Server_HTMLEncode([In, MarshalAs(UnmanagedType.LPWStr)] string str);
        [return: MarshalAs(UnmanagedType.BStr)]
        string Server_URLEncode([In, MarshalAs(UnmanagedType.LPWStr)] string str);
        [return: MarshalAs(UnmanagedType.BStr)]
        string Server_URLPathEncode([In, MarshalAs(UnmanagedType.LPWStr)] string str);
        [return: MarshalAs(UnmanagedType.I4)]
        int Server_GetScriptTimeout();
        void Server_SetScriptTimeout([In, MarshalAs(UnmanagedType.I4)] int timeoutSeconds);
        void Server_Execute([In, MarshalAs(UnmanagedType.LPWStr)] string url);
        void Server_Transfer([In, MarshalAs(UnmanagedType.LPWStr)] string url);
        [return: MarshalAs(UnmanagedType.I4)]
        int Session_IsPresent();
        [return: MarshalAs(UnmanagedType.BStr)]
        string Session_GetID();
        [return: MarshalAs(UnmanagedType.I4)]
        int Session_GetTimeout();
        void Session_SetTimeout([In, MarshalAs(UnmanagedType.I4)] int value);
        [return: MarshalAs(UnmanagedType.I4)]
        int Session_GetCodePage();
        void Session_SetCodePage([In, MarshalAs(UnmanagedType.I4)] int value);
        [return: MarshalAs(UnmanagedType.I4)]
        int Session_GetLCID();
        void Session_SetLCID([In, MarshalAs(UnmanagedType.I4)] int value);
        void Session_Abandon();
        [return: MarshalAs(UnmanagedType.BStr)]
        string Session_GetContentsNames();
        [return: MarshalAs(UnmanagedType.BStr)]
        string Session_GetStaticNames();
        object Session_GetContentsObject([In, MarshalAs(UnmanagedType.LPWStr)] string name);
        void Session_SetContentsObject([In, MarshalAs(UnmanagedType.LPWStr)] string name, [In] object obj);
        void Session_RemoveContentsObject([In, MarshalAs(UnmanagedType.LPWStr)] string name);
        void Session_RemoveAllContentsObjects();
        object Session_GetStaticObject([In, MarshalAs(UnmanagedType.LPWStr)] string name);
    }
}

