namespace System.Workflow.ComponentModel.Design
{
    using Microsoft.Win32;
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Drawing;
    using System.Drawing.Design;
    using System.Drawing.Drawing2D;
    using System.IO;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.Windows.Forms;

    public class AmbientTheme : DesignerTheme
    {
        internal const int ArcDiameter = 8;
        private System.Drawing.Color backColor;
        private Brush backgroundBrush;
        private System.Drawing.Font boldFont;
        private static readonly int[] BorderWidths = new int[] { 1, 1, 3 };
        private Brush commentIndicatorBrush;
        private System.Drawing.Color commentIndicatorColor;
        private Pen commentIndicatorPen;
        internal static readonly Image ConfigErrorImage = DR.GetImage("ConfigError");
        private const int DefaultShadowDepth = 6;
        private System.Workflow.ComponentModel.Design.DesignerSize designerStyle;
        internal static readonly Brush DisabledBrush = new SolidBrush(System.Drawing.Color.FromArgb(40, System.Drawing.Color.Gray));
        internal static readonly Size DragImageIconSize = new Size(0x10, 0x10);
        internal static readonly Size DragImageMargins = new Size(4, 4);
        internal static readonly Size DragImageTextSize = new Size(100, 60);
        private bool drawGrayscale;
        private bool drawRounded;
        private bool drawShadow;
        private Brush dropIndicatorBrush;
        private System.Drawing.Color dropIndicatorColor;
        internal static readonly Image DropIndicatorImage = DR.GetImage("DropShapeShort");
        private Pen dropIndicatorPen;
        private static readonly Size[] DropIndicatorSizes = new Size[] { new Size(8, 8), new Size(12, 12), new Size(0x10, 0x10) };
        internal const int DropShadowWidth = 4;
        internal static readonly Brush FadeBrush = new SolidBrush(System.Drawing.Color.FromArgb(120, 0xff, 0xff, 0xff));
        private System.Drawing.Font font;
        private string fontName;
        private static float[] fontSizes = null;
        private System.Drawing.Color foreColor;
        private Brush foregroundBrush;
        private Pen foregroundPen;
        private static readonly Size[] GlyphSizes = new Size[] { new Size(10, 10), new Size(14, 14), new Size(0x12, 0x12) };
        private System.Drawing.Color gridColor;
        private static readonly Size[] GridSizes = new Size[] { new Size(30, 30), new Size(40, 40), new Size(60, 60) };
        private DashStyle gridStyle;
        internal static readonly Image LockImage = DR.GetImage("PreviewIndicator");
        internal static readonly Pen MagnifierPen = new Pen(System.Drawing.Color.Black, 2f);
        private static readonly Size[] MagnifierSizes = new Size[] { new Size(50, 50), new Size(100, 100), new Size(150, 150) };
        private Brush majorGridBrush;
        private Pen majorGridPen;
        private static readonly Size[] MarginSizes = new Size[] { new Size(2, 2), new Size(4, 4), new Size(6, 6) };
        internal const int MaxShadowDepth = 8;
        internal const int MaxZoom = 400;
        private Brush minorGridBrush;
        private Pen minorGridPen;
        internal const int MinShadowDepth = 0;
        internal const int MinZoom = 10;
        internal static readonly Brush PageShadowBrush = new SolidBrush(System.Drawing.Color.FromArgb(0x4b, 0x4b, 0x4b));
        internal static readonly Image ReadOnlyImage = DR.GetImage("ReadOnly");
        private Brush readonlyIndicatorBrush;
        private System.Drawing.Color readonlyIndicatorColor;
        internal static readonly Image ScrollIndicatorImage = DR.GetImage("ArrowLeft");
        private static readonly Size[] ScrollIndicatorSizes = new Size[] { new Size(0x18, 0x18), new Size(0x20, 0x20), new Size(40, 40) };
        internal const float ScrollIndicatorTransparency = 0.7f;
        internal const int ScrollUnit = 0x19;
        private System.Drawing.Color selectionForeColor;
        private Brush selectionForegroundBrush;
        private Pen selectionForegroundPen;
        private System.Drawing.Color selectionPatternColor;
        private Pen selectionPatternPen;
        private static readonly Size[] SelectionSizes = new Size[] { new Size(2, 2), new Size(4, 4), new Size(6, 6) };
        private bool showConfigErrors;
        private bool showDesignerBorder;
        private bool showGrid;
        internal static readonly Pen SmartTagBorderPen = new Pen(System.Drawing.Color.Black, 1f);
        private System.Workflow.ComponentModel.Design.TextQuality textQuality;
        internal static System.Drawing.Color TransparentColor = System.Drawing.Color.FromArgb(0xff, 0, 0xff);
        private bool useDefaultFont;
        private bool useOperatingSystemSettings;
        private DesignerContentAlignment watermarkAlignment;
        private Image watermarkImage;
        private string watermarkImagePath;
        internal const float WatermarkTransparency = 0.25f;
        internal static readonly Pen WorkflowBorderPen = new Pen(System.Drawing.Color.FromArgb(0x7f, 0x9d, 0xb9), 1f);
        internal static readonly Brush WorkspaceBackgroundBrush = new SolidBrush(System.Drawing.Color.FromArgb(0xea, 0xea, 0xec));

