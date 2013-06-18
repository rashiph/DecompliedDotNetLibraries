namespace System.Windows.Forms.Design
{
    using System;
    using System.ComponentModel;
    using System.Design;
    using System.Drawing;

    public class DesignerOptions
    {
        private bool enableComponentCache;
        private bool enableInSituEditing = true;
        private Size gridSize = new Size(8, 8);
        private const int maxGridSize = 200;
        private const int minGridSize = 2;
        private bool objectBoundSmartTagAutoShow = true;
        private bool showGrid = true;
        private bool snapToGrid = true;
        private bool useSmartTags;
        private bool useSnapLines;

        [System.Design.SRCategory("DesignerOptions_EnableInSituEditingCat"), SRDisplayName("DesignerOptions_EnableInSituEditingDisplay"), System.Design.SRDescription("DesignerOptions_EnableInSituEditingDesc"), Browsable(false)]
        public virtual bool EnableInSituEditing
        {
            get
            {
                return this.enableInSituEditing;
            }
            set
            {
                this.enableInSituEditing = value;
            }
        }

        [System.Design.SRCategory("DesignerOptions_LayoutSettings"), System.Design.SRDescription("DesignerOptions_GridSizeDesc")]
        public virtual Size GridSize
        {
            get
            {
                return this.gridSize;
            }
            set
            {
                if (value.Width < 2)
                {
                    value.Width = 2;
                }
                if (value.Height < 2)
                {
                    value.Height = 2;
                }
                if (value.Width > 200)
                {
                    value.Width = 200;
                }
                if (value.Height > 200)
                {
                    value.Height = 200;
                }
                this.gridSize = value;
            }
        }

        [System.Design.SRDescription("DesignerOptions_ObjectBoundSmartTagAutoShow"), SRDisplayName("DesignerOptions_ObjectBoundSmartTagAutoShowDisplayName"), System.Design.SRCategory("DesignerOptions_ObjectBoundSmartTagSettings")]
        public virtual bool ObjectBoundSmartTagAutoShow
        {
            get
            {
                return this.objectBoundSmartTagAutoShow;
            }
            set
            {
                this.objectBoundSmartTagAutoShow = value;
            }
        }

        [System.Design.SRDescription("DesignerOptions_ShowGridDesc"), System.Design.SRCategory("DesignerOptions_LayoutSettings")]
        public virtual bool ShowGrid
        {
            get
            {
                return this.showGrid;
            }
            set
            {
                this.showGrid = value;
            }
        }

        [System.Design.SRCategory("DesignerOptions_LayoutSettings"), System.Design.SRDescription("DesignerOptions_SnapToGridDesc")]
        public virtual bool SnapToGrid
        {
            get
            {
                return this.snapToGrid;
            }
            set
            {
                this.snapToGrid = value;
            }
        }

        [System.Design.SRDescription("DesignerOptions_OptimizedCodeGen"), System.Design.SRCategory("DesignerOptions_CodeGenSettings"), SRDisplayName("DesignerOptions_CodeGenDisplay")]
        public virtual bool UseOptimizedCodeGeneration
        {
            get
            {
                return this.enableComponentCache;
            }
            set
            {
                this.enableComponentCache = value;
            }
        }

        [System.Design.SRCategory("DesignerOptions_LayoutSettings"), System.Design.SRDescription("DesignerOptions_UseSmartTags")]
        public virtual bool UseSmartTags
        {
            get
            {
                return this.useSmartTags;
            }
            set
            {
                this.useSmartTags = value;
            }
        }

        [System.Design.SRDescription("DesignerOptions_UseSnapLines"), System.Design.SRCategory("DesignerOptions_LayoutSettings")]
        public virtual bool UseSnapLines
        {
            get
            {
                return this.useSnapLines;
            }
            set
            {
                this.useSnapLines = value;
            }
        }
    }
}

