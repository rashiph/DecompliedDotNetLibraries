namespace System.ComponentModel.Design
{
    using System;
    using System.Collections;

    public class DesignerCommandSet
    {
        public virtual ICollection GetCommands(string name)
        {
            return null;
        }

        public DesignerActionListCollection ActionLists
        {
            get
            {
                return (DesignerActionListCollection) this.GetCommands("ActionLists");
            }
        }

        public DesignerVerbCollection Verbs
        {
            get
            {
                return (DesignerVerbCollection) this.GetCommands("Verbs");
            }
        }
    }
}

