namespace System.Workflow.ComponentModel.Design
{
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Drawing.Design;
    using System.Runtime;
    using System.Runtime.InteropServices;

    public class ActivityPreviewDesignerTheme : CompositeDesignerTheme
    {
        private const int DefaultItemCount = 5;
        internal static readonly Bitmap EditButtonImage = (DR.GetImage("EditModeIcon") as Bitmap);
        private static readonly Size[] ItemSizes = new Size[] { new Size(20, 20), new Size(20, 20), new Size(30, 30) };
        internal static readonly Bitmap LeftScrollImage = (DR.GetImage("MoveLeft") as Bitmap);
        internal static readonly Bitmap LeftScrollImageUp = (DR.GetImage("MoveLeftUp") as Bitmap);
        private Color previewBackColor;
        private Brush previewBackgroundBrush;
        private Color previewBorderColor;
        private Pen previewBorderPen;
        internal static readonly Bitmap PreviewButtonImage = (DR.GetImage("PreviewModeIcon") as Bitmap);
        private static readonly Size[] PreviewButtonSizes = new Size[] { new Size(0x10, 0x10), new Size(0x10, 0x10), new Size(20, 20) };
        private Color previewForeColor;
        private Brush previewForegroundBrush;
        internal static readonly Bitmap PreviewImage = (DR.GetImage("PreviewIndicator") as Bitmap);
        private static readonly Size[] PreviewWindowSizes = new Size[] { new Size(0xac, 120), new Size(0xac, 120), new Size(0xd4, 160) };
        internal static readonly Bitmap RightScrollImage = (DR.GetImage("MoveRight") as Bitmap);
        internal static readonly Bitmap RightScrollImageUp = (DR.GetImage("MoveRightUp") as Bitmap);

        public ActivityPreviewDesignerTheme(WorkflowTheme theme) : base(theme)
        {
            this.previewForeColor = Color.WhiteSmoke;
            this.previewBackColor = Color.White;
            this.previewBorderColor = Color.Gray;
        }

        private void ApplySystemColors()
        {
            this.PreviewForeColor = SystemColors.ButtonFace;
            this.PreviewBackColor = SystemColors.Window;
            this.PreviewBorderColor = SystemColors.ControlDarkDark;
            this.BorderColor = SystemColors.ControlDarkDark;
        }

        protected override void Dispose(bool disposing)
        {
            try
            {
                if (this.previewForegroundBrush != null)
                {
                    this.previewForegroundBrush.Dispose();
                    this.previewForegroundBrush = null;
                }
                if (this.previewBackgroundBrush != null)
                {
                    this.previewBackgroundBrush.Dispose();
                    this.previewBackgroundBrush = null;
                }
                if (this.previewBorderPen != null)
                {
                    this.previewBorderPen.Dispose();
                    this.previewBorderPen = null;
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
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
            if (ambientProperty == AmbientProperty.DesignerSize)
            {
                this.PreviewBorderColor = this.previewBorderColor;
            }
            else if (ambientProperty == AmbientProperty.OperatingSystemSetting)
            {
                this.ApplySystemColors();
            }
        }

        [TypeConverter(typeof(ColorPickerConverter)), DispId(14), SRDescription("PreviewBackColorDesc", "System.Workflow.ComponentModel.Design.DesignerResources"), SRCategory("BackgroundCategory", "System.Workflow.ComponentModel.Design.DesignerResources"), Editor(typeof(ColorPickerEditor), typeof(UITypeEditor))]
        public Color PreviewBackColor
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.previewBackColor;
            }
            set
            {
                if (base.ReadOnly)
                {
                    throw new InvalidOperationException(DR.GetString("ThemePropertyReadOnly", new object[0]));
                }
                this.previewBackColor = value;
                if (this.previewBackgroundBrush != null)
                {
                    this.previewBackgroundBrush.Dispose();
                    this.previewBackgroundBrush = null;
                }
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        internal Brush PreviewBackgroundBrush
        {
            get
            {
                if (this.previewBackgroundBrush == null)
                {
                    this.previewBackgroundBrush = new SolidBrush(this.previewBackColor);
                }
                return this.previewBackgroundBrush;
            }
        }

        [DispId(15), TypeConverter(typeof(ColorPickerConverter)), SRDescription("PreviewBorderColorDesc", "System.Workflow.ComponentModel.Design.DesignerResources"), SRCategory("ForegroundCategory", "System.Workflow.ComponentModel.Design.DesignerResources"), Editor(typeof(ColorPickerEditor), typeof(UITypeEditor))]
        public Color PreviewBorderColor
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.previewBorderColor;
            }
            set
            {
                if (base.ReadOnly)
                {
                    throw new InvalidOperationException(DR.GetString("ThemePropertyReadOnly", new object[0]));
                }
                this.previewBorderColor = value;
                if (this.previewBorderPen != null)
                {
                    this.previewBorderPen.Dispose();
                    this.previewBorderPen = null;
                }
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        internal Pen PreviewBorderPen
        {
            get
            {
                if (this.previewBorderPen == null)
                {
                    this.previewBorderPen = new Pen(this.previewBorderColor, (float) base.BorderWidth);
                }
                return this.previewBorderPen;
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        internal Size PreviewButtonSize
        {
            get
            {
                return PreviewButtonSizes[(int) base.ContainingTheme.AmbientTheme.DesignerSize];
            }
        }

        [SRDescription("PreviewForeColorDesc", "System.Workflow.ComponentModel.Design.DesignerResources"), TypeConverter(typeof(ColorPickerConverter)), DispId(13), SRCategory("ForegroundCategory", "System.Workflow.ComponentModel.Design.DesignerResources"), Editor(typeof(ColorPickerEditor), typeof(UITypeEditor))]
        public Color PreviewForeColor
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.previewForeColor;
            }
            set
            {
                if (base.ReadOnly)
                {
                    throw new InvalidOperationException(DR.GetString("ThemePropertyReadOnly", new object[0]));
                }
                this.previewForeColor = value;
                if (this.previewForegroundBrush != null)
                {
                    this.previewForegroundBrush.Dispose();
                    this.previewForegroundBrush = null;
                }
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        internal Brush PreviewForegroundBrush
        {
            get
            {
                if (this.previewForegroundBrush == null)
                {
                    this.previewForegroundBrush = new SolidBrush(this.previewForeColor);
                }
                return this.previewForegroundBrush;
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        internal int PreviewItemCount
        {
            get
            {
                return 5;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        internal Size PreviewItemSize
        {
            get
            {
                return ItemSizes[(int) base.ContainingTheme.AmbientTheme.DesignerSize];
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        internal Size PreviewWindowSize
        {
            get
            {
                return PreviewWindowSizes[(int) base.ContainingTheme.AmbientTheme.DesignerSize];
            }
        }
    }
}

