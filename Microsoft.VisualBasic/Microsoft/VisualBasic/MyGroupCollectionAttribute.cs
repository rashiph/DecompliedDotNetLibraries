namespace Microsoft.VisualBasic
{
    using System;
    using System.ComponentModel;
    using System.Runtime;

    [AttributeUsage(AttributeTargets.Class, AllowMultiple=false, Inherited=false), EditorBrowsable(EditorBrowsableState.Advanced)]
    public sealed class MyGroupCollectionAttribute : Attribute
    {
        private string m_DefaultInstanceAlias;
        private string m_NameOfBaseTypeToCollect;
        private string m_NameOfCreateMethod;
        private string m_NameOfDisposeMethod;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public MyGroupCollectionAttribute(string typeToCollect, string createInstanceMethodName, string disposeInstanceMethodName, string defaultInstanceAlias)
        {
            this.m_NameOfBaseTypeToCollect = typeToCollect;
            this.m_NameOfCreateMethod = createInstanceMethodName;
            this.m_NameOfDisposeMethod = disposeInstanceMethodName;
            this.m_DefaultInstanceAlias = defaultInstanceAlias;
        }

        public string CreateMethod
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.m_NameOfCreateMethod;
            }
        }

        public string DefaultInstanceAlias
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.m_DefaultInstanceAlias;
            }
        }

        public string DisposeMethod
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.m_NameOfDisposeMethod;
            }
        }

        public string MyGroupName
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.m_NameOfBaseTypeToCollect;
            }
        }
    }
}

