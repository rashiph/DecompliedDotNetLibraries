namespace System.Net
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using System.Threading;

    internal class ConnectionReturnResult
    {
        private List<RequestContext> m_Context;
        private static readonly WaitCallback s_InvokeConnectionCallback = new WaitCallback(ConnectionReturnResult.InvokeConnectionCallback);

        internal ConnectionReturnResult()
        {
            this.m_Context = new List<RequestContext>(5);
        }

        internal ConnectionReturnResult(int capacity)
        {
            this.m_Context = new List<RequestContext>(capacity);
        }

        internal static void Add(ref ConnectionReturnResult returnResult, HttpWebRequest request, CoreResponseData coreResponseData)
        {
            if (coreResponseData == null)
            {
                throw new InternalException();
            }
            if (returnResult == null)
            {
                returnResult = new ConnectionReturnResult();
            }
            returnResult.m_Context.Add(new RequestContext(request, coreResponseData));
        }

        internal static void AddExceptionRange(ref ConnectionReturnResult returnResult, HttpWebRequest[] requests, Exception exception)
        {
            AddExceptionRange(ref returnResult, requests, exception, exception);
        }

        internal static void AddExceptionRange(ref ConnectionReturnResult returnResult, HttpWebRequest[] requests, Exception exception, Exception firstRequestException)
        {
            if (exception == null)
            {
                throw new InternalException();
            }
            if (returnResult == null)
            {
                returnResult = new ConnectionReturnResult(requests.Length);
            }
            for (int i = 0; i < requests.Length; i++)
            {
                if (i == 0)
                {
                    returnResult.m_Context.Add(new RequestContext(requests[i], firstRequestException));
                }
                else
                {
                    returnResult.m_Context.Add(new RequestContext(requests[i], exception));
                }
            }
        }

        private static void InvokeConnectionCallback(object objectReturnResult)
        {
            ConnectionReturnResult returnResult = (ConnectionReturnResult) objectReturnResult;
            SetResponses(returnResult);
        }

        internal static void SetResponses(ConnectionReturnResult returnResult)
        {
            if (returnResult != null)
            {
                for (int i = 0; i < returnResult.m_Context.Count; i++)
                {
                    try
                    {
                        returnResult.m_Context[i].Request.SetAndOrProcessResponse(returnResult.m_Context[i].CoreResponse);
                    }
                    catch (Exception)
                    {
                        returnResult.m_Context.RemoveRange(0, i + 1);
                        if (returnResult.m_Context.Count > 0)
                        {
                            ThreadPool.UnsafeQueueUserWorkItem(s_InvokeConnectionCallback, returnResult);
                        }
                        throw;
                    }
                }
                returnResult.m_Context.Clear();
            }
        }

        internal bool IsNotEmpty
        {
            get
            {
                return (this.m_Context.Count != 0);
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct RequestContext
        {
            internal HttpWebRequest Request;
            internal object CoreResponse;
            internal RequestContext(HttpWebRequest request, object coreResponse)
            {
                this.Request = request;
                this.CoreResponse = coreResponse;
            }
        }
    }
}

