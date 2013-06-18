namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.Runtime;
    using System.ServiceModel;
    using System.Xml.XPath;

    public sealed class XPathResult : IDisposable
    {
        private bool boolResult;
        private SafeNodeSequenceIterator internalIterator;
        private XPathNodeIterator nodeSetResult;
        private double numberResult;
        private XPathResultType resultType;
        private string stringResult;

        private XPathResult()
        {
        }

        internal XPathResult(bool boolResult) : this()
        {
            this.boolResult = boolResult;
            this.resultType = XPathResultType.Boolean;
        }

        internal XPathResult(double numberResult) : this()
        {
            this.numberResult = numberResult;
            this.resultType = XPathResultType.Number;
        }

        internal XPathResult(string stringResult) : this()
        {
            this.stringResult = stringResult;
            this.resultType = XPathResultType.String;
        }

        internal XPathResult(XPathNodeIterator nodeSetResult) : this()
        {
            this.nodeSetResult = nodeSetResult;
            this.internalIterator = nodeSetResult as SafeNodeSequenceIterator;
            this.resultType = XPathResultType.NodeSet;
        }

        internal XPathResult Copy()
        {
            XPathResult result = new XPathResult {
                resultType = this.resultType
            };
            switch (this.resultType)
            {
                case XPathResultType.Number:
                    result.numberResult = this.numberResult;
                    return result;

                case XPathResultType.String:
                    result.stringResult = this.stringResult;
                    return result;

                case XPathResultType.Boolean:
                    result.boolResult = this.boolResult;
                    return result;

                case XPathResultType.NodeSet:
                    result.nodeSetResult = this.nodeSetResult.Clone();
                    return result;
            }
            throw Fx.AssertAndThrow("Unexpected result type.");
        }

        public void Dispose()
        {
            if (this.internalIterator != null)
            {
                this.internalIterator.Dispose();
            }
        }

        public bool GetResultAsBoolean()
        {
            switch (this.resultType)
            {
                case XPathResultType.Number:
                    return QueryValueModel.Boolean(this.numberResult);

                case XPathResultType.String:
                    return QueryValueModel.Boolean(this.stringResult);

                case XPathResultType.Boolean:
                    return this.boolResult;

                case XPathResultType.NodeSet:
                    return QueryValueModel.Boolean(this.nodeSetResult);
            }
            throw Fx.AssertAndThrow("Unexpected result type.");
        }

        public XPathNodeIterator GetResultAsNodeset()
        {
            if (this.resultType != XPathResultType.NodeSet)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("CannotRepresentResultAsNodeset")));
            }
            return this.nodeSetResult;
        }

        public double GetResultAsNumber()
        {
            switch (this.resultType)
            {
                case XPathResultType.Number:
                    return this.numberResult;

                case XPathResultType.String:
                    return QueryValueModel.Double(this.stringResult);

                case XPathResultType.Boolean:
                    return QueryValueModel.Double(this.boolResult);

                case XPathResultType.NodeSet:
                    return QueryValueModel.Double(this.nodeSetResult);
            }
            throw Fx.AssertAndThrow("Unexpected result type.");
        }

        public string GetResultAsString()
        {
            switch (this.resultType)
            {
                case XPathResultType.Number:
                    return QueryValueModel.String(this.numberResult);

                case XPathResultType.String:
                    return this.stringResult;

                case XPathResultType.Boolean:
                    return QueryValueModel.String(this.boolResult);

                case XPathResultType.NodeSet:
                    return QueryValueModel.String(this.nodeSetResult);
            }
            throw Fx.AssertAndThrow("Unexpected result type.");
        }

        public XPathResultType ResultType
        {
            get
            {
                return this.resultType;
            }
        }
    }
}

