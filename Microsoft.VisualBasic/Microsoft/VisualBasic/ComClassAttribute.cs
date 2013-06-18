namespace Microsoft.VisualBasic
{
    using System;
    using System.Runtime;

    [AttributeUsage(AttributeTargets.Class, Inherited=false, AllowMultiple=false)]
    public sealed class ComClassAttribute : Attribute
    {
        private string m_ClassID;
        private string m_EventID;
        private string m_InterfaceID;
        private bool m_InterfaceShadows;

        public ComClassAttribute()
        {
            this.m_InterfaceShadows = false;
        }

        public ComClassAttribute(string _ClassID)
        {
            this.m_InterfaceShadows = false;
            this.m_ClassID = _ClassID;
        }

        public ComClassAttribute(string _ClassID, string _InterfaceID)
        {
            this.m_InterfaceShadows = false;
            this.m_ClassID = _ClassID;
            this.m_InterfaceID = _InterfaceID;
        }

        public ComClassAttribute(string _ClassID, string _InterfaceID, string _EventId)
        {
            this.m_InterfaceShadows = false;
            this.m_ClassID = _ClassID;
            this.m_InterfaceID = _InterfaceID;
            this.m_EventID = _EventId;
        }

        public string ClassID
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.m_ClassID;
            }
        }

        public string EventID
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.m_EventID;
            }
        }

        public string InterfaceID
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.m_InterfaceID;
            }
        }

        public bool InterfaceShadows
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.m_InterfaceShadows;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.m_InterfaceShadows = value;
            }
        }
    }
}

