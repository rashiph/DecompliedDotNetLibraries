namespace System.Workflow.Runtime.DebugEngine
{
    using System;
    using System.Runtime.InteropServices;
    using System.Threading;

    internal sealed class ProgramPublisher
    {
        private DebugController controller;
        private GCHandle gchWdeProgramNode;
        private bool isPublished;
        private IWDEProgramPublisher publisher = null;
        private IWDEProgramNode wdeProgramNodeSingleton;

        public bool Publish(DebugController controller)
        {
            if (this.isPublished)
            {
                return false;
            }
            try
            {
                this.controller = controller;
                Thread thread = new Thread(new ThreadStart(this.PublisherThreadFunc));
                thread.SetApartmentState(ApartmentState.MTA);
                thread.IsBackground = true;
                thread.Start();
                thread.Join();
            }
            catch (Exception)
            {
            }
            return this.isPublished;
        }

        private void PublisherThreadFunc()
        {
            try
            {
                this.publisher = new WDEProgramPublisher() as IWDEProgramPublisher;
                this.wdeProgramNodeSingleton = new ProgramNode(this.controller);
                this.gchWdeProgramNode = GCHandle.Alloc(this.wdeProgramNodeSingleton);
                this.publisher.Publish(this.wdeProgramNodeSingleton);
                this.isPublished = true;
            }
            catch (Exception)
            {
            }
        }

        public void Unpublish()
        {
            if (this.isPublished)
            {
                try
                {
                    Thread thread = new Thread(new ThreadStart(this.UnpublishThreadFunc));
                    thread.SetApartmentState(ApartmentState.MTA);
                    thread.IsBackground = true;
                    thread.Start();
                    thread.Join();
                }
                catch (Exception)
                {
                }
            }
        }

        private void UnpublishThreadFunc()
        {
            try
            {
                this.publisher.Unpublish(this.wdeProgramNodeSingleton);
            }
            catch (Exception)
            {
            }
            finally
            {
                this.gchWdeProgramNode.Free();
                Marshal.ReleaseComObject(this.publisher);
                this.publisher = null;
            }
            this.isPublished = false;
        }
    }
}

