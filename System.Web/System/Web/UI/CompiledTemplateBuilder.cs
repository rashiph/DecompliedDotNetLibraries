namespace System.Web.UI
{
    using System;

    public sealed class CompiledTemplateBuilder : ITemplate
    {
        private BuildTemplateMethod _buildTemplateMethod;

        public CompiledTemplateBuilder(BuildTemplateMethod buildTemplateMethod)
        {
            this._buildTemplateMethod = buildTemplateMethod;
        }

        public void InstantiateIn(Control container)
        {
            this._buildTemplateMethod(container);
        }
    }
}

