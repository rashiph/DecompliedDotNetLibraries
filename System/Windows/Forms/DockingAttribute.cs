namespace System.Windows.Forms
{
    using System;

    [AttributeUsage(AttributeTargets.Class)]
    public sealed class DockingAttribute : Attribute
    {
        public static readonly DockingAttribute Default = new DockingAttribute();
        private System.Windows.Forms.DockingBehavior dockingBehavior;

        public DockingAttribute()
        {
            this.dockingBehavior = System.Windows.Forms.DockingBehavior.Never;
        }

        public DockingAttribute(System.Windows.Forms.DockingBehavior dockingBehavior)
        {
            this.dockingBehavior = dockingBehavior;
        }

        public override bool Equals(object obj)
        {
            if (obj == this)
            {
                return true;
            }
            DockingAttribute attribute = obj as DockingAttribute;
            return ((attribute != null) && (attribute.DockingBehavior == this.dockingBehavior));
        }

        public override int GetHashCode()
        {
            return this.dockingBehavior.GetHashCode();
        }

        public override bool IsDefaultAttribute()
        {
            return this.Equals(Default);
        }

        public System.Windows.Forms.DockingBehavior DockingBehavior
        {
            get
            {
                return this.dockingBehavior;
            }
        }
    }
}

