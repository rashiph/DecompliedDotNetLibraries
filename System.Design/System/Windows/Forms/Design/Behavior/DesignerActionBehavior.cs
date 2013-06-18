namespace System.Windows.Forms.Design.Behavior
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Design;
    using System.Drawing;
    using System.Windows.Forms;
    using System.Windows.Forms.Design;

    internal sealed class DesignerActionBehavior : System.Windows.Forms.Design.Behavior.Behavior
    {
        private DesignerActionListCollection actionLists;
        private bool ignoreNextMouseUp;
        private DesignerActionUI parentUI;
        private IComponent relatedComponent;
        private IServiceProvider serviceProvider;

        internal DesignerActionBehavior(IServiceProvider serviceProvider, IComponent relatedComponent, DesignerActionListCollection actionLists, DesignerActionUI parentUI)
        {
            this.actionLists = actionLists;
            this.serviceProvider = serviceProvider;
            this.relatedComponent = relatedComponent;
            this.parentUI = parentUI;
        }

        internal DesignerActionPanel CreateDesignerActionPanel(IComponent relatedComponent)
        {
            DesignerActionListCollection actionLists = new DesignerActionListCollection();
            actionLists.AddRange(this.ActionLists);
            DesignerActionPanel panel = new DesignerActionPanel(this.serviceProvider);
            panel.UpdateTasks(actionLists, new DesignerActionListCollection(), System.Design.SR.GetString("DesignerActionPanel_DefaultPanelTitle", new object[] { relatedComponent.GetType().Name }), null);
            return panel;
        }

        internal void HideUI()
        {
            this.ParentUI.HideDesignerActionPanel();
        }

        public override bool OnMouseDoubleClick(Glyph g, MouseButtons button, Point mouseLoc)
        {
            this.ignoreNextMouseUp = true;
            return true;
        }

        public override bool OnMouseDown(Glyph g, MouseButtons button, Point mouseLoc)
        {
            return !this.ParentUI.IsDesignerActionPanelVisible;
        }

        public override bool OnMouseUp(Glyph g, MouseButtons button)
        {
            if ((button != MouseButtons.Left) || (this.ParentUI == null))
            {
                return true;
            }
            bool flag = true;
            if (this.ParentUI.IsDesignerActionPanelVisible)
            {
                this.HideUI();
            }
            else if (!this.ignoreNextMouseUp)
            {
                if (this.serviceProvider != null)
                {
                    ISelectionService service = (ISelectionService) this.serviceProvider.GetService(typeof(ISelectionService));
                    if ((service != null) && (service.PrimarySelection != this.RelatedComponent))
                    {
                        List<IComponent> components = new List<IComponent> {
                            this.RelatedComponent
                        };
                        service.SetSelectedComponents(components, SelectionTypes.Click);
                    }
                }
                this.ShowUI(g);
            }
            else
            {
                flag = false;
            }
            this.ignoreNextMouseUp = false;
            return flag;
        }

        internal void ShowUI(Glyph g)
        {
            DesignerActionGlyph glyph = g as DesignerActionGlyph;
            if (glyph != null)
            {
                DesignerActionPanel panel = this.CreateDesignerActionPanel(this.RelatedComponent);
                this.ParentUI.ShowDesignerActionPanel(this.RelatedComponent, panel, glyph);
            }
        }

        internal DesignerActionListCollection ActionLists
        {
            get
            {
                return this.actionLists;
            }
            set
            {
                this.actionLists = value;
            }
        }

        internal bool IgnoreNextMouseUp
        {
            set
            {
                this.ignoreNextMouseUp = value;
            }
        }

        internal DesignerActionUI ParentUI
        {
            get
            {
                return this.parentUI;
            }
        }

        internal IComponent RelatedComponent
        {
            get
            {
                return this.relatedComponent;
            }
        }
    }
}

