namespace System.Management
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Threading;

    internal class ThreadDispatch
    {
        private ApartmentState apartmentType;
        private bool backgroundThread;
        private System.Exception exception;
        private Thread thread;
        private object threadParams;
        private object threadReturn;
        private ThreadWorkerMethod threadWorkerMethod;
        private ThreadWorkerMethodWithParam threadWorkerMethodWithParam;
        private ThreadWorkerMethodWithReturn threadWorkerMethodWithReturn;
        private ThreadWorkerMethodWithReturnAndParam threadWorkerMethodWithReturnAndParam;

        private ThreadDispatch()
        {
            this.thread = null;
            this.exception = null;
            this.threadParams = null;
            this.threadWorkerMethodWithReturn = null;
            this.threadWorkerMethodWithReturnAndParam = null;
            this.threadWorkerMethod = null;
            this.threadWorkerMethodWithParam = null;
            this.threadReturn = null;
            this.backgroundThread = false;
            this.apartmentType = ApartmentState.MTA;
        }

        public ThreadDispatch(ThreadWorkerMethod workerMethod) : this()
        {
            this.InitializeThreadState(null, workerMethod, ApartmentState.MTA, false);
        }

        public ThreadDispatch(ThreadWorkerMethodWithParam workerMethod) : this()
        {
            this.InitializeThreadState(null, workerMethod, ApartmentState.MTA, false);
        }

        public ThreadDispatch(ThreadWorkerMethodWithReturn workerMethod) : this()
        {
            this.InitializeThreadState(null, workerMethod, ApartmentState.MTA, false);
        }

        public ThreadDispatch(ThreadWorkerMethodWithReturnAndParam workerMethod) : this()
        {
            this.InitializeThreadState(null, workerMethod, ApartmentState.MTA, false);
        }

        private void DispatchThread()
        {
            this.thread.Start();
            this.thread.Join();
        }

        private void InitializeThreadState(object threadParams, ThreadWorkerMethod workerMethod, ApartmentState aptState, bool background)
        {
            this.threadParams = threadParams;
            this.threadWorkerMethod = workerMethod;
            this.thread = new Thread(new ThreadStart(this.ThreadEntryPoint));
            this.thread.SetApartmentState(aptState);
            this.backgroundThread = background;
        }

        private void InitializeThreadState(object threadParams, ThreadWorkerMethodWithParam workerMethod, ApartmentState aptState, bool background)
        {
            this.threadParams = threadParams;
            this.threadWorkerMethodWithParam = workerMethod;
            this.thread = new Thread(new ThreadStart(this.ThreadEntryPointMethodWithParam));
            this.thread.SetApartmentState(aptState);
            this.backgroundThread = background;
        }

        private void InitializeThreadState(object threadParams, ThreadWorkerMethodWithReturn workerMethod, ApartmentState aptState, bool background)
        {
            this.threadParams = threadParams;
            this.threadWorkerMethodWithReturn = workerMethod;
            this.thread = new Thread(new ThreadStart(this.ThreadEntryPointMethodWithReturn));
            this.thread.SetApartmentState(aptState);
            this.backgroundThread = background;
        }

        private void InitializeThreadState(object threadParams, ThreadWorkerMethodWithReturnAndParam workerMethod, ApartmentState aptState, bool background)
        {
            this.threadParams = threadParams;
            this.threadWorkerMethodWithReturnAndParam = workerMethod;
            this.thread = new Thread(new ThreadStart(this.ThreadEntryPointMethodWithReturnAndParam));
            this.thread.SetApartmentState(aptState);
            this.backgroundThread = background;
        }

        public void Start()
        {
            this.exception = null;
            this.DispatchThread();
            if (this.Exception != null)
            {
                throw this.Exception;
            }
        }

        private void ThreadEntryPoint()
        {
            try
            {
                this.threadWorkerMethod();
            }
            catch (System.Exception exception)
            {
                this.exception = exception;
            }
        }

        private void ThreadEntryPointMethodWithParam()
        {
            try
            {
                this.threadWorkerMethodWithParam(this.threadParams);
            }
            catch (System.Exception exception)
            {
                this.exception = exception;
            }
        }

        private void ThreadEntryPointMethodWithReturn()
        {
            try
            {
                this.threadReturn = this.threadWorkerMethodWithReturn();
            }
            catch (System.Exception exception)
            {
                this.exception = exception;
            }
        }

        private void ThreadEntryPointMethodWithReturnAndParam()
        {
            try
            {
                this.threadReturn = this.threadWorkerMethodWithReturnAndParam(this.threadParams);
            }
            catch (System.Exception exception)
            {
                this.exception = exception;
            }
        }

        public ApartmentState ApartmentType
        {
            get
            {
                return this.apartmentType;
            }
            set
            {
                this.apartmentType = value;
            }
        }

        public System.Exception Exception
        {
            get
            {
                return this.exception;
            }
        }

        public bool IsBackgroundThread
        {
            get
            {
                return this.backgroundThread;
            }
            set
            {
                this.backgroundThread = value;
            }
        }

        public object Parameter
        {
            get
            {
                return this.threadParams;
            }
            set
            {
                this.threadParams = value;
            }
        }

        public object Result
        {
            get
            {
                return this.threadReturn;
            }
        }

        public delegate void ThreadWorkerMethod();

        public delegate void ThreadWorkerMethodWithParam(object param);

        public delegate object ThreadWorkerMethodWithReturn();

        public delegate object ThreadWorkerMethodWithReturnAndParam(object param);
    }
}

