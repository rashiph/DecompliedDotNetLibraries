namespace System.Web
{
    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Data;
    using System.Diagnostics;
    using System.Globalization;
    using System.Text;
    using System.Web.Configuration;
    using System.Web.Handlers;
    using System.Web.SessionState;
    using System.Web.UI;
    using System.Web.Util;

    public sealed class TraceContext
    {
        private HttpContext _context;
        private bool _endDataCollected;
        private EventHandlerList _events = new EventHandlerList();
        private long _firstTime;
        private TraceEnable _isEnabled = (DeploymentSection.RetailInternal ? TraceEnable.Disable : TraceEnable.Default);
        private long _lastTime;
        private static DataSet _masterRequest;
        private DataSet _requestData;
        private System.Web.TraceMode _traceMode = System.Web.TraceMode.Default;
        private ArrayList _traceRecords;
        private int _uniqueIdCounter;
        private static bool _writeToDiagnosticsTrace = false;
        private bool _writing;
        private static readonly object EventTraceFinished = new object();
        private const string NULLIDPREFIX = "__UnassignedID";
        private const string NULLSTRING = "<null>";
        private const string PAGEKEYNAME = "__PAGE";

        public event TraceContextEventHandler TraceFinished
        {
            add
            {
                this._events.AddHandler(EventTraceFinished, value);
            }
            remove
            {
                this._events.RemoveHandler(EventTraceFinished, value);
            }
        }

        public TraceContext(HttpContext context)
        {
            this._context = context;
            this._firstTime = -1L;
            this._lastTime = -1L;
            this._endDataCollected = false;
            this._traceRecords = new ArrayList();
        }

        internal void AddControlSize(string controlId, int renderSize)
        {
            this.VerifyStart();
            DataTable table = this._requestData.Tables["Trace_Control_Tree"];
            if (controlId == null)
            {
                controlId = "__PAGE";
            }
            DataRow row = table.Rows.Find(controlId);
            if (row != null)
            {
                row["Trace_Render_Size"] = renderSize;
            }
        }

        internal void AddControlStateSize(string controlId, int viewstateSize, int controlstateSize)
        {
            this.VerifyStart();
            DataTable table = this._requestData.Tables["Trace_Control_Tree"];
            if (controlId == null)
            {
                controlId = "__PAGE";
            }
            DataRow row = table.Rows.Find(controlId);
            if (row != null)
            {
                row["Trace_Viewstate_Size"] = viewstateSize;
                row["Trace_Controlstate_Size"] = controlstateSize;
            }
        }

        internal void AddNewControl(string id, string parentId, string type, int viewStateSize, int controlStateSize)
        {
            this.VerifyStart();
            DataRow row = this.NewRow(this._requestData, "Trace_Control_Tree");
            if (id == null)
            {
                id = "__UnassignedID" + this._uniqueIdCounter++;
            }
            row["Trace_Control_Id"] = id;
            if (parentId == null)
            {
                parentId = "__PAGE";
            }
            row["Trace_Parent_Id"] = parentId;
            row["Trace_Type"] = type;
            row["Trace_Viewstate_Size"] = viewStateSize;
            row["Trace_Controlstate_Size"] = controlStateSize;
            row["Trace_Render_Size"] = 0;
            try
            {
                this.AddRow(this._requestData, "Trace_Control_Tree", row);
            }
            catch (ConstraintException)
            {
                throw new HttpException(System.Web.SR.GetString("Duplicate_id_used", new object[] { id, "Trace" }));
            }
        }

        private void AddRow(DataSet ds, string table, DataRow row)
        {
            ds.Tables[table].Rows.Add(row);
        }

        private void ApplyTraceMode()
        {
            this.VerifyStart();
            if (this.TraceMode == System.Web.TraceMode.SortByCategory)
            {
                this._requestData.Tables["Trace_Trace_Information"].DefaultView.Sort = "Trace_Category";
            }
            else
            {
                this._requestData.Tables["Trace_Trace_Information"].DefaultView.Sort = "Trace_From_First";
            }
        }

        internal void CopySettingsTo(TraceContext tc)
        {
            tc._traceMode = this._traceMode;
            tc._isEnabled = this._isEnabled;
        }

        internal void EndRequest()
        {
            this.VerifyStart();
            if (!this._endDataCollected)
            {
                IEnumerator enumerator;
                string current;
                object obj2;
                int num;
                DataRow row = this._requestData.Tables["Trace_Request"].Rows[0];
                row["Trace_Status_Code"] = this._context.Response.StatusCode;
                row["Trace_Response_Encoding"] = this._context.Response.ContentEncoding.EncodingName;
                this._context.Application.Lock();
                try
                {
                    enumerator = this._context.Application.GetEnumerator();
                    while (enumerator.MoveNext())
                    {
                        row = this.NewRow(this._requestData, "Trace_Application_State");
                        current = (string) enumerator.Current;
                        row["Trace_Application_Key"] = (current != null) ? current : "<null>";
                        obj2 = this._context.Application[current];
                        if (obj2 != null)
                        {
                            row["Trace_Type"] = obj2.GetType();
                            row["Trace_Value"] = obj2.ToString();
                        }
                        else
                        {
                            row["Trace_Type"] = "<null>";
                            row["Trace_Value"] = "<null>";
                        }
                        this.AddRow(this._requestData, "Trace_Application_State", row);
                    }
                }
                finally
                {
                    this._context.Application.UnLock();
                }
                HttpCookieCollection cookieCollection = new HttpCookieCollection();
                this._context.Request.FillInCookiesCollection(cookieCollection, false);
                HttpCookie[] dest = new HttpCookie[cookieCollection.Count];
                cookieCollection.CopyTo(dest, 0);
                for (num = 0; num < dest.Length; num++)
                {
                    row = this.NewRow(this._requestData, "Trace_Request_Cookies_Collection");
                    row["Trace_Name"] = dest[num].Name;
                    if (dest[num].Values.HasKeys())
                    {
                        NameValueCollection values = dest[num].Values;
                        StringBuilder builder = new StringBuilder();
                        enumerator = values.GetEnumerator();
                        while (enumerator.MoveNext())
                        {
                            current = (string) enumerator.Current;
                            builder.Append("(");
                            builder.Append(current + "=");
                            builder.Append(dest[num][current] + ")  ");
                        }
                        row["Trace_Value"] = builder.ToString();
                    }
                    else
                    {
                        row["Trace_Value"] = dest[num].Value;
                    }
                    int num2 = (dest[num].Name == null) ? 0 : dest[num].Name.Length;
                    num2 += (dest[num].Value == null) ? 0 : dest[num].Value.Length;
                    row["Trace_Size"] = num2 + 1;
                    this.AddRow(this._requestData, "Trace_Request_Cookies_Collection", row);
                }
                dest = new HttpCookie[this._context.Response.Cookies.Count];
                this._context.Response.Cookies.CopyTo(dest, 0);
                for (num = 0; num < dest.Length; num++)
                {
                    row = this.NewRow(this._requestData, "Trace_Response_Cookies_Collection");
                    row["Trace_Name"] = dest[num].Name;
                    if (dest[num].Values.HasKeys())
                    {
                        NameValueCollection values2 = dest[num].Values;
                        StringBuilder builder2 = new StringBuilder();
                        enumerator = values2.GetEnumerator();
                        while (enumerator.MoveNext())
                        {
                            current = (string) enumerator.Current;
                            builder2.Append("(");
                            builder2.Append(current + "=");
                            builder2.Append(dest[num][current] + ")  ");
                        }
                        row["Trace_Value"] = builder2.ToString();
                    }
                    else
                    {
                        row["Trace_Value"] = dest[num].Value;
                    }
                    int num3 = (dest[num].Name == null) ? 0 : dest[num].Name.Length;
                    num3 += (dest[num].Value == null) ? 0 : dest[num].Value.Length;
                    row["Trace_Size"] = num3 + 1;
                    this.AddRow(this._requestData, "Trace_Response_Cookies_Collection", row);
                }
                HttpSessionState session = this._context.Session;
                if (session != null)
                {
                    row = this._requestData.Tables["Trace_Request"].Rows[0];
                    try
                    {
                        row["Trace_Session_Id"] = HttpUtility.UrlEncode(session.SessionID);
                    }
                    catch
                    {
                    }
                    enumerator = session.GetEnumerator();
                    while (enumerator.MoveNext())
                    {
                        row = this.NewRow(this._requestData, "Trace_Session_State");
                        current = (string) enumerator.Current;
                        row["Trace_Session_Key"] = (current != null) ? current : "<null>";
                        obj2 = session[current];
                        if (obj2 != null)
                        {
                            row["Trace_Type"] = obj2.GetType();
                            row["Trace_Value"] = obj2.ToString();
                        }
                        else
                        {
                            row["Trace_Type"] = "<null>";
                            row["Trace_Value"] = "<null>";
                        }
                        this.AddRow(this._requestData, "Trace_Session_State", row);
                    }
                }
                this.ApplyTraceMode();
                this.OnTraceFinished(new TraceContextEventArgs(this._traceRecords));
            }
        }

        internal DataSet GetData()
        {
            return this._requestData;
        }

        private void InitMaster()
        {
            DataSet set = new DataSet {
                Locale = CultureInfo.InvariantCulture
            };
            Type type = typeof(string);
            Type type2 = typeof(int);
            Type type3 = typeof(double);
            DataTable table = set.Tables.Add("Trace_Request");
            table.Columns.Add("Trace_No", type2);
            table.Columns.Add("Trace_Time_of_Request", type);
            table.Columns.Add("Trace_Url", type);
            table.Columns.Add("Trace_Request_Type", type);
            table.Columns.Add("Trace_Status_Code", type2);
            table.Columns.Add("Trace_Session_Id", type);
            table.Columns.Add("Trace_Request_Encoding", type);
            table.Columns.Add("Trace_Response_Encoding", type);
            table = set.Tables.Add("Trace_Control_Tree");
            table.Columns.Add("Trace_Parent_Id", type);
            DataColumn[] columnArray = new DataColumn[] { new DataColumn("Trace_Control_Id", type) };
            table.Columns.Add(columnArray[0]);
            table.PrimaryKey = columnArray;
            table.Columns.Add("Trace_Type", type);
            table.Columns.Add("Trace_Render_Size", type2);
            table.Columns.Add("Trace_Viewstate_Size", type2);
            table.Columns.Add("Trace_Controlstate_Size", type2);
            table = set.Tables.Add("Trace_Session_State");
            table.Columns.Add("Trace_Session_Key", type);
            table.Columns.Add("Trace_Type", type);
            table.Columns.Add("Trace_Value", type);
            table = set.Tables.Add("Trace_Application_State");
            table.Columns.Add("Trace_Application_Key", type);
            table.Columns.Add("Trace_Type", type);
            table.Columns.Add("Trace_Value", type);
            table = set.Tables.Add("Trace_Request_Cookies_Collection");
            table.Columns.Add("Trace_Name", type);
            table.Columns.Add("Trace_Value", type);
            table.Columns.Add("Trace_Size", type2);
            table = set.Tables.Add("Trace_Response_Cookies_Collection");
            table.Columns.Add("Trace_Name", type);
            table.Columns.Add("Trace_Value", type);
            table.Columns.Add("Trace_Size", type2);
            table = set.Tables.Add("Trace_Headers_Collection");
            table.Columns.Add("Trace_Name", type);
            table.Columns.Add("Trace_Value", type);
            table = set.Tables.Add("Trace_Response_Headers_Collection");
            table.Columns.Add("Trace_Name", type);
            table.Columns.Add("Trace_Value", type);
            table = set.Tables.Add("Trace_Form_Collection");
            table.Columns.Add("Trace_Name", type);
            table.Columns.Add("Trace_Value", type);
            table = set.Tables.Add("Trace_Querystring_Collection");
            table.Columns.Add("Trace_Name", type);
            table.Columns.Add("Trace_Value", type);
            table = set.Tables.Add("Trace_Trace_Information");
            table.Columns.Add("Trace_Category", type);
            table.Columns.Add("Trace_Warning", type);
            table.Columns.Add("Trace_Message", type);
            table.Columns.Add("Trace_From_First", type3);
            table.Columns.Add("Trace_From_Last", type);
            table.Columns.Add("ErrorInfoMessage", type);
            table.Columns.Add("ErrorInfoStack", type);
            table = set.Tables.Add("Trace_Server_Variables");
            table.Columns.Add("Trace_Name", type);
            table.Columns.Add("Trace_Value", type);
            _masterRequest = set;
        }

        private void InitRequest()
        {
            int num2;
            DataSet ds = _masterRequest.Clone();
            DataRow row = this.NewRow(ds, "Trace_Request");
            row["Trace_Time_of_Request"] = this._context.Timestamp.ToString("G");
            string rawUrl = this._context.Request.RawUrl;
            int index = rawUrl.IndexOf("?", StringComparison.Ordinal);
            if (index != -1)
            {
                rawUrl = rawUrl.Substring(0, index);
            }
            row["Trace_Url"] = rawUrl;
            row["Trace_Request_Type"] = this._context.Request.HttpMethod;
            try
            {
                row["Trace_Request_Encoding"] = this._context.Request.ContentEncoding.EncodingName;
            }
            catch
            {
            }
            if (this.TraceMode == System.Web.TraceMode.SortByCategory)
            {
                ds.Tables["Trace_Trace_Information"].DefaultView.Sort = "Trace_Category";
            }
            this.AddRow(ds, "Trace_Request", row);
            string[] allKeys = this._context.Request.Headers.AllKeys;
            for (num2 = 0; num2 < allKeys.Length; num2++)
            {
                row = this.NewRow(ds, "Trace_Headers_Collection");
                row["Trace_Name"] = allKeys[num2];
                row["Trace_Value"] = this._context.Request.Headers[allKeys[num2]];
                this.AddRow(ds, "Trace_Headers_Collection", row);
            }
            ArrayList list = this._context.Response.GenerateResponseHeaders(false);
            int num3 = (list != null) ? list.Count : 0;
            for (num2 = 0; num2 < num3; num2++)
            {
                HttpResponseHeader header = (HttpResponseHeader) list[num2];
                row = this.NewRow(ds, "Trace_Response_Headers_Collection");
                row["Trace_Name"] = header.Name;
                row["Trace_Value"] = header.Value;
                this.AddRow(ds, "Trace_Response_Headers_Collection", row);
            }
            allKeys = this._context.Request.Form.AllKeys;
            for (num2 = 0; num2 < allKeys.Length; num2++)
            {
                row = this.NewRow(ds, "Trace_Form_Collection");
                row["Trace_Name"] = allKeys[num2];
                row["Trace_Value"] = this._context.Request.Form[allKeys[num2]];
                this.AddRow(ds, "Trace_Form_Collection", row);
            }
            allKeys = this._context.Request.QueryString.AllKeys;
            for (num2 = 0; num2 < allKeys.Length; num2++)
            {
                row = this.NewRow(ds, "Trace_Querystring_Collection");
                row["Trace_Name"] = allKeys[num2];
                row["Trace_Value"] = this._context.Request.QueryString[allKeys[num2]];
                this.AddRow(ds, "Trace_Querystring_Collection", row);
            }
            if (HttpRuntime.HasAppPathDiscoveryPermission())
            {
                allKeys = this._context.Request.ServerVariables.AllKeys;
                for (num2 = 0; num2 < allKeys.Length; num2++)
                {
                    row = this.NewRow(ds, "Trace_Server_Variables");
                    row["Trace_Name"] = allKeys[num2];
                    row["Trace_Value"] = this._context.Request.ServerVariables.Get(allKeys[num2]);
                    this.AddRow(ds, "Trace_Server_Variables", row);
                }
            }
            this._requestData = ds;
        }

        private DataRow NewRow(DataSet ds, string table)
        {
            return ds.Tables[table].NewRow();
        }

        internal void OnTraceFinished(TraceContextEventArgs e)
        {
            TraceContextEventHandler handler = (TraceContextEventHandler) this._events[EventTraceFinished];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        internal void Render(HtmlTextWriter output)
        {
            if (this.PageOutput && (this._requestData != null))
            {
                TraceEnable enable = this._isEnabled;
                this._isEnabled = TraceEnable.Disable;
                output.Write("<div id=\"__asptrace\">\r\n");
                output.Write(TraceHandler.StyleSheet);
                output.Write("<span class=\"tracecontent\">\r\n");
                Control control = TraceHandler.CreateDetailsTable(this._requestData.Tables["Trace_Request"]);
                if (control != null)
                {
                    control.RenderControl(output);
                }
                control = TraceHandler.CreateTraceTable(this._requestData.Tables["Trace_Trace_Information"]);
                if (control != null)
                {
                    control.RenderControl(output);
                }
                control = TraceHandler.CreateControlTable(this._requestData.Tables["Trace_Control_Tree"]);
                if (control != null)
                {
                    control.RenderControl(output);
                }
                control = TraceHandler.CreateTable(this._requestData.Tables["Trace_Session_State"], true);
                if (control != null)
                {
                    control.RenderControl(output);
                }
                control = TraceHandler.CreateTable(this._requestData.Tables["Trace_Application_State"], true);
                if (control != null)
                {
                    control.RenderControl(output);
                }
                control = TraceHandler.CreateTable(this._requestData.Tables["Trace_Request_Cookies_Collection"], true);
                if (control != null)
                {
                    control.RenderControl(output);
                }
                control = TraceHandler.CreateTable(this._requestData.Tables["Trace_Response_Cookies_Collection"], true);
                if (control != null)
                {
                    control.RenderControl(output);
                }
                control = TraceHandler.CreateTable(this._requestData.Tables["Trace_Headers_Collection"], true);
                if (control != null)
                {
                    control.RenderControl(output);
                }
                control = TraceHandler.CreateTable(this._requestData.Tables["Trace_Response_Headers_Collection"], true);
                if (control != null)
                {
                    control.RenderControl(output);
                }
                control = TraceHandler.CreateTable(this._requestData.Tables["Trace_Form_Collection"]);
                if (control != null)
                {
                    control.RenderControl(output);
                }
                control = TraceHandler.CreateTable(this._requestData.Tables["Trace_Querystring_Collection"]);
                if (control != null)
                {
                    control.RenderControl(output);
                }
                control = TraceHandler.CreateTable(this._requestData.Tables["Trace_Server_Variables"], true);
                if (control != null)
                {
                    control.RenderControl(output);
                }
                output.Write("<hr width=100% size=1 color=silver>\r\n\r\n");
                output.Write(System.Web.SR.GetString("Error_Formatter_CLR_Build") + VersionInfo.ClrVersion + System.Web.SR.GetString("Error_Formatter_ASPNET_Build") + VersionInfo.EngineVersion + "\r\n\r\n");
                output.Write("</font>\r\n\r\n");
                output.Write("</span>\r\n</div>\r\n");
                this._isEnabled = enable;
            }
        }

        internal static void SetWriteToDiagnosticsTrace(bool value)
        {
            _writeToDiagnosticsTrace = value;
        }

        internal void StopTracing()
        {
            this._endDataCollected = true;
        }

        internal void VerifyStart()
        {
            if (_masterRequest == null)
            {
                lock (this)
                {
                    if (_masterRequest == null)
                    {
                        this.InitMaster();
                    }
                }
            }
            if (this._requestData == null)
            {
                this.InitRequest();
            }
        }

        public void Warn(string message)
        {
            this.Write(string.Empty, message, null, true, _writeToDiagnosticsTrace);
        }

        public void Warn(string category, string message)
        {
            this.Write(category, message, null, true, _writeToDiagnosticsTrace);
        }

        public void Warn(string category, string message, Exception errorInfo)
        {
            this.Write(category, message, errorInfo, true, _writeToDiagnosticsTrace);
        }

        internal void WarnInternal(string category, string message, bool writeToDiagnostics)
        {
            this.Write(category, message, null, true, writeToDiagnostics);
        }

        public void Write(string message)
        {
            this.Write(string.Empty, message, null, false, _writeToDiagnosticsTrace);
        }

        public void Write(string category, string message)
        {
            this.Write(category, message, null, false, _writeToDiagnosticsTrace);
        }

        public void Write(string category, string message, Exception errorInfo)
        {
            this.Write(category, message, errorInfo, false, _writeToDiagnosticsTrace);
        }

        private void Write(string category, string message, Exception errorInfo, bool isWarning, bool writeToDiagnostics)
        {
            lock (this)
            {
                if ((!this.IsEnabled || this._writing) || this._endDataCollected)
                {
                    return;
                }
                this.VerifyStart();
                if (category == null)
                {
                    category = string.Empty;
                }
                if (message == null)
                {
                    message = string.Empty;
                }
                long num = Counter.Value;
                DataRow row = this.NewRow(this._requestData, "Trace_Trace_Information");
                row["Trace_Category"] = category;
                row["Trace_Message"] = message;
                row["Trace_Warning"] = isWarning ? "yes" : "no";
                if (errorInfo != null)
                {
                    row["ErrorInfoMessage"] = errorInfo.Message;
                    row["ErrorInfoStack"] = errorInfo.StackTrace;
                }
                if (this._firstTime != -1L)
                {
                    row["Trace_From_First"] = ((double) (num - this._firstTime)) / ((double) Counter.Frequency);
                }
                else
                {
                    this._firstTime = num;
                }
                if (this._lastTime != -1L)
                {
                    row["Trace_From_Last"] = (((double) (num - this._lastTime)) / ((double) Counter.Frequency)).ToString("0.000000", CultureInfo.CurrentCulture);
                }
                this._lastTime = num;
                this.AddRow(this._requestData, "Trace_Trace_Information", row);
                string str = message;
                if (errorInfo != null)
                {
                    string str2 = errorInfo.Message;
                    if (str2 == null)
                    {
                        str2 = string.Empty;
                    }
                    string stackTrace = errorInfo.StackTrace;
                    if (stackTrace == null)
                    {
                        stackTrace = string.Empty;
                    }
                    StringBuilder builder = new StringBuilder((message.Length + str2.Length) + stackTrace.Length);
                    builder.Append(message);
                    builder.Append(" -- ");
                    builder.Append(str2);
                    builder.Append(": ");
                    builder.Append(stackTrace);
                    str = builder.ToString();
                }
                if (writeToDiagnostics)
                {
                    this._writing = true;
                    Trace.WriteLine(str, category);
                    this._writing = false;
                }
                if ((this._context != null) && (this._context.WorkerRequest != null))
                {
                    this._context.WorkerRequest.RaiseTraceEvent(isWarning ? IntegratedTraceType.TraceWarn : IntegratedTraceType.TraceWrite, str);
                }
            }
            this._traceRecords.Add(new TraceContextRecord(category, message, isWarning, errorInfo));
        }

        internal void WriteInternal(string message, bool writeToDiagnostics)
        {
            this.Write(string.Empty, message, null, false, writeToDiagnostics);
        }

        internal void WriteInternal(string category, string message, bool writeToDiagnostics)
        {
            this.Write(category, message, null, false, writeToDiagnostics);
        }

        public bool IsEnabled
        {
            get
            {
                if (this._isEnabled == TraceEnable.Default)
                {
                    return HttpRuntime.Profile.IsEnabled;
                }
                if (this._isEnabled != TraceEnable.Enable)
                {
                    return false;
                }
                return true;
            }
            set
            {
                if (!DeploymentSection.RetailInternal)
                {
                    if (value)
                    {
                        this._isEnabled = TraceEnable.Enable;
                    }
                    else
                    {
                        this._isEnabled = TraceEnable.Disable;
                    }
                }
            }
        }

        internal bool PageOutput
        {
            get
            {
                if (this._isEnabled == TraceEnable.Default)
                {
                    return HttpRuntime.Profile.PageOutput;
                }
                if (this._isEnabled != TraceEnable.Enable)
                {
                    return false;
                }
                return true;
            }
        }

        internal int StatusCode
        {
            set
            {
                this.VerifyStart();
                DataRow row = this._requestData.Tables["Trace_Request"].Rows[0];
                row["Trace_Status_Code"] = value;
            }
        }

        public System.Web.TraceMode TraceMode
        {
            get
            {
                if (this._traceMode == System.Web.TraceMode.Default)
                {
                    return HttpRuntime.Profile.OutputMode;
                }
                return this._traceMode;
            }
            set
            {
                if ((value < System.Web.TraceMode.SortByTime) || (value > System.Web.TraceMode.Default))
                {
                    throw new ArgumentOutOfRangeException("value");
                }
                this._traceMode = value;
                if (this.IsEnabled)
                {
                    this.ApplyTraceMode();
                }
            }
        }
    }
}

