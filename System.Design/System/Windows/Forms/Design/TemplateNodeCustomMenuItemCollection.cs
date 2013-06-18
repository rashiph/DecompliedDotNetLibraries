namespace System.Windows.Forms.Design
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Design;
    using System.Drawing;
    using System.Windows.Forms;

    internal class TemplateNodeCustomMenuItemCollection : CustomMenuItemCollection
    {
        private ToolStripItem currentItem;
        private ToolStripMenuItem insertToolStripMenuItem;
        private IServiceProvider serviceProvider;

        public TemplateNodeCustomMenuItemCollection(IServiceProvider provider, Component currentItem)
        {
            this.serviceProvider = provider;
            this.currentItem = currentItem as ToolStripItem;
            this.PopulateList();
        }

        private void AddNewItemClick(object sender, EventArgs e)
        {
            ItemTypeToolStripMenuItem item = (ItemTypeToolStripMenuItem) sender;
            System.Type itemType = item.ItemType;
            this.InsertItem(itemType);
        }

        private void InsertItem(System.Type t)
        {
            this.InsertToolStripItem(t);
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
                ToolStripDesigner._autoAddNewItems = false;
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
                    PropertyDescriptor descriptor = TypeDescriptor.GetProperties(component)["Image"];
                    if ((descriptor != null) && (image != null))
                    {
                        descriptor.SetValue(component, image);
                    }
                    PropertyDescriptor descriptor2 = TypeDescriptor.GetProperties(component)["DisplayStyle"];
                    if (descriptor2 != null)
                    {
                        descriptor2.SetValue(component, ToolStripItemDisplayStyle.Image);
                    }
                    PropertyDescriptor descriptor3 = TypeDescriptor.GetProperties(component)["ImageTransparentColor"];
                    if (descriptor3 != null)
                    {
                        descriptor3.SetValue(component, Color.Magenta);
                    }
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
                ToolStripDesigner._autoAddNewItems = true;
                ToolStripDropDown down = parentTool as ToolStripDropDown;
                if ((down != null) && down.Visible)
                {
                    ToolStripDropDownItem ownerItem = down.OwnerItem as ToolStripDropDownItem;
                    if (ownerItem != null)
                    {
                        ToolStripMenuItemDesigner designer2 = host.GetDesigner(ownerItem) as ToolStripMenuItemDesigner;
                        if (designer2 != null)
                        {
                            designer2.ResetGlyphs(ownerItem);
                        }
                    }
                }
            }
        }

        private void PopulateList()
        {
            this.insertToolStripMenuItem = new ToolStripMenuItem();
            this.insertToolStripMenuItem.Text = System.Design.SR.GetString("ToolStripItemContextMenuInsert");
            this.insertToolStripMenuItem.DropDown = ToolStripDesignerUtils.GetNewItemDropDown(this.ParentTool, this.currentItem, new EventHandler(this.AddNewItemClick), false, this.serviceProvider);
            base.Add(this.insertToolStripMenuItem);
        }

        private ToolStrip ParentTool
        {
            get
            {
                return this.currentItem.Owner;
            }
        }
    }
}

