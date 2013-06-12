namespace System.Web.Handlers
{
    using System;
    using System.Collections;
    using System.Data;
    using System.Globalization;
    using System.Text;
    using System.Web;
    using System.Web.Configuration;
    using System.Web.Hosting;
    using System.Web.UI;
    using System.Web.UI.HtmlControls;
    using System.Web.UI.WebControls;
    using System.Web.Util;

    public class TraceHandler : IHttpHandler
    {
        private HttpContext _context;
        private HttpRequest _request;
        private HttpResponse _response;
        private const string _style = "<style type=\"text/css\">\r\nspan.tracecontent b { color:white }\r\nspan.tracecontent { background-color:white; color:black;font: 10pt verdana, arial; }\r\nspan.tracecontent table { clear:left; font: 10pt verdana, arial; cellspacing:0; cellpadding:0; margin-bottom:25}\r\nspan.tracecontent tr.subhead { background-color:#cccccc;}\r\nspan.tracecontent th { padding:0,3,0,3 }\r\nspan.tracecontent th.alt { background-color:black; color:white; padding:3,3,2,3; }\r\nspan.tracecontent td { color: black; padding:0,3,0,3; text-align: left }\r\nspan.tracecontent td.err { color: red; }\r\nspan.tracecontent tr.alt { background-color:#eeeeee }\r\nspan.tracecontent h1 { font: 24pt verdana, arial; margin:0,0,0,0}\r\nspan.tracecontent h2 { font: 18pt verdana, arial; margin:0,0,0,0}\r\nspan.tracecontent h3 { font: 12pt verdana, arial; margin:0,0,0,0}\r\nspan.tracecontent th a { color:darkblue; font: 8pt verdana, arial; }\r\nspan.tracecontent a { color:darkblue;text-decoration:none }\r\nspan.tracecontent a:hover { color:darkblue;text-decoration:underline; }\r\nspan.tracecontent div.outer { width:90%; margin:15,15,15,15}\r\nspan.tracecontent table.viewmenu td { background-color:#006699; color:white; padding:0,5,0,5; }\r\nspan.tracecontent table.viewmenu td.end { padding:0,0,0,0; }\r\nspan.tracecontent table.viewmenu a {color:white; font: 8pt verdana, arial; }\r\nspan.tracecontent table.viewmenu a:hover {color:white; font: 8pt verdana, arial; }\r\nspan.tracecontent a.tinylink {color:darkblue; background-color:black; font: 8pt verdana, arial;text-decoration:underline;}\r\nspan.tracecontent a.link {color:darkblue; text-decoration:underline;}\r\nspan.tracecontent div.buffer {padding-top:7; padding-bottom:17;}\r\nspan.tracecontent .small { font: 8pt verdana, arial }\r\nspan.tracecontent table td { padding-right:20 }\r\nspan.tracecontent table td.nopad { padding-right:5 }\r\n</style>\r\n";
        private HtmlTextWriter _writer;

        private static TableCell AddCell(TableRow trow, string text)
        {
            TableCell cell = new TableCell {
                Text = text
            };
            trow.Cells.Add(cell);
            return cell;
        }

        private static TableCell AddHeaderCell(TableRow trow, string text)
        {
            TableHeaderCell cell = new TableHeaderCell {
                Text = text
            };
            trow.Cells.Add(cell);
            return cell;
        }

        private static TableRow AddRow(Table t)
        {
            TableRow row = new TableRow();
            t.Rows.Add(row);
            return row;
        }

        internal static Table CreateControlTable(DataTable datatable)
        {
            Table t = new Table();
            if (datatable != null)
            {
                Hashtable hashtable = new Hashtable();
                bool flag = false;
                t.Width = Unit.Percentage(100.0);
                t.CellPadding = 0;
                t.CellSpacing = 0;
                TableCell cell = AddHeaderCell(AddRow(t), "<h3><b>" + System.Web.SR.GetString(datatable.TableName) + "</b></h3>");
                cell.CssClass = "alt";
                cell.ColumnSpan = 5;
                cell.HorizontalAlign = HorizontalAlign.Left;
                TableRow trow = AddRow(t);
                trow.CssClass = "subhead";
                trow.HorizontalAlign = HorizontalAlign.Left;
                AddHeaderCell(trow, System.Web.SR.GetString("Trace_Control_Id"));
                AddHeaderCell(trow, System.Web.SR.GetString("Trace_Type"));
                AddHeaderCell(trow, System.Web.SR.GetString("Trace_Render_Size_children"));
                AddHeaderCell(trow, System.Web.SR.GetString("Trace_Viewstate_Size_Nochildren"));
                AddHeaderCell(trow, System.Web.SR.GetString("Trace_Controlstate_Size_Nochildren"));
                hashtable["ROOT"] = 0;
                IEnumerator enumerator = datatable.Rows.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    string str = HttpUtility.HtmlEncode((string) ((DataRow) enumerator.Current)["Trace_Parent_Id"]);
                    string str2 = HttpUtility.HtmlEncode((string) ((DataRow) enumerator.Current)["Trace_Control_Id"]);
                    int num = (int) hashtable[str];
                    hashtable[str2] = num + 1;
                    StringBuilder builder = new StringBuilder();
                    builder.Append("<nobr>");
                    for (int i = 0; i < num; i++)
                    {
                        builder.Append("&nbsp;&nbsp;&nbsp;&nbsp;");
                    }
                    if (str2.Length == 0)
                    {
                        builder.Append(System.Web.SR.GetString("Trace_Page"));
                    }
                    else
                    {
                        builder.Append(str2);
                    }
                    trow = AddRow(t);
                    AddCell(trow, builder.ToString());
                    AddCell(trow, (string) ((DataRow) enumerator.Current)["Trace_Type"]);
                    object obj2 = ((DataRow) enumerator.Current)["Trace_Render_Size"];
                    if (obj2 != null)
                    {
                        AddCell(trow, ((int) obj2).ToString(NumberFormatInfo.InvariantInfo));
                    }
                    else
                    {
                        AddCell(trow, "---");
                    }
                    obj2 = ((DataRow) enumerator.Current)["Trace_Viewstate_Size"];
                    if (obj2 != null)
                    {
                        AddCell(trow, ((int) obj2).ToString(NumberFormatInfo.InvariantInfo));
                    }
                    else
                    {
                        AddCell(trow, "---");
                    }
                    obj2 = ((DataRow) enumerator.Current)["Trace_Controlstate_Size"];
                    if (obj2 != null)
                    {
                        AddCell(trow, ((int) obj2).ToString(NumberFormatInfo.InvariantInfo));
                    }
                    else
                    {
                        AddCell(trow, "---");
                    }
                    if (flag)
                    {
                        trow.CssClass = "alt";
                    }
                    flag = !flag;
                }
            }
            return t;
        }

        internal static Table CreateDetailsTable(DataTable datatable)
        {
            Table t = new Table {
                Width = Unit.Percentage(100.0),
                CellPadding = 0,
                CellSpacing = 0
            };
            if (datatable != null)
            {
                TableCell cell = AddHeaderCell(AddRow(t), "<h3><b>" + System.Web.SR.GetString("Trace_Request_Details") + "</b></h3>");
                cell.ColumnSpan = 10;
                cell.CssClass = "alt";
                cell.HorizontalAlign = HorizontalAlign.Left;
                TableRow trow = AddRow(t);
                trow.HorizontalAlign = HorizontalAlign.Left;
                AddHeaderCell(trow, System.Web.SR.GetString("Trace_Session_Id") + ":");
                AddCell(trow, HttpUtility.HtmlEncode(datatable.Rows[0]["Trace_Session_Id"].ToString()));
                AddHeaderCell(trow, System.Web.SR.GetString("Trace_Request_Type") + ":");
                AddCell(trow, datatable.Rows[0]["Trace_Request_Type"].ToString());
                trow = AddRow(t);
                trow.HorizontalAlign = HorizontalAlign.Left;
                AddHeaderCell(trow, System.Web.SR.GetString("Trace_Time_of_Request") + ":");
                AddCell(trow, datatable.Rows[0]["Trace_Time_of_Request"].ToString());
                AddHeaderCell(trow, System.Web.SR.GetString("Trace_Status_Code") + ":");
                AddCell(trow, datatable.Rows[0]["Trace_Status_Code"].ToString());
                trow = AddRow(t);
                trow.HorizontalAlign = HorizontalAlign.Left;
                AddHeaderCell(trow, System.Web.SR.GetString("Trace_Request_Encoding") + ":");
                AddCell(trow, datatable.Rows[0]["Trace_Request_Encoding"].ToString());
                AddHeaderCell(trow, System.Web.SR.GetString("Trace_Response_Encoding") + ":");
                AddCell(trow, datatable.Rows[0]["Trace_Response_Encoding"].ToString());
            }
            return t;
        }

        internal static Table CreateTable(DataTable datatable)
        {
            return CreateTable(datatable, false);
        }

        internal static Table CreateTable(DataTable datatable, bool encodeSpaces)
        {
            Table t = new Table {
                Width = Unit.Percentage(100.0),
                CellPadding = 0,
                CellSpacing = 0
            };
            if (datatable != null)
            {
                bool flag = false;
                TableCell cell = AddHeaderCell(AddRow(t), "<h3><b>" + System.Web.SR.GetString(datatable.TableName) + "</b></h3>");
                cell.CssClass = "alt";
                cell.ColumnSpan = 10;
                cell.HorizontalAlign = HorizontalAlign.Left;
                TableRow trow = AddRow(t);
                trow.CssClass = "subhead";
                trow.HorizontalAlign = HorizontalAlign.Left;
                IEnumerator enumerator = datatable.Columns.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    AddHeaderCell(trow, System.Web.SR.GetString(((DataColumn) enumerator.Current).ColumnName));
                }
                enumerator = datatable.Rows.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    object[] itemArray = ((DataRow) enumerator.Current).ItemArray;
                    trow = AddRow(t);
                    for (int i = 0; i < itemArray.Length; i++)
                    {
                        string str;
                        if (encodeSpaces)
                        {
                            str = HttpUtility.FormatPlainTextSpacesAsHtml(HttpUtility.HtmlEncode(itemArray[i].ToString()));
                        }
                        else
                        {
                            str = HttpUtility.HtmlEncode(itemArray[i].ToString());
                        }
                        AddCell(trow, (str.Length != 0) ? str : "&nbsp;");
                    }
                    if (flag)
                    {
                        trow.CssClass = "alt";
                    }
                    flag = !flag;
                }
            }
            return t;
        }

        internal static Table CreateTraceTable(DataTable datatable)
        {
            Table t = new Table {
                Width = Unit.Percentage(100.0),
                CellPadding = 0,
                CellSpacing = 0
            };
            if (datatable != null)
            {
                bool flag = false;
                TableCell cell = AddHeaderCell(AddRow(t), "<h3><b>" + System.Web.SR.GetString(datatable.TableName) + "</b></h3>");
                cell.CssClass = "alt";
                cell.ColumnSpan = 10;
                cell.HorizontalAlign = HorizontalAlign.Left;
                TableRow trow = AddRow(t);
                trow.CssClass = "subhead";
                trow.HorizontalAlign = HorizontalAlign.Left;
                AddHeaderCell(trow, System.Web.SR.GetString("Trace_Category"));
                AddHeaderCell(trow, System.Web.SR.GetString("Trace_Message"));
                AddHeaderCell(trow, System.Web.SR.GetString("Trace_From_First"));
                AddHeaderCell(trow, System.Web.SR.GetString("Trace_From_Last"));
                IEnumerator enumerator = datatable.DefaultView.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    trow = AddRow(t);
                    DataRow row = ((DataRowView) enumerator.Current).Row;
                    bool flag2 = row["Trace_Warning"].Equals("yes");
                    cell = AddCell(trow, HttpUtility.FormatPlainTextAsHtml((string) row["Trace_Category"]));
                    if (flag2)
                    {
                        cell.CssClass = "err";
                    }
                    StringBuilder builder = new StringBuilder(HttpUtility.FormatPlainTextAsHtml((string) row["Trace_Message"]));
                    object obj2 = row["ErrorInfoMessage"];
                    object obj3 = row["ErrorInfoStack"];
                    if (!(obj2 is DBNull))
                    {
                        builder.Append("<br>" + HttpUtility.FormatPlainTextAsHtml((string) obj2));
                    }
                    if (!(obj3 is DBNull))
                    {
                        builder.Append("<br>" + HttpUtility.FormatPlainTextAsHtml((string) obj3));
                    }
                    cell = AddCell(trow, builder.ToString());
                    if (flag2)
                    {
                        cell.CssClass = "err";
                    }
                    cell = AddCell(trow, row["Trace_From_First"].ToString());
                    if (flag2)
                    {
                        cell.CssClass = "err";
                    }
                    cell = AddCell(trow, row["Trace_From_Last"].ToString());
                    if (flag2)
                    {
                        cell.CssClass = "err";
                    }
                    if (flag)
                    {
                        trow.CssClass = "alt";
                    }
                    flag = !flag;
                }
            }
            return t;
        }

        protected void ProcessRequest(HttpContext context)
        {
            ((IHttpHandler) this).ProcessRequest(context);
        }

        protected void ShowDetails(DataSet data)
        {
            if (data != null)
            {
                this._writer.Write("<h1>" + System.Web.SR.GetString("Trace_Request_Details") + "</h1><br>");
                Table table = CreateDetailsTable(data.Tables["Trace_Request"]);
                if (table != null)
                {
                    table.RenderControl(this._writer);
                }
                table = CreateTraceTable(data.Tables["Trace_Trace_Information"]);
                if (table != null)
                {
                    table.RenderControl(this._writer);
                }
                table = CreateControlTable(data.Tables["Trace_Control_Tree"]);
                if (table != null)
                {
                    table.RenderControl(this._writer);
                }
                table = CreateTable(data.Tables["Trace_Session_State"], true);
                if (table != null)
                {
                    table.RenderControl(this._writer);
                }
                table = CreateTable(data.Tables["Trace_Application_State"], true);
                if (table != null)
                {
                    table.RenderControl(this._writer);
                }
                table = CreateTable(data.Tables["Trace_Request_Cookies_Collection"], true);
                if (table != null)
                {
                    table.RenderControl(this._writer);
                }
                table = CreateTable(data.Tables["Trace_Response_Cookies_Collection"], true);
                if (table != null)
                {
                    table.RenderControl(this._writer);
                }
                table = CreateTable(data.Tables["Trace_Headers_Collection"], true);
                if (table != null)
                {
                    table.RenderControl(this._writer);
                }
                table = CreateTable(data.Tables["Trace_Form_Collection"]);
                if (table != null)
                {
                    table.RenderControl(this._writer);
                }
                table = CreateTable(data.Tables["Trace_Querystring_Collection"]);
                if (table != null)
                {
                    table.RenderControl(this._writer);
                }
                table = CreateTable(data.Tables["Trace_Server_Variables"], true);
                if (table != null)
                {
                    table.RenderControl(this._writer);
                }
            }
        }

        protected void ShowRequests(IList data)
        {
            Table t = new Table {
                CellPadding = 0,
                CellSpacing = 0,
                Width = Unit.Percentage(100.0)
            };
            AddCell(AddRow(t), System.Web.SR.GetString("Trace_Application_Trace"));
            string applicationPath = this._request.ApplicationPath;
            int length = applicationPath.Length;
            AddCell(AddRow(t), "<h2>" + HttpUtility.HtmlEncode(applicationPath.Substring(1)) + "<h2><p>");
            AddCell(AddRow(t), "[ <a href=\"Trace.axd?clear=1\" class=\"link\">" + System.Web.SR.GetString("Trace_Clear_Current") + "</a> ]");
            string text = "&nbsp";
            if (HttpRuntime.HasAppPathDiscoveryPermission())
            {
                text = System.Web.SR.GetString("Trace_Physical_Directory") + this._request.PhysicalApplicationPath;
            }
            TableCell cell = AddCell(AddRow(t), text);
            t.RenderControl(this._writer);
            t = new Table {
                CellPadding = 0,
                CellSpacing = 0,
                Width = Unit.Percentage(100.0)
            };
            TableRow trow = AddRow(t);
            cell = AddHeaderCell(trow, "<h3><b>" + System.Web.SR.GetString("Trace_Requests_This") + "</b></h3>");
            cell.ColumnSpan = 5;
            cell.CssClass = "alt";
            cell.HorizontalAlign = HorizontalAlign.Left;
            cell = AddHeaderCell(trow, System.Web.SR.GetString("Trace_Remaining") + " " + HttpRuntime.Profile.RequestsRemaining.ToString(NumberFormatInfo.InvariantInfo));
            cell.CssClass = "alt";
            cell.HorizontalAlign = HorizontalAlign.Right;
            trow = AddRow(t);
            trow.HorizontalAlign = HorizontalAlign.Left;
            trow.CssClass = "subhead";
            AddHeaderCell(trow, System.Web.SR.GetString("Trace_No"));
            AddHeaderCell(trow, System.Web.SR.GetString("Trace_Time_of_Request"));
            AddHeaderCell(trow, System.Web.SR.GetString("Trace_File"));
            AddHeaderCell(trow, System.Web.SR.GetString("Trace_Status_Code"));
            AddHeaderCell(trow, System.Web.SR.GetString("Trace_Verb"));
            AddHeaderCell(trow, "&nbsp");
            bool flag = true;
            for (int i = 0; i < data.Count; i++)
            {
                DataSet set = (DataSet) data[i];
                trow = AddRow(t);
                if (flag)
                {
                    trow.CssClass = "alt";
                }
                AddCell(trow, (i + 1).ToString(NumberFormatInfo.InvariantInfo));
                AddCell(trow, (string) set.Tables["Trace_Request"].Rows[0]["Trace_Time_of_Request"]);
                AddCell(trow, ((string) set.Tables["Trace_Request"].Rows[0]["Trace_Url"]).Substring(length));
                AddCell(trow, set.Tables["Trace_Request"].Rows[0]["Trace_Status_Code"].ToString());
                AddCell(trow, (string) set.Tables["Trace_Request"].Rows[0]["Trace_Request_Type"]);
                TableCell cell2 = AddCell(trow, string.Empty);
                HtmlAnchor child = new HtmlAnchor {
                    HRef = "Trace.axd?id=" + i,
                    InnerHtml = "<nobr>" + System.Web.SR.GetString("Trace_View_Details")
                };
                child.Attributes["class"] = "link";
                cell2.Controls.Add(child);
                flag = !flag;
            }
            t.RenderControl(this._writer);
        }

        protected void ShowVersionDetails()
        {
            this._writer.Write("<hr width=100% size=1 color=silver>\r\n\r\n");
            this._writer.Write(System.Web.SR.GetString("Error_Formatter_CLR_Build") + VersionInfo.ClrVersion + System.Web.SR.GetString("Error_Formatter_ASPNET_Build") + VersionInfo.EngineVersion + "\r\n\r\n");
            this._writer.Write("</font>\r\n\r\n");
        }

        void IHttpHandler.ProcessRequest(HttpContext context)
        {
            if (DeploymentSection.RetailInternal || (!context.Request.IsLocal && HttpRuntime.Profile.LocalOnly))
            {
                HttpException exception = new HttpException(0x193, null);
                exception.SetFormatter(new TraceHandlerErrorFormatter(!DeploymentSection.RetailInternal));
                throw exception;
            }
            this._context = context;
            this._response = this._context.Response;
            this._request = this._context.Request;
            this._writer = Page.CreateHtmlTextWriterInternal(this._response.Output, this._request);
            if (context.WorkerRequest is IIS7WorkerRequest)
            {
                this._response.ContentType = this._request.Browser.PreferredRenderingMime;
            }
            if (this._writer != null)
            {
                this._context.Trace.IsEnabled = false;
                this._request.ValidateInput();
                this._writer.Write("<html>\r\n");
                this._writer.Write("<head>\r\n");
                this._writer.Write(StyleSheet);
                this._writer.Write("</head>\r\n");
                this._writer.Write("<body>\r\n");
                this._writer.Write("<span class=\"tracecontent\">\r\n");
                if (!HttpRuntime.Profile.IsConfigEnabled)
                {
                    HttpException exception2 = new HttpException();
                    exception2.SetFormatter(new TraceHandlerErrorFormatter(false));
                    throw exception2;
                }
                IList data = HttpRuntime.Profile.GetData();
                if (this._request.QueryString["clear"] != null)
                {
                    HttpRuntime.Profile.Reset();
                    string rawUrl = this._request.RawUrl;
                    this._response.Redirect(rawUrl.Substring(0, rawUrl.IndexOf("?", StringComparison.Ordinal)));
                }
                string s = this._request.QueryString["id"];
                if (s != null)
                {
                    int num = int.Parse(s, CultureInfo.InvariantCulture);
                    if ((num >= 0) && (num < data.Count))
                    {
                        this.ShowDetails((DataSet) data[num]);
                        this.ShowVersionDetails();
                        this._writer.Write("</span>\r\n</body>\r\n</html>\r\n");
                        return;
                    }
                }
                this.ShowRequests(data);
                this.ShowVersionDetails();
                this._writer.Write("</span>\r\n</body>\r\n</html>\r\n");
            }
        }

        protected bool IsReusable
        {
            get
            {
                return ((IHttpHandler) this).IsReusable;
            }
        }

        internal static string StyleSheet
        {
            get
            {
                return "<style type=\"text/css\">\r\nspan.tracecontent b { color:white }\r\nspan.tracecontent { background-color:white; color:black;font: 10pt verdana, arial; }\r\nspan.tracecontent table { clear:left; font: 10pt verdana, arial; cellspacing:0; cellpadding:0; margin-bottom:25}\r\nspan.tracecontent tr.subhead { background-color:#cccccc;}\r\nspan.tracecontent th { padding:0,3,0,3 }\r\nspan.tracecontent th.alt { background-color:black; color:white; padding:3,3,2,3; }\r\nspan.tracecontent td { color: black; padding:0,3,0,3; text-align: left }\r\nspan.tracecontent td.err { color: red; }\r\nspan.tracecontent tr.alt { background-color:#eeeeee }\r\nspan.tracecontent h1 { font: 24pt verdana, arial; margin:0,0,0,0}\r\nspan.tracecontent h2 { font: 18pt verdana, arial; margin:0,0,0,0}\r\nspan.tracecontent h3 { font: 12pt verdana, arial; margin:0,0,0,0}\r\nspan.tracecontent th a { color:darkblue; font: 8pt verdana, arial; }\r\nspan.tracecontent a { color:darkblue;text-decoration:none }\r\nspan.tracecontent a:hover { color:darkblue;text-decoration:underline; }\r\nspan.tracecontent div.outer { width:90%; margin:15,15,15,15}\r\nspan.tracecontent table.viewmenu td { background-color:#006699; color:white; padding:0,5,0,5; }\r\nspan.tracecontent table.viewmenu td.end { padding:0,0,0,0; }\r\nspan.tracecontent table.viewmenu a {color:white; font: 8pt verdana, arial; }\r\nspan.tracecontent table.viewmenu a:hover {color:white; font: 8pt verdana, arial; }\r\nspan.tracecontent a.tinylink {color:darkblue; background-color:black; font: 8pt verdana, arial;text-decoration:underline;}\r\nspan.tracecontent a.link {color:darkblue; text-decoration:underline;}\r\nspan.tracecontent div.buffer {padding-top:7; padding-bottom:17;}\r\nspan.tracecontent .small { font: 8pt verdana, arial }\r\nspan.tracecontent table td { padding-right:20 }\r\nspan.tracecontent table td.nopad { padding-right:5 }\r\n</style>\r\n";
            }
        }

        bool IHttpHandler.IsReusable
        {
            get
            {
                return false;
            }
        }
    }
}

