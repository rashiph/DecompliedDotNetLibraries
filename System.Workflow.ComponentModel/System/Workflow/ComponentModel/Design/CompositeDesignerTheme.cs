namespace System.Workflow.ComponentModel.Design
{
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Drawing.Design;
    using System.Drawing.Drawing2D;
    using System.IO;
    using System.Runtime;
    using System.Runtime.InteropServices;

    public class CompositeDesignerTheme : ActivityDesignerTheme
    {
        private static readonly Size[] ConnectorSizes = new Size[] { new Size(15, 30), new Size(15, 0x13), new Size(0x19, 50) };
        private bool dropShadow;
        private LineAnchor endCap;
        private Brush expandButtonBackBrush;
        internal static readonly Pen ExpandButtonBorderPen = new Pen(Color.FromArgb(0x7b, 0x9a, 0xb5), 1f);
        internal static readonly Pen ExpandButtonForegoundPen = new Pen(Color.Black, 1f);
        private Rectangle expandButtonRectangle;
        private static readonly Size[] ExpandButtonSizes = new Size[] { new Size(8, 8), new Size(8, 8), new Size(12, 12) };
        private LineAnchor startCap;
        private DesignerContentAlignment watermarkAlignment;
        private Image watermarkImage;
        private string watermarkImagePath;

        public CompositeDesignerTheme(WorkflowTheme theme) : base(theme)
        {
            this.watermarkAlignment = DesignerContentAlignment.BottomRight;
            this.watermarkImagePath = string.Empty;
            this.endCap = LineAnchor.ArrowAnchor;
            this.expandButtonRectangle = Rectangle.Empty;
        }

        private void ApplySystemColors()
        {
            this.BackColorStart = Color.Empty;
            this.BackColorEnd = Color.Empty;
        }

        protected override void Dispose(bool disposing)
        {
            try
            {
                if (this.expandButtonBackBrush != null)
                {
                    this.expandButtonBackBrush.Dispose();
                    this.expandButtonBackBrush = null;
                }
                if (this.watermarkImage != null)
                {
                    this.watermarkImage.Dispose();
                    this.watermarkImage = null;
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }

        public Brush GetExpandButtonBackgroundBrush(Rectangle rectangle)
        {
            if ((this.expandButtonBackBrush == null) || (this.expandButtonRectangle != rectangle))
            {
                if (this.expandButtonBackBrush != null)
                {
                    this.expandButtonBackBrush.Dispose();
                }
                this.expandButtonRectangle = rectangle;
                this.expandButtonBackBrush = new LinearGradientBrush(this.expandButtonRectangle, Color.White, Color.FromArgb(0xad, 170, 0x9c), LinearGradientMode.ForwardDiagonal);
            }
            return this.expandButtonBackBrush;
        }

        public override void Initialize()
        {
            base.Initialize();
            if (base.ContainingTheme.AmbientTheme.UseOperatingSystemSettings)
            {
                this.ApplySystemColors();
            }
        }

        public override void OnAmbientPropertyChanged(AmbientProperty ambientProperty)
        {
            base.OnAmbientPropertyChanged(ambientProperty);
            if (ambientProperty == AmbientProperty.OperatingSystemSetting)
            {
                this.ApplySystemColors();
            }
        }

        [DefaultValue(2), SRCategory("ForegroundCategory", "System.Workflow.ComponentModel.Design.DesignerResources"), DispId(12), SRDescription("ConnectorEndCapDesc", "System.Workflow.ComponentModel.Design.DesignerResources")]
        public virtual LineAnchor ConnectorEndCap
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.endCap;
            }
            set
            {
                if (base.ReadOnly)
                {
                    throw new InvalidOperationException(DR.GetString("ThemePropertyReadOnly", new object[0]));
                }
                this.endCap = value;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public virtual Size ConnectorSize
        {
            get
            {
                if ((this.DesignerType != null) && typeof(FreeformActivityDesigner).IsAssignableFrom(this.DesignerType))
                {
                    int height = ConnectorSizes[(int) base.ContainingTheme.AmbientTheme.DesignerSize].Height;
                    return new Size(height, height);
                }
                return ConnectorSizes[(int) base.ContainingTheme.AmbientTheme.DesignerSize];
            }
        }

        [SRCategory("ForegroundCategory", "System.Workflow.ComponentModel.Design.DesignerResources"), SRDescription("ConnectorStartCapDesc", "System.Workflow.ComponentModel.Design.DesignerResources"), DefaultValue(0), DispId(11)]
        public virtual LineAnchor ConnectorStartCap
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.startCap;
            }
            set
            {
                if (base.ReadOnly)
                {
                    throw new InvalidOperationException(DR.GetString("ThemePropertyReadOnly", new object[0]));
                }
                this.startCap = value;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public virtual Size ExpandButtonSize
        {
            get
            {
                return ExpandButtonSizes[(int) base.ContainingTheme.AmbientTheme.DesignerSize];
            }
        }

        [DefaultValue(false), SRDescription("DropShadowDesc", "System.Workflow.ComponentModel.Design.DesignerResources"), SRCategory("BackgroundCategory", "System.Workflow.ComponentModel.Design.DesignerResources"), DispId(10)]
        public virtual bool ShowDropShadow
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.dropShadow;
            }
            set
            {
                if (base.ReadOnly)
                {
                    throw new InvalidOperationException(DR.GetString("ThemePropertyReadOnly", new object[0]));
                }
                this.dropShadow = value;
            }
        }

        [SRDescription("WatermarkAlignmentDesc", "System.Workflow.ComponentModel.Design.DesignerResources"), SRCategory("BackgroundCategory", "System.Workflow.ComponentModel.Design.DesignerResources"), DispId(9), DefaultValue(12)]
        public virtual DesignerContentAlignment WatermarkAlignment
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.watermarkAlignment;
            }
            set
            {
                if (base.ReadOnly)
                {
                    throw new InvalidOperationException(DR.GetString("ThemePropertyReadOnly", new object[0]));
                }
                this.watermarkAlignment = value;
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Image WatermarkImage
        {
            get
            {
                if ((this.watermarkImage == null) && (this.watermarkImagePath.Length > 0))
                {
                    this.watermarkImage = DesignerHelpers.GetImageFromPath(this, base.ContainingTheme.ContainingFileDirectory, this.watermarkImagePath);
                }
                return this.watermarkImage;
            }
        }

        [DispId(8), SRCategory("BackgroundCategory", "System.Workflow.ComponentModel.Design.DesignerResources"), Editor(typeof(ImageBrowserEditor), typeof(UITypeEditor)), SRDescription("WatermarkDesc", "System.Workflow.ComponentModel.Design.DesignerResources")]
        public virtual string WatermarkImagePath
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.watermarkImagePath;
            }
            set
            {
                if (base.ReadOnly)
                {
                    throw new InvalidOperationException(DR.GetString("ThemePropertyReadOnly", new object[0]));
                }
                if ((!string.IsNullOrEmpty(value) && value.Contains(Path.DirectorySeparatorChar.ToString())) && Path.IsPathRooted(value))
                {
                    value = DesignerHelpers.GetRelativePath(base.ContainingTheme.ContainingFileDirectory, value);
                    if (!DesignerHelpers.IsValidImageResource(this, base.ContainingTheme.ContainingFileDirectory, value))
                    {
                        throw new InvalidOperationException(DR.GetString("Error_InvalidImageResource", new object[0]));
                    }
                }
                this.watermarkImagePath = value;
                if (this.watermarkImage != null)
                {
                    this.watermarkImage.Dispose();
                    this.watermarkImage = null;
                }
            }
        }
    }
}

