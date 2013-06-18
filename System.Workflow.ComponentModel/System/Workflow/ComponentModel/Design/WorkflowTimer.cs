namespace System.Workflow.ComponentModel.Design
{
    using System;
    using System.Collections.Generic;
    using System.Windows.Forms;

    internal sealed class WorkflowTimer : IDisposable
    {
        private List<ElapsedEventUnit> elapsedEvents = new List<ElapsedEventUnit>();
        private Timer timer = new Timer();
        private const int TimerInterval = 50;
        private static WorkflowTimer workflowTimer;

        private WorkflowTimer()
        {
            this.timer.Interval = 50;
            this.timer.Tick += new EventHandler(this.OnTimer);
            this.timer.Stop();
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (this.timer != null)
            {
                if (this.timer.Enabled)
                {
                    this.timer.Stop();
                }
                this.timer.Dispose();
                this.timer = null;
            }
        }

        ~WorkflowTimer()
        {
            this.Dispose(false);
        }

        private void OnTimer(object sender, EventArgs e)
        {
            List<ElapsedEventUnit> list = new List<ElapsedEventUnit>(this.elapsedEvents);
            foreach (ElapsedEventUnit unit in list)
            {
                unit.elapsedTime++;
                if (unit.elapsedInterval <= unit.elapsedTime)
                {
                    unit.elapsedTime = 0;
                    unit.elapsedEventHandler(this, EventArgs.Empty);
                }
            }
        }

        internal void Subscribe(int elapsedInterval, EventHandler elapsedEventHandler)
        {
            this.elapsedEvents.Add(new ElapsedEventUnit(elapsedInterval / 50, elapsedEventHandler));
            if (!this.timer.Enabled)
            {
                this.timer.Start();
            }
        }

        internal void Unsubscribe(EventHandler elapsedEventHandler)
        {
            List<ElapsedEventUnit> list = new List<ElapsedEventUnit>();
            foreach (ElapsedEventUnit unit in this.elapsedEvents)
            {
                if (unit.elapsedEventHandler == elapsedEventHandler)
                {
                    list.Add(unit);
                }
            }
            foreach (ElapsedEventUnit unit2 in list)
            {
                this.elapsedEvents.Remove(unit2);
            }
            if ((this.elapsedEvents.Count == 0) && this.timer.Enabled)
            {
                this.timer.Stop();
            }
        }

        internal static WorkflowTimer Default
        {
            get
            {
                if (workflowTimer == null)
                {
                    workflowTimer = new WorkflowTimer();
                }
                return workflowTimer;
            }
        }

        private sealed class ElapsedEventUnit
        {
            internal EventHandler elapsedEventHandler;
            internal int elapsedInterval;
            internal int elapsedTime;

            internal ElapsedEventUnit(int interval, EventHandler eventHandler)
            {
                this.elapsedInterval = interval;
                this.elapsedEventHandler = eventHandler;
            }
        }
    }
}

