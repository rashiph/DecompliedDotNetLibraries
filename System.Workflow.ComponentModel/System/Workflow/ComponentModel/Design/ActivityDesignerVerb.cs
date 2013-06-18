namespace System.Workflow.ComponentModel.Design
{
    using System;
    using System.ComponentModel.Design;
    using System.Runtime;

    public class ActivityDesignerVerb : DesignerVerb
    {
        private System.Workflow.ComponentModel.Design.ActivityDesigner activityDesigner;
        private int id;
        private EventHandler invokeHandler;
        private EventHandler statusHandler;
        private DesignerVerbGroup verbGroup;

        public ActivityDesignerVerb(System.Workflow.ComponentModel.Design.ActivityDesigner activityDesigner, DesignerVerbGroup verbGroup, string text, EventHandler invokeHandler) : base(text, new EventHandler(ActivityDesignerVerb.OnExecuteDesignerVerb), new System.ComponentModel.Design.CommandID(WorkflowMenuCommands.MenuGuid, 0))
        {
            if ((text == null) || (text.Length == 0))
            {
                throw new ArgumentNullException("text");
            }
            if (invokeHandler == null)
            {
                throw new ArgumentNullException("invokeHandler");
            }
            this.verbGroup = verbGroup;
            this.invokeHandler = invokeHandler;
            this.activityDesigner = activityDesigner;
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public ActivityDesignerVerb(System.Workflow.ComponentModel.Design.ActivityDesigner activityDesigner, DesignerVerbGroup verbGroup, string text, EventHandler invokeHandler, EventHandler statusHandler) : this(activityDesigner, verbGroup, text, invokeHandler)
        {
            this.statusHandler = statusHandler;
        }

        private static void OnExecuteDesignerVerb(object sender, EventArgs e)
        {
            ActivityDesignerVerb verb = sender as ActivityDesignerVerb;
            if (verb != null)
            {
                if (verb.invokeHandler != null)
                {
                    verb.invokeHandler(sender, e);
                }
                int oleStatus = verb.OleStatus;
                if (verb.activityDesigner != null)
                {
                    foreach (DesignerVerb verb2 in ((IDesigner) verb.activityDesigner).Verbs)
                    {
                        if (verb2 is ActivityDesignerVerb)
                        {
                            int num2 = verb2.OleStatus;
                        }
                    }
                }
            }
        }

        internal System.Workflow.ComponentModel.Design.ActivityDesigner ActivityDesigner
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.activityDesigner;
            }
        }

        public override System.ComponentModel.Design.CommandID CommandID
        {
            get
            {
                return new System.ComponentModel.Design.CommandID(WorkflowMenuCommands.MenuGuid, this.id);
            }
        }

        public DesignerVerbGroup Group
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.verbGroup;
            }
        }

        internal int Id
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.id;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.id = value;
            }
        }

        public override int OleStatus
        {
            get
            {
                if (this.statusHandler != null)
                {
                    try
                    {
                        this.statusHandler(this, EventArgs.Empty);
                    }
                    catch
                    {
                    }
                }
                return base.OleStatus;
            }
        }
    }
}

