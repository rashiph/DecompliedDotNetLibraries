namespace System.Web.UI.Design
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.Design;

    public class WebFormsDesignerActionService : DesignerActionService
    {
        public WebFormsDesignerActionService(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        protected override void GetComponentDesignerActions(IComponent component, DesignerActionListCollection actionLists)
        {
            if (component == null)
            {
                throw new ArgumentNullException("component");
            }
            if (actionLists == null)
            {
                throw new ArgumentNullException("actionLists");
            }
            IServiceContainer site = component.Site as IServiceContainer;
            if (site != null)
            {
                DesignerCommandSet service = (DesignerCommandSet) site.GetService(typeof(DesignerCommandSet));
                if (service != null)
                {
                    DesignerActionListCollection lists = service.ActionLists;
                    if (lists != null)
                    {
                        actionLists.AddRange(lists);
                    }
                }
                if ((actionLists.Count == 0) || ((actionLists.Count == 1) && (actionLists[0] is ControlDesigner.ControlDesignerActionList)))
                {
                    DesignerVerbCollection verbs = service.Verbs;
                    if ((verbs != null) && (verbs.Count != 0))
                    {
                        DesignerVerb[] array = new DesignerVerb[verbs.Count];
                        verbs.CopyTo(array, 0);
                        actionLists.Add(new DesignerActionVerbList(array));
                    }
                }
            }
        }
    }
}

