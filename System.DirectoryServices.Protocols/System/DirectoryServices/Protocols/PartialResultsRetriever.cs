namespace System.DirectoryServices.Protocols
{
    using System;
    using System.Threading;

    internal class PartialResultsRetriever
    {
        private Thread oThread;
        private LdapPartialResultsProcessor processor;
        private ManualResetEvent workThreadWaitHandle;

        internal PartialResultsRetriever(ManualResetEvent eventHandle, LdapPartialResultsProcessor processor)
        {
            this.workThreadWaitHandle = eventHandle;
            this.processor = processor;
            this.oThread = new Thread(new ThreadStart(this.ThreadRoutine));
            this.oThread.IsBackground = true;
            this.oThread.Start();
        }

        private void ThreadRoutine()
        {
            while (true)
            {
                this.workThreadWaitHandle.WaitOne();
                try
                {
                    this.processor.RetrievingSearchResults();
                }
                catch (Exception)
                {
                }
                Thread.Sleep(250);
            }
        }
    }
}

