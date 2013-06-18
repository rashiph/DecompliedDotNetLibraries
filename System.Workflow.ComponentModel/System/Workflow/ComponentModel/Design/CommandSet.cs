namespace System.Workflow.ComponentModel.Design
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Drawing.Design;
    using System.Drawing.Imaging;
    using System.Drawing.Printing;
    using System.IO;
    using System.Windows.Forms;
    using System.Windows.Forms.Design;
    using System.Workflow.ComponentModel;

    internal sealed class CommandSet : IDisposable
    {
        private WorkflowDesignerMessageFilter activeFilter;
        private const string CF_DESIGNER = "CF_WINOEDESIGNERCOMPONENTS";
        private const string CF_DESIGNERSTATE = "CF_WINOEDESIGNERCOMPONENTSSTATE";
        private List<System.Workflow.ComponentModel.Design.CommandSetItem> commandSet;
        private System.Workflow.ComponentModel.Design.CommandSetItem[] layoutCommands;
        private IMenuCommandService menuCommandService;
        internal static CommandID[] NavigationToolCommandIds = new CommandID[] { WorkflowMenuCommands.ZoomIn, WorkflowMenuCommands.ZoomOut, WorkflowMenuCommands.Pan, WorkflowMenuCommands.DefaultFilter };
        private System.Workflow.ComponentModel.Design.CommandSetItem[] navigationToolCommands;
        private ISelectionService selectionService;
        private IServiceProvider serviceProvider;
        private WorkflowView workflowView;
        private System.Workflow.ComponentModel.Design.CommandSetItem[] zoomCommands;

        public CommandSet(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
            this.menuCommandService = (IMenuCommandService) this.serviceProvider.GetService(typeof(IMenuCommandService));
            if (this.menuCommandService == null)
            {
                throw new InvalidOperationException(SR.GetString("General_MissingService", new object[] { typeof(IMenuCommandService).FullName }));
            }
            this.workflowView = serviceProvider.GetService(typeof(WorkflowView)) as WorkflowView;
            if (this.workflowView == null)
            {
                throw new InvalidOperationException(SR.GetString("General_MissingService", new object[] { typeof(WorkflowView).FullName }));
            }
            this.selectionService = (ISelectionService) this.serviceProvider.GetService(typeof(ISelectionService));
            if (this.selectionService == null)
            {
                throw new InvalidOperationException(SR.GetString("General_MissingService", new object[] { typeof(ISelectionService).FullName }));
            }
            this.commandSet = new List<System.Workflow.ComponentModel.Design.CommandSetItem>();
            this.commandSet.AddRange(new System.Workflow.ComponentModel.Design.CommandSetItem[] { 
                new System.Workflow.ComponentModel.Design.CommandSetItem(new EventHandler(this.OnStatusAlways), new EventHandler(this.OnMenuSaveWorkflowAsImage), WorkflowMenuCommands.SaveAsImage), new System.Workflow.ComponentModel.Design.CommandSetItem(new EventHandler(this.OnStatusAlways), new EventHandler(this.OnMenuCopyToClipboard), WorkflowMenuCommands.CopyToClipboard), new System.Workflow.ComponentModel.Design.CommandSetItem(new EventHandler(this.OnStatusPrint), new EventHandler(this.OnMenuPrint), WorkflowMenuCommands.Print), new System.Workflow.ComponentModel.Design.CommandSetItem(new EventHandler(this.OnStatusPageSetup), new EventHandler(this.OnMenuPageSetup), WorkflowMenuCommands.PageSetup), new System.Workflow.ComponentModel.Design.CommandSetItem(new EventHandler(this.OnStatusDelete), new EventHandler(this.OnMenuDelete), StandardCommands.Delete), new System.Workflow.ComponentModel.Design.CommandSetItem(new EventHandler(this.OnStatusCopy), new EventHandler(this.OnMenuCopy), StandardCommands.Copy), new System.Workflow.ComponentModel.Design.CommandSetItem(new EventHandler(this.OnStatusCut), new EventHandler(this.OnMenuCut), StandardCommands.Cut), new System.Workflow.ComponentModel.Design.CommandSetItem(new EventHandler(this.OnStatusPaste), new EventHandler(this.OnMenuPaste), StandardCommands.Paste, true), new System.Workflow.ComponentModel.Design.CommandSetItem(new EventHandler(this.OnStatusAlways), new EventHandler(this.OnMenuSelectAll), StandardCommands.SelectAll), new System.Workflow.ComponentModel.Design.CommandSetItem(new EventHandler(this.OnStatusAlways), new EventHandler(this.OnMenuDesignerProperties), WorkflowMenuCommands.DesignerProperties), new System.Workflow.ComponentModel.Design.CommandSetItem(new EventHandler(this.OnStatusAlways), new EventHandler(this.OnViewCode), new CommandID(StandardCommands.Cut.Guid, 0x14d)), new System.Workflow.ComponentModel.Design.CommandSetItem(new EventHandler(this.OnStatusAlways), new EventHandler(this.OnKeyCancel), MenuCommands.KeyCancel), new System.Workflow.ComponentModel.Design.CommandSetItem(new EventHandler(this.OnStatusAlways), new EventHandler(this.OnKeyCancel), MenuCommands.KeyReverseCancel), new System.Workflow.ComponentModel.Design.CommandSetItem(new EventHandler(this.OnStatusAlways), new EventHandler(this.OnKeyMove), MenuCommands.KeyMoveUp), new System.Workflow.ComponentModel.Design.CommandSetItem(new EventHandler(this.OnStatusAlways), new EventHandler(this.OnKeyMove), MenuCommands.KeyMoveDown), new System.Workflow.ComponentModel.Design.CommandSetItem(new EventHandler(this.OnStatusAlways), new EventHandler(this.OnKeyMove), MenuCommands.KeyMoveLeft), 
                new System.Workflow.ComponentModel.Design.CommandSetItem(new EventHandler(this.OnStatusAlways), new EventHandler(this.OnKeyMove), MenuCommands.KeyMoveRight), new System.Workflow.ComponentModel.Design.CommandSetItem(new EventHandler(this.OnStatusAlways), new EventHandler(this.OnKeyMove), MenuCommands.KeySelectNext), new System.Workflow.ComponentModel.Design.CommandSetItem(new EventHandler(this.OnStatusAlways), new EventHandler(this.OnKeyMove), MenuCommands.KeySelectPrevious), new System.Workflow.ComponentModel.Design.CommandSetItem(new EventHandler(this.OnStatusExpandCollapse), new EventHandler(this.OnExpandCollapse), WorkflowMenuCommands.Expand), new System.Workflow.ComponentModel.Design.CommandSetItem(new EventHandler(this.OnStatusExpandCollapse), new EventHandler(this.OnExpandCollapse), WorkflowMenuCommands.Collapse), new System.Workflow.ComponentModel.Design.CommandSetItem(new EventHandler(this.OnStatusEnable), new EventHandler(this.OnEnable), WorkflowMenuCommands.Disable, true), new System.Workflow.ComponentModel.Design.CommandSetItem(new EventHandler(this.OnStatusEnable), new EventHandler(this.OnEnable), WorkflowMenuCommands.Enable, true), new System.Workflow.ComponentModel.Design.CommandSetItem(new EventHandler(this.OnStatusAlways), new EventHandler(this.OnCreateTheme), WorkflowMenuCommands.CreateTheme), new System.Workflow.ComponentModel.Design.CommandSetItem(new EventHandler(this.OnStatusAlways), new EventHandler(this.OnChangeTheme), WorkflowMenuCommands.ChangeTheme), new System.Workflow.ComponentModel.Design.CommandSetItem(new EventHandler(this.OnStatusAnySelection), new EventHandler(this.OnKeyDefault), MenuCommands.KeyDefaultAction), new System.Workflow.ComponentModel.Design.CommandSetItem(new EventHandler(this.OnStatusAlways), new EventHandler(this.OnKeyPageDnUp), WorkflowMenuCommands.PageUp), new System.Workflow.ComponentModel.Design.CommandSetItem(new EventHandler(this.OnStatusAlways), new EventHandler(this.OnKeyPageDnUp), WorkflowMenuCommands.PageDown)
             });
            this.zoomCommands = new System.Workflow.ComponentModel.Design.CommandSetItem[] { new System.Workflow.ComponentModel.Design.CommandSetItem(new EventHandler(this.OnStatusZoom), new EventHandler(this.OnZoom), WorkflowMenuCommands.Zoom400Mode, DR.GetString("Zoom400Mode", new object[0])), new System.Workflow.ComponentModel.Design.CommandSetItem(new EventHandler(this.OnStatusZoom), new EventHandler(this.OnZoom), WorkflowMenuCommands.Zoom300Mode, DR.GetString("Zoom300Mode", new object[0])), new System.Workflow.ComponentModel.Design.CommandSetItem(new EventHandler(this.OnStatusZoom), new EventHandler(this.OnZoom), WorkflowMenuCommands.Zoom200Mode, DR.GetString("Zoom200Mode", new object[0])), new System.Workflow.ComponentModel.Design.CommandSetItem(new EventHandler(this.OnStatusZoom), new EventHandler(this.OnZoom), WorkflowMenuCommands.Zoom150Mode, DR.GetString("Zoom150Mode", new object[0])), new System.Workflow.ComponentModel.Design.CommandSetItem(new EventHandler(this.OnStatusZoom), new EventHandler(this.OnZoom), WorkflowMenuCommands.Zoom100Mode, DR.GetString("Zoom100Mode", new object[0])), new System.Workflow.ComponentModel.Design.CommandSetItem(new EventHandler(this.OnStatusZoom), new EventHandler(this.OnZoom), WorkflowMenuCommands.Zoom75Mode, DR.GetString("Zoom75Mode", new object[0])), new System.Workflow.ComponentModel.Design.CommandSetItem(new EventHandler(this.OnStatusZoom), new EventHandler(this.OnZoom), WorkflowMenuCommands.Zoom50Mode, DR.GetString("Zoom50Mode", new object[0])), new System.Workflow.ComponentModel.Design.CommandSetItem(new EventHandler(this.OnStatusZoom), new EventHandler(this.OnZoom), WorkflowMenuCommands.ShowAll, DR.GetString("ZoomShowAll", new object[0])) };
            this.commandSet.AddRange(this.zoomCommands);
            this.layoutCommands = new System.Workflow.ComponentModel.Design.CommandSetItem[] { new System.Workflow.ComponentModel.Design.CommandSetItem(new EventHandler(this.OnStatusLayout), new EventHandler(this.OnPageLayout), WorkflowMenuCommands.DefaultPage), new System.Workflow.ComponentModel.Design.CommandSetItem(new EventHandler(this.OnStatusLayout), new EventHandler(this.OnPageLayout), WorkflowMenuCommands.PrintPreviewPage), new System.Workflow.ComponentModel.Design.CommandSetItem(new EventHandler(this.OnStatusLayout), new EventHandler(this.OnPageLayout), WorkflowMenuCommands.PrintPreview) };
            this.commandSet.AddRange(this.layoutCommands);
            this.navigationToolCommands = new System.Workflow.ComponentModel.Design.CommandSetItem[] { new System.Workflow.ComponentModel.Design.CommandSetItem(new EventHandler(this.OnStatusMessageFilter), new EventHandler(this.OnMessageFilterChanged), NavigationToolCommandIds[0]), new System.Workflow.ComponentModel.Design.CommandSetItem(new EventHandler(this.OnStatusMessageFilter), new EventHandler(this.OnMessageFilterChanged), NavigationToolCommandIds[1]), new System.Workflow.ComponentModel.Design.CommandSetItem(new EventHandler(this.OnStatusMessageFilter), new EventHandler(this.OnMessageFilterChanged), NavigationToolCommandIds[2]), new System.Workflow.ComponentModel.Design.CommandSetItem(new EventHandler(this.OnStatusMessageFilter), new EventHandler(this.OnMessageFilterChanged), NavigationToolCommandIds[3]) };
            this.commandSet.AddRange(this.navigationToolCommands);
            for (int i = 0; i < this.commandSet.Count; i++)
            {
                if (this.menuCommandService.FindCommand(this.commandSet[i].CommandID) == null)
                {
                    this.menuCommandService.AddCommand(this.commandSet[i]);
                }
            }
            IComponentChangeService service = this.serviceProvider.GetService(typeof(IComponentChangeService)) as IComponentChangeService;
            if (service != null)
            {
                service.ComponentChanged += new ComponentChangedEventHandler(this.OnComponentChanged);
            }
            IDictionaryService service2 = this.serviceProvider.GetService(typeof(IDictionaryService)) as IDictionaryService;
            if (service2 != null)
            {
                service2.SetValue(typeof(CommandID), new CommandID(new Guid("5f1c3c8d-60f1-4b98-b85b-8679f97e8eac"), 0));
            }
        }

        private CommandID ConvertMessageFilterToCommandID()
        {
            if (this.activeFilter is PanningMessageFilter)
            {
                return WorkflowMenuCommands.Pan;
            }
            if (!(this.activeFilter is ZoomingMessageFilter))
            {
                return WorkflowMenuCommands.DefaultFilter;
            }
            if (((ZoomingMessageFilter) this.activeFilter).ZoomingIn)
            {
                return WorkflowMenuCommands.ZoomIn;
            }
            return WorkflowMenuCommands.ZoomOut;
        }

        private int ConvertToZoomCommand(int zoomLevel)
        {
            int iD = 0;
            if (zoomLevel == 400)
            {
                return WorkflowMenuCommands.Zoom400Mode.ID;
            }
            if (zoomLevel == 300)
            {
                return WorkflowMenuCommands.Zoom300Mode.ID;
            }
            if (zoomLevel == 200)
            {
                return WorkflowMenuCommands.Zoom200Mode.ID;
            }
            if (zoomLevel == 150)
            {
                return WorkflowMenuCommands.Zoom150Mode.ID;
            }
            if (zoomLevel == 100)
            {
                return WorkflowMenuCommands.Zoom100Mode.ID;
            }
            if (zoomLevel == 0x4b)
            {
                return WorkflowMenuCommands.Zoom75Mode.ID;
            }
            if (zoomLevel == 50)
            {
                iD = WorkflowMenuCommands.Zoom50Mode.ID;
            }
            return iD;
        }

        private int ConvertToZoomLevel(int commandId)
        {
            int num = 100;
            if (commandId == WorkflowMenuCommands.Zoom400Mode.ID)
            {
                return 400;
            }
            if (commandId == WorkflowMenuCommands.Zoom300Mode.ID)
            {
                return 300;
            }
            if (commandId == WorkflowMenuCommands.Zoom200Mode.ID)
            {
                return 200;
            }
            if (commandId == WorkflowMenuCommands.Zoom150Mode.ID)
            {
                return 150;
            }
            if (commandId == WorkflowMenuCommands.Zoom100Mode.ID)
            {
                return 100;
            }
            if (commandId == WorkflowMenuCommands.Zoom75Mode.ID)
            {
                return 0x4b;
            }
            if (commandId == WorkflowMenuCommands.Zoom50Mode.ID)
            {
                num = 50;
            }
            return num;
        }

        public void Dispose()
        {
            IComponentChangeService service = this.serviceProvider.GetService(typeof(IComponentChangeService)) as IComponentChangeService;
            if (service != null)
            {
                service.ComponentChanged -= new ComponentChangedEventHandler(this.OnComponentChanged);
            }
            if (this.activeFilter != null)
            {
                this.workflowView.RemoveDesignerMessageFilter(this.activeFilter);
                this.activeFilter.Dispose();
                this.activeFilter = null;
            }
            this.selectionService = null;
            for (int i = 0; i < this.commandSet.Count; i++)
            {
                this.menuCommandService.RemoveCommand(this.commandSet[i]);
            }
            this.menuCommandService = null;
        }

        private void OnChangeTheme(object sender, EventArgs e)
        {
            IExtendedUIService service = this.serviceProvider.GetService(typeof(IExtendedUIService)) as IExtendedUIService;
            if (service != null)
            {
                service.ShowToolsOptions();
            }
        }

        private void OnComponentChanged(object sender, ComponentChangedEventArgs e)
        {
            if (this.activeFilter != null)
            {
                this.workflowView.RemoveDesignerMessageFilter(this.activeFilter);
                this.activeFilter = null;
                this.UpdatePanCommands(true);
            }
        }

        private void OnCreateTheme(object sender, EventArgs e)
        {
            ThemeConfigurationDialog dialog = new ThemeConfigurationDialog(this.serviceProvider);
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                WorkflowTheme theme = dialog.ComposedTheme.Clone();
                if (theme != null)
                {
                    WorkflowTheme.CurrentTheme = theme;
                    WorkflowTheme.SaveThemeSettingToRegistry();
                }
            }
        }

        private void OnEnable(object sender, EventArgs e)
        {
            MenuCommand command1 = (MenuCommand) sender;
            DesignerTransaction transaction = null;
            IComponent primarySelection = this.selectionService.PrimarySelection as IComponent;
            if ((primarySelection != null) && (primarySelection.Site != null))
            {
                IDesignerHost service = primarySelection.Site.GetService(typeof(IDesignerHost)) as IDesignerHost;
                if (service != null)
                {
                    transaction = service.CreateTransaction(SR.GetString("ChangingEnabled"));
                }
            }
            try
            {
                foreach (object obj2 in this.selectionService.GetSelectedComponents())
                {
                    Activity activity = obj2 as Activity;
                    if (activity != null)
                    {
                        ActivityDesigner designer = ActivityDesigner.GetDesigner(activity);
                        if ((designer != null) && !designer.IsLocked)
                        {
                            PropertyDescriptor descriptor = TypeDescriptor.GetProperties(activity)["Enabled"];
                            if (descriptor != null)
                            {
                                descriptor.SetValue(activity, !activity.Enabled);
                            }
                        }
                    }
                }
                if (transaction != null)
                {
                    transaction.Commit();
                }
            }
            finally
            {
                if (transaction != null)
                {
                    ((IDisposable) transaction).Dispose();
                }
            }
            MenuCommand command = this.menuCommandService.FindCommand(WorkflowMenuCommands.Disable);
            if (command != null)
            {
                this.OnStatusEnable(command, EventArgs.Empty);
            }
            MenuCommand command2 = this.menuCommandService.FindCommand(WorkflowMenuCommands.Enable);
            if (command2 != null)
            {
                this.OnStatusEnable(command2, EventArgs.Empty);
            }
        }

        private void OnExpandCollapse(object sender, EventArgs e)
        {
            MenuCommand command = (MenuCommand) sender;
            foreach (object obj2 in this.selectionService.GetSelectedComponents())
            {
                Activity activity = obj2 as Activity;
                if (activity != null)
                {
                    CompositeActivityDesigner designer = ActivityDesigner.GetDesigner(activity) as CompositeActivityDesigner;
                    if (designer != null)
                    {
                        designer.Expanded = command.CommandID.ID == WorkflowMenuCommands.Expand.ID;
                    }
                }
            }
            MenuCommand command2 = this.menuCommandService.FindCommand(WorkflowMenuCommands.Expand);
            if (command2 != null)
            {
                this.OnStatusExpandCollapse(command2, EventArgs.Empty);
            }
            MenuCommand command3 = this.menuCommandService.FindCommand(WorkflowMenuCommands.Collapse);
            if (command3 != null)
            {
                this.OnStatusExpandCollapse(command3, EventArgs.Empty);
            }
        }

        private void OnKeyCancel(object sender, EventArgs e)
        {
            this.SendKeyDownCommand(Keys.Escape);
        }

        private void OnKeyDefault(object sender, EventArgs e)
        {
            this.SendKeyDownCommand(Keys.Enter);
        }

        private void OnKeyMove(object sender, EventArgs e)
        {
            if (this.selectionService.PrimarySelection != null)
            {
                MenuCommand command = (MenuCommand) sender;
                Keys left = Keys.Left;
                if (command.CommandID.ID == MenuCommands.KeyMoveDown.ID)
                {
                    left = Keys.Down;
                }
                else if (command.CommandID.ID == MenuCommands.KeyMoveUp.ID)
                {
                    left = Keys.Up;
                }
                else if (command.CommandID.ID == MenuCommands.KeyMoveLeft.ID)
                {
                    left = Keys.Left;
                }
                else if (command.CommandID.ID == MenuCommands.KeyMoveRight.ID)
                {
                    left = Keys.Right;
                }
                else if (command.CommandID.ID == MenuCommands.KeySelectNext.ID)
                {
                    left = Keys.Tab;
                }
                else if (command.CommandID.ID == MenuCommands.KeySelectPrevious.ID)
                {
                    left = Keys.Shift | Keys.Tab;
                }
                this.SendKeyDownCommand(left);
            }
        }

        private void OnKeyPageDnUp(object sender, EventArgs e)
        {
            MenuCommand command = (MenuCommand) sender;
            this.SendKeyDownCommand((command.CommandID == WorkflowMenuCommands.PageUp) ? Keys.PageUp : Keys.Next);
        }

        private void OnMenuCopy(object sender, EventArgs e)
        {
            if (Helpers.AreAllActivities(this.selectionService.GetSelectedComponents()))
            {
                Activity[] topLevelActivities = Helpers.GetTopLevelActivities(this.selectionService.GetSelectedComponents());
                Clipboard.SetDataObject(CompositeActivityDesigner.SerializeActivitiesToDataObject(this.serviceProvider, topLevelActivities));
            }
        }

        private void OnMenuCopyToClipboard(object sender, EventArgs e)
        {
            this.workflowView.SaveWorkflowImageToClipboard();
        }

        private void OnMenuCut(object sender, EventArgs e)
        {
            IDesignerHost service = this.serviceProvider.GetService(typeof(IDesignerHost)) as IDesignerHost;
            if ((service == null) || !this.selectionService.GetComponentSelected(service.RootComponent))
            {
                ICollection selectedComponents = this.selectionService.GetSelectedComponents();
                if (Helpers.AreAllActivities(selectedComponents) && DesignerHelpers.AreAssociatedDesignersMovable(selectedComponents))
                {
                    this.OnMenuCopy(sender, e);
                    string description = string.Empty;
                    if (selectedComponents.Count > 1)
                    {
                        description = SR.GetString("CutMultipleActivities", new object[] { selectedComponents.Count });
                    }
                    else
                    {
                        ArrayList list = new ArrayList(selectedComponents);
                        if (list.Count > 0)
                        {
                            description = SR.GetString("CutSingleActivity", new object[] { (list[0] as Activity).Name });
                        }
                        else
                        {
                            description = SR.GetString("CutActivity");
                        }
                    }
                    DesignerTransaction transaction = service.CreateTransaction(description);
                    try
                    {
                        this.OnMenuDelete(sender, e);
                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Cancel();
                    }
                }
            }
        }

        private void OnMenuDelete(object sender, EventArgs e)
        {
            this.SendKeyDownCommand(Keys.Delete);
        }

        private void OnMenuDesignerProperties(object sender, EventArgs e)
        {
            if (this.menuCommandService != null)
            {
                this.menuCommandService.GlobalInvoke(StandardCommands.PropertiesWindow);
            }
        }

        private void OnMenuPageSetup(object sender, EventArgs e)
        {
            if (PrinterSettings.InstalledPrinters.Count < 1)
            {
                DesignerHelpers.ShowError(this.serviceProvider, DR.GetString("ThereIsNoPrinterInstalledErrorMessage", new object[0]));
            }
            else
            {
                WorkflowPageSetupDialog dialog = new WorkflowPageSetupDialog(this.serviceProvider);
                if (DialogResult.OK == dialog.ShowDialog())
                {
                    this.workflowView.PerformLayout(false);
                }
            }
        }

        private void OnMenuPaste(object sender, EventArgs e)
        {
            object primarySelection = this.selectionService.PrimarySelection;
            CompositeActivityDesigner parentDesigner = ActivityDesigner.GetDesigner(primarySelection as Activity) as CompositeActivityDesigner;
            if (parentDesigner == null)
            {
                parentDesigner = ActivityDesigner.GetParentDesigner(primarySelection);
            }
            if ((parentDesigner != null) && parentDesigner.IsEditable)
            {
                IDataObject dataObject = Clipboard.GetDataObject();
                ICollection activities = null;
                try
                {
                    activities = CompositeActivityDesigner.DeserializeActivitiesFromDataObject(this.serviceProvider, dataObject, true);
                }
                catch (Exception exception)
                {
                    if (exception != CheckoutException.Canceled)
                    {
                        throw new Exception(DR.GetString("ActivityInsertError", new object[0]) + "\n" + exception.Message, exception);
                    }
                }
                if (activities == null)
                {
                    throw new InvalidOperationException(DR.GetString("InvalidOperationBadClipboardFormat", new object[0]));
                }
                System.Workflow.ComponentModel.Design.HitTestInfo insertLocation = null;
                if (primarySelection is System.Workflow.ComponentModel.Design.HitTestInfo)
                {
                    insertLocation = (System.Workflow.ComponentModel.Design.HitTestInfo) primarySelection;
                }
                else if (primarySelection is CompositeActivity)
                {
                    insertLocation = new System.Workflow.ComponentModel.Design.HitTestInfo(parentDesigner, HitTestLocations.Designer);
                }
                else if (primarySelection is Activity)
                {
                    Activity item = primarySelection as Activity;
                    CompositeActivity parent = item.Parent;
                    CompositeActivityDesigner designer = ActivityDesigner.GetDesigner(parent) as CompositeActivityDesigner;
                    if (designer != null)
                    {
                        insertLocation = new ConnectorHitTestInfo(designer, HitTestLocations.Designer, parent.Activities.IndexOf(item) + 1);
                    }
                }
                List<Activity> list = new List<Activity>(Helpers.GetTopLevelActivities(activities));
                if ((insertLocation == null) || !parentDesigner.CanInsertActivities(insertLocation, list.AsReadOnly()))
                {
                    throw new Exception(SR.GetString("Error_NoPasteSupport"));
                }
                IExtendedUIService service = this.serviceProvider.GetService(typeof(IExtendedUIService)) as IExtendedUIService;
                if (service != null)
                {
                    foreach (Activity activity3 in activities)
                    {
                        service.AddAssemblyReference(activity3.GetType().Assembly.GetName());
                    }
                }
                CompositeActivityDesigner.InsertActivities(parentDesigner, insertLocation, list.AsReadOnly(), SR.GetString("PastingActivities"));
                Stream data = dataObject.GetData("CF_WINOEDESIGNERCOMPONENTSSTATE") as Stream;
                if (data != null)
                {
                    Helpers.DeserializeDesignersFromStream(activities, data);
                }
                this.selectionService.SetSelectedComponents(list.ToArray(), SelectionTypes.Replace);
                this.workflowView.EnsureVisible(this.selectionService.PrimarySelection);
            }
        }

        private void OnMenuPrint(object sender, EventArgs e)
        {
            if (PrinterSettings.InstalledPrinters.Count < 1)
            {
                DesignerHelpers.ShowError(this.serviceProvider, DR.GetString("ThereIsNoPrinterInstalledErrorMessage", new object[0]));
            }
            else
            {
                PrintDocument printDocument = this.workflowView.PrintDocument;
                PrintDialog dialog = new PrintDialog {
                    AllowPrintToFile = false,
                    Document = printDocument
                };
                try
                {
                    if (DialogResult.OK == dialog.ShowDialog())
                    {
                        PrinterSettings printerSettings = printDocument.PrinterSettings;
                        PageSettings defaultPageSettings = printDocument.DefaultPageSettings;
                        printDocument.PrinterSettings = dialog.PrinterSettings;
                        printDocument.DefaultPageSettings = dialog.Document.DefaultPageSettings;
                        printDocument.Print();
                        printDocument.PrinterSettings = printerSettings;
                        printDocument.DefaultPageSettings = defaultPageSettings;
                    }
                }
                catch (Exception exception)
                {
                    string message = DR.GetString("SelectedPrinterIsInvalidErrorMessage", new object[0]) + "\n" + exception.Message;
                    DesignerHelpers.ShowError(this.serviceProvider, message);
                }
            }
        }

        private void OnMenuSaveWorkflowAsImage(object sender, EventArgs e)
        {
            List<SupportedImageFormats> list = new List<SupportedImageFormats> {
                new SupportedImageFormats(DR.GetString("BMPImageFormat", new object[0]), ImageFormat.Bmp),
                new SupportedImageFormats(DR.GetString("JPEGImageFormat", new object[0]), ImageFormat.Jpeg),
                new SupportedImageFormats(DR.GetString("PNGImageFormat", new object[0]), ImageFormat.Png),
                new SupportedImageFormats(DR.GetString("TIFFImageFormat", new object[0]), ImageFormat.Tiff),
                new SupportedImageFormats(DR.GetString("WMFImageFormat", new object[0]), ImageFormat.Wmf),
                new SupportedImageFormats(DR.GetString("EXIFImageFormat", new object[0]), ImageFormat.Exif),
                new SupportedImageFormats(DR.GetString("EMFImageFormat", new object[0]), ImageFormat.Emf)
            };
            SaveFileDialog dialog = new SaveFileDialog {
                Title = DR.GetString("SaveWorkflowImageDialogTitle", new object[0]),
                DefaultExt = "bmp"
            };
            string str = string.Empty;
            foreach (SupportedImageFormats formats in list)
            {
                str = str + ((str.Length > 0) ? ("|" + formats.Description) : formats.Description);
            }
            dialog.Filter = str;
            dialog.FilterIndex = 0;
            if (((dialog.ShowDialog() == DialogResult.OK) && (dialog.FilterIndex > 0)) && (dialog.FilterIndex <= list.Count))
            {
                this.workflowView.SaveWorkflowImage(dialog.FileName, list[dialog.FilterIndex - 1].Format);
            }
        }

        private void OnMenuSelectAll(object sender, EventArgs e)
        {
            ActivityDesigner safeRootDesigner = ActivityDesigner.GetSafeRootDesigner(this.serviceProvider);
            if (safeRootDesigner != null)
            {
                List<Activity> list = new List<Activity>();
                if (safeRootDesigner.Activity is CompositeActivity)
                {
                    list.AddRange(Helpers.GetNestedActivities(safeRootDesigner.Activity as CompositeActivity));
                }
                this.selectionService.SetSelectedComponents(list.ToArray(), SelectionTypes.Replace);
            }
        }

        private void OnMessageFilterChanged(object sender, EventArgs e)
        {
            if (this.activeFilter != null)
            {
                this.workflowView.RemoveDesignerMessageFilter(this.activeFilter);
                this.activeFilter = null;
            }
            MenuCommand command = (MenuCommand) sender;
            int iD = command.CommandID.ID;
            if (WorkflowMenuCommands.ZoomIn.ID == iD)
            {
                this.activeFilter = new ZoomingMessageFilter(true);
            }
            else if (WorkflowMenuCommands.ZoomOut.ID == iD)
            {
                this.activeFilter = new ZoomingMessageFilter(false);
            }
            else if (WorkflowMenuCommands.Pan.ID == iD)
            {
                this.activeFilter = new PanningMessageFilter();
            }
            if (this.activeFilter != null)
            {
                this.workflowView.AddDesignerMessageFilter(this.activeFilter);
            }
            this.workflowView.Focus();
            this.UpdatePanCommands(true);
        }

        private void OnPageLayout(object sender, EventArgs e)
        {
            MenuCommand command = (MenuCommand) sender;
            this.workflowView.PrintPreviewMode = (command.CommandID == WorkflowMenuCommands.PrintPreview) ? !this.workflowView.PrintPreviewMode : (command.CommandID == WorkflowMenuCommands.PrintPreviewPage);
            this.UpdatePageLayoutCommands(true);
        }

        private void OnStatusAlways(object sender, EventArgs e)
        {
            MenuCommand command = (MenuCommand) sender;
            command.Enabled = true;
        }

        private void OnStatusAnySelection(object sender, EventArgs e)
        {
            MenuCommand command = (MenuCommand) sender;
            IDesignerHost service = this.serviceProvider.GetService(typeof(IDesignerHost)) as IDesignerHost;
            command.Enabled = ((service != null) && (this.selectionService.GetSelectedComponents().Count > 0)) && !this.selectionService.GetComponentSelected(service.RootComponent);
        }

        private void OnStatusCopy(object sender, EventArgs e)
        {
            MenuCommand command = (MenuCommand) sender;
            bool flag = false;
            IDesignerHost service = this.serviceProvider.GetService(typeof(IDesignerHost)) as IDesignerHost;
            if ((service != null) && !service.Loading)
            {
                ArrayList c = new ArrayList(this.selectionService.GetSelectedComponents());
                flag = Helpers.AreAllActivities(c);
                if (flag)
                {
                    foreach (Activity activity in c)
                    {
                        if (activity.Site != null)
                        {
                            service = activity.Site.GetService(typeof(IDesignerHost)) as IDesignerHost;
                            if ((service != null) && this.selectionService.GetComponentSelected(service.RootComponent))
                            {
                                flag = false;
                                break;
                            }
                        }
                    }
                }
            }
            command.Enabled = flag;
        }

        private void OnStatusCut(object sender, EventArgs e)
        {
            this.OnStatusDelete(sender, e);
        }

        private void OnStatusDelete(object sender, EventArgs e)
        {
            MenuCommand command = (MenuCommand) sender;
            command.Enabled = false;
            IDesignerHost service = this.serviceProvider.GetService(typeof(IDesignerHost)) as IDesignerHost;
            if (((service == null) || (service.RootComponent == null)) || !this.selectionService.GetComponentSelected(service.RootComponent))
            {
                ICollection selectedComponents = this.selectionService.GetSelectedComponents();
                if (DesignerHelpers.AreComponentsRemovable(selectedComponents))
                {
                    foreach (DictionaryEntry entry in Helpers.PairUpCommonParentActivities(Helpers.GetTopLevelActivities(selectedComponents)))
                    {
                        CompositeActivityDesigner designer = ActivityDesigner.GetDesigner(entry.Key as Activity) as CompositeActivityDesigner;
                        if ((designer != null) && !designer.CanRemoveActivities(new List<Activity>((Activity[]) ((ArrayList) entry.Value).ToArray(typeof(Activity))).AsReadOnly()))
                        {
                            command.Enabled = false;
                            return;
                        }
                    }
                    command.Enabled = true;
                }
            }
        }

        private void OnStatusEnable(object sender, EventArgs e)
        {
            MenuCommand command = (MenuCommand) sender;
            bool enabled = true;
            bool flag2 = true;
            ArrayList list = new ArrayList(this.selectionService.GetSelectedComponents());
            for (int i = 0; (i < list.Count) && flag2; i++)
            {
                Activity activity = list[i] as Activity;
                if (activity != null)
                {
                    ActivityDesigner designer = ActivityDesigner.GetDesigner(activity);
                    if ((((designer == null) || designer.IsLocked) || ((i > 0) && (enabled != activity.Enabled))) || ((this.workflowView.RootDesigner != null) && (this.workflowView.RootDesigner.Activity == activity)))
                    {
                        flag2 = false;
                    }
                    else
                    {
                        enabled = activity.Enabled;
                    }
                }
                else
                {
                    flag2 = false;
                }
            }
            command.Visible = command.Enabled = flag2 && (((command.CommandID == WorkflowMenuCommands.Enable) && !enabled) || ((command.CommandID == WorkflowMenuCommands.Disable) && enabled));
        }

        private void OnStatusExpandCollapse(object sender, EventArgs e)
        {
            MenuCommand command = (MenuCommand) sender;
            int num = 0;
            foreach (object obj2 in this.selectionService.GetSelectedComponents())
            {
                Activity activity = obj2 as Activity;
                if (activity != null)
                {
                    CompositeActivityDesigner designer = ActivityDesigner.GetDesigner(activity) as CompositeActivityDesigner;
                    if (((designer != null) && designer.CanExpandCollapse) && (((command.CommandID == WorkflowMenuCommands.Expand) && !designer.Expanded) || ((command.CommandID == WorkflowMenuCommands.Collapse) && designer.Expanded)))
                    {
                        num++;
                    }
                }
            }
            command.Visible = command.Enabled = num == this.selectionService.SelectionCount;
        }

        private void OnStatusLayout(object sender, EventArgs e)
        {
            this.UpdatePageLayoutCommands(true);
        }

        private void OnStatusMessageFilter(object sender, EventArgs e)
        {
            this.UpdatePanCommands(true);
        }

        private void OnStatusPageSetup(object sender, EventArgs e)
        {
            this.OnStatusAlways(sender, e);
        }

        private void OnStatusPaste(object sender, EventArgs e)
        {
            MenuCommand command = (MenuCommand) sender;
            command.Enabled = false;
            object primarySelection = this.selectionService.PrimarySelection;
            CompositeActivityDesigner parentDesigner = ActivityDesigner.GetDesigner(primarySelection as Activity) as CompositeActivityDesigner;
            if (parentDesigner == null)
            {
                parentDesigner = ActivityDesigner.GetParentDesigner(primarySelection);
            }
            if ((parentDesigner != null) && parentDesigner.IsEditable)
            {
                IDesignerHost host = this.serviceProvider.GetService(typeof(IDesignerHost)) as IDesignerHost;
                IToolboxService service = (IToolboxService) this.serviceProvider.GetService(typeof(IToolboxService));
                IDataObject dataObject = Clipboard.GetDataObject();
                if (((dataObject != null) && (host != null)) && ((dataObject.GetDataPresent("CF_WINOEDESIGNERCOMPONENTS") || (service == null)) || service.IsSupported(dataObject, host)))
                {
                    System.Workflow.ComponentModel.Design.HitTestInfo insertLocation = null;
                    if (primarySelection is System.Workflow.ComponentModel.Design.HitTestInfo)
                    {
                        insertLocation = (System.Workflow.ComponentModel.Design.HitTestInfo) primarySelection;
                    }
                    else if (primarySelection is CompositeActivity)
                    {
                        insertLocation = new System.Workflow.ComponentModel.Design.HitTestInfo(parentDesigner, HitTestLocations.Designer);
                    }
                    else if (primarySelection is Activity)
                    {
                        Activity item = primarySelection as Activity;
                        CompositeActivity parent = item.Parent;
                        CompositeActivityDesigner designer = ActivityDesigner.GetDesigner(parent) as CompositeActivityDesigner;
                        if (designer != null)
                        {
                            insertLocation = new ConnectorHitTestInfo(designer, HitTestLocations.Designer, parent.Activities.IndexOf(item) + 1);
                        }
                    }
                    ICollection activities = null;
                    try
                    {
                        activities = CompositeActivityDesigner.DeserializeActivitiesFromDataObject(this.serviceProvider, dataObject);
                    }
                    catch (CheckoutException exception)
                    {
                        if (exception != CheckoutException.Canceled)
                        {
                            throw exception;
                        }
                    }
                    command.Enabled = ((activities != null) && (insertLocation != null)) && parentDesigner.CanInsertActivities(insertLocation, new List<Activity>(Helpers.GetTopLevelActivities(activities)).AsReadOnly());
                }
            }
        }

        private void OnStatusPrint(object sender, EventArgs e)
        {
            this.OnStatusAlways(sender, e);
        }

        private void OnStatusZoom(object sender, EventArgs e)
        {
            this.UpdateZoomCommands(true);
        }

        private void OnViewCode(object sender, EventArgs e)
        {
            IDesignerHost host = this.serviceProvider.GetService(typeof(IDesignerHost)) as IDesignerHost;
            IComponent component = (host != null) ? host.RootComponent : null;
            if (component != null)
            {
                IMemberCreationService service = component.Site.GetService(typeof(IMemberCreationService)) as IMemberCreationService;
                if (service != null)
                {
                    service.ShowCode();
                }
            }
        }

        private void OnZoom(object sender, EventArgs e)
        {
            MenuCommand command = (MenuCommand) sender;
            if (command.CommandID.ID == WorkflowMenuCommands.ShowAll.ID)
            {
                int num = (int) ((100f / this.workflowView.ActiveLayout.Scaling) * Math.Min((float) (((float) this.workflowView.ViewPortSize.Width) / ((float) this.workflowView.ActiveLayout.Extent.Width)), (float) (((float) this.workflowView.ViewPortSize.Height) / ((float) this.workflowView.ActiveLayout.Extent.Height))));
                this.workflowView.Zoom = Math.Min(Math.Max(num, 10), 400);
            }
            else
            {
                this.workflowView.Zoom = this.ConvertToZoomLevel(command.CommandID.ID);
            }
            this.UpdateZoomCommands(true);
        }

        private bool SendKeyDownCommand(Keys key)
        {
            IDesignerHost service = this.serviceProvider.GetService(typeof(IDesignerHost)) as IDesignerHost;
            if (service != null)
            {
                IRootDesigner designer = ActivityDesigner.GetDesigner(service.RootComponent as Activity);
                if (designer != null)
                {
                    WorkflowView view = designer.GetView(ViewTechnology.Default) as WorkflowView;
                    if (view != null)
                    {
                        KeyEventArgs e = new KeyEventArgs(key);
                        view.OnCommandKey(e);
                        return e.Handled;
                    }
                }
            }
            return false;
        }

        internal void UpdateCommandSet()
        {
            for (int i = 0; i < this.commandSet.Count; i++)
            {
                this.commandSet[i].UpdateStatus();
            }
        }

        internal void UpdatePageLayoutCommands(bool enable)
        {
            System.Workflow.ComponentModel.Design.CommandSetItem[] layoutCommands = this.layoutCommands;
            for (int i = 0; i < layoutCommands.Length; i++)
            {
                MenuCommand command = layoutCommands[i];
                command.Enabled = enable;
                command.Checked = this.workflowView.PrintPreviewMode ? ((command.CommandID == WorkflowMenuCommands.PrintPreview) || (command.CommandID == WorkflowMenuCommands.PrintPreviewPage)) : (command.CommandID == WorkflowMenuCommands.DefaultPage);
            }
        }

        internal void UpdatePanCommands(bool enable)
        {
            CommandID did = this.ConvertMessageFilterToCommandID();
            System.Workflow.ComponentModel.Design.CommandSetItem[] navigationToolCommands = this.navigationToolCommands;
            for (int i = 0; i < navigationToolCommands.Length; i++)
            {
                MenuCommand command = navigationToolCommands[i];
                command.Enabled = enable;
                command.Checked = did == command.CommandID;
            }
        }

        internal void UpdateZoomCommands(bool enable)
        {
            int num = this.ConvertToZoomCommand(this.workflowView.Zoom);
            System.Workflow.ComponentModel.Design.CommandSetItem[] zoomCommands = this.zoomCommands;
            for (int i = 0; i < zoomCommands.Length; i++)
            {
                MenuCommand command = zoomCommands[i];
                command.Enabled = enable;
                command.Checked = num == command.CommandID.ID;
            }
        }
    }
}

