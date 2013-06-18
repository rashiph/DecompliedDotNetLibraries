namespace System.Xml.Xsl
{
    using System;
    using System.Collections;
    using System.Runtime;
    using System.Xml;
    using System.Xml.Xsl.Runtime;

    internal class XmlILCommand
    {
        private System.Xml.Xsl.ExecuteDelegate delExec;
        private XmlQueryStaticData staticData;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public XmlILCommand(System.Xml.Xsl.ExecuteDelegate delExec, XmlQueryStaticData staticData)
        {
            this.delExec = delExec;
            this.staticData = staticData;
        }

        public IList Evaluate(string contextDocumentUri, XmlResolver dataSources, XsltArgumentList argumentList)
        {
            XmlCachedSequenceWriter results = new XmlCachedSequenceWriter();
            this.Execute(contextDocumentUri, dataSources, argumentList, results);
            return results.ResultSequence;
        }

        public void Execute(object defaultDocument, XmlResolver dataSources, XsltArgumentList argumentList, XmlWriter writer)
        {
            try
            {
                XmlWellFormedWriter writer2 = writer as XmlWellFormedWriter;
                if (((writer2 != null) && (writer2.RawWriter != null)) && ((writer2.WriteState == WriteState.Start) && (writer2.Settings.ConformanceLevel != ConformanceLevel.Document)))
                {
                    this.Execute(defaultDocument, dataSources, argumentList, new XmlMergeSequenceWriter(writer2.RawWriter));
                }
                else
                {
                    this.Execute(defaultDocument, dataSources, argumentList, new XmlMergeSequenceWriter(new XmlRawWriterWrapper(writer)));
                }
            }
            finally
            {
                writer.Flush();
            }
        }

        private void Execute(object defaultDocument, XmlResolver dataSources, XsltArgumentList argumentList, XmlSequenceWriter results)
        {
            if (dataSources == null)
            {
                dataSources = XmlNullResolver.Singleton;
            }
            this.delExec(new XmlQueryRuntime(this.staticData, defaultDocument, dataSources, argumentList, results));
        }

        public System.Xml.Xsl.ExecuteDelegate ExecuteDelegate
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.delExec;
            }
        }

        public XmlQueryStaticData StaticData
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.staticData;
            }
        }
    }
}

