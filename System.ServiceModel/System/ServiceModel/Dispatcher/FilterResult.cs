namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.Collections.ObjectModel;
    using System.Runtime.InteropServices;
    using System.ServiceModel;

    [StructLayout(LayoutKind.Sequential)]
    internal struct FilterResult
    {
        private QueryProcessor processor;
        private bool result;
        internal FilterResult(QueryProcessor processor)
        {
            this.processor = processor;
            this.result = this.processor.Result;
        }

        internal FilterResult(bool result)
        {
            this.processor = null;
            this.result = result;
        }

        internal QueryProcessor Processor
        {
            get
            {
                return this.processor;
            }
        }
        internal bool Result
        {
            get
            {
                return this.result;
            }
        }
        internal MessageFilter GetSingleMatch()
        {
            Collection<MessageFilter> matchList = this.processor.MatchList;
            switch (matchList.Count)
            {
                case 0:
                    return null;

                case 1:
                    return matchList[0];
            }
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MultipleFilterMatchesException(System.ServiceModel.SR.GetString("FilterMultipleMatches"), null, matchList));
        }
    }
}

