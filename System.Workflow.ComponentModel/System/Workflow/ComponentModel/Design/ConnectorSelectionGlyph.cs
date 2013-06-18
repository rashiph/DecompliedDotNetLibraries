namespace System.Workflow.ComponentModel.Design
{
    using System;

    internal abstract class ConnectorSelectionGlyph : SelectionGlyph
    {
        protected int connectorIndex;
        protected bool isPrimarySelectionGlyph = true;

        public ConnectorSelectionGlyph(int connectorIndex, bool isPrimarySelectionGlyph)
        {
            this.connectorIndex = connectorIndex;
            this.isPrimarySelectionGlyph = isPrimarySelectionGlyph;
        }
    }
}