        public AmbientTheme(WorkflowTheme theme) : base(theme)
        {
            this.showConfigErrors = true;
            this.dropIndicatorColor = System.Drawing.Color.Green;
            this.selectionForeColor = System.Drawing.Color.Blue;
            this.selectionPatternColor = System.Drawing.Color.DarkGray;
            this.foreColor = System.Drawing.Color.Gray;
            this.backColor = System.Drawing.Color.White;
            this.commentIndicatorColor = System.Drawing.Color.FromArgb(0x31, 0xc6, 0x69);
            this.readonlyIndicatorColor = System.Drawing.Color.Gray;
            this.watermarkAlignment = DesignerContentAlignment.BottomRight;
            this.watermarkImagePath = string.Empty;
            this.gridStyle = DashStyle.Dash;
            this.gridColor = System.Drawing.Color.FromArgb(0xc0, 0xc0, 0xc0);
            this.fontName = WorkflowTheme.GetDefaultFont().FontFamily.Name;
            this.designerStyle = System.Workflow.ComponentModel.Design.DesignerSize.Medium;
            this.drawRounded = true;
            this.showDesignerBorder = true;
        }

        private void ApplySystemColors()
        {
            this.DropIndicatorColor = SystemColors.HotTrack;
            this.SelectionForeColor = SystemColors.Highlight;
            this.SelectionPatternColor = SystemColors.Highlight;
            this.ForeColor = SystemColors.WindowText;
            this.CommentIndicatorColor = SystemColors.GrayText;
            this.ReadonlyIndicatorColor = SystemColors.GrayText;
            this.BackColor = SystemColors.Window;
            this.GridColor = SystemColors.InactiveBorder;
            this.FontName = WorkflowTheme.GetDefaultFont().FontFamily.Name;
        }

