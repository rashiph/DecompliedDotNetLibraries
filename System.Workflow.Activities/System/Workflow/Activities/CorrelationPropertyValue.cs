namespace System.Workflow.Activities
{
    using System;
    using System.Reflection;
    using System.Runtime;

    internal sealed class CorrelationPropertyValue
    {
        private string locationPath;
        private string name;
        private int signaturePosition;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        internal CorrelationPropertyValue(string name, string locationPath, int signaturePosition)
        {
            this.name = name;
            this.locationPath = locationPath;
            this.signaturePosition = signaturePosition;
        }

        private Type GetMemberType(MemberInfo mInfo)
        {
            MemberTypes memberType = mInfo.MemberType;
            if (memberType != MemberTypes.Field)
            {
                if (memberType != MemberTypes.Property)
                {
                    return null;
                }
            }
            else
            {
                return ((FieldInfo) mInfo).FieldType;
            }
            return ((PropertyInfo) mInfo).PropertyType;
        }

        internal object GetValue(object[] args)
        {
            if (args.Length <= this.signaturePosition)
            {
                throw new ArgumentOutOfRangeException("args");
            }
            object target = args[this.signaturePosition];
            if (target == null)
            {
                return target;
            }
            Type memberType = target.GetType();
            object obj3 = target;
            if (this.locationPath.Length != 0)
            {
                string[] strArray = this.locationPath.Split(new char[] { '.' });
                for (int i = 1; i < strArray.Length; i++)
                {
                    string name = strArray[i];
                    if (target == null)
                    {
                        return obj3;
                    }
                    obj3 = memberType.InvokeMember(name, BindingFlags.GetProperty | BindingFlags.GetField | BindingFlags.Public | BindingFlags.Instance, null, target, null, null);
                    MemberInfo[] member = memberType.GetMember(name, BindingFlags.GetProperty | BindingFlags.GetField | BindingFlags.Public | BindingFlags.Instance);
                    memberType = this.GetMemberType(member[0]);
                    target = obj3;
                }
            }
            return obj3;
        }

        internal string Name
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.name;
            }
        }
    }
}

