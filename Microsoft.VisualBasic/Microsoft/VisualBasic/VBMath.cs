namespace Microsoft.VisualBasic
{
    using Microsoft.VisualBasic.CompilerServices;
    using System;
    using System.Runtime;

    [StandardModule]
    public sealed class VBMath
    {
        private static float GetTimer()
        {
            DateTime now = DateTime.Now;
            return (float) (((((60 * now.Hour) + now.Minute) * 60) + now.Second) + (((double) now.Millisecond) / 1000.0));
        }

        public static void Randomize()
        {
            ProjectData projectData = ProjectData.GetProjectData();
            float timer = GetTimer();
            int rndSeed = projectData.m_rndSeed;
            int num = BitConverter.ToInt32(BitConverter.GetBytes(timer), 0);
            num = ((num & 0xffff) ^ (num >> 0x10)) << 8;
            rndSeed = (rndSeed & -16776961) | num;
            projectData.m_rndSeed = rndSeed;
        }

        public static void Randomize(double Number)
        {
            int num;
            ProjectData projectData = ProjectData.GetProjectData();
            int rndSeed = projectData.m_rndSeed;
            if (BitConverter.IsLittleEndian)
            {
                num = BitConverter.ToInt32(BitConverter.GetBytes(Number), 4);
            }
            else
            {
                num = BitConverter.ToInt32(BitConverter.GetBytes(Number), 0);
            }
            num = ((num & 0xffff) ^ (num >> 0x10)) << 8;
            rndSeed = (rndSeed & -16776961) | num;
            projectData.m_rndSeed = rndSeed;
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public static float Rnd()
        {
            return Rnd(1f);
        }

        public static float Rnd(float Number)
        {
            ProjectData projectData = ProjectData.GetProjectData();
            int rndSeed = projectData.m_rndSeed;
            if (Number != 0.0)
            {
                if (Number < 0.0)
                {
                    long num3 = BitConverter.ToInt32(BitConverter.GetBytes(Number), 0);
                    num3 &= (long) 0xffffffffL;
                    rndSeed = (int) ((num3 + (num3 >> 0x18)) & 0xffffffL);
                }
                rndSeed = (int) (((rndSeed * 0x43fd43fdL) + 0xc39ec3L) & 0xffffffL);
            }
            projectData.m_rndSeed = rndSeed;
            return (((float) rndSeed) / 1.677722E+07f);
        }
    }
}

