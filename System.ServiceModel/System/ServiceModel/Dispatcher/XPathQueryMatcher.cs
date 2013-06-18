namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.Xml;
    using System.Xml.XPath;

    internal class XPathQueryMatcher : QueryMatcher
    {
        private XPathFilterFlags flags = XPathFilterFlags.None;
        private bool match;
        private static PushBooleanOpcode matchAlwaysFilter;
        private static OpcodeBlock rootFilter;

        static XPathQueryMatcher()
        {
            ValueDataType type;
            matchAlwaysFilter = new PushBooleanOpcode(true);
            rootFilter = QueryMatcher.CompileForInternalEngine("/", null, QueryCompilerFlags.None, out type);
            rootFilter.Append(new MatchResultOpcode());
        }

        internal XPathQueryMatcher(bool match)
        {
            this.match = match;
        }

        internal void Compile(string expression, XmlNamespaceManager namespaces)
        {
            if (base.query == null)
            {
                try
                {
                    this.CompileForInternal(expression, namespaces);
                }
                catch (QueryCompileException)
                {
                }
                if (base.query == null)
                {
                    this.CompileForExternal(expression, namespaces);
                }
            }
        }

        internal void CompileForExternal(string xpath, XmlNamespaceManager names)
        {
            Opcode first = QueryMatcher.CompileForExternalEngine(xpath, names, null, this.match).First;
            base.query = first;
            this.flags |= XPathFilterFlags.IsFxFilter;
        }

        internal void CompileForInternal(string xpath, XmlNamespaceManager names)
        {
            base.query = null;
            xpath = xpath.Trim();
            if (xpath.Length == 0)
            {
                base.query = matchAlwaysFilter;
                this.flags |= XPathFilterFlags.AlwaysMatch;
            }
            else if ((1 == xpath.Length) && ('/' == xpath[0]))
            {
                base.query = rootFilter.First;
                this.flags |= XPathFilterFlags.AlwaysMatch;
            }
            else
            {
                ValueDataType type;
                OpcodeBlock block = QueryMatcher.CompileForInternalEngine(xpath, names, QueryCompilerFlags.None, out type);
                if (this.match)
                {
                    block.Append(new MatchResultOpcode());
                }
                else
                {
                    block.Append(new QueryResultOpcode());
                }
                base.query = block.First;
            }
            this.flags &= ~XPathFilterFlags.IsFxFilter;
        }

        internal FilterResult Match(MessageBuffer messageBuffer)
        {
            FilterResult result;
            Message message = messageBuffer.CreateMessage();
            try
            {
                result = this.Match(message, true);
            }
            finally
            {
                message.Close();
            }
            return result;
        }

        internal FilterResult Match(SeekableXPathNavigator navigator)
        {
            if (this.IsAlwaysMatch)
            {
                return new FilterResult(true);
            }
            if (this.IsFxFilter)
            {
                return new FilterResult(this.MatchFx(navigator));
            }
            return base.Match(navigator, null);
        }

        internal FilterResult Match(XPathNavigator navigator)
        {
            if (this.IsAlwaysMatch)
            {
                return new FilterResult(true);
            }
            if (this.IsFxFilter)
            {
                return new FilterResult(this.MatchFx(navigator));
            }
            return base.Match(navigator, null);
        }

        internal FilterResult Match(Message message, bool matchBody)
        {
            if (this.IsAlwaysMatch)
            {
                return new FilterResult(true);
            }
            return base.Match(message, matchBody, null);
        }

        internal bool MatchFx(XPathNavigator navigator)
        {
            bool flag;
            INodeCounter counter = navigator as INodeCounter;
            if (counter == null)
            {
                navigator = new SafeSeekableNavigator(new GenericSeekableNavigator(navigator), base.NodeQuota);
            }
            else
            {
                counter.CounterMarker = base.NodeQuota;
                counter.MaxCounter = base.NodeQuota;
            }
            try
            {
                flag = ((MatchSingleFxEngineResultOpcode) base.query).Match(navigator);
            }
            catch (XPathNavigatorException exception)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(exception.Process(base.query));
            }
            catch (NavigatorInvalidBodyAccessException exception2)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(exception2.Process(base.query));
            }
            return flag;
        }

        internal bool IsAlwaysMatch
        {
            get
            {
                return (XPathFilterFlags.None != (this.flags & XPathFilterFlags.AlwaysMatch));
            }
        }

        internal bool IsFxFilter
        {
            get
            {
                return (XPathFilterFlags.None != (this.flags & XPathFilterFlags.IsFxFilter));
            }
        }
    }
}

