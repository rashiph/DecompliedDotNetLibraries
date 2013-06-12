namespace System.ComponentModel.Design.Serialization
{
    using System;
    using System.Collections;
    using System.Reflection;
    using System.Security.Permissions;

    [PermissionSet(SecurityAction.LinkDemand, Name="FullTrust"), HostProtection(SecurityAction.LinkDemand, SharedState=true)]
    public sealed class InstanceDescriptor
    {
        private ICollection arguments;
        private bool isComplete;
        private System.Reflection.MemberInfo member;

        public InstanceDescriptor(System.Reflection.MemberInfo member, ICollection arguments) : this(member, arguments, true)
        {
        }

        public InstanceDescriptor(System.Reflection.MemberInfo member, ICollection arguments, bool isComplete)
        {
            this.member = member;
            this.isComplete = isComplete;
            if (arguments == null)
            {
                this.arguments = new object[0];
            }
            else
            {
                object[] array = new object[arguments.Count];
                arguments.CopyTo(array, 0);
                this.arguments = array;
            }
            if (member is FieldInfo)
            {
                FieldInfo info = (FieldInfo) member;
                if (!info.IsStatic)
                {
                    throw new ArgumentException(SR.GetString("InstanceDescriptorMustBeStatic"));
                }
                if (this.arguments.Count != 0)
                {
                    throw new ArgumentException(SR.GetString("InstanceDescriptorLengthMismatch"));
                }
            }
            else if (member is ConstructorInfo)
            {
                ConstructorInfo info2 = (ConstructorInfo) member;
                if (info2.IsStatic)
                {
                    throw new ArgumentException(SR.GetString("InstanceDescriptorCannotBeStatic"));
                }
                if (this.arguments.Count != info2.GetParameters().Length)
                {
                    throw new ArgumentException(SR.GetString("InstanceDescriptorLengthMismatch"));
                }
            }
            else if (member is MethodInfo)
            {
                MethodInfo info3 = (MethodInfo) member;
                if (!info3.IsStatic)
                {
                    throw new ArgumentException(SR.GetString("InstanceDescriptorMustBeStatic"));
                }
                if (this.arguments.Count != info3.GetParameters().Length)
                {
                    throw new ArgumentException(SR.GetString("InstanceDescriptorLengthMismatch"));
                }
            }
            else if (member is PropertyInfo)
            {
                PropertyInfo info4 = (PropertyInfo) member;
                if (!info4.CanRead)
                {
                    throw new ArgumentException(SR.GetString("InstanceDescriptorMustBeReadable"));
                }
                MethodInfo getMethod = info4.GetGetMethod();
                if ((getMethod != null) && !getMethod.IsStatic)
                {
                    throw new ArgumentException(SR.GetString("InstanceDescriptorMustBeStatic"));
                }
            }
        }

        public object Invoke()
        {
            object[] array = new object[this.arguments.Count];
            this.arguments.CopyTo(array, 0);
            for (int i = 0; i < array.Length; i++)
            {
                if (array[i] is InstanceDescriptor)
                {
                    array[i] = ((InstanceDescriptor) array[i]).Invoke();
                }
            }
            if (this.member is ConstructorInfo)
            {
                return ((ConstructorInfo) this.member).Invoke(array);
            }
            if (this.member is MethodInfo)
            {
                return ((MethodInfo) this.member).Invoke(null, array);
            }
            if (this.member is PropertyInfo)
            {
                return ((PropertyInfo) this.member).GetValue(null, array);
            }
            if (this.member is FieldInfo)
            {
                return ((FieldInfo) this.member).GetValue(null);
            }
            return null;
        }

        public ICollection Arguments
        {
            get
            {
                return this.arguments;
            }
        }

        public bool IsComplete
        {
            get
            {
                return this.isComplete;
            }
        }

        public System.Reflection.MemberInfo MemberInfo
        {
            get
            {
                return this.member;
            }
        }
    }
}

