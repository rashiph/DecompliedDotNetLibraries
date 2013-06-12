namespace System.Runtime.InteropServices
{
    using System;
    using System.Runtime.InteropServices.ComTypes;
    using System.Security;

    [SecurityCritical]
    internal class ComEventsSink : NativeMethods.IDispatch, ICustomQueryInterface
    {
        private IConnectionPoint _connectionPoint;
        private int _cookie;
        private Guid _iidSourceItf;
        private ComEventsMethod _methods;
        private ComEventsSink _next;
        private static Guid IID_IManagedObject = new Guid("{C3FCC19E-A970-11D2-8B5A-00A0C9B7C9C4}");

        internal ComEventsSink(object rcw, Guid iid)
        {
            this._iidSourceItf = iid;
            this.Advise(rcw);
        }

        internal static ComEventsSink Add(ComEventsSink sinks, ComEventsSink sink)
        {
            sink._next = sinks;
            return sink;
        }

        public ComEventsMethod AddMethod(int dispid)
        {
            ComEventsMethod method = new ComEventsMethod(dispid);
            this._methods = ComEventsMethod.Add(this._methods, method);
            return method;
        }

        private void Advise(object rcw)
        {
            IConnectionPoint point;
            ((IConnectionPointContainer) rcw).FindConnectionPoint(ref this._iidSourceItf, out point);
            object pUnkSink = this;
            point.Advise(pUnkSink, out this._cookie);
            this._connectionPoint = point;
        }

        internal static ComEventsSink Find(ComEventsSink sinks, ref Guid iid)
        {
            ComEventsSink sink = sinks;
            while ((sink != null) && (sink._iidSourceItf != iid))
            {
                sink = sink._next;
            }
            return sink;
        }

        public ComEventsMethod FindMethod(int dispid)
        {
            return ComEventsMethod.Find(this._methods, dispid);
        }

        [SecurityCritical]
        internal static ComEventsSink Remove(ComEventsSink sinks, ComEventsSink sink)
        {
            if (sink == sinks)
            {
                sinks = sinks._next;
            }
            else
            {
                ComEventsSink sink2 = sinks;
                while ((sink2 != null) && (sink2._next != sink))
                {
                    sink2 = sink2._next;
                }
                if (sink2 != null)
                {
                    sink2._next = sink._next;
                }
            }
            sink.Unadvise();
            return sinks;
        }

        [SecurityCritical]
        internal static ComEventsSink RemoveAll(ComEventsSink sinks)
        {
            while (sinks != null)
            {
                sinks.Unadvise();
                sinks = sinks._next;
            }
            return null;
        }

        public ComEventsMethod RemoveMethod(ComEventsMethod method)
        {
            this._methods = ComEventsMethod.Remove(this._methods, method);
            return this._methods;
        }

        [SecurityCritical]
        CustomQueryInterfaceResult ICustomQueryInterface.GetInterface(ref Guid iid, out IntPtr ppv)
        {
            ppv = IntPtr.Zero;
            if ((iid == this._iidSourceItf) || (iid == typeof(NativeMethods.IDispatch).GUID))
            {
                ppv = Marshal.GetComInterfaceForObject(this, typeof(NativeMethods.IDispatch), CustomQueryInterfaceMode.Ignore);
                return CustomQueryInterfaceResult.Handled;
            }
            if (iid == IID_IManagedObject)
            {
                return CustomQueryInterfaceResult.Failed;
            }
            return CustomQueryInterfaceResult.NotHandled;
        }

        [SecurityCritical]
        void NativeMethods.IDispatch.GetIDsOfNames(ref Guid iid, string[] names, uint cNames, int lcid, int[] rgDispId)
        {
            throw new NotImplementedException();
        }

        [SecurityCritical]
        void NativeMethods.IDispatch.GetTypeInfo(uint iTInfo, int lcid, out IntPtr info)
        {
            throw new NotImplementedException();
        }

        [SecurityCritical]
        void NativeMethods.IDispatch.GetTypeInfoCount(out uint pctinfo)
        {
            pctinfo = 0;
        }

        [SecurityCritical]
        unsafe void NativeMethods.IDispatch.Invoke(int dispid, ref Guid riid, int lcid, System.Runtime.InteropServices.ComTypes.INVOKEKIND wFlags, ref System.Runtime.InteropServices.ComTypes.DISPPARAMS pDispParams, IntPtr pvarResult, IntPtr pExcepInfo, IntPtr puArgErr)
        {
            ComEventsMethod method = this.FindMethod(dispid);
            if (method != null)
            {
                int num2;
                object[] args = new object[pDispParams.cArgs];
                int[] numArray = new int[pDispParams.cArgs];
                bool[] flagArray = new bool[pDispParams.cArgs];
                System.Runtime.InteropServices.Variant* rgvarg = (System.Runtime.InteropServices.Variant*) pDispParams.rgvarg;
                int* rgdispidNamedArgs = (int*) pDispParams.rgdispidNamedArgs;
                int index = 0;
                while (index < pDispParams.cNamedArgs)
                {
                    num2 = rgdispidNamedArgs[index];
                    args[num2] = (rgvarg + index).ToObject();
                    flagArray[num2] = true;
                    if ((rgvarg + index).IsByRef)
                    {
                        numArray[num2] = index;
                    }
                    else
                    {
                        numArray[num2] = -1;
                    }
                    index++;
                }
                num2 = 0;
                while (index < pDispParams.cArgs)
                {
                    while (flagArray[num2])
                    {
                        num2++;
                    }
                    System.Runtime.InteropServices.Variant* variantPtr2 = rgvarg + ((pDispParams.cArgs - 1) - index);
                    args[num2] = variantPtr2.ToObject();
                    if (variantPtr2.IsByRef)
                    {
                        numArray[num2] = (pDispParams.cArgs - 1) - index;
                    }
                    else
                    {
                        numArray[num2] = -1;
                    }
                    num2++;
                    index++;
                }
                object obj2 = method.Invoke(args);
                if (pvarResult != IntPtr.Zero)
                {
                    Marshal.GetNativeVariantForObject(obj2, pvarResult);
                }
                for (index = 0; index < pDispParams.cArgs; index++)
                {
                    int num3 = numArray[index];
                    if (num3 != -1)
                    {
                        (rgvarg + num3).CopyFromIndirect(args[index]);
                    }
                }
            }
        }

        [SecurityCritical]
        private void Unadvise()
        {
            try
            {
                this._connectionPoint.Unadvise(this._cookie);
                Marshal.ReleaseComObject(this._connectionPoint);
            }
            catch (Exception)
            {
            }
            finally
            {
                this._connectionPoint = null;
            }
        }
    }
}