        protected override void Dispose(bool disposing)
        {
            try
            {
                this.UseOperatingSystemSettings = false;
                if (this.font != null)
                {
                    this.font.Dispose();
                    this.font = null;
                }
                if (this.boldFont != null)
                {
                    this.boldFont.Dispose();
                    this.boldFont = null;
                }
                if (this.watermarkImage != null)
                {
                    this.watermarkImage.Dispose();
                    this.watermarkImage = null;
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
                if (this.backgroundBrush != null)
                {
                    this.backgroundBrush.Dispose();
                    this.backgroundBrush = null;
                }
                if (this.dropIndicatorPen != null)
                {
                    this.dropIndicatorPen.Dispose();
                    this.dropIndicatorPen = null;
                }
                if (this.selectionPatternPen != null)
                {
                    this.selectionPatternPen.Dispose();
                    this.selectionPatternPen = null;
                }
                if (this.selectionForegroundPen != null)
                {
                    this.selectionForegroundPen.Dispose();
                    this.selectionForegroundPen = null;
                }
                if (this.majorGridPen != null)
                {
                    this.majorGridPen.Dispose();
                    this.majorGridPen = null;
                }
                if (this.majorGridBrush != null)
                {
                    this.majorGridBrush.Dispose();
                    this.majorGridBrush = null;
                }
                if (this.minorGridPen != null)
                {
                    this.minorGridPen.Dispose();
                    this.minorGridPen = null;
                }
                if (this.minorGridBrush != null)
                {
                    this.minorGridBrush.Dispose();
                    this.minorGridBrush = null;
                }
                if (this.commentIndicatorPen != null)
                {
                    this.commentIndicatorPen.Dispose();
                    this.commentIndicatorPen = null;
                }
                if (this.commentIndicatorBrush != null)
                {
                    this.commentIndicatorBrush.Dispose();
                    this.commentIndicatorBrush = null;
                }
                if (this.readonlyIndicatorBrush != null)
                {
                    this.readonlyIndicatorBrush.Dispose();
                    this.readonlyIndicatorBrush = null;
                }
                if (this.dropIndicatorBrush != null)
                {
                    this.dropIndicatorBrush.Dispose();
                    this.dropIndicatorBrush = null;
                }
                if (this.selectionForegroundBrush != null)
                {
                    this.selectionForegroundBrush.Dispose();
                    this.selectionForegroundBrush = null;
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }

        internal override ICollection GetPropertyValues(ITypeDescriptorContext context)
        {
            object[] objArray = new object[0];
            if (string.Equals(context.PropertyDescriptor.Name, "GridStyle", StringComparison.Ordinal))
            {
                objArray = new object[] { DashStyle.Solid, DashStyle.Dash, DashStyle.Dot };
            }
            return objArray;
        }

        public override void Initialize()
        {
            base.Initialize();
            if (this.useOperatingSystemSettings)
            {
                this.ApplySystemColors();
            }
        }

        public override void OnAmbientPropertyChanged(System.Workflow.ComponentModel.Design.AmbientProperty ambientProperty)
        {
            base.OnAmbientPropertyChanged(ambientProperty);
            if (ambientProperty == System.Workflow.ComponentModel.Design.AmbientProperty.DesignerSize)
            {
                this.DropIndicatorColor = this.dropIndicatorColor;
                this.FontName = this.fontName;
            }
            else if (ambientProperty == System.Workflow.ComponentModel.Design.AmbientProperty.OperatingSystemSetting)
            {
                this.ApplySystemColors();
            }
        }

        private void OnOperatingSystemSettingsChanged(object sender, UserPreferenceChangedEventArgs e)
        {
            if ((e.Category == UserPreferenceCategory.Color) || (e.Category == UserPreferenceCategory.VisualStyle))
            {
                base.ContainingTheme.AmbientPropertyChanged(System.Workflow.ComponentModel.Design.AmbientProperty.OperatingSystemSetting);
                WorkflowTheme.FireThemeChange();
            }
        }

        internal void UpdateFont()
        {
            if (this.useDefaultFont)
            {
                bool readOnly = base.ReadOnly;
                base.ReadOnly = false;
                this.FontName = WorkflowTheme.GetDefaultFont().FontFamily.Name;
                base.ReadOnly = readOnly;
            }
        }

        internal void UseDefaultFont()
        {
            this.useDefaultFont = true;
        }

        [Editor(typeof(ColorPickerEditor), typeof(UITypeEditor)), DispId(13), TypeConverter(typeof(ColorPickerConverter)), SRDescription("WorkflowBackgroundDesc", "System.Workflow.ComponentModel.Design.DesignerResources"), SRCategory("BackgroundCategory", "System.Workflow.ComponentModel.Design.DesignerResources")]
        public virtual System.Drawing.Color BackColor
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.backColor;
            }
            set
            {
                if (base.ReadOnly)
                {
                    throw new InvalidOperationException(DR.GetString("ThemePropertyReadOnly", new object[0]));
                }
                this.backColor = value;
                if (this.backgroundBrush != null)
                {
                    this.backgroundBrush.Dispose();
                    this.backgroundBrush = null;
                }
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public Brush BackgroundBrush
        {
            get
            {
                if (this.backgroundBrush == null)
                {
                    this.backgroundBrush = new SolidBrush(this.backColor);
                }
                return this.backgroundBrush;
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public System.Drawing.Font BoldFont
        {
            get
            {
                if (this.boldFont == null)
                {
                    if ((this.fontName == null) || (this.fontName.Length == 0))
                    {
                        this.fontName = WorkflowTheme.GetDefaultFont().FontFamily.Name;
                    }
                    ArrayList list = new ArrayList(SupportedFonts);
                    if (!list.Contains(this.fontName))
                    {
                        this.fontName = WorkflowTheme.GetDefaultFont().FontFamily.Name;
                    }
                    this.boldFont = new System.Drawing.Font(this.fontName, this.FontSize, FontStyle.Bold);
                }
                return this.boldFont;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public virtual int BorderWidth
        {
            get
            {
                return BorderWidths[(int) base.ContainingTheme.AmbientTheme.DesignerSize];
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public Brush CommentIndicatorBrush
        {
            get
            {
                if (this.commentIndicatorBrush == null)
                {
                    this.commentIndicatorBrush = new SolidBrush(System.Drawing.Color.FromArgb(40, this.commentIndicatorColor));
                }
                return this.commentIndicatorBrush;
            }
        }

        [SRCategory("ForegroundCategory", "System.Workflow.ComponentModel.Design.DesignerResources"), DispId(11), Editor(typeof(ColorPickerEditor), typeof(UITypeEditor)), TypeConverter(typeof(ColorPickerConverter)), SRDescription("CommentColorDesc", "System.Workflow.ComponentModel.Design.DesignerResources")]
        public virtual System.Drawing.Color CommentIndicatorColor
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.commentIndicatorColor;
            }
            set
            {
                if (base.ReadOnly)
                {
                    throw new InvalidOperationException(DR.GetString("ThemePropertyReadOnly", new object[0]));
                }
                this.commentIndicatorColor = value;
                if (this.commentIndicatorPen != null)
                {
                    this.commentIndicatorPen.Dispose();
                    this.commentIndicatorPen = null;
                }
                if (this.commentIndicatorBrush != null)
                {
                    this.commentIndicatorBrush.Dispose();
                    this.commentIndicatorBrush = null;
                }
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Pen CommentIndicatorPen
        {
            get
            {
                if (this.commentIndicatorPen == null)
                {
                    this.commentIndicatorPen = new Pen(this.commentIndicatorColor, 1f);
                }
                return this.commentIndicatorPen;
            }
        }

        [SRCategory("ActivityAppearanceCategory", "System.Workflow.ComponentModel.Design.DesignerResources"), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), DefaultValue(1), DispId(20), SRDescription("DesignerSizeDesc", "System.Workflow.ComponentModel.Design.DesignerResources")]
        public virtual System.Workflow.ComponentModel.Design.DesignerSize DesignerSize
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.designerStyle;
            }
            set
            {
                if (base.ReadOnly)
                {
                    throw new InvalidOperationException(DR.GetString("ThemePropertyReadOnly", new object[0]));
                }
                this.designerStyle = value;
                base.ContainingTheme.AmbientPropertyChanged(System.Workflow.ComponentModel.Design.AmbientProperty.DesignerSize);
            }
        }

        [SRCategory("WorkflowAppearanceCategory", "System.Workflow.ComponentModel.Design.DesignerResources"), SRDescription("GrayscaleWorkflowDesc", "System.Workflow.ComponentModel.Design.DesignerResources"), DefaultValue(false), DispId(6)]
        public virtual bool DrawGrayscale
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.drawGrayscale;
            }
            set
            {
                if (base.ReadOnly)
                {
                    throw new InvalidOperationException(DR.GetString("ThemePropertyReadOnly", new object[0]));
                }
                this.drawGrayscale = value;
            }
        }

        [DispId(0x15), SRCategory("ActivityAppearanceCategory", "System.Workflow.ComponentModel.Design.DesignerResources"), DefaultValue(true), SRDescription("DrawRoundedDesignersDesc", "System.Workflow.ComponentModel.Design.DesignerResources")]
        public virtual bool DrawRounded
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.drawRounded;
            }
            set
            {
                if (base.ReadOnly)
                {
                    throw new InvalidOperationException(DR.GetString("ThemePropertyReadOnly", new object[0]));
                }
                this.drawRounded = value;
            }
        }

        [DefaultValue(false), SRCategory("WorkflowAppearanceCategory", "System.Workflow.ComponentModel.Design.DesignerResources"), SRDescription("WorkflowShadowDesc", "System.Workflow.ComponentModel.Design.DesignerResources"), DispId(14)]
        public virtual bool DrawShadow
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.drawShadow;
            }
            set
            {
                if (base.ReadOnly)
                {
                    throw new InvalidOperationException(DR.GetString("ThemePropertyReadOnly", new object[0]));
                }
                this.drawShadow = value;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public Brush DropIndicatorBrush
        {
            get
            {
                if (this.dropIndicatorBrush == null)
                {
                    this.dropIndicatorBrush = new SolidBrush(this.dropIndicatorColor);
                }
                return this.dropIndicatorBrush;
            }
        }

        [DispId(7), TypeConverter(typeof(ColorPickerConverter)), SRDescription("DropHiliteDesc", "System.Workflow.ComponentModel.Design.DesignerResources"), SRCategory("ForegroundCategory", "System.Workflow.ComponentModel.Design.DesignerResources"), Editor(typeof(ColorPickerEditor), typeof(UITypeEditor))]
        public virtual System.Drawing.Color DropIndicatorColor
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.dropIndicatorColor;
            }
            set
            {
                if (base.ReadOnly)
                {
                    throw new InvalidOperationException(DR.GetString("ThemePropertyReadOnly", new object[0]));
                }
                this.dropIndicatorColor = value;
                if (this.dropIndicatorPen != null)
                {
                    this.dropIndicatorPen.Dispose();
                    this.dropIndicatorPen = null;
                }
                if (this.dropIndicatorBrush != null)
                {
                    this.dropIndicatorBrush.Dispose();
                    this.dropIndicatorBrush = null;
                }
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public Pen DropIndicatorPen
        {
            get
            {
                if (this.dropIndicatorPen == null)
                {
                    this.dropIndicatorPen = new Pen(this.dropIndicatorColor, (float) this.BorderWidth);
                }
                return this.dropIndicatorPen;
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        internal Size DropIndicatorSize
        {
            get
            {
                return DropIndicatorSizes[(int) this.designerStyle];
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public System.Drawing.Font Font
        {
            get
            {
                if (this.font == null)
                {
                    if ((this.fontName == null) || (this.fontName.Length == 0))
                    {
                        this.fontName = WorkflowTheme.GetDefaultFont().FontFamily.Name;
                    }
                    ArrayList list = new ArrayList(SupportedFonts);
                    if (!list.Contains(this.fontName))
                    {
                        this.fontName = WorkflowTheme.GetDefaultFont().FontFamily.Name;
                    }
                    this.font = new System.Drawing.Font(this.fontName, this.FontSize);
                }
                return this.font;
            }
        }

        [SRCategory("WorkflowAppearanceCategory", "System.Workflow.ComponentModel.Design.DesignerResources"), TypeConverter(typeof(FontFamilyConverter)), SRDescription("FontDesc", "System.Workflow.ComponentModel.Design.DesignerResources"), DispId(1)]
        public virtual string FontName
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.fontName;
            }
            set
            {
                if (base.ReadOnly)
                {
                    throw new InvalidOperationException(DR.GetString("ThemePropertyReadOnly", new object[0]));
                }
                if ((value == null) || (value.Length == 0))
                {
                    throw new Exception(DR.GetString("EmptyFontFamilyNotSupported", new object[0]));
                }
                try
                {
                    System.Drawing.Font font = new System.Drawing.Font(value, this.FontSize);
                    if (font != null)
                    {
                        font.Dispose();
                    }
                }
                catch (Exception exception)
                {
                    throw new Exception(DR.GetString("FontFamilyNotSupported", new object[] { value }), exception);
                }
                this.fontName = value;
                if (this.font != null)
                {
                    this.font.Dispose();
                    this.font = null;
                }
                if (this.boldFont != null)
                {
                    this.boldFont.Dispose();
                    this.boldFont = null;
                }
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        private float FontSize
        {
            get
            {
                if (this.useOperatingSystemSettings)
                {
                    return SystemInformation.MenuFont.SizeInPoints;
                }
                return FontSizes[(int) this.DesignerSize];
            }
        }

        private static float[] FontSizes
        {
            get
            {
                if (fontSizes == null)
                {
                    fontSizes = new float[] { WorkflowTheme.GetDefaultFont().SizeInPoints - 2f, WorkflowTheme.GetDefaultFont().SizeInPoints, WorkflowTheme.GetDefaultFont().SizeInPoints + 2f };
                }
                return fontSizes;
            }
        }

        [Editor(typeof(ColorPickerEditor), typeof(UITypeEditor)), DispId(10), SRDescription("WorkflowForegroundDesc", "System.Workflow.ComponentModel.Design.DesignerResources"), TypeConverter(typeof(ColorPickerConverter)), SRCategory("ForegroundCategory", "System.Workflow.ComponentModel.Design.DesignerResources")]
        public virtual System.Drawing.Color ForeColor
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

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public Pen ForegroundPen
        {
            get
            {
                if (this.foregroundPen == null)
                {
                    this.foregroundPen = new Pen(this.foreColor, 1f);
                    this.foregroundPen.DashStyle = DashStyle.Dot;
                }
                return this.foregroundPen;
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public virtual Size GlyphSize
        {
            get
            {
                return GlyphSizes[(int) this.designerStyle];
            }
        }

        [SRDescription("GridColorDesc", "System.Workflow.ComponentModel.Design.DesignerResources"), Editor(typeof(ColorPickerEditor), typeof(UITypeEditor)), TypeConverter(typeof(ColorPickerConverter)), SRCategory("BackgroundCategory", "System.Workflow.ComponentModel.Design.DesignerResources"), DispId(0x13)]
        public virtual System.Drawing.Color GridColor
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.gridColor;
            }
            set
            {
                if (base.ReadOnly)
                {
                    throw new InvalidOperationException(DR.GetString("ThemePropertyReadOnly", new object[0]));
                }
                this.gridColor = value;
                if (this.majorGridPen != null)
                {
                    this.majorGridPen.Dispose();
                    this.majorGridPen = null;
                }
                if (this.majorGridBrush != null)
                {
                    this.majorGridBrush.Dispose();
                    this.majorGridBrush = null;
                }
                if (this.minorGridPen != null)
                {
                    this.minorGridPen.Dispose();
                    this.minorGridPen = null;
                }
                if (this.minorGridBrush != null)
                {
                    this.minorGridBrush.Dispose();
                    this.minorGridBrush = null;
                }
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public virtual Size GridSize
        {
            get
            {
                return GridSizes[(int) this.designerStyle];
            }
        }

        [SRCategory("BackgroundCategory", "System.Workflow.ComponentModel.Design.DesignerResources"), SRDescription("GridStyleDesc", "System.Workflow.ComponentModel.Design.DesignerResources"), DefaultValue(1), DispId(0x12)]
        public virtual DashStyle GridStyle
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.gridStyle;
            }
            set
            {
                if (base.ReadOnly)
                {
                    throw new InvalidOperationException(DR.GetString("ThemePropertyReadOnly", new object[0]));
                }
                this.gridStyle = value;
                if (this.majorGridPen != null)
                {
                    this.majorGridPen.Dispose();
                    this.majorGridPen = null;
                }
                if (this.minorGridPen != null)
                {
                    this.minorGridPen.Dispose();
                    this.minorGridPen = null;
                }
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        internal Size MagnifierSize
        {
            get
            {
                return MagnifierSizes[(int) this.designerStyle];
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Brush MajorGridBrush
        {
            get
            {
                if (this.majorGridBrush == null)
                {
                    this.majorGridBrush = new SolidBrush(this.gridColor);
                }
                return this.majorGridBrush;
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Pen MajorGridPen
        {
            get
            {
                if (this.majorGridPen == null)
                {
                    this.majorGridPen = new Pen(this.gridColor, 1f);
                    this.majorGridPen.DashStyle = DashStyle.Dash;
                }
                return this.majorGridPen;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public virtual Size Margin
        {
            get
            {
                return MarginSizes[(int) this.designerStyle];
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        internal Brush MinorGridBrush
        {
            get
            {
                if (this.minorGridBrush == null)
                {
                    int red = Math.Min(this.gridColor.R + 0x20, 0xff);
                    int green = Math.Min(this.gridColor.G + 0x20, 0xff);
                    System.Drawing.Color color = System.Drawing.Color.FromArgb(this.gridColor.A, red, green, Math.Min(this.gridColor.B + 0x20, 0xff));
                    this.minorGridBrush = new SolidBrush(color);
                }
                return this.minorGridBrush;
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Pen MinorGridPen
        {
            get
            {
                if (this.minorGridPen == null)
                {
                    int red = Math.Min(this.gridColor.R + 0x20, 0xff);
                    int green = Math.Min(this.gridColor.G + 0x20, 0xff);
                    System.Drawing.Color color = System.Drawing.Color.FromArgb(this.gridColor.A, red, green, Math.Min(this.gridColor.B + 0x20, 0xff));
                    this.minorGridPen = new Pen(color, 1f);
                    this.minorGridPen.DashStyle = DashStyle.Dot;
                }
                return this.minorGridPen;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public Brush ReadonlyIndicatorBrush
        {
            get
            {
                if (this.readonlyIndicatorBrush == null)
                {
                    this.readonlyIndicatorBrush = new SolidBrush(System.Drawing.Color.FromArgb(20, this.readonlyIndicatorColor));
                }
                return this.readonlyIndicatorBrush;
            }
        }

        [DispId(12), SRCategory("ForegroundCategory", "System.Workflow.ComponentModel.Design.DesignerResources"), Editor(typeof(ColorPickerEditor), typeof(UITypeEditor)), TypeConverter(typeof(ColorPickerConverter)), SRDescription("LockColorDesc", "System.Workflow.ComponentModel.Design.DesignerResources")]
        public virtual System.Drawing.Color ReadonlyIndicatorColor
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.readonlyIndicatorColor;
            }
            set
            {
                if (base.ReadOnly)
                {
                    throw new InvalidOperationException(DR.GetString("ThemePropertyReadOnly", new object[0]));
                }
                this.readonlyIndicatorColor = value;
                if (this.readonlyIndicatorBrush != null)
                {
                    this.readonlyIndicatorBrush.Dispose();
                    this.readonlyIndicatorBrush = null;
                }
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        internal Size ScrollIndicatorSize
        {
            get
            {
                return ScrollIndicatorSizes[(int) this.designerStyle];
            }
        }

        [Editor(typeof(ColorPickerEditor), typeof(UITypeEditor)), TypeConverter(typeof(ColorPickerConverter)), SRDescription("SelectionForegroundDesc", "System.Workflow.ComponentModel.Design.DesignerResources"), SRCategory("ForegroundCategory", "System.Workflow.ComponentModel.Design.DesignerResources"), DispId(8)]
        public virtual System.Drawing.Color SelectionForeColor
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.selectionForeColor;
            }
            set
            {
                if (base.ReadOnly)
                {
                    throw new InvalidOperationException(DR.GetString("ThemePropertyReadOnly", new object[0]));
                }
                this.selectionForeColor = value;
                if (this.selectionForegroundPen != null)
                {
                    this.selectionForegroundPen.Dispose();
                    this.selectionForegroundPen = null;
                }
                if (this.selectionForegroundBrush != null)
                {
                    this.selectionForegroundBrush.Dispose();
                    this.selectionForegroundBrush = null;
                }
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public Brush SelectionForegroundBrush
        {
            get
            {
                if (this.selectionForegroundBrush == null)
                {
                    this.selectionForegroundBrush = new SolidBrush(this.selectionForeColor);
                }
                return this.selectionForegroundBrush;
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Pen SelectionForegroundPen
        {
            get
            {
                if (this.selectionForegroundPen == null)
                {
                    this.selectionForegroundPen = new Pen(this.selectionForeColor, 1f);
                }
                return this.selectionForegroundPen;
            }
        }

        [TypeConverter(typeof(ColorPickerConverter)), Editor(typeof(ColorPickerEditor), typeof(UITypeEditor)), DispId(9), SRCategory("ForegroundCategory", "System.Workflow.ComponentModel.Design.DesignerResources"), SRDescription("SelectionPatternDesc", "System.Workflow.ComponentModel.Design.DesignerResources")]
        public virtual System.Drawing.Color SelectionPatternColor
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.selectionPatternColor;
            }
            set
            {
                if (base.ReadOnly)
                {
                    throw new InvalidOperationException(DR.GetString("ThemePropertyReadOnly", new object[0]));
                }
                this.selectionPatternColor = value;
                if (this.selectionPatternPen != null)
                {
                    this.selectionPatternPen.Dispose();
                    this.selectionPatternPen = null;
                }
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public Pen SelectionPatternPen
        {
            get
            {
                if (this.selectionPatternPen == null)
                {
                    this.selectionPatternPen = new Pen(this.selectionPatternColor, 1f);
                    this.selectionPatternPen.DashStyle = DashStyle.Dot;
                }
                return this.selectionPatternPen;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public virtual Size SelectionSize
        {
            get
            {
                return SelectionSizes[(int) this.designerStyle];
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        internal int ShadowDepth
        {
            get
            {
                if (!this.drawShadow)
                {
                    return 0;
                }
                return 6;
            }
        }

        [SRCategory("WorkflowAppearanceCategory", "System.Workflow.ComponentModel.Design.DesignerResources"), SRDescription("ShowConfigErrorDesc", "System.Workflow.ComponentModel.Design.DesignerResources"), DispId(3), DefaultValue(true)]
        public virtual bool ShowConfigErrors
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.showConfigErrors;
            }
            set
            {
                if (base.ReadOnly)
                {
                    throw new InvalidOperationException(DR.GetString("ThemePropertyReadOnly", new object[0]));
                }
                this.showConfigErrors = value;
            }
        }

        [SRDescription("DesignerBorderDesc", "System.Workflow.ComponentModel.Design.DesignerResources"), SRCategory("ActivityAppearanceCategory", "System.Workflow.ComponentModel.Design.DesignerResources"), DefaultValue(true), DispId(0x18)]
        public virtual bool ShowDesignerBorder
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.showDesignerBorder;
            }
            set
            {
                if (base.ReadOnly)
                {
                    throw new InvalidOperationException(DR.GetString("ThemePropertyReadOnly", new object[0]));
                }
                this.showDesignerBorder = value;
            }
        }

        [DispId(0x11), SRCategory("BackgroundCategory", "System.Workflow.ComponentModel.Design.DesignerResources"), DefaultValue(false), SRDescription("ShowGridDesc", "System.Workflow.ComponentModel.Design.DesignerResources")]
        public virtual bool ShowGrid
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.showGrid;
            }
            set
            {
                if (base.ReadOnly)
                {
                    throw new InvalidOperationException(DR.GetString("ThemePropertyReadOnly", new object[0]));
                }
                this.showGrid = value;
            }
        }

        internal static string[] SupportedFonts
        {
            get
            {
                ArrayList list = new ArrayList();
                foreach (FontFamily family in FontFamily.Families)
                {
                    list.Add(family.Name);
                }
                list.Sort(CaseInsensitiveComparer.Default);
                return (string[]) list.ToArray(typeof(string));
            }
        }

        [SRCategory("WorkflowAppearanceCategory", "System.Workflow.ComponentModel.Design.DesignerResources"), DefaultValue(0), DispId(2), SRDescription("TextQualityDesc", "System.Workflow.ComponentModel.Design.DesignerResources")]
        public virtual System.Workflow.ComponentModel.Design.TextQuality TextQuality
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.textQuality;
            }
            set
            {
                if (base.ReadOnly)
                {
                    throw new InvalidOperationException(DR.GetString("ThemePropertyReadOnly", new object[0]));
                }
                this.textQuality = value;
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public virtual bool UseOperatingSystemSettings
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.useOperatingSystemSettings;
            }
            internal set
            {
                this.useOperatingSystemSettings = value;
                if (this.useOperatingSystemSettings)
                {
                    SystemEvents.UserPreferenceChanged += new UserPreferenceChangedEventHandler(this.OnOperatingSystemSettingsChanged);
                    this.OnOperatingSystemSettingsChanged(this, new UserPreferenceChangedEventArgs(UserPreferenceCategory.Color));
                }
                else
                {
                    SystemEvents.UserPreferenceChanged -= new UserPreferenceChangedEventHandler(this.OnOperatingSystemSettingsChanged);
                }
            }
        }

        [DispId(0x10), SRCategory("BackgroundCategory", "System.Workflow.ComponentModel.Design.DesignerResources"), DefaultValue(12), SRDescription("WatermarkAlignmentDesc", "System.Workflow.ComponentModel.Design.DesignerResources")]
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

        [SRCategory("BackgroundCategory", "System.Workflow.ComponentModel.Design.DesignerResources"), Editor(typeof(ImageBrowserEditor), typeof(UITypeEditor)), SRDescription("WorkflowWatermarkDesc", "System.Workflow.ComponentModel.Design.DesignerResources"), DispId(15)]
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

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Image WorkflowWatermarkImage
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
    }
}

