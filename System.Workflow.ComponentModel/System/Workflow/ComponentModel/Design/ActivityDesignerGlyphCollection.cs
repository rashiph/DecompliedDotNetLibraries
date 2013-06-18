namespace System.Workflow.ComponentModel.Design
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Runtime;

    public sealed class ActivityDesignerGlyphCollection : List<DesignerGlyph>
    {
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public ActivityDesignerGlyphCollection()
        {
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public ActivityDesignerGlyphCollection(IEnumerable<DesignerGlyph> glyphs) : base(glyphs)
        {
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public ActivityDesignerGlyphCollection(ActivityDesignerGlyphCollection glyphs) : base(glyphs)
        {
        }

        internal DesignerGlyph this[Type type]
        {
            get
            {
                if (type == null)
                {
                    throw new ArgumentNullException();
                }
                DesignerGlyph glyph = null;
                foreach (DesignerGlyph glyph2 in this)
                {
                    if (glyph2.GetType() == type)
                    {
                        return glyph2;
                    }
                    if (type.IsAssignableFrom(glyph2.GetType()) && (glyph == null))
                    {
                        glyph = glyph2;
                    }
                }
                return glyph;
            }
        }
    }
}

