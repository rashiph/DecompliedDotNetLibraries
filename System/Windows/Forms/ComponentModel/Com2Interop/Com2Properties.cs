namespace System.Windows.Forms.ComponentModel.Com2Interop
{
    using System;
    using System.Collections;
    using System.Diagnostics;
    using System.Runtime.InteropServices;
    using System.Runtime.InteropServices.ComTypes;
    using System.Threading;
    using System.Windows.Forms;

    internal class Com2Properties
    {
        private static long AGE_THRESHHOLD = 0xb2d05e00L;
        private int alwaysValid;
        private static int countOffset = -1;
        private static TraceSwitch DbgCom2PropertiesSwitch = new TraceSwitch("DbgCom2Properties", "Com2Properties: debug Com2 properties manager");
        private int defaultIndex = -1;
        private static System.Type[] extendedInterfaceHandlerTypes = new System.Type[] { typeof(Com2ICategorizePropertiesHandler), typeof(Com2IProvidePropertyBuilderHandler), typeof(Com2IPerPropertyBrowsingHandler), typeof(Com2IVsPerPropertyBrowsingHandler), typeof(Com2IManagedPerPropertyBrowsingHandler) };
        private static System.Type[] extendedInterfaces = new System.Type[] { typeof(System.Windows.Forms.NativeMethods.ICategorizeProperties), typeof(System.Windows.Forms.NativeMethods.IProvidePropertyBuilder), typeof(System.Windows.Forms.NativeMethods.IPerPropertyBrowsing), typeof(System.Windows.Forms.NativeMethods.IVsPerPropertyBrowsing), typeof(System.Windows.Forms.NativeMethods.IManagedPerPropertyBrowsing) };
        private Com2PropertyDescriptor[] props;
        private long touchedTime;
        private long[] typeInfoVersions;
        private static int versionOffset = -1;
        internal WeakReference weakObjRef;

        public event EventHandler Disposed;

        public Com2Properties(object obj, Com2PropertyDescriptor[] props, int defaultIndex)
        {
            this.SetProps(props);
            this.weakObjRef = new WeakReference(obj);
            this.defaultIndex = defaultIndex;
            this.typeInfoVersions = this.GetTypeInfoVersions(obj);
            this.touchedTime = DateTime.Now.Ticks;
        }

        public void AddExtendedBrowsingHandlers(Hashtable handlers)
        {
            object targetObject = this.TargetObject;
            if (targetObject != null)
            {
                for (int i = 0; i < extendedInterfaces.Length; i++)
                {
                    System.Type type = extendedInterfaces[i];
                    if (type.IsInstanceOfType(targetObject))
                    {
                        Com2ExtendedBrowsingHandler handler = (Com2ExtendedBrowsingHandler) handlers[type];
                        if (handler == null)
                        {
                            handler = (Com2ExtendedBrowsingHandler) Activator.CreateInstance(extendedInterfaceHandlerTypes[i]);
                            handlers[type] = handler;
                        }
                        if (!type.IsAssignableFrom(handler.Interface))
                        {
                            throw new ArgumentException(System.Windows.Forms.SR.GetString("COM2BadHandlerType", new object[] { type.Name, handler.Interface.Name }));
                        }
                        handler.SetupPropertyHandlers(this.props);
                    }
                }
            }
        }

        public bool CheckValid()
        {
            return this.CheckValid(false);
        }

        public bool CheckValid(bool checkVersions)
        {
            return this.CheckValid(checkVersions, true);
        }

        internal bool CheckValid(bool checkVersions, bool callDispose)
        {
            if (this.AlwaysValid)
            {
                return true;
            }
            bool flag = (this.weakObjRef != null) && this.weakObjRef.IsAlive;
            if (flag && checkVersions)
            {
                long[] typeInfoVersions = this.GetTypeInfoVersions(this.weakObjRef.Target);
                if (typeInfoVersions.Length != this.typeInfoVersions.Length)
                {
                    flag = false;
                }
                else
                {
                    for (int i = 0; i < typeInfoVersions.Length; i++)
                    {
                        if (typeInfoVersions[i] != this.typeInfoVersions[i])
                        {
                            flag = false;
                            break;
                        }
                    }
                }
                if (!flag)
                {
                    this.typeInfoVersions = typeInfoVersions;
                }
            }
            if (!flag && callDispose)
            {
                this.Dispose();
            }
            return flag;
        }

        public void Dispose()
        {
            if (this.props != null)
            {
                if (this.Disposed != null)
                {
                    this.Disposed(this, EventArgs.Empty);
                }
                this.weakObjRef = null;
                this.props = null;
                this.touchedTime = 0L;
            }
        }

        private unsafe long GetTypeInfoVersion(UnsafeNativeMethods.ITypeInfo pTypeInfo)
        {
            long num3;
            IntPtr zero = IntPtr.Zero;
            if (!System.Windows.Forms.NativeMethods.Succeeded(pTypeInfo.GetTypeAttr(ref zero)))
            {
                return 0L;
            }
            try
            {
                System.Runtime.InteropServices.ComTypes.TYPEATTR typeattr;
                try
                {
                    typeattr = *((System.Runtime.InteropServices.ComTypes.TYPEATTR*) zero);
                }
                catch
                {
                    return 0L;
                }
                long num2 = 0L;
                int* numPtr = (int*) &num2;
                byte* numPtr2 = (byte*) &typeattr;
                numPtr[0] = *((int*) (numPtr2 + CountMemberOffset));
                numPtr++;
                numPtr[0] = *((int*) (numPtr2 + VersionOffset));
                num3 = num2;
            }
            finally
            {
                pTypeInfo.ReleaseTypeAttr(zero);
            }
            return num3;
        }

        private long[] GetTypeInfoVersions(object comObject)
        {
            UnsafeNativeMethods.ITypeInfo[] infoArray = Com2TypeInfoProcessor.FindTypeInfos(comObject, false);
            long[] numArray = new long[infoArray.Length];
            for (int i = 0; i < infoArray.Length; i++)
            {
                numArray[i] = this.GetTypeInfoVersion(infoArray[i]);
            }
            return numArray;
        }

        internal void SetProps(Com2PropertyDescriptor[] props)
        {
            this.props = props;
            if (props != null)
            {
                for (int i = 0; i < props.Length; i++)
                {
                    props[i].PropertyManager = this;
                }
            }
        }

        internal bool AlwaysValid
        {
            get
            {
                return (this.alwaysValid > 0);
            }
            set
            {
                if (value)
                {
                    if ((this.alwaysValid != 0) || this.CheckValid())
                    {
                        this.alwaysValid++;
                    }
                }
                else if (this.alwaysValid > 0)
                {
                    this.alwaysValid--;
                }
            }
        }

        private static int CountMemberOffset
        {
            get
            {
                if (countOffset == -1)
                {
                    countOffset = (Marshal.SizeOf(typeof(Guid)) + IntPtr.Size) + 0x18;
                }
                return countOffset;
            }
        }

        public Com2PropertyDescriptor DefaultProperty
        {
            get
            {
                if (this.CheckValid(true))
                {
                    if (this.defaultIndex != -1)
                    {
                        return this.props[this.defaultIndex];
                    }
                    if (this.props.Length > 0)
                    {
                        return this.props[0];
                    }
                }
                return null;
            }
        }

        public Com2PropertyDescriptor[] Properties
        {
            get
            {
                this.CheckValid(true);
                if ((this.touchedTime == 0L) || (this.props == null))
                {
                    return null;
                }
                this.touchedTime = DateTime.Now.Ticks;
                for (int i = 0; i < this.props.Length; i++)
                {
                    this.props[i].SetNeedsRefresh(0xff, true);
                }
                return this.props;
            }
        }

        public object TargetObject
        {
            get
            {
                if (this.CheckValid(false) && (this.touchedTime != 0L))
                {
                    return this.weakObjRef.Target;
                }
                return null;
            }
        }

        public long TicksSinceTouched
        {
            get
            {
                if (this.touchedTime == 0L)
                {
                    return 0L;
                }
                return (DateTime.Now.Ticks - this.touchedTime);
            }
        }

        public bool TooOld
        {
            get
            {
                this.CheckValid(false, false);
                if (this.touchedTime == 0L)
                {
                    return false;
                }
                return (this.TicksSinceTouched > AGE_THRESHHOLD);
            }
        }

        private static int VersionOffset
        {
            get
            {
                if (versionOffset == -1)
                {
                    versionOffset = CountMemberOffset + 12;
                }
                return versionOffset;
            }
        }
    }
}

