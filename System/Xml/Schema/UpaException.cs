namespace System.Xml.Schema
{
    using System;

    internal class UpaException : Exception
    {
        private object particle1;
        private object particle2;

        public UpaException(object particle1, object particle2)
        {
            this.particle1 = particle1;
            this.particle2 = particle2;
        }

        public object Particle1
        {
            get
            {
                return this.particle1;
            }
        }

        public object Particle2
        {
            get
            {
                return this.particle2;
            }
        }
    }
}

