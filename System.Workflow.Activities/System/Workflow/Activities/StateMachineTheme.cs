namespace System.Workflow.Activities
{
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Drawing.Design;
    using System.IO;
    using System.Resources;
    using System.Runtime;
    using System.Web;
    using System.Workflow.ComponentModel.Design;

    internal class StateMachineTheme : CompositeDesignerTheme
    {
        private Image _completedStateDesignerImage;
        private string _completedStateDesignerImagePath;
        private Color _connectorColor;
        private Pen _connectorPen;
        private Size _connectorSize;
        private Image _initialStateDesignerImage;
        private string _initialStateDesignerImagePath;
        internal const string DefaultThemeFileExtension = "*.wtm";

        public StateMachineTheme(WorkflowTheme theme) : base(theme)
        {
            this._connectorColor = Color.Black;
            this._connectorSize = new Size(20, 20);
        }

        protected override void Dispose(bool disposing)
        {
            try
            {
                if (this._connectorPen != null)
                {
                    this._connectorPen.Dispose();
                    this._connectorPen = null;
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }

        internal static Image GetImageFromPath(DesignerTheme designerTheme, string directory, string path)
        {
            Bitmap bitmap = null;
            if (path.Contains(Path.DirectorySeparatorChar.ToString()) && (directory.Length > 0))
            {
                string str = HttpUtility.UrlDecode(new Uri(new Uri(directory), path).LocalPath);
                if (File.Exists(str))
                {
                    try
                    {
                        bitmap = new Bitmap(str);
                    }
                    catch
                    {
                    }
                }
            }
            else if (designerTheme.DesignerType != null)
            {
                int length = path.LastIndexOf('.');
                if (length > 0)
                {
                    string baseName = path.Substring(0, length);
                    string name = path.Substring(length + 1);
                    if (((baseName != null) && (baseName.Length > 0)) && ((name != null) && (name.Length > 0)))
                    {
                        try
                        {
                            ResourceManager manager = new ResourceManager(baseName, designerTheme.DesignerType.Assembly);
                            bitmap = manager.GetObject(name) as Bitmap;
                        }
                        catch
                        {
                        }
                    }
                }
            }
            if (bitmap != null)
            {
                bitmap.MakeTransparent(System.Workflow.Activities.DR.TransparentColor);
            }
            return bitmap;
        }

        internal static string GetRelativePath(string pathFrom, string pathTo)
        {
            Uri uri = new Uri(pathFrom);
            string str = Uri.UnescapeDataString(uri.MakeRelativeUri(new Uri(pathTo)).ToString()).Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
            if (!str.Contains(Path.DirectorySeparatorChar.ToString()))
            {
                str = "." + Path.DirectorySeparatorChar + str;
            }
            return str;
        }

        internal static bool IsValidImageResource(DesignerTheme designerTheme, string directory, string path)
        {
            Image image = GetImageFromPath(designerTheme, directory, path);
            bool flag = image != null;
            if (image != null)
            {
                image.Dispose();
            }
            return flag;
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public Image CompletedStateDesignerImage
        {
            get
            {
                if ((this._completedStateDesignerImage == null) && !string.IsNullOrEmpty(this._completedStateDesignerImagePath))
                {
                    this._completedStateDesignerImage = GetImageFromPath(this, base.ContainingTheme.ContainingFileDirectory, this._completedStateDesignerImagePath);
                }
                return this._completedStateDesignerImage;
            }
        }

        [SRCategory("ForegroundCategory"), SRDescription("CompletedStateImagePathDescription"), Editor(typeof(System.Workflow.Activities.ImageBrowserEditor), typeof(UITypeEditor))]
        public virtual string CompletedStateDesignerImagePath
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._completedStateDesignerImagePath;
            }
            set
            {
                if (base.ReadOnly)
                {
                    throw new InvalidOperationException(System.Workflow.Activities.DR.GetString("ThemePropertyReadOnly"));
                }
                if (((value != null) && (value.Length > 0)) && (value.Contains(Path.DirectorySeparatorChar.ToString()) && Path.IsPathRooted(value)))
                {
                    value = GetRelativePath(base.ContainingTheme.ContainingFileDirectory, value);
                    if (!IsValidImageResource(this, base.ContainingTheme.ContainingFileDirectory, value))
                    {
                        throw new InvalidOperationException(System.Workflow.Activities.DR.GetString("Error_InvalidImageResource"));
                    }
                }
                this._completedStateDesignerImagePath = value;
                if (this._completedStateDesignerImage != null)
                {
                    this._completedStateDesignerImage.Dispose();
                    this._completedStateDesignerImage = null;
                }
            }
        }

        [SRCategory("ForegroundCategory"), SRDescription("ConnectorColorDescription")]
        public Color ConnectorColor
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._connectorColor;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this._connectorColor = value;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public Pen ConnectorPen
        {
            get
            {
                if (this._connectorPen == null)
                {
                    this._connectorPen = new Pen(this._connectorColor, (float) base.BorderWidth);
                }
                return this._connectorPen;
            }
        }

        public override Size ConnectorSize
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._connectorSize;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public Image InitialStateDesignerImage
        {
            get
            {
                if ((this._initialStateDesignerImage == null) && !string.IsNullOrEmpty(this._initialStateDesignerImagePath))
                {
                    this._initialStateDesignerImage = GetImageFromPath(this, base.ContainingTheme.ContainingFileDirectory, this._initialStateDesignerImagePath);
                }
                return this._initialStateDesignerImage;
            }
        }

        [SRCategory("ForegroundCategory"), SRDescription("InitialStateImagePathDescription"), Editor(typeof(System.Workflow.Activities.ImageBrowserEditor), typeof(UITypeEditor))]
        public virtual string InitialStateDesignerImagePath
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._initialStateDesignerImagePath;
            }
            set
            {
                if (base.ReadOnly)
                {
                    throw new InvalidOperationException(System.Workflow.Activities.DR.GetString("ThemePropertyReadOnly"));
                }
                if (((value != null) && (value.Length > 0)) && (value.Contains(Path.DirectorySeparatorChar.ToString()) && Path.IsPathRooted(value)))
                {
                    value = GetRelativePath(base.ContainingTheme.ContainingFileDirectory, value);
                    if (!IsValidImageResource(this, base.ContainingTheme.ContainingFileDirectory, value))
                    {
                        throw new InvalidOperationException(System.Workflow.Activities.DR.GetString("Error_InvalidImageResource"));
                    }
                }
                this._initialStateDesignerImagePath = value;
                if (this._initialStateDesignerImage != null)
                {
                    this._initialStateDesignerImage.Dispose();
                    this._initialStateDesignerImage = null;
                }
            }
        }
    }
}

