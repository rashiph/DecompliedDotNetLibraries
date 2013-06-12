namespace System.Web.UI
{
    using System;
    using System.Collections.Specialized;

    public sealed class CompiledBindableTemplateBuilder : IBindableTemplate, ITemplate
    {
        private BuildTemplateMethod _buildTemplateMethod;
        private ExtractTemplateValuesMethod _extractTemplateValuesMethod;

        public CompiledBindableTemplateBuilder(BuildTemplateMethod buildTemplateMethod, ExtractTemplateValuesMethod extractTemplateValuesMethod)
        {
            this._buildTemplateMethod = buildTemplateMethod;
            this._extractTemplateValuesMethod = extractTemplateValuesMethod;
        }

        public IOrderedDictionary ExtractValues(Control container)
        {
            if (this._extractTemplateValuesMethod != null)
            {
                return this._extractTemplateValuesMethod(container);
            }
            return new OrderedDictionary();
        }

        public void InstantiateIn(Control container)
        {
            this._buildTemplateMethod(container);
        }
    }
}

