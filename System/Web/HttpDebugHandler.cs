namespace System.Web
{
    using System;
    using System.Globalization;
    using System.Text;
    using System.Web.Util;

    internal class HttpDebugHandler : IHttpHandler
    {
        private static string[] validClsIds = new string[] { "{70f65411-fe8c-4248-bcff-701c8b2f4529}", "{62a78ac2-7d9a-4377-b97e-6965919fdd02}", "{cc23651f-4574-438f-b4aa-bcb28b6b3ecf}", "{dbfdb1d0-04a4-4315-b15c-f874f6b6e90b}", "{a4fcb474-2687-4924-b0ad-7caf331db826}", "{beb261f6-d5f0-43ba-baf4-8b79785fffaf}", "{8e2f5e28-d4e2-44c0-aa02-f8c5beb70cac}", "{08100915-0f41-4ccf-9564-ebaa5d49446c}" };

        internal HttpDebugHandler()
        {
        }

        public void ProcessRequest(HttpContext context)
        {
            if (!HttpRuntime.DebuggingEnabled)
            {
                context.Response.Write(System.Web.SR.GetString("Debugging_forbidden", new object[] { context.Request.Path }));
                context.Response.StatusCode = 0x193;
            }
            else
            {
                string str = context.Request.Headers["Command"];
                if (str == null)
                {
                    context.Response.Write(System.Web.SR.GetString("Invalid_Debug_Request"));
                    context.Response.StatusCode = 500;
                }
                else if (StringUtil.EqualsIgnoreCase(str, "stop-debug"))
                {
                    context.Response.Write("OK");
                }
                else if (!StringUtil.EqualsIgnoreCase(str, "start-debug"))
                {
                    context.Response.Write(System.Web.SR.GetString("Invalid_Debug_Request"));
                    context.Response.StatusCode = 500;
                }
                else
                {
                    string serverVariable = context.WorkerRequest.GetServerVariable("AUTH_TYPE");
                    if ((string.IsNullOrEmpty(context.WorkerRequest.GetServerVariable("LOGON_USER")) || string.IsNullOrEmpty(serverVariable)) || StringUtil.EqualsIgnoreCase(serverVariable, "basic"))
                    {
                        context.Response.Write(System.Web.SR.GetString("Debug_Access_Denied", new object[] { context.Request.Path }));
                        context.Response.StatusCode = 0x191;
                    }
                    else
                    {
                        string str4 = context.Request.Form["DebugSessionID"];
                        if (string.IsNullOrEmpty(str4))
                        {
                            context.Response.Write(System.Web.SR.GetString("Invalid_Debug_ID"));
                            context.Response.StatusCode = 500;
                        }
                        else
                        {
                            HttpValueCollection values = new HttpValueCollection(str4.Replace(';', '&'), true, true, Encoding.UTF8);
                            string str6 = values["autoattachclsid"];
                            bool flag = false;
                            if (str6 != null)
                            {
                                for (int i = 0; i < validClsIds.Length; i++)
                                {
                                    if (StringUtil.EqualsIgnoreCase(str6, validClsIds[i]))
                                    {
                                        flag = true;
                                        break;
                                    }
                                }
                            }
                            if (!flag)
                            {
                                context.Response.Write(System.Web.SR.GetString("Debug_Access_Denied", new object[] { context.Request.Path }));
                                context.Response.StatusCode = 0x191;
                            }
                            else
                            {
                                int num2 = UnsafeNativeMethods.AttachDebugger(str6, str4, context.WorkerRequest.GetUserToken());
                                if (num2 != 0)
                                {
                                    context.Response.Write(System.Web.SR.GetString("Error_Attaching_with_MDM", new object[] { "0x" + num2.ToString("X8", CultureInfo.InvariantCulture) }));
                                    context.Response.StatusCode = 500;
                                }
                                else
                                {
                                    PerfCounters.IncrementCounter(AppPerfCounter.DEBUGGING_REQUESTS);
                                    context.Response.Write("OK");
                                }
                            }
                        }
                    }
                }
            }
        }

        public bool IsReusable
        {
            get
            {
                return true;
            }
        }
    }
}

