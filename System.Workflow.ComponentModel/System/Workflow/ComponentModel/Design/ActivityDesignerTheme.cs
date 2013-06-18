namespace System.Workflow.ComponentModel.Design
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Drawing;
    using System.Drawing.Design;
    using System.Drawing.Drawing2D;
    using System.IO;
    using System.Runtime;
    using System.Runtime.InteropServices;

    public class ActivityDesignerTheme : DesignerTheme
    {
        private Color backColorEnd;
        private Color backColorStart;
        private Brush backgroundBrush;
        private Rectangle backgroundBrushRect;
        private LinearGradientMode backgroundStyle;
        private Color borderColor;
        private Pen borderPen;
        private DashStyle borderStyle;
        private Image designerImage;
        private string designerImagePath;
        private static readonly System.Drawing.Size[] DesignerSizes = new System.Drawing.Size[] { new System.Drawing.Size(90, 40), new System.Drawing.Size(130, 0x29), new System.Drawing.Size(110, 50) };
        private Color foreColor;
        private Brush foregroundBrush;
        private Pen foregroundPen;
        private static readonly System.Drawing.Size[] ImageSizes = new System.Drawing.Size[] { new System.Drawing.Size(0x10, 0x10), new System.Drawing.Size(0x10, 0x10), new System.Drawing.Size(0x18, 0x18) };

        public ActivityDesignerTheme(WorkflowTheme theme) : base(theme)
        {
            this.designerImagePath = string.Empty;
            this.foreColor = Color.Black;
            this.borderColor = Color.Black;
            this.backColorStart = Color.White;
            this.backColorEnd = Color.Empty;
        }

        private void ApplySystemColors()
        {
            this.ForeColor = SystemColors.ControlText;
            this.BorderColor = SystemColors.ControlDark;
            this.BackColorStart = SystemColors.Control;
            this.BackColorEnd = SystemColors.ControlLight;
        }

        protected override void Dispose(bool disposing)
        {
            try
            {
                if (this.designerImage != null)
                {
                    this.designerImage.Dispose();
                    this.designerImage = null;
                }
                if (this.foregroundPen != null)
                {
                    this.foregroundPen.Dispose();
                    this.foregroundPen = null;
                }
                if (this.foregroundBrush != null)
                {
                    this.foregroundBrush.Dispose();
                    this.foregroundBrush = null;
                }
                if (this.borderPen != null)
                {
                    this.borderPen.Dispose();
                    this.borderPen = null;
                }
                if (this.backgroundBrush != null)
                {
                    this.backgroundBrush.Dispose();
                    this.backgroundBrush = null;
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public Brush GetBackgroundBrush(Rectangle rectangle)
        {
            if ((this.backgroundBrush == null) || (this.backgroundBrushRect != rectangle))
            {
                if (this.backgroundBrush != null)
                {
                    this.backgroundBrush.Dispose();
                }
                this.backgroundBrushRect = rectangle;
                if (this.backColorStart == this.backColorEnd)
                {
                    this.backgroundBrush = new SolidBrush(this.backColorStart);
                }
                else
                {
                    this.backgroundBrush = new LinearGradientBrush(this.backgroundBrushRect, this.backColorStart, this.backColorEnd, this.backgroundStyle);
                }
            }
            return this.backgroundBrush;
        }

        internal override ICollection GetPropertyValues(ITypeDescriptorContext context)
        {
            object[] objArray = new object[0];
            if (string.Equals(context.PropertyDescriptor.Name, "BorderStyle", StringComparison.Ordinal))
            {
                objArray = new object[] { DashStyle.Solid, DashStyle.Dash, DashStyle.DashDot, DashStyle.DashDotDot, DashStyle.Dot };
            }
            return objArray;
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
            if (ambientProperty == AmbientProperty.DesignerSize)
            {
                this.ForeColor = this.foreColor;
                this.BorderColor = this.borderColor;
            }
            else if (ambientProperty == AmbientProperty.OperatingSystemSetting)
            {
                this.ApplySystemColors();
            }
        }

        [Editor(typeof(ColorPickerEditor), typeof(UITypeEditor)), TypeConverter(typeof(ColorPickerConverter)), DispId(6), SRDescription("BackColorEndDesc", "System.Workflow.ComponentModel.Design.DesignerResources"), SRCategory("BackgroundCategory", "System.Workflow.ComponentModel.Design.DesignerResources")]
        public virtual Color BackColorEnd
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.backColorEnd;
            }
            set
            {
                if (base.ReadOnly)
                {
                    throw new InvalidOperationException(DR.GetString("ThemePropertyReadOnly", new object[0]));
                }
                this.backColorEnd = value;
                if (this.backgroundBrush != null)
                {
                    this.backgroundBrush.Dispose();
                    this.backgroundBrush = null;
                }
            }
        }

        [TypeConverter(typeof(ColorPickerConverter)), Editor(typeof(ColorPickerEditor), typeof(UITypeEditor)), SRDescription("BackColorStartDesc", "System.Workflow.ComponentModel.Design.DesignerResources"), DispId(5), SRCategory("BackgroundCategory", "System.Workflow.ComponentModel.Design.DesignerResources")]
        public virtual Color BackColorStart
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.backColorStart;
            }
            set
            {
                if (base.ReadOnly)
                {
                    throw new InvalidOperationException(DR.GetString("ThemePropertyReadOnly", new object[0]));
                }
                this.backColorStart = value;
                if (this.backgroundBrush != null)
                {
                    this.backgroundBrush.Dispose();
                    this.backgroundBrush = null;
                }
            }
        }

        [SRCategory("BackgroundCategory", "System.Workflow.ComponentModel.Design.DesignerResources"), DispId(7), SRDescription("BackgroundStyleDesc", "System.Workflow.ComponentModel.Design.DesignerResources")]
        public virtual LinearGradientMode BackgroundStyle
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.backgroundStyle;
            }
            set
            {
                if (base.ReadOnly)
                {
                    throw new InvalidOperationException(DR.GetString("ThemePropertyReadOnly", new object[0]));
                }
                this.backgroundStyle = value;
                if (this.backgroundBrush != null)
                {
                    this.backgroundBrush.Dispose();
                    this.backgroundBrush = null;
                }
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public System.Drawing.Font BoldFont
        {
            get
            {
                return base.ContainingTheme.AmbientTheme.BoldFont;
            }
        }

        [DispId(3), TypeConverter(typeof(ColorPickerConverter)), SRDescription("BorderColorDesc", "System.Workflow.ComponentModel.Design.DesignerResources"), Editor(typeof(ColorPickerEditor), typeof(UITypeEditor)), SRCategory("ForegroundCategory", "System.Workflow.ComponentModel.Design.DesignerResources")]
        public virtual Color BorderColor
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.borderColor;
            }
            set
            {
                if (base.ReadOnly)
                {
                    throw new InvalidOperationException(DR.GetString("ThemePropertyReadOnly", new object[0]));
                }
                this.borderColor = value;
                if (this.borderPen != null)
                {
                    this.borderPen.Dispose();
                    this.borderPen = null;
                }
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public Pen BorderPen
        {
            get
            {
                if (this.borderPen == null)
                {
                    this.borderPen = new Pen(this.borderColor, (float) this.BorderWidth);
                    this.borderPen.DashStyle = this.borderStyle;
                }
                return this.borderPen;
            }
        }

        [TypeConverter(typeof(FilteredEnumConverter)), SRDescription("BorderStyleDesc", "System.Workflow.ComponentModel.Design.DesignerResources"), SRCategory("ForegroundCategory", "System.Workflow.ComponentModel.Design.DesignerResources"), DispId(4)]
        public virtual DashStyle BorderStyle
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.borderStyle;
            }
            set
            {
                if (base.ReadOnly)
                {
                    throw new InvalidOperationException(DR.GetString("ThemePropertyReadOnly", new object[0]));
                }
                if (value == DashStyle.Custom)
                {
                    throw new Exception(DR.GetString("CustomStyleNotSupported", new object[0]));
                }
                this.borderStyle = value;
                if (this.borderPen != null)
                {
                    this.borderPen.Dispose();
                    this.borderPen = null;
                }
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int BorderWidth
        {
            get
            {
                return base.ContainingTheme.AmbientTheme.BorderWidth;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public System.Workflow.ComponentModel.Design.DesignerGeometry DesignerGeometry
        {
            get
            {
                if (base.ContainingTheme.AmbientTheme.DrawRounded)
                {
                    return System.Workflow.ComponentModel.Design.DesignerGeometry.RoundedRectangle;
                }
                return System.Workflow.ComponentModel.Design.DesignerGeometry.Rectangle;
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Image DesignerImage
        {
            get
            {
                if ((this.designerImage == null) && (this.designerImagePath.Length > 0))
                {
                    this.designerImage = DesignerHelpers.GetImageFromPath(this, base.ContainingTheme.ContainingFileDirectory, this.designerImagePath);
                }
                return this.designerImage;
            }
        }

        [SRDescription("ImageDesc", "System.Workflow.ComponentModel.Design.DesignerResources"), DispId(1), SRCategory("ForegroundCategory", "System.Workflow.ComponentModel.Design.DesignerResources"), Editor(typeof(ImageBrowserEditor), typeof(UITypeEditor))]
        public virtual string DesignerImagePath
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.designerImagePath;
            }
            set
            {
                if (base.ReadOnly)
                {
                    throw new InvalidOperationException(DR.GetString("ThemePropertyReadOnly", new object[0]));
                }
                if (((value != null) && (value.Length > 0)) && (value.Contains(Path.DirectorySeparatorChar.ToString()) && Path.IsPathRooted(value)))
                {
                    value = DesignerHelpers.GetRelativePath(base.ContainingTheme.ContainingFileDirectory, value);
                    if (!DesignerHelpers.IsValidImageResource(this, base.ContainingTheme.ContainingFileDirectory, value))
                    {
                        throw new InvalidOperationException(DR.GetString("Error_InvalidImageResource", new object[0]));
                    }
                }
                this.designerImagePath = value;
                if (this.designerImage != null)
                {
                    this.designerImage.Dispose();
                    this.designerImage = null;
                }
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public System.Drawing.Font Font
        {
            get
            {
                return base.ContainingTheme.AmbientTheme.Font;
            }
        }

        [SRCategory("ForegroundCategory", "System.Workflow.ComponentModel.Design.DesignerResources"), TypeConverter(typeof(ColorPickerConverter)), Editor(typeof(ColorPickerEditor), typeof(UITypeEditor)), SRDescription("ForeColorDesc", "System.Workflow.ComponentModel.Design.DesignerResources"), DispId(2)]
        public virtual Color ForeColor
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.foreColor;
            }
            set
            {
                if (base.ReadOnly)
                {
                    throw new InvalidOperationException(DR.GetString("ThemePropertyReadOnly", new object[0]));
                }
                this.foreColor = value;
                if (this.foregroundPen != null)
                {
                    this.foregroundPen.Dispose();
                    this.foregroundPen = null;
                }
                if (this.foregroundBrush != null)
                {
                    this.foregroundBrush.Dispose();
                    this.foregroundBrush = null;
                }
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Brush ForegroundBrush
        {
            get
            {
                if (this.foregroundBrush == null)
                {
                    this.foregroundBrush = new SolidBrush(this.foreColor);
                }
                return this.foregroundBrush;
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Pen ForegroundPen
        {
            get
            {
                if (this.foregroundPen == null)
                {
                    this.foregroundPen = new Pen(this.foreColor, (float) this.BorderWidth);
                }
                return this.foregroundPen;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public System.Drawing.Size ImageSize
        {
            get
            {
                return ImageSizes[(int) base.ContainingTheme.AmbientTheme.DesignerSize];
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public System.Drawing.Size Size
        {
            get
            {
                return DesignerSizes[(int) base.ContainingTheme.AmbientTheme.DesignerSize];
            }
        }
    }
}

