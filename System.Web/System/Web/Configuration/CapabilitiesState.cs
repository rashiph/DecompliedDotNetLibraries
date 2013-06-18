namespace System.Web.Configuration
{
    using System;
    using System.Collections;
    using System.Security.Permissions;
    using System.Text.RegularExpressions;
    using System.Web;

    internal class CapabilitiesState
    {
        internal bool _evaluateOnlyUserAgent;
        internal bool _exit;
        internal ArrayList _matchlist;
        internal ArrayList _regexlist;
        internal HttpRequest _request;
        internal IDictionary _values;

        internal CapabilitiesState(HttpRequest request, IDictionary values)
        {
            this._request = request;
            this._values = values;
            this._matchlist = new ArrayList();
            this._regexlist = new ArrayList();
        }

        internal virtual void AddMatch(DelayedRegex regex, Match match)
        {
            this._regexlist.Add(regex);
            this._matchlist.Add(match);
        }

        internal virtual void ClearMatch()
        {
            if (this._matchlist == null)
            {
                this._regexlist = new ArrayList();
                this._matchlist = new ArrayList();
            }
            else
            {
                this._regexlist.Clear();
                this._matchlist.Clear();
            }
        }

        internal virtual void PopMatch()
        {
            this._regexlist.RemoveAt(this._regexlist.Count - 1);
            this._matchlist.RemoveAt(this._matchlist.Count - 1);
        }

        internal virtual string ResolveReference(string refname)
        {
            if (this._matchlist != null)
            {
                int count = this._matchlist.Count;
                while (count > 0)
                {
                    count--;
                    int num2 = ((DelayedRegex) this._regexlist[count]).GroupNumberFromName(refname);
                    if (num2 >= 0)
                    {
                        Group group = ((Match) this._matchlist[count]).Groups[num2];
                        if (group.Success)
                        {
                            return group.ToString();
                        }
                    }
                }
            }
            return string.Empty;
        }

        internal virtual string ResolveServerVariable(string varname)
        {
            if ((varname.Length == 0) || (varname == "HTTP_USER_AGENT"))
            {
                return HttpCapabilitiesDefaultProvider.GetUserAgent(this._request);
            }
            if (this.EvaluateOnlyUserAgent)
            {
                return string.Empty;
            }
            return this.ResolveServerVariableWithAssert(varname);
        }

        [AspNetHostingPermission(SecurityAction.Assert, Level=AspNetHostingPermissionLevel.Low)]
        private string ResolveServerVariableWithAssert(string varname)
        {
            string str = this._request.ServerVariables[varname];
            if (str == null)
            {
                return string.Empty;
            }
            return str;
        }

        internal virtual string ResolveVariable(string varname)
        {
            string str = (string) this._values[varname];
            if (str == null)
            {
                return string.Empty;
            }
            return str;
        }

        internal virtual void SetVariable(string varname, string value)
        {
            this._values[varname] = value;
        }

        internal bool EvaluateOnlyUserAgent
        {
            get
            {
                return this._evaluateOnlyUserAgent;
            }
            set
            {
                this._evaluateOnlyUserAgent = value;
            }
        }

        internal virtual bool Exit
        {
            get
            {
                return this._exit;
            }
            set
            {
                this._exit = value;
            }
        }
    }
}

