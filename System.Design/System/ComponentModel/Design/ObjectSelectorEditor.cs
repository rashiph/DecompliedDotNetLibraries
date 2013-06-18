namespace System.ComponentModel.Design
{
    using System;
    using System.ComponentModel;
    using System.Design;
    using System.Drawing.Design;
    using System.Runtime.InteropServices;
    using System.Windows.Forms;
    using System.Windows.Forms.Design;

    public abstract class ObjectSelectorEditor : UITypeEditor
    {
        protected object currValue;
        protected object prevValue;
        private Selector selector;
        public bool SubObjectSelector;

        public ObjectSelectorEditor()
        {
        }

        public ObjectSelectorEditor(bool subObjectSelector)
        {
            this.SubObjectSelector = subObjectSelector;
        }

        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            if (provider != null)
            {
                IWindowsFormsEditorService edSvc = (IWindowsFormsEditorService) provider.GetService(typeof(IWindowsFormsEditorService));
                if (edSvc == null)
                {
                    return value;
                }
                if (this.selector == null)
                {
                    this.selector = new Selector(this);
                    DesignerUtils.ApplyTreeViewThemeStyles(this.selector);
                }
                this.prevValue = value;
                this.currValue = value;
                this.FillTreeWithData(this.selector, context, provider);
                this.selector.Start(edSvc, value);
                edSvc.DropDownControl(this.selector);
                this.selector.Stop();
                if (this.prevValue != this.currValue)
                {
                    value = this.currValue;
                }
            }
            return value;
        }

        public bool EqualsToValue(object value)
        {
            return (value == this.currValue);
        }

        protected virtual void FillTreeWithData(Selector selector, ITypeDescriptorContext context, IServiceProvider provider)
        {
            selector.Clear();
        }

        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.DropDown;
        }

        public virtual void SetValue(object value)
        {
            this.currValue = value;
        }

        public class Selector : TreeView
        {
            public bool clickSeen;
            private ObjectSelectorEditor editor;
            private IWindowsFormsEditorService edSvc;

            public Selector(ObjectSelectorEditor editor)
            {
                this.CreateHandle();
                this.editor = editor;
                base.BorderStyle = BorderStyle.None;
                base.FullRowSelect = !editor.SubObjectSelector;
                base.Scrollable = true;
                base.CheckBoxes = false;
                base.ShowPlusMinus = editor.SubObjectSelector;
                base.ShowLines = editor.SubObjectSelector;
                base.ShowRootLines = editor.SubObjectSelector;
                base.AfterSelect += new TreeViewEventHandler(this.OnAfterSelect);
            }

            public ObjectSelectorEditor.SelectorNode AddNode(string label, object value, ObjectSelectorEditor.SelectorNode parent)
            {
                ObjectSelectorEditor.SelectorNode node = new ObjectSelectorEditor.SelectorNode(label, value);
                if (parent != null)
                {
                    parent.Nodes.Add(node);
                    return node;
                }
                base.Nodes.Add(node);
                return node;
            }

            private bool ChooseSelectedNodeIfEqual()
            {
                if ((this.editor != null) && (this.edSvc != null))
                {
                    this.editor.SetValue(((ObjectSelectorEditor.SelectorNode) base.SelectedNode).value);
                    if (this.editor.EqualsToValue(((ObjectSelectorEditor.SelectorNode) base.SelectedNode).value))
                    {
                        this.edSvc.CloseDropDown();
                        return true;
                    }
                }
                return false;
            }

            public void Clear()
            {
                this.clickSeen = false;
                base.Nodes.Clear();
            }

            protected void OnAfterSelect(object sender, TreeViewEventArgs e)
            {
                if (this.clickSeen)
                {
                    this.ChooseSelectedNodeIfEqual();
                    this.clickSeen = false;
                }
            }

            protected override void OnKeyDown(KeyEventArgs e)
            {
                switch (e.KeyCode)
                {
                    case Keys.Enter:
                        if (this.ChooseSelectedNodeIfEqual())
                        {
                            e.Handled = true;
                        }
                        break;

                    case Keys.Escape:
                        this.editor.SetValue(this.editor.prevValue);
                        e.Handled = true;
                        this.edSvc.CloseDropDown();
                        break;
                }
                base.OnKeyDown(e);
            }

            protected override void OnKeyPress(KeyPressEventArgs e)
            {
                if (e.KeyChar == '\r')
                {
                    e.Handled = true;
                }
                base.OnKeyPress(e);
            }

            protected override void OnNodeMouseClick(TreeNodeMouseClickEventArgs e)
            {
                if (e.Node == base.SelectedNode)
                {
                    this.ChooseSelectedNodeIfEqual();
                }
                base.OnNodeMouseClick(e);
            }

            public bool SetSelection(object value, TreeNodeCollection nodes)
            {
                TreeNode[] nodeArray;
                if (nodes == null)
                {
                    nodeArray = new TreeNode[base.Nodes.Count];
                    base.Nodes.CopyTo(nodeArray, 0);
                }
                else
                {
                    nodeArray = new TreeNode[nodes.Count];
                    nodes.CopyTo(nodeArray, 0);
                }
                int length = nodeArray.Length;
                if (length != 0)
                {
                    for (int i = 0; i < length; i++)
                    {
                        if (((ObjectSelectorEditor.SelectorNode) nodeArray[i]).value == value)
                        {
                            base.SelectedNode = nodeArray[i];
                            return true;
                        }
                        if ((nodeArray[i].Nodes != null) && (nodeArray[i].Nodes.Count != 0))
                        {
                            nodeArray[i].Expand();
                            if (this.SetSelection(value, nodeArray[i].Nodes))
                            {
                                return true;
                            }
                            nodeArray[i].Collapse();
                        }
                    }
                }
                return false;
            }

            public void Start(IWindowsFormsEditorService edSvc, object value)
            {
                this.edSvc = edSvc;
                this.clickSeen = false;
                this.SetSelection(value, base.Nodes);
            }

            public void Stop()
            {
                this.edSvc = null;
            }

            protected override void WndProc(ref Message m)
            {
                switch (m.Msg)
                {
                    case 0x87:
                        m.Result = (IntPtr) (((long) m.Result) | 4L);
                        return;

                    case 0x200:
                        if (this.clickSeen)
                        {
                            this.clickSeen = false;
                        }
                        break;

                    case 0x204e:
                    {
                        System.Design.NativeMethods.NMTREEVIEW nmtreeview = (System.Design.NativeMethods.NMTREEVIEW) Marshal.PtrToStructure(m.LParam, typeof(System.Design.NativeMethods.NMTREEVIEW));
                        if (nmtreeview.nmhdr.code == -2)
                        {
                            this.clickSeen = true;
                        }
                        break;
                    }
                }
                base.WndProc(ref m);
            }
        }

        public class SelectorNode : TreeNode
        {
            public object value;

            public SelectorNode(string label, object value) : base(label)
            {
                this.value = value;
            }
        }
    }
}

