namespace System.Workflow.Activities
{
    using System;
    using System.ComponentModel;
    using System.Drawing.Design;
    using System.Windows.Forms;
    using System.Windows.Forms.Design;
    using System.Workflow.ComponentModel;

    internal sealed class StateDropDownEditor : UITypeEditor
    {
        private ITypeDescriptorContext _context;
        private IWindowsFormsEditorService _editorService;
        private object _selectedObject;

        private void dataSourceDropDown_SelectedIndexChanged(object sender, EventArgs e)
        {
            this._editorService.CloseDropDown();
            this._selectedObject = null;
            ListBox box = sender as ListBox;
            if (box == null)
            {
                throw new ArgumentNullException("sender");
            }
            if (box.SelectedIndex >= 0)
            {
                this._selectedObject = box.Items[box.SelectedIndex];
            }
        }

        public override object EditValue(ITypeDescriptorContext typeDescriptorContext, IServiceProvider serviceProvider, object value)
        {
            if (typeDescriptorContext == null)
            {
                throw new ArgumentNullException("typeDescriptorContext");
            }
            if (serviceProvider == null)
            {
                throw new ArgumentNullException("serviceProvider");
            }
            this._editorService = (IWindowsFormsEditorService) serviceProvider.GetService(typeof(IWindowsFormsEditorService));
            this._context = typeDescriptorContext;
            ListBox dropDownList = new ListBox {
                BorderStyle = BorderStyle.None
            };
            Activity instance = this._context.Instance as Activity;
            if (instance == null)
            {
                object[] objArray = this._context.Instance as object[];
                if ((objArray != null) && (objArray.Length > 0))
                {
                    instance = (Activity) objArray[0];
                }
            }
            this.PopulateDropDownList(dropDownList, instance);
            dropDownList.SelectedIndexChanged += new EventHandler(this.dataSourceDropDown_SelectedIndexChanged);
            this._editorService.DropDownControl(dropDownList);
            if ((dropDownList.SelectedIndex != -1) && (this._selectedObject != null))
            {
                return this._selectedObject;
            }
            return value;
        }

        private void FindStates(ListBox dropDownList, StateActivity parent)
        {
            foreach (Activity activity in parent.EnabledActivities)
            {
                StateActivity state = activity as StateActivity;
                if (state != null)
                {
                    if (StateMachineHelpers.IsLeafState(state))
                    {
                        dropDownList.Items.Add(state.QualifiedName);
                    }
                    else
                    {
                        this.FindStates(dropDownList, state);
                    }
                }
            }
        }

        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext typeDescriptorContext)
        {
            return UITypeEditorEditStyle.DropDown;
        }

        private void PopulateDropDownList(ListBox dropDownList, Activity activity)
        {
            StateActivity state = StateMachineHelpers.FindEnclosingState(activity);
            if (state != null)
            {
                StateActivity rootState = StateMachineHelpers.GetRootState(state);
                this.FindStates(dropDownList, rootState);
            }
        }
    }
}

