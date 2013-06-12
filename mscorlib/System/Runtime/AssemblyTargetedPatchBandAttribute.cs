namespace System.Runtime
{
    using System;

    [AttributeUsage(AttributeTargets.Assembly, Inherited=false)]
    public sealed class AssemblyTargetedPatchBandAttribute : Attribute
    {
        private string m_targetedPatchBand;

        public AssemblyTargetedPatchBandAttribute(string targetedPatchBand)
        {
            this.m_targetedPatchBand = targetedPatchBand;
        }

        public string TargetedPatchBand
        {
            get
            {
                return this.m_targetedPatchBand;
            }
        }
    }
}

