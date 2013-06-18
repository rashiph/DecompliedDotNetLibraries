namespace System.Windows.Forms.Design
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Drawing;
    using System.Windows.Forms;

    internal class StatusCommandUI
    {
        private IMenuCommandService menuService;
        private IServiceProvider serviceProvider;
        private MenuCommand statusRectCommand;

        public StatusCommandUI(IServiceProvider provider)
        {
            this.serviceProvider = provider;
        }

        public void SetStatusInformation(Component selectedComponent)
        {
            if (selectedComponent != null)
            {
                Rectangle empty = Rectangle.Empty;
                Control control = selectedComponent as Control;
                if (control != null)
                {
                    empty = control.Bounds;
                }
                else
                {
                    PropertyDescriptor descriptor = TypeDescriptor.GetProperties(selectedComponent)["Bounds"];
                    if ((descriptor != null) && typeof(Rectangle).IsAssignableFrom(descriptor.PropertyType))
                    {
                        empty = (Rectangle) descriptor.GetValue(selectedComponent);
                    }
                }
                if (this.StatusRectCommand != null)
                {
                    this.StatusRectCommand.Invoke(empty);
                }
            }
        }

        public void SetStatusInformation(Rectangle bounds)
        {
            if (this.StatusRectCommand != null)
            {
                this.StatusRectCommand.Invoke(bounds);
            }
        }

        public void SetStatusInformation(Component selectedComponent, Point location)
        {
            if (selectedComponent != null)
            {
                Rectangle empty = Rectangle.Empty;
                Control control = selectedComponent as Control;
                if (control != null)
                {
                    empty = control.Bounds;
                }
                else
                {
                    PropertyDescriptor descriptor = TypeDescriptor.GetProperties(selectedComponent)["Bounds"];
                    if ((descriptor != null) && typeof(Rectangle).IsAssignableFrom(descriptor.PropertyType))
                    {
                        empty = (Rectangle) descriptor.GetValue(selectedComponent);
                    }
                }
                if (location != Point.Empty)
                {
                    empty.X = location.X;
                    empty.Y = location.Y;
                }
                if (this.StatusRectCommand != null)
                {
                    this.StatusRectCommand.Invoke(empty);
                }
            }
        }

        private IMenuCommandService MenuService
        {
            get
            {
                if (this.menuService == null)
                {
                    this.menuService = (IMenuCommandService) this.serviceProvider.GetService(typeof(IMenuCommandService));
                }
                return this.menuService;
            }
        }

        private MenuCommand StatusRectCommand
        {
            get
            {
                if ((this.statusRectCommand == null) && (this.MenuService != null))
                {
                    this.statusRectCommand = this.MenuService.FindCommand(MenuCommands.SetStatusRectangle);
                }
                return this.statusRectCommand;
            }
        }
    }
}

