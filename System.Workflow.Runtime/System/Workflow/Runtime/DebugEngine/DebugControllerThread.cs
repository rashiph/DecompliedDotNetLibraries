namespace System.Workflow.Runtime.DebugEngine
{
    using Microsoft.Win32;
    using System;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Workflow.Runtime;

    internal sealed class DebugControllerThread
    {
        private Thread controllerThread;
        private static readonly string ExpressionEvaluationFrameTypeName = "ExpressionEvaluationFrameTypeName";
        private volatile bool runThread;
        private int threadId;
        private ManualResetEvent threadInitializedEvent = new ManualResetEvent(false);

        public DebugControllerThread()
        {
            this.threadInitializedEvent.Reset();
            this.controllerThread = new Thread(new ParameterizedThreadStart(this.ControllerThreadFunction));
            this.controllerThread.IsBackground = true;
            this.controllerThread.Priority = ThreadPriority.Lowest;
            this.controllerThread.Name = "__dct__";
        }

        private void ControllerThreadFunction(object instanceTable)
        {
            try
            {
                IExpressionEvaluationFrame frame = null;
                try
                {
                    RegistryKey key = Registry.LocalMachine.OpenSubKey(RegistryKeys.DebuggerSubKey);
                    if (key != null)
                    {
                        string str = key.GetValue(ExpressionEvaluationFrameTypeName, string.Empty) as string;
                        if (!string.IsNullOrEmpty(str) && (Type.GetType(str) != null))
                        {
                            frame = Activator.CreateInstance(Type.GetType(str)) as IExpressionEvaluationFrame;
                        }
                    }
                }
                catch
                {
                }
                finally
                {
                    if (frame == null)
                    {
                        frame = Activator.CreateInstance(Type.GetType("Microsoft.Workflow.DebugEngine.ExpressionEvaluationFrame, Microsoft.Workflow.ExpressionEvaluation, Version=10.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35")) as IExpressionEvaluationFrame;
                    }
                }
                if (frame != null)
                {
                    frame.CreateEvaluationFrame((IInstanceTable) instanceTable, (DebugEngineCallback) Delegate.CreateDelegate(typeof(DebugEngineCallback), this, "ExpressionEvaluationFunction"));
                }
            }
            catch
            {
            }
        }

        public void ExpressionEvaluationFunction()
        {
            this.threadId = System.Workflow.Runtime.DebugEngine.NativeMethods.GetCurrentThreadId();
            this.threadInitializedEvent.Set();
            using (new DebuggerThreadMarker())
            {
                while (this.runThread)
                {
                    try
                    {
                        if (IntPtr.Size == 8)
                        {
                            Thread.Sleep(-1);
                        }
                        else
                        {
                            while (this.runThread)
                            {
                            }
                        }
                        continue;
                    }
                    catch (ThreadAbortException)
                    {
                        throw;
                    }
                    catch
                    {
                        continue;
                    }
                }
            }
        }

        public void RunThread(IInstanceTable instanceTable)
        {
            if (this.controllerThread != null)
            {
                this.runThread = true;
                this.controllerThread.Start(instanceTable);
                this.threadInitializedEvent.WaitOne();
            }
        }

        public void StopThread()
        {
            try
            {
                if (this.controllerThread != null)
                {
                    this.runThread = false;
                    Thread.Sleep(10);
                    if (this.controllerThread.IsAlive && (IntPtr.Size == 8))
                    {
                        while (this.controllerThread.ThreadState == ThreadState.WaitSleepJoin)
                        {
                            this.controllerThread.Start();
                            this.controllerThread.Join();
                        }
                    }
                    else
                    {
                        this.controllerThread.Join();
                    }
                }
            }
            catch (ThreadStateException)
            {
            }
            finally
            {
                this.controllerThread = null;
            }
            this.controllerThread = null;
            this.threadId = 0;
            this.threadInitializedEvent = null;
        }

        public int ManagedThreadId
        {
            get
            {
                return this.controllerThread.ManagedThreadId;
            }
        }

        public int ThreadId
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.threadId;
            }
        }
    }
}

