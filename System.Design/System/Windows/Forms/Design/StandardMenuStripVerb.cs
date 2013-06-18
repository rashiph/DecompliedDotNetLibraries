namespace System.Windows.Forms.Design
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.ComponentModel.Design.Serialization;
    using System.Design;
    using System.Drawing;
    using System.Globalization;
    using System.Text;
    using System.Windows.Forms;
    using System.Windows.Forms.Design.Behavior;

    internal class StandardMenuStripVerb
    {
        private ToolStripDesigner _designer;
        private IDesignerHost _host;
        private IServiceProvider _provider;
        private IComponentChangeService componentChangeSvc;

        internal StandardMenuStripVerb(string text, ToolStripDesigner designer)
        {
            this._designer = designer;
            this._provider = designer.Component.Site;
            this._host = (IDesignerHost) this._provider.GetService(typeof(IDesignerHost));
            this.componentChangeSvc = (IComponentChangeService) this._provider.GetService(typeof(IComponentChangeService));
            if (text == null)
            {
                text = System.Design.SR.GetString("ToolStripDesignerStandardItemsVerb");
            }
        }

        private void CreateStandardMenuStrip(IDesignerHost host, MenuStrip tool)
        {
            string[][] strArray = new string[][] { new string[] { System.Design.SR.GetString("StandardMenuFile"), System.Design.SR.GetString("StandardMenuNew"), System.Design.SR.GetString("StandardMenuOpen"), "-", System.Design.SR.GetString("StandardMenuSave"), System.Design.SR.GetString("StandardMenuSaveAs"), "-", System.Design.SR.GetString("StandardMenuPrint"), System.Design.SR.GetString("StandardMenuPrintPreview"), "-", System.Design.SR.GetString("StandardMenuExit") }, new string[] { System.Design.SR.GetString("StandardMenuEdit"), System.Design.SR.GetString("StandardMenuUndo"), System.Design.SR.GetString("StandardMenuRedo"), "-", System.Design.SR.GetString("StandardMenuCut"), System.Design.SR.GetString("StandardMenuCopy"), System.Design.SR.GetString("StandardMenuPaste"), "-", System.Design.SR.GetString("StandardMenuSelectAll") }, new string[] { System.Design.SR.GetString("StandardMenuTools"), System.Design.SR.GetString("StandardMenuCustomize"), System.Design.SR.GetString("StandardMenuOptions") }, new string[] { System.Design.SR.GetString("StandardMenuHelp"), System.Design.SR.GetString("StandardMenuContents"), System.Design.SR.GetString("StandardMenuIndex"), System.Design.SR.GetString("StandardMenuSearch"), "-", System.Design.SR.GetString("StandardMenuAbout") } };
            string[][] strArray2 = new string[][] { new string[] { "", "new", "open", "-", "save", "", "-", "print", "printPreview", "-", "" }, new string[] { "", "", "", "-", "cut", "copy", "paste", "-", "" }, new string[] { "", "", "" }, new string[] { "", "", "", "", "-", "" } };
            Keys[][] keysArray2 = new Keys[4][];
            Keys[] keysArray3 = new Keys[11];
            keysArray3[1] = Keys.Control | Keys.N;
            keysArray3[2] = Keys.Control | Keys.O;
            keysArray3[4] = Keys.Control | Keys.S;
            keysArray3[7] = Keys.Control | Keys.P;
            keysArray2[0] = keysArray3;
            Keys[] keysArray4 = new Keys[9];
            keysArray4[1] = Keys.Control | Keys.Z;
            keysArray4[2] = Keys.Control | Keys.Y;
            keysArray4[4] = Keys.Control | Keys.X;
            keysArray4[5] = Keys.Control | Keys.C;
            keysArray4[6] = Keys.Control | Keys.V;
            keysArray2[1] = keysArray4;
            keysArray2[2] = new Keys[3];
            keysArray2[3] = new Keys[6];
            Keys[][] keysArray = keysArray2;
            if (host != null)
            {
                tool.SuspendLayout();
                ToolStripDesigner._autoAddNewItems = false;
                DesignerTransaction transaction = this._host.CreateTransaction(System.Design.SR.GetString("StandardMenuCreateDesc"));
                try
                {
                    INameCreationService nameCreationService = (INameCreationService) this._provider.GetService(typeof(INameCreationService));
                    string str = "standardMainMenuStrip";
                    string name = str;
                    int num = 1;
                    if (host != null)
                    {
                        while (this._host.Container.Components[name] != null)
                        {
                            name = str + num++.ToString(CultureInfo.InvariantCulture);
                        }
                    }
                    for (int i = 0; i < strArray.Length; i++)
                    {
                        string[] strArray3 = strArray[i];
                        ToolStripMenuItem component = null;
                        for (int j = 0; j < strArray3.Length; j++)
                        {
                            name = null;
                            string text = strArray3[j];
                            name = this.NameFromText(text, typeof(ToolStripMenuItem), nameCreationService, true);
                            ToolStripItem item2 = null;
                            if (name.Contains("Separator"))
                            {
                                item2 = (ToolStripSeparator) this._host.CreateComponent(typeof(ToolStripSeparator), name);
                                IDesigner designer = this._host.GetDesigner(item2);
                                if (designer is ComponentDesigner)
                                {
                                    ((ComponentDesigner) designer).InitializeNewComponent(null);
                                }
                                item2.Text = text;
                            }
                            else
                            {
                                item2 = (ToolStripMenuItem) this._host.CreateComponent(typeof(ToolStripMenuItem), name);
                                IDesigner designer2 = this._host.GetDesigner(item2);
                                if (designer2 is ComponentDesigner)
                                {
                                    ((ComponentDesigner) designer2).InitializeNewComponent(null);
                                }
                                item2.Text = text;
                                Keys shortcut = keysArray[i][j];
                                if (((item2 is ToolStripMenuItem) && (shortcut != Keys.None)) && (!ToolStripManager.IsShortcutDefined(shortcut) && ToolStripManager.IsValidShortcut(shortcut)))
                                {
                                    ((ToolStripMenuItem) item2).ShortcutKeys = shortcut;
                                }
                                Bitmap image = null;
                                try
                                {
                                    image = this.GetImage(strArray2[i][j]);
                                }
                                catch
                                {
                                }
                                if (image != null)
                                {
                                    PropertyDescriptor descriptor = TypeDescriptor.GetProperties(item2)["Image"];
                                    if (descriptor != null)
                                    {
                                        descriptor.SetValue(item2, image);
                                    }
                                    item2.ImageTransparentColor = Color.Magenta;
                                }
                            }
                            if (j == 0)
                            {
                                component = (ToolStripMenuItem) item2;
                                component.DropDown.SuspendLayout();
                            }
                            else
                            {
                                component.DropDownItems.Add(item2);
                            }
                            if (j == (strArray3.Length - 1))
                            {
                                MemberDescriptor member = TypeDescriptor.GetProperties(component)["DropDownItems"];
                                this.componentChangeSvc.OnComponentChanging(component, member);
                                this.componentChangeSvc.OnComponentChanged(component, member, null, null);
                            }
                        }
                        component.DropDown.ResumeLayout(false);
                        tool.Items.Add(component);
                        if (i == (strArray.Length - 1))
                        {
                            MemberDescriptor descriptor3 = TypeDescriptor.GetProperties(tool)["Items"];
                            this.componentChangeSvc.OnComponentChanging(tool, descriptor3);
                            this.componentChangeSvc.OnComponentChanged(tool, descriptor3, null, null);
                        }
                    }
                }
                catch (Exception exception)
                {
                    if (exception is InvalidOperationException)
                    {
                        ((IUIService) this._provider.GetService(typeof(IUIService))).ShowError(exception.Message);
                    }
                    if (transaction != null)
                    {
                        transaction.Cancel();
                        transaction = null;
                    }
                }
                finally
                {
                    ToolStripDesigner._autoAddNewItems = true;
                    if (transaction != null)
                    {
                        transaction.Commit();
                        transaction = null;
                    }
                    tool.ResumeLayout();
                    ISelectionService service = (ISelectionService) this._provider.GetService(typeof(ISelectionService));
                    if (service != null)
                    {
                        service.SetSelectedComponents(new object[] { this._designer.Component });
                    }
                    DesignerActionUIService service4 = (DesignerActionUIService) this._provider.GetService(typeof(DesignerActionUIService));
                    if (service4 != null)
                    {
                        service4.Refresh(this._designer.Component);
                    }
                    ((SelectionManager) this._provider.GetService(typeof(SelectionManager))).Refresh();
                }
            }
        }

        private void CreateStandardToolStrip(IDesignerHost host, ToolStrip tool)
        {
            string[] strArray = new string[] { System.Design.SR.GetString("StandardMenuNew"), System.Design.SR.GetString("StandardMenuOpen"), System.Design.SR.GetString("StandardMenuSave"), System.Design.SR.GetString("StandardMenuPrint"), "-", System.Design.SR.GetString("StandardToolCut"), System.Design.SR.GetString("StandardMenuCopy"), System.Design.SR.GetString("StandardMenuPaste"), "-", System.Design.SR.GetString("StandardToolHelp") };
            string[] strArray2 = new string[] { "new", "open", "save", "print", "-", "cut", "copy", "paste", "-", "help" };
            if (host != null)
            {
                tool.SuspendLayout();
                ToolStripDesigner._autoAddNewItems = false;
                DesignerTransaction transaction = this._host.CreateTransaction(System.Design.SR.GetString("StandardMenuCreateDesc"));
                try
                {
                    INameCreationService nameCreationService = (INameCreationService) this._provider.GetService(typeof(INameCreationService));
                    string str = "standardMainToolStrip";
                    string name = str;
                    int num = 1;
                    if (host != null)
                    {
                        while (this._host.Container.Components[name] != null)
                        {
                            name = str + num++.ToString(CultureInfo.InvariantCulture);
                        }
                    }
                    int index = 0;
                    foreach (string str3 in strArray)
                    {
                        name = null;
                        str = "ToolStripButton";
                        name = this.NameFromText(str3, typeof(ToolStripButton), nameCreationService, true);
                        ToolStripItem component = null;
                        if (name.Contains("Separator"))
                        {
                            component = (ToolStripSeparator) this._host.CreateComponent(typeof(ToolStripSeparator), name);
                            IDesigner designer = this._host.GetDesigner(component);
                            if (designer is ComponentDesigner)
                            {
                                ((ComponentDesigner) designer).InitializeNewComponent(null);
                            }
                        }
                        else
                        {
                            component = (ToolStripButton) this._host.CreateComponent(typeof(ToolStripButton), name);
                            IDesigner designer2 = this._host.GetDesigner(component);
                            if (designer2 is ComponentDesigner)
                            {
                                ((ComponentDesigner) designer2).InitializeNewComponent(null);
                            }
                            PropertyDescriptor descriptor = TypeDescriptor.GetProperties(component)["DisplayStyle"];
                            if (descriptor != null)
                            {
                                descriptor.SetValue(component, ToolStripItemDisplayStyle.Image);
                            }
                            PropertyDescriptor descriptor2 = TypeDescriptor.GetProperties(component)["Text"];
                            if (descriptor2 != null)
                            {
                                descriptor2.SetValue(component, str3);
                            }
                            Bitmap image = null;
                            try
                            {
                                image = this.GetImage(strArray2[index]);
                            }
                            catch
                            {
                            }
                            if (image != null)
                            {
                                PropertyDescriptor descriptor3 = TypeDescriptor.GetProperties(component)["Image"];
                                if (descriptor3 != null)
                                {
                                    descriptor3.SetValue(component, image);
                                }
                                component.ImageTransparentColor = Color.Magenta;
                            }
                        }
                        tool.Items.Add(component);
                        index++;
                    }
                    MemberDescriptor member = TypeDescriptor.GetProperties(tool)["Items"];
                    this.componentChangeSvc.OnComponentChanging(tool, member);
                    this.componentChangeSvc.OnComponentChanged(tool, member, null, null);
                }
                catch (Exception exception)
                {
                    if (exception is InvalidOperationException)
                    {
                        ((IUIService) this._provider.GetService(typeof(IUIService))).ShowError(exception.Message);
                    }
                    if (transaction != null)
                    {
                        transaction.Cancel();
                        transaction = null;
                    }
                }
                finally
                {
                    ToolStripDesigner._autoAddNewItems = true;
                    if (transaction != null)
                    {
                        transaction.Commit();
                        transaction = null;
                    }
                    tool.ResumeLayout();
                    ISelectionService service = (ISelectionService) this._provider.GetService(typeof(ISelectionService));
                    if (service != null)
                    {
                        service.SetSelectedComponents(new object[] { this._designer.Component });
                    }
                    DesignerActionUIService service4 = (DesignerActionUIService) this._provider.GetService(typeof(DesignerActionUIService));
                    if (service4 != null)
                    {
                        service4.Refresh(this._designer.Component);
                    }
                    ((SelectionManager) this._provider.GetService(typeof(SelectionManager))).Refresh();
                }
            }
        }

        private Bitmap GetImage(string name)
        {
            Bitmap bitmap = null;
            if (name.StartsWith("new"))
            {
                return new Bitmap(typeof(ToolStripMenuItem), "new.bmp");
            }
            if (name.StartsWith("open"))
            {
                return new Bitmap(typeof(ToolStripMenuItem), "open.bmp");
            }
            if (name.StartsWith("save"))
            {
                return new Bitmap(typeof(ToolStripMenuItem), "save.bmp");
            }
            if (name.StartsWith("printPreview"))
            {
                return new Bitmap(typeof(ToolStripMenuItem), "printPreview.bmp");
            }
            if (name.StartsWith("print"))
            {
                return new Bitmap(typeof(ToolStripMenuItem), "print.bmp");
            }
            if (name.StartsWith("cut"))
            {
                return new Bitmap(typeof(ToolStripMenuItem), "cut.bmp");
            }
            if (name.StartsWith("copy"))
            {
                return new Bitmap(typeof(ToolStripMenuItem), "copy.bmp");
            }
            if (name.StartsWith("paste"))
            {
                return new Bitmap(typeof(ToolStripMenuItem), "paste.bmp");
            }
            if (name.StartsWith("help"))
            {
                bitmap = new Bitmap(typeof(ToolStripMenuItem), "help.bmp");
            }
            return bitmap;
        }

        public void InsertItems()
        {
            DesignerActionUIService service = (DesignerActionUIService) this._host.GetService(typeof(DesignerActionUIService));
            if (service != null)
            {
                service.HideUI(this._designer.Component);
            }
            Cursor current = Cursor.Current;
            try
            {
                Cursor.Current = Cursors.WaitCursor;
                if (this._designer.Component is MenuStrip)
                {
                    this.CreateStandardMenuStrip(this._host, (MenuStrip) this._designer.Component);
                }
                else
                {
                    this.CreateStandardToolStrip(this._host, (ToolStrip) this._designer.Component);
                }
            }
            finally
            {
                Cursor.Current = current;
            }
        }

        private string NameFromText(string text, System.Type itemType, INameCreationService nameCreationService, bool adjustCapitalization)
        {
            string name = null;
            if (text == "-")
            {
                name = "toolStripSeparator";
            }
            else
            {
                string str2 = itemType.Name;
                StringBuilder builder = new StringBuilder(text.Length + str2.Length);
                bool flag = false;
                for (int j = 0; j < text.Length; j++)
                {
                    char c = text[j];
                    if (char.IsLetterOrDigit(c))
                    {
                        if (!flag)
                        {
                            c = char.ToLower(c, CultureInfo.CurrentCulture);
                            flag = true;
                        }
                        builder.Append(c);
                    }
                }
                builder.Append(str2);
                name = builder.ToString();
                if (adjustCapitalization)
                {
                    string str3 = ToolStripDesigner.NameFromText(null, typeof(ToolStripMenuItem), this._designer.Component.Site);
                    if (!string.IsNullOrEmpty(str3) && char.IsUpper(str3[0]))
                    {
                        name = char.ToUpper(name[0], CultureInfo.InvariantCulture) + name.Substring(1);
                    }
                }
            }
            if (this._host.Container.Components[name] == null)
            {
                if (!nameCreationService.IsValidName(name))
                {
                    return nameCreationService.CreateName(this._host.Container, itemType);
                }
                return name;
            }
            string str4 = name;
            for (int i = 1; !nameCreationService.IsValidName(str4); i++)
            {
                str4 = name + i.ToString(CultureInfo.InvariantCulture);
            }
            return str4;
        }
    }
}

