namespace System.Workflow.ComponentModel.Design
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel.Design;
    using System.Runtime;
    using System.Security.Permissions;

    [PermissionSet(SecurityAction.InheritanceDemand, Name="FullTrust"), PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")]
    public sealed class ActivityDesignerVerbCollection : DesignerVerbCollection
    {
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public ActivityDesignerVerbCollection()
        {
        }

        public ActivityDesignerVerbCollection(IEnumerable<ActivityDesignerVerb> verbs)
        {
            if (verbs == null)
            {
                throw new ArgumentNullException("verbs");
            }
            foreach (ActivityDesignerVerb verb in verbs)
            {
                base.Add(verb);
            }
        }

        private int ConvertGroupToId(DesignerVerbGroup group)
        {
            if (group == DesignerVerbGroup.General)
            {
                return WorkflowMenuCommands.VerbGroupGeneral;
            }
            if (group == DesignerVerbGroup.View)
            {
                return WorkflowMenuCommands.VerbGroupView;
            }
            if (group == DesignerVerbGroup.Edit)
            {
                return WorkflowMenuCommands.VerbGroupEdit;
            }
            if (group == DesignerVerbGroup.Options)
            {
                return WorkflowMenuCommands.VerbGroupOptions;
            }
            if (group == DesignerVerbGroup.Actions)
            {
                return WorkflowMenuCommands.VerbGroupActions;
            }
            return WorkflowMenuCommands.VerbGroupMisc;
        }

        private void OnDummyVerb(object sender, EventArgs e)
        {
        }

        protected override void OnValidate(object value)
        {
            if (!(value is ActivityDesignerVerb))
            {
                throw new InvalidOperationException(SR.GetString("Error_InvalidDesignerVerbValue"));
            }
        }

        internal ActivityDesignerVerbCollection SafeCollection
        {
            get
            {
                if (base.Count == 0)
                {
                    return this;
                }
                Dictionary<DesignerVerbGroup, List<ActivityDesignerVerb>> dictionary = new Dictionary<DesignerVerbGroup, List<ActivityDesignerVerb>>();
                ArrayList list = new ArrayList(this);
                foreach (ActivityDesignerVerb verb in list)
                {
                    List<ActivityDesignerVerb> list2 = null;
                    if (!dictionary.ContainsKey(verb.Group))
                    {
                        list2 = new List<ActivityDesignerVerb>();
                        dictionary.Add(verb.Group, list2);
                    }
                    else
                    {
                        list2 = dictionary[verb.Group];
                    }
                    if (!list2.Contains(verb))
                    {
                        verb.Id = this.ConvertGroupToId(verb.Group) + list2.Count;
                        list2.Add(verb);
                    }
                }
                list.Sort(new ActivityDesignerVerbComparer());
                if (((ActivityDesignerVerb) list[0]).Id != StandardCommands.VerbFirst.ID)
                {
                    list.Insert(0, new ActivityDesignerVerb(null, DesignerVerbGroup.General, "Dummy", new EventHandler(this.OnDummyVerb)));
                    ((ActivityDesignerVerb) list[0]).Visible = false;
                }
                ActivityDesignerVerbCollection verbs = new ActivityDesignerVerbCollection();
                foreach (ActivityDesignerVerb verb2 in list)
                {
                    verbs.Add(verb2);
                }
                return verbs;
            }
        }

        private class ActivityDesignerVerbComparer : IComparer
        {
            public int Compare(object x, object y)
            {
                ActivityDesignerVerb verb = x as ActivityDesignerVerb;
                ActivityDesignerVerb verb2 = y as ActivityDesignerVerb;
                if (verb.Id == verb2.Id)
                {
                    return 0;
                }
                if (verb.Id > verb2.Id)
                {
                    return 1;
                }
                return -1;
            }
        }
    }
}

