namespace System.Workflow.ComponentModel
{
    using System;
    using System.Collections;
    using System.Collections.Generic;

    internal sealed class BindRecursionContext
    {
        private Hashtable activityBinds = new Hashtable();

        public void Add(Activity activity, ActivityBind bind)
        {
            if (activity == null)
            {
                throw new ArgumentNullException("activity");
            }
            if (bind == null)
            {
                throw new ArgumentNullException("bind");
            }
            if (this.activityBinds[activity] == null)
            {
                this.activityBinds[activity] = new List<ActivityBind>();
            }
            ((List<ActivityBind>) this.activityBinds[activity]).Add(bind);
        }

        public bool Contains(Activity activity, ActivityBind bind)
        {
            if (activity == null)
            {
                throw new ArgumentNullException("activity");
            }
            if (bind == null)
            {
                throw new ArgumentNullException("bind");
            }
            if (this.activityBinds[activity] != null)
            {
                List<ActivityBind> list = this.activityBinds[activity] as List<ActivityBind>;
                foreach (ActivityBind bind2 in list)
                {
                    if (bind2.Path == bind.Path)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}

