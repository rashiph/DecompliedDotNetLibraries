namespace System.Windows.Forms.Design
{
    using System;
    using System.Design;
    using System.Windows.Forms;

    internal class RichTextBoxContextMenu : ContextMenu
    {
        private MenuItem copyMenu;
        private MenuItem cutMenu;
        private MenuItem deleteMenu;
        private RichTextBox parent;
        private MenuItem pasteMenu;
        private MenuItem selectAllMenu;
        private MenuItem undoMenu;

        public RichTextBoxContextMenu(RichTextBox parent)
        {
            this.undoMenu = new MenuItem(System.Design.SR.GetString("StandardMenuUndo"), new EventHandler(this.undoMenu_Clicked));
            this.cutMenu = new MenuItem(System.Design.SR.GetString("StandardMenuCut"), new EventHandler(this.cutMenu_Clicked));
            this.copyMenu = new MenuItem(System.Design.SR.GetString("StandardMenuCopy"), new EventHandler(this.copyMenu_Clicked));
            this.pasteMenu = new MenuItem(System.Design.SR.GetString("StandardMenuPaste"), new EventHandler(this.pasteMenu_Clicked));
            this.deleteMenu = new MenuItem(System.Design.SR.GetString("StandardMenuDelete"), new EventHandler(this.deleteMenu_Clicked));
            this.selectAllMenu = new MenuItem(System.Design.SR.GetString("StandardMenuSelectAll"), new EventHandler(this.selectAllMenu_Clicked));
            MenuItem item = new MenuItem("-");
            MenuItem item2 = new MenuItem("-");
            base.MenuItems.Add(this.undoMenu);
            base.MenuItems.Add(item);
            base.MenuItems.Add(this.cutMenu);
            base.MenuItems.Add(this.copyMenu);
            base.MenuItems.Add(this.pasteMenu);
            base.MenuItems.Add(this.deleteMenu);
            base.MenuItems.Add(item2);
            base.MenuItems.Add(this.selectAllMenu);
            this.parent = parent;
        }

        private void copyMenu_Clicked(object sender, EventArgs e)
        {
            Clipboard.SetText(this.parent.SelectedText);
        }

        private void cutMenu_Clicked(object sender, EventArgs e)
        {
            Clipboard.SetText(this.parent.SelectedText);
            this.parent.SelectedText = "";
        }

        private void deleteMenu_Clicked(object sender, EventArgs e)
        {
            this.parent.SelectedText = "";
        }

        protected override void OnPopup(EventArgs e)
        {
            if (this.parent.SelectionLength > 0)
            {
                this.cutMenu.Enabled = true;
                this.copyMenu.Enabled = true;
                this.deleteMenu.Enabled = true;
            }
            else
            {
                this.cutMenu.Enabled = false;
                this.copyMenu.Enabled = false;
                this.deleteMenu.Enabled = false;
            }
            if (Clipboard.GetText() != null)
            {
                this.pasteMenu.Enabled = true;
            }
            else
            {
                this.pasteMenu.Enabled = false;
            }
            if (this.parent.CanUndo)
            {
                this.undoMenu.Enabled = true;
            }
            else
            {
                this.undoMenu.Enabled = false;
            }
        }

        private void pasteMenu_Clicked(object sender, EventArgs e)
        {
            this.parent.SelectedText = Clipboard.GetText();
        }

        private void selectAllMenu_Clicked(object sender, EventArgs e)
        {
            this.parent.SelectAll();
        }

        private void undoMenu_Clicked(object sender, EventArgs e)
        {
            this.parent.Undo();
        }
    }
}

