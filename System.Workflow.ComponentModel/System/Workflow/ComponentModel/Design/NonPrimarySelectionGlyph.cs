namespace System.Workflow.ComponentModel.Design
{
    using System;

    internal sealed class NonPrimarySelectionGlyph : SelectionGlyph
    {
        private static NonPrimarySelectionGlyph defaultNonPrimarySelectionGlyph;

        internal static NonPrimarySelectionGlyph Default
        {
            get
            {
                if (defaultNonPrimarySelectionGlyph == null)
                {
                    defaultNonPrimarySelectionGlyph = new NonPrimarySelectionGlyph();
                }
                return defaultNonPrimarySelectionGlyph;
            }
        }

        public override bool IsPrimarySelection
        {
            get
            {
                return false;
            }
        }
    }
}

