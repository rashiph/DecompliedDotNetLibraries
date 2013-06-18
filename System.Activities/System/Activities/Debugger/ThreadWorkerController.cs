namespace System.Activities.Debugger
{
    using System;
    using System.Activities;
    using System.Diagnostics;
    using System.Threading;

    [DebuggerNonUserCode]
    public class ThreadWorkerController
    {
        private bool breakOnStartup;
        private VirtualStackFrame enterStackParameter;
        private EventCode eventCode;
        private AutoResetEvent eventDone;
        private AutoResetEvent eventSend;
        private StateManager stateManager;
        private Thread worker;

        internal void Break()
        {
            this.eventCode = EventCode.Break;
            this.eventSend.Set();
            this.eventDone.WaitOne();
        }

        private void CreateWorkerThread(string threadName)
        {
            this.eventSend = new AutoResetEvent(false);
            this.eventDone = new AutoResetEvent(false);
            this.worker = new Thread(new ThreadStart(this.WorkerThreadProc));
            string str = string.IsNullOrEmpty(threadName) ? this.stateManager.ManagerProperties.AuxiliaryThreadName : threadName;
            if (str != null)
            {
                this.worker.Name = str;
            }
            this.worker.Start();
        }

        internal void EnterState(VirtualStackFrame newFrame)
        {
            this.eventCode = EventCode.Enter;
            this.enterStackParameter = newFrame;
            this.eventSend.Set();
            this.eventDone.WaitOne();
        }

        internal void Exit()
        {
            this.LeaveState();
            this.worker.Join();
        }

        internal void Initialize(string threadName, StateManager manager)
        {
            this.stateManager = manager;
            this.breakOnStartup = this.stateManager.ManagerProperties.BreakOnStartup;
            this.CreateWorkerThread(threadName);
        }

        [DebuggerHidden]
        public static void IslandWorker(ThreadWorkerController controller)
        {
            if (controller == null)
            {
                throw FxTrace.Exception.ArgumentNull("controller");
            }
            controller.Worker(true);
        }

        internal void LeaveState()
        {
            this.eventCode = EventCode.Leave;
            this.eventSend.Set();
            this.eventDone.WaitOne();
        }

        [DebuggerHidden]
        internal void Worker(bool isAtStartup)
        {
            if (isAtStartup)
            {
                if (this.breakOnStartup)
                {
                    if (Debugger.IsAttached)
                    {
                        Debugger.Break();
                    }
                    this.breakOnStartup = false;
                }
                this.eventDone.Set();
            }
            bool flag = false;
            while (!flag)
            {
                this.eventSend.WaitOne();
                switch (this.eventCode)
                {
                    case EventCode.Enter:
                        this.stateManager.InvokeWorker(this, this.enterStackParameter);
                        this.eventDone.Set();
                        break;

                    case EventCode.Leave:
                        flag = true;
                        return;

                    case EventCode.Break:
                        Debugger.Break();
                        this.eventDone.Set();
                        break;
                }
            }
        }

        [DebuggerHidden]
        private void WorkerThreadProc()
        {
            this.Worker(false);
            this.eventDone.Set();
        }

        private enum EventCode
        {
            Enter,
            Leave,
            Break
        }
    }
}

