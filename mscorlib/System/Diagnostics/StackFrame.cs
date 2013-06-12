namespace System.Diagnostics
{
    using System;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Permissions;
    using System.Text;

    [Serializable, ComVisible(true), SecurityPermission(SecurityAction.InheritanceDemand, UnmanagedCode=true)]
    public class StackFrame
    {
        private int iColumnNumber;
        private int iLineNumber;
        private int ILOffset;
        private MethodBase method;
        private int offset;
        public const int OFFSET_UNKNOWN = -1;
        private string strFileName;

        public StackFrame()
        {
            this.InitMembers();
            this.BuildStackFrame(0, false);
        }

        public StackFrame(bool fNeedFileInfo)
        {
            this.InitMembers();
            this.BuildStackFrame(0, fNeedFileInfo);
        }

        [SecuritySafeCritical]
        public StackFrame(int skipFrames)
        {
            this.InitMembers();
            this.BuildStackFrame(skipFrames, false);
        }

        internal StackFrame(bool DummyFlag1, bool DummyFlag2)
        {
            this.InitMembers();
        }

        public StackFrame(int skipFrames, bool fNeedFileInfo)
        {
            this.InitMembers();
            this.BuildStackFrame(skipFrames, fNeedFileInfo);
        }

        [SecuritySafeCritical]
        public StackFrame(string fileName, int lineNumber)
        {
            this.InitMembers();
            this.BuildStackFrame(0, false);
            this.strFileName = fileName;
            this.iLineNumber = lineNumber;
            this.iColumnNumber = 0;
        }

        [SecuritySafeCritical]
        public StackFrame(string fileName, int lineNumber, int colNumber)
        {
            this.InitMembers();
            this.BuildStackFrame(0, false);
            this.strFileName = fileName;
            this.iLineNumber = lineNumber;
            this.iColumnNumber = colNumber;
        }

        private void BuildStackFrame(int skipFrames, bool fNeedFileInfo)
        {
            StackFrameHelper sfh = new StackFrameHelper(fNeedFileInfo, null);
            StackTrace.GetStackFramesInternal(sfh, 0, null);
            int numberOfFrames = sfh.GetNumberOfFrames();
            skipFrames += StackTrace.CalculateFramesToSkip(sfh, numberOfFrames);
            if ((numberOfFrames - skipFrames) > 0)
            {
                this.method = sfh.GetMethodBase(skipFrames);
                this.offset = sfh.GetOffset(skipFrames);
                this.ILOffset = sfh.GetILOffset(skipFrames);
                if (fNeedFileInfo)
                {
                    this.strFileName = sfh.GetFilename(skipFrames);
                    this.iLineNumber = sfh.GetLineNumber(skipFrames);
                    this.iColumnNumber = sfh.GetColumnNumber(skipFrames);
                }
            }
        }

        public virtual int GetFileColumnNumber()
        {
            return this.iColumnNumber;
        }

        public virtual int GetFileLineNumber()
        {
            return this.iLineNumber;
        }

        [SecuritySafeCritical]
        public virtual string GetFileName()
        {
            if (this.strFileName != null)
            {
                new FileIOPermission(PermissionState.None) { AllFiles = FileIOPermissionAccess.PathDiscovery }.Demand();
            }
            return this.strFileName;
        }

        public virtual int GetILOffset()
        {
            return this.ILOffset;
        }

        public virtual MethodBase GetMethod()
        {
            return this.method;
        }

        public virtual int GetNativeOffset()
        {
            return this.offset;
        }

        internal void InitMembers()
        {
            this.method = null;
            this.offset = -1;
            this.ILOffset = -1;
            this.strFileName = null;
            this.iLineNumber = 0;
            this.iColumnNumber = 0;
        }

        internal virtual void SetColumnNumber(int iCol)
        {
            this.iColumnNumber = iCol;
        }

        internal virtual void SetFileName(string strFName)
        {
            this.strFileName = strFName;
        }

        internal virtual void SetILOffset(int iOffset)
        {
            this.ILOffset = iOffset;
        }

        internal virtual void SetLineNumber(int iLine)
        {
            this.iLineNumber = iLine;
        }

        internal virtual void SetMethodBase(MethodBase mb)
        {
            this.method = mb;
        }

        internal virtual void SetOffset(int iOffset)
        {
            this.offset = iOffset;
        }

        [SecuritySafeCritical]
        public override string ToString()
        {
            StringBuilder builder = new StringBuilder(0xff);
            if (this.method != null)
            {
                builder.Append(this.method.Name);
                if ((this.method is MethodInfo) && ((MethodInfo) this.method).IsGenericMethod)
                {
                    Type[] genericArguments = ((MethodInfo) this.method).GetGenericArguments();
                    builder.Append("<");
                    int index = 0;
                    bool flag = true;
                    while (index < genericArguments.Length)
                    {
                        if (!flag)
                        {
                            builder.Append(",");
                        }
                        else
                        {
                            flag = false;
                        }
                        builder.Append(genericArguments[index].Name);
                        index++;
                    }
                    builder.Append(">");
                }
                builder.Append(" at offset ");
                if (this.offset == -1)
                {
                    builder.Append("<offset unknown>");
                }
                else
                {
                    builder.Append(this.offset);
                }
                builder.Append(" in file:line:column ");
                bool flag2 = this.strFileName != null;
                if (flag2)
                {
                    try
                    {
                        new FileIOPermission(PermissionState.None) { AllFiles = FileIOPermissionAccess.PathDiscovery }.Demand();
                    }
                    catch (SecurityException)
                    {
                        flag2 = false;
                    }
                }
                if (!flag2)
                {
                    builder.Append("<filename unknown>");
                }
                else
                {
                    builder.Append(this.strFileName);
                }
                builder.Append(":");
                builder.Append(this.iLineNumber);
                builder.Append(":");
                builder.Append(this.iColumnNumber);
            }
            else
            {
                builder.Append("<null>");
            }
            builder.Append(Environment.NewLine);
            return builder.ToString();
        }
    }
}

