namespace System.Resources
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.Runtime.CompilerServices;

    internal class ResourceFallbackManager : IEnumerable<CultureInfo>, IEnumerable
    {
        private CultureInfo m_neutralResourcesCulture;
        private CultureInfo m_startingCulture;
        private bool m_useParents;

        internal ResourceFallbackManager(CultureInfo startingCulture, CultureInfo neutralResourcesCulture, bool useParents)
        {
            if (startingCulture != null)
            {
                this.m_startingCulture = startingCulture;
            }
            else
            {
                this.m_startingCulture = CultureInfo.CurrentUICulture;
            }
            this.m_neutralResourcesCulture = neutralResourcesCulture;
            this.m_useParents = useParents;
        }

        public IEnumerator<CultureInfo> GetEnumerator()
        {
            bool iteratorVariable0 = false;
            CultureInfo startingCulture = this.m_startingCulture;
        Label_PostSwitchInIterator:;
            if ((this.m_neutralResourcesCulture != null) && (startingCulture.Name == this.m_neutralResourcesCulture.Name))
            {
                yield return CultureInfo.InvariantCulture;
                iteratorVariable0 = true;
            }
            else
            {
                yield return startingCulture;
                startingCulture = startingCulture.Parent;
                if (this.m_useParents && !startingCulture.HasInvariantCultureName)
                {
                    goto Label_PostSwitchInIterator;
                }
            }
            if ((this.m_useParents && !this.m_startingCulture.HasInvariantCultureName) && !iteratorVariable0)
            {
                yield return CultureInfo.InvariantCulture;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

    }
}

