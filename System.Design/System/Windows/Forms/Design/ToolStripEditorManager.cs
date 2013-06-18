namespace System.Windows.Forms.Design
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Drawing;
    using System.Windows.Forms;
    using System.Windows.Forms.Design.Behavior;

    internal class ToolStripEditorManager
    {
        private BehaviorService behaviorService;
        private IComponent comp;
        private ToolStripItem currentItem;
        private IDesignerHost designerHost;
        private ToolStripEditorControl editor;
        private ToolStripTemplateNode editorUI;
        private ToolStripItemDesigner itemDesigner;
        private Rectangle lastKnownEditorBounds = Rectangle.Empty;

        public ToolStripEditorManager(IComponent comp)
        {
            this.comp = comp;
            this.behaviorService = (BehaviorService) comp.Site.GetService(typeof(BehaviorService));
            this.designerHost = (IDesignerHost) comp.Site.GetService(typeof(IDesignerHost));
        }

        internal void ActivateEditor(ToolStripItem item, bool clicked)
        {
            if (item != this.currentItem)
            {
                if (this.editor != null)
                {
                    this.behaviorService.AdornerWindowControl.Controls.Remove(this.editor);
                    this.behaviorService.Invalidate(this.editor.Bounds);
                    this.editorUI = null;
                    this.editor = null;
                    this.currentItem = null;
                    this.itemDesigner.IsEditorActive = false;
                    if (this.currentItem != null)
                    {
                        this.currentItem = null;
                    }
                }
                if (item != null)
                {
                    this.currentItem = item;
                    if (this.designerHost != null)
                    {
                        this.itemDesigner = (ToolStripItemDesigner) this.designerHost.GetDesigner(this.currentItem);
                    }
                    this.editorUI = this.itemDesigner.Editor;
                    if (this.editorUI != null)
                    {
                        this.itemDesigner.IsEditorActive = true;
                        this.editor = new ToolStripEditorControl(this.editorUI.EditorToolStrip, this.editorUI.Bounds);
                        this.behaviorService.AdornerWindowControl.Controls.Add(this.editor);
                        this.lastKnownEditorBounds = this.editor.Bounds;
                        this.editor.BringToFront();
                        this.editorUI.ignoreFirstKeyUp = true;
                        this.editorUI.FocusEditor(this.currentItem);
                    }
                }
            }
        }

        internal void CloseManager()
        {
        }

        private void OnEditorResize(object sender, EventArgs e)
        {
            this.behaviorService.Invalidate(this.lastKnownEditorBounds);
            if (this.editor != null)
            {
                this.lastKnownEditorBounds = this.editor.Bounds;
            }
        }

        private class ToolStripEditorControl : Panel
        {
            private Rectangle bounds;
            private Control wrappedEditor;

            public ToolStripEditorControl(Control editorToolStrip, Rectangle bounds)
            {
                this.wrappedEditor = editorToolStrip;
                this.bounds = bounds;
                this.wrappedEditor.Resize += new EventHandler(this.OnWrappedEditorResize);
                base.Controls.Add(editorToolStrip);
                base.Location = new Point(bounds.X, bounds.Y);
                this.Text = "InSituEditorWrapper";
                this.UpdateSize();
            }

            private void OnWrappedEditorResize(object sender, EventArgs e)
            {
            }

            private void UpdateSize()
            {
                base.Size = new Size(this.wrappedEditor.Size.Width, this.wrappedEditor.Size.Height);
            }
        }
    }
}

