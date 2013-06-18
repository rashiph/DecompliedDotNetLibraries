namespace System.Windows.Forms.Design
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Design;
    using System.Drawing;
    using System.Windows.Forms;

    internal class ToolStripItemCustomMenuItemCollection : CustomMenuItemCollection
    {
        private ToolStripMenuItem alignmentToolStripMenuItem;
        private ToolStripMenuItem checkedToolStripMenuItem;
        private ToolStripMenuItem convertToolStripMenuItem;
        private ToolStripItem currentItem;
        private ToolStripMenuItem displayStyleToolStripMenuItem;
        private ToolStripMenuItem editItemsToolStripMenuItem;
        private ToolStripMenuItem enabledToolStripMenuItem;
        private ToolStripMenuItem imageStyleToolStripMenuItem;
        private ToolStripMenuItem imageTextStyleToolStripMenuItem;
        private ToolStripMenuItem imageToolStripMenuItem;
        private ToolStripMenuItem insertToolStripMenuItem;
        private ToolStripMenuItem isLinkToolStripMenuItem;
        private ToolStripMenuItem leftToolStripMenuItem;
        private ToolStripMenuItem noneStyleToolStripMenuItem;
        private ToolStripMenuItem rightToolStripMenuItem;
        private IServiceProvider serviceProvider;
        private ToolStripMenuItem showShortcutKeysToolStripMenuItem;
        private ToolStripMenuItem springToolStripMenuItem;
        private ToolStripMenuItem textStyleToolStripMenuItem;
        private ToolStripSeparator toolStripSeparator1;
        private CollectionEditVerbManager verbManager;

        public ToolStripItemCustomMenuItemCollection(IServiceProvider provider, Component currentItem)
        {
            this.serviceProvider = provider;
            this.currentItem = currentItem as ToolStripItem;
            this.PopulateList();
        }

        private void AddNewItemClick(object sender, EventArgs e)
        {
            ItemTypeToolStripMenuItem item = (ItemTypeToolStripMenuItem) sender;
            System.Type itemType = item.ItemType;
            if (item.ConvertTo)
            {
                this.MorphToolStripItem(itemType);
            }
            else
            {
                this.InsertItem(itemType);
            }
        }

        protected void ChangeProperty(string propertyName, object value)
        {
            this.ChangeProperty(this.currentItem, propertyName, value);
        }

        protected void ChangeProperty(IComponent target, string propertyName, object value)
        {
            PropertyDescriptor descriptor = TypeDescriptor.GetProperties(target)[propertyName];
            try
            {
                if (descriptor != null)
                {
                    descriptor.SetValue(target, value);
                }
            }
            catch (InvalidOperationException exception)
            {
                ((IUIService) this.serviceProvider.GetService(typeof(IUIService))).ShowError(exception.Message);
            }
        }

        private ToolStripMenuItem CreateBooleanItem(string text, string propertyName)
        {
            ToolStripMenuItem item = new ToolStripMenuItem(text);
            bool flag = this.IsPropertyBrowsable(propertyName);
            item.Visible = flag;
            item.Tag = propertyName;
            item.CheckOnClick = true;
            item.Click += new EventHandler(this.OnBooleanValueChanged);
            return item;
        }

        private ToolStripMenuItem CreateEnumValueItem(string propertyName, string name, object value)
        {
            ToolStripMenuItem item = new ToolStripMenuItem(name) {
                Tag = new EnumValueDescription(propertyName, value)
            };
            item.Click += new EventHandler(this.OnEnumValueChanged);
            return item;
        }

        private ToolStripMenuItem CreatePropertyBasedItem(string text, string propertyName, string imageName)
        {
            ToolStripMenuItem item = new ToolStripMenuItem(text);
            bool flag = this.IsPropertyBrowsable(propertyName);
            item.Visible = flag;
            if (flag)
            {
                if (!string.IsNullOrEmpty(imageName))
                {
                    item.Image = new Bitmap(typeof(ToolStripMenuItem), imageName);
                    item.ImageTransparentColor = Color.Magenta;
                }
                IUIService service = this.serviceProvider.GetService(typeof(IUIService)) as IUIService;
                if (service != null)
                {
                    item.DropDown.Renderer = (ToolStripProfessionalRenderer) service.Styles["VsRenderer"];
                    item.DropDown.Font = (Font) service.Styles["DialogFont"];
                }
            }
            return item;
        }

        private object GetProperty(string propertyName)
        {
            PropertyDescriptor descriptor = TypeDescriptor.GetProperties(this.currentItem)[propertyName];
            if (descriptor != null)
            {
                return descriptor.GetValue(this.currentItem);
            }
            return null;
        }

        private void InsertIntoDropDown(ToolStripDropDown parent, System.Type t)
        {
            IDesignerHost host = (IDesignerHost) this.serviceProvider.GetService(typeof(IDesignerHost));
            int index = parent.Items.IndexOf(this.currentItem);
            if (parent != null)
            {
                ToolStripDropDownItem ownerItem = parent.OwnerItem as ToolStripDropDownItem;
                if ((ownerItem != null) && ((ownerItem.DropDownDirection == ToolStripDropDownDirection.AboveLeft) || (ownerItem.DropDownDirection == ToolStripDropDownDirection.AboveRight)))
                {
                    index++;
                }
            }
            IDesigner designer = null;
            DesignerTransaction transaction = host.CreateTransaction(System.Design.SR.GetString("ToolStripAddingItem"));
            try
            {
                IComponent component = host.CreateComponent(t);
                designer = host.GetDesigner(component);
                if (designer is ComponentDesigner)
                {
                    ((ComponentDesigner) designer).InitializeNewComponent(null);
                }
                parent.Items.Insert(index, (ToolStripItem) component);
                ISelectionService service = (ISelectionService) this.serviceProvider.GetService(typeof(ISelectionService));
                if (service != null)
                {
                    service.SetSelectedComponents(new object[] { component }, SelectionTypes.Replace);
                }
            }
            catch (Exception exception)
            {
                if (((parent != null) && (parent.OwnerItem != null)) && (parent.OwnerItem.Owner != null))
                {
                    ToolStripDesigner designer2 = host.GetDesigner(parent.OwnerItem.Owner) as ToolStripDesigner;
                    if (designer2 != null)
                    {
                        designer2.CancelPendingMenuItemTransaction();
                    }
                }
                this.TryCancelTransaction(ref transaction);
                if (System.Windows.Forms.ClientUtils.IsCriticalException(exception))
                {
                    throw;
                }
            }
            finally
            {
                if (transaction != null)
                {
                    transaction.Commit();
                    transaction = null;
                }
            }
        }

        private void InsertIntoMainMenu(MenuStrip parent, System.Type t)
        {
            IDesignerHost host = (IDesignerHost) this.serviceProvider.GetService(typeof(IDesignerHost));
            int index = parent.Items.IndexOf(this.currentItem);
            IDesigner designer = null;
            DesignerTransaction transaction = host.CreateTransaction(System.Design.SR.GetString("ToolStripAddingItem"));
            try
            {
                IComponent component = host.CreateComponent(t);
                designer = host.GetDesigner(component);
                if (designer is ComponentDesigner)
                {
                    ((ComponentDesigner) designer).InitializeNewComponent(null);
                }
                parent.Items.Insert(index, (ToolStripItem) component);
                ISelectionService service = (ISelectionService) this.serviceProvider.GetService(typeof(ISelectionService));
                if (service != null)
                {
                    service.SetSelectedComponents(new object[] { component }, SelectionTypes.Replace);
                }
            }
            catch (Exception exception)
            {
                this.TryCancelTransaction(ref transaction);
                if (System.Windows.Forms.ClientUtils.IsCriticalException(exception))
                {
                    throw;
                }
            }
            finally
            {
                if (transaction != null)
                {
                    transaction.Commit();
                    transaction = null;
                }
            }
        }

        private void InsertIntoStatusStrip(StatusStrip parent, System.Type t)
        {
            IDesignerHost host = (IDesignerHost) this.serviceProvider.GetService(typeof(IDesignerHost));
            int index = parent.Items.IndexOf(this.currentItem);
            IDesigner designer = null;
            DesignerTransaction transaction = host.CreateTransaction(System.Design.SR.GetString("ToolStripAddingItem"));
            try
            {
                IComponent component = host.CreateComponent(t);
                designer = host.GetDesigner(component);
                if (designer is ComponentDesigner)
                {
                    ((ComponentDesigner) designer).InitializeNewComponent(null);
                }
                parent.Items.Insert(index, (ToolStripItem) component);
                ISelectionService service = (ISelectionService) this.serviceProvider.GetService(typeof(ISelectionService));
                if (service != null)
                {
                    service.SetSelectedComponents(new object[] { component }, SelectionTypes.Replace);
                }
            }
            catch (Exception exception)
            {
                this.TryCancelTransaction(ref transaction);
                if (System.Windows.Forms.ClientUtils.IsCriticalException(exception))
                {
                    throw;
                }
            }
            finally
            {
                if (transaction != null)
                {
                    transaction.Commit();
                    transaction = null;
                }
            }
        }

        private void InsertItem(System.Type t)
        {
            if (this.currentItem is ToolStripMenuItem)
            {
                this.InsertMenuItem(t);
            }
            else
            {
                this.InsertStripItem(t);
            }
        }

        private void InsertMenuItem(System.Type t)
        {
            MenuStrip parentTool = this.ParentTool as MenuStrip;
            if (parentTool != null)
            {
                this.InsertIntoMainMenu(parentTool, t);
            }
            else
            {
                this.InsertIntoDropDown((ToolStripDropDown) this.currentItem.Owner, t);
            }
        }

        private void InsertStripItem(System.Type t)
        {
            StatusStrip parentTool = this.ParentTool as StatusStrip;
            if (parentTool != null)
            {
                this.InsertIntoStatusStrip(parentTool, t);
            }
            else
            {
                this.InsertToolStripItem(t);
            }
        }

        private void InsertToolStripItem(System.Type t)
        {
            IDesignerHost host = (IDesignerHost) this.serviceProvider.GetService(typeof(IDesignerHost));
            ToolStrip parentTool = this.ParentTool;
            int index = parentTool.Items.IndexOf(this.currentItem);
            IDesigner designer = null;
            DesignerTransaction transaction = host.CreateTransaction(System.Design.SR.GetString("ToolStripAddingItem"));
            try
            {
                IComponent component = host.CreateComponent(t);
                designer = host.GetDesigner(component);
                if (designer is ComponentDesigner)
                {
                    ((ComponentDesigner) designer).InitializeNewComponent(null);
                }
                if (((component is ToolStripButton) || (component is ToolStripSplitButton)) || (component is ToolStripDropDownButton))
                {
                    Image image = null;
                    try
                    {
                        image = new Bitmap(typeof(ToolStripButton), "blank.bmp");
                    }
                    catch (Exception exception)
                    {
                        if (System.Windows.Forms.ClientUtils.IsCriticalException(exception))
                        {
                            throw;
                        }
                    }
                    this.ChangeProperty(component, "Image", image);
                    this.ChangeProperty(component, "DisplayStyle", ToolStripItemDisplayStyle.Image);
                    this.ChangeProperty(component, "ImageTransparentColor", Color.Magenta);
                }
                parentTool.Items.Insert(index, (ToolStripItem) component);
                ISelectionService service = (ISelectionService) this.serviceProvider.GetService(typeof(ISelectionService));
                if (service != null)
                {
                    service.SetSelectedComponents(new object[] { component }, SelectionTypes.Replace);
                }
            }
            catch (Exception exception2)
            {
                if (transaction != null)
                {
                    transaction.Cancel();
                    transaction = null;
                }
                if (System.Windows.Forms.ClientUtils.IsCriticalException(exception2))
                {
                    throw;
                }
            }
            finally
            {
                if (transaction != null)
                {
                    transaction.Commit();
                    transaction = null;
                }
            }
        }

        private bool IsPropertyBrowsable(string propertyName)
        {
            PropertyDescriptor descriptor = TypeDescriptor.GetProperties(this.currentItem)[propertyName];
            if (descriptor != null)
            {
                BrowsableAttribute attribute = descriptor.Attributes[typeof(BrowsableAttribute)] as BrowsableAttribute;
                if (attribute != null)
                {
                    return attribute.Browsable;
                }
            }
            return true;
        }

        private void MorphToolStripItem(System.Type t)
        {
            if (t != this.currentItem.GetType())
            {
                IDesignerHost service = (IDesignerHost) this.serviceProvider.GetService(typeof(IDesignerHost));
                ((ToolStripItemDesigner) service.GetDesigner(this.currentItem)).MorphCurrentItem(t);
            }
        }

        private void OnBooleanValueChanged(object sender, EventArgs e)
        {
            ToolStripItem item = sender as ToolStripItem;
            if (item != null)
            {
                string tag = item.Tag as string;
                if (tag != null)
                {
                    bool property = (bool) this.GetProperty(tag);
                    this.ChangeProperty(tag, !property);
                }
            }
        }

        private void OnEditItemsMenuItemClick(object sender, EventArgs e)
        {
            if (this.verbManager != null)
            {
                this.verbManager.EditItemsVerb.Invoke();
            }
        }

        private void OnEnumValueChanged(object sender, EventArgs e)
        {
            ToolStripItem item = sender as ToolStripItem;
            if (item != null)
            {
                EnumValueDescription tag = item.Tag as EnumValueDescription;
                if ((tag != null) && !string.IsNullOrEmpty(tag.PropertyName))
                {
                    this.ChangeProperty(tag.PropertyName, tag.Value);
                }
            }
        }

        private void OnImageToolStripMenuItemClick(object sender, EventArgs e)
        {
            IDesignerHost service = (IDesignerHost) this.serviceProvider.GetService(typeof(IDesignerHost));
            if (service != null)
            {
                ToolStripItemDesigner designer = service.GetDesigner(this.currentItem) as ToolStripItemDesigner;
                if (designer != null)
                {
                    try
                    {
                        EditorServiceContext.EditValue(designer, this.currentItem, "Image");
                    }
                    catch (InvalidOperationException exception)
                    {
                        ((IUIService) this.serviceProvider.GetService(typeof(IUIService))).ShowError(exception.Message);
                    }
                }
            }
        }

        private void PopulateList()
        {
            ToolStripItem currentItem = this.currentItem;
            if (!(currentItem is ToolStripControlHost) && !(currentItem is ToolStripSeparator))
            {
                this.imageToolStripMenuItem = new ToolStripMenuItem();
                this.imageToolStripMenuItem.Text = System.Design.SR.GetString("ToolStripItemContextMenuSetImage");
                this.imageToolStripMenuItem.Image = new Bitmap(typeof(ToolStripMenuItem), "image.bmp");
                this.imageToolStripMenuItem.ImageTransparentColor = Color.Magenta;
                this.imageToolStripMenuItem.Click += new EventHandler(this.OnImageToolStripMenuItemClick);
                this.enabledToolStripMenuItem = this.CreateBooleanItem("E&nabled", "Enabled");
                base.AddRange(new ToolStripItem[] { this.imageToolStripMenuItem, this.enabledToolStripMenuItem });
                if (currentItem is ToolStripMenuItem)
                {
                    this.checkedToolStripMenuItem = this.CreateBooleanItem("C&hecked", "Checked");
                    this.showShortcutKeysToolStripMenuItem = this.CreateBooleanItem("ShowShortcut&Keys", "ShowShortcutKeys");
                    base.AddRange(new ToolStripItem[] { this.checkedToolStripMenuItem, this.showShortcutKeysToolStripMenuItem });
                }
                else
                {
                    if (currentItem is ToolStripLabel)
                    {
                        this.isLinkToolStripMenuItem = this.CreateBooleanItem("IsLin&k", "IsLink");
                        base.Add(this.isLinkToolStripMenuItem);
                    }
                    if (currentItem is ToolStripStatusLabel)
                    {
                        this.springToolStripMenuItem = this.CreateBooleanItem("Sprin&g", "Spring");
                        base.Add(this.springToolStripMenuItem);
                    }
                    this.leftToolStripMenuItem = this.CreateEnumValueItem("Alignment", "Left", ToolStripItemAlignment.Left);
                    this.rightToolStripMenuItem = this.CreateEnumValueItem("Alignment", "Right", ToolStripItemAlignment.Right);
                    this.noneStyleToolStripMenuItem = this.CreateEnumValueItem("DisplayStyle", "None", ToolStripItemDisplayStyle.None);
                    this.textStyleToolStripMenuItem = this.CreateEnumValueItem("DisplayStyle", "Text", ToolStripItemDisplayStyle.Text);
                    this.imageStyleToolStripMenuItem = this.CreateEnumValueItem("DisplayStyle", "Image", ToolStripItemDisplayStyle.Image);
                    this.imageTextStyleToolStripMenuItem = this.CreateEnumValueItem("DisplayStyle", "ImageAndText", ToolStripItemDisplayStyle.ImageAndText);
                    this.alignmentToolStripMenuItem = this.CreatePropertyBasedItem("Ali&gnment", "Alignment", "alignment.bmp");
                    this.alignmentToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { this.leftToolStripMenuItem, this.rightToolStripMenuItem });
                    this.displayStyleToolStripMenuItem = this.CreatePropertyBasedItem("Displa&yStyle", "DisplayStyle", "displaystyle.bmp");
                    this.displayStyleToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { this.noneStyleToolStripMenuItem, this.textStyleToolStripMenuItem, this.imageStyleToolStripMenuItem, this.imageTextStyleToolStripMenuItem });
                    base.AddRange(new ToolStripItem[] { this.alignmentToolStripMenuItem, this.displayStyleToolStripMenuItem });
                }
                this.toolStripSeparator1 = new ToolStripSeparator();
                base.Add(this.toolStripSeparator1);
            }
            this.convertToolStripMenuItem = new ToolStripMenuItem();
            this.convertToolStripMenuItem.Text = System.Design.SR.GetString("ToolStripItemContextMenuConvertTo");
            this.convertToolStripMenuItem.DropDown = ToolStripDesignerUtils.GetNewItemDropDown(this.ParentTool, this.currentItem, new EventHandler(this.AddNewItemClick), true, this.serviceProvider);
            this.insertToolStripMenuItem = new ToolStripMenuItem();
            this.insertToolStripMenuItem.Text = System.Design.SR.GetString("ToolStripItemContextMenuInsert");
            this.insertToolStripMenuItem.DropDown = ToolStripDesignerUtils.GetNewItemDropDown(this.ParentTool, this.currentItem, new EventHandler(this.AddNewItemClick), false, this.serviceProvider);
            base.AddRange(new ToolStripItem[] { this.convertToolStripMenuItem, this.insertToolStripMenuItem });
            if (this.currentItem is ToolStripDropDownItem)
            {
                IDesignerHost service = (IDesignerHost) this.serviceProvider.GetService(typeof(IDesignerHost));
                if (service != null)
                {
                    ToolStripItemDesigner designer = service.GetDesigner(this.currentItem) as ToolStripItemDesigner;
                    if (designer != null)
                    {
                        this.verbManager = new CollectionEditVerbManager(System.Design.SR.GetString("ToolStripDropDownItemCollectionEditorVerb"), designer, TypeDescriptor.GetProperties(this.currentItem)["DropDownItems"], false);
                        this.editItemsToolStripMenuItem = new ToolStripMenuItem();
                        this.editItemsToolStripMenuItem.Text = System.Design.SR.GetString("ToolStripDropDownItemCollectionEditorVerb");
                        this.editItemsToolStripMenuItem.Click += new EventHandler(this.OnEditItemsMenuItemClick);
                        this.editItemsToolStripMenuItem.Image = new Bitmap(typeof(ToolStripMenuItem), "editdropdownlist.bmp");
                        this.editItemsToolStripMenuItem.ImageTransparentColor = Color.Magenta;
                        base.Add(this.editItemsToolStripMenuItem);
                    }
                }
            }
        }

        private void RefreshAlignment()
        {
            ToolStripItemAlignment property = (ToolStripItemAlignment) this.GetProperty("Alignment");
            this.leftToolStripMenuItem.Checked = property == ToolStripItemAlignment.Left;
            this.rightToolStripMenuItem.Checked = property == ToolStripItemAlignment.Right;
        }

        private void RefreshDisplayStyle()
        {
            ToolStripItemDisplayStyle property = (ToolStripItemDisplayStyle) this.GetProperty("DisplayStyle");
            this.noneStyleToolStripMenuItem.Checked = property == ToolStripItemDisplayStyle.None;
            this.textStyleToolStripMenuItem.Checked = property == ToolStripItemDisplayStyle.Text;
            this.imageStyleToolStripMenuItem.Checked = property == ToolStripItemDisplayStyle.Image;
            this.imageTextStyleToolStripMenuItem.Checked = property == ToolStripItemDisplayStyle.ImageAndText;
        }

        public override void RefreshItems()
        {
            base.RefreshItems();
            ToolStripItem currentItem = this.currentItem;
            if (!(currentItem is ToolStripControlHost) && !(currentItem is ToolStripSeparator))
            {
                this.enabledToolStripMenuItem.Checked = (bool) this.GetProperty("Enabled");
                if (currentItem is ToolStripMenuItem)
                {
                    this.checkedToolStripMenuItem.Checked = (bool) this.GetProperty("Checked");
                    this.showShortcutKeysToolStripMenuItem.Checked = (bool) this.GetProperty("ShowShortcutKeys");
                }
                else
                {
                    if (currentItem is ToolStripLabel)
                    {
                        this.isLinkToolStripMenuItem.Checked = (bool) this.GetProperty("IsLink");
                    }
                    this.RefreshAlignment();
                    this.RefreshDisplayStyle();
                }
            }
        }

        private void TryCancelTransaction(ref DesignerTransaction transaction)
        {
            if (transaction != null)
            {
                try
                {
                    transaction.Cancel();
                    transaction = null;
                }
                catch
                {
                }
            }
        }

        private ToolStrip ParentTool
        {
            get
            {
                return this.currentItem.Owner;
            }
        }

        private class EnumValueDescription
        {
            public string PropertyName;
            public object Value;

            public EnumValueDescription(string propertyName, object value)
            {
                this.PropertyName = propertyName;
                this.Value = value;
            }
        }
    }
}

