namespace System.Workflow.ComponentModel.Design
{
    using System;

    internal sealed class PrimarySelectionGlyph : SelectionGlyph
    {
        private static PrimarySelectionGlyph defaultPrimarySelectionGlyph;

        internal static PrimarySelectionGlyph Default
        {
            get
            {
                if (defaultPrimarySelectionGlyph == null)
                {
                    defaultPrimarySelectionGlyph = new PrimarySelectionGlyph();
                }
                return defaultPrimarySelectionGlyph;
            }
        }

        public override bool IsPrimarySelection
        {
            get
            {
                return true;
            }
        }
    }
}

