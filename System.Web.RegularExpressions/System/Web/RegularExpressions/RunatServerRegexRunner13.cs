namespace System.Web.RegularExpressions
{
    using System;
    using System.Globalization;
    using System.Text.RegularExpressions;

    internal class RunatServerRegexRunner13 : RegexRunner
    {
        public override bool FindFirstChar()
        {
            // This item is obfuscated and can not be translated.
            string runtext = base.runtext;
            int runtextend = base.runtextend;
            for (int i = base.runtextpos + 4; i < runtextend; i = 5 + i)
            {
                int num2 = char.ToLower(runtext[i], CultureInfo.InvariantCulture);
                if (num2 == 't')
                {
                    num2 = i;
                    if (((char.ToLower(runtext[--num2], CultureInfo.InvariantCulture) == 'a') && (char.ToLower(runtext[--num2], CultureInfo.InvariantCulture) == 'n')) && ((char.ToLower(runtext[--num2], CultureInfo.InvariantCulture) == 'u') && (char.ToLower(runtext[--num2], CultureInfo.InvariantCulture) == 'r')))
                    {
                        base.runtextpos = num2;
                        return true;
                    }
                }
                else
                {
                    num2 -= 0x61;
                    if (num2 <= 20)
                    {
                    }
                }
            }
            base.runtextpos = base.runtextend;
            return false;
        }

        public override void Go()
        {
            int num5;
            string runtext = base.runtext;
            int runtextstart = base.runtextstart;
            int runtextbeg = base.runtextbeg;
            int runtextend = base.runtextend;
            int runtextpos = base.runtextpos;
            int[] runtrack = base.runtrack;
            int runtrackpos = base.runtrackpos;
            int[] runstack = base.runstack;
            int runstackpos = base.runstackpos;
            runtrack[--runtrackpos] = runtextpos;
            runtrack[--runtrackpos] = 0;
            runstack[--runstackpos] = runtextpos;
            runtrack[--runtrackpos] = 1;
            if ((((5 > (runtextend - runtextpos)) || (char.ToLower(runtext[runtextpos], CultureInfo.InvariantCulture) != 'r')) || ((char.ToLower(runtext[runtextpos + 1], CultureInfo.InvariantCulture) != 'u') || (char.ToLower(runtext[runtextpos + 2], CultureInfo.InvariantCulture) != 'n'))) || ((char.ToLower(runtext[runtextpos + 3], CultureInfo.InvariantCulture) != 'a') || (char.ToLower(runtext[runtextpos + 4], CultureInfo.InvariantCulture) != 't')))
            {
                goto Label_028E;
            }
            runtextpos += 5;
            int start = (num5 = runtextend - runtextpos) + 1;
            do
            {
                if (--start <= 0)
                {
                    goto Label_0164;
                }
                runtextpos++;
            }
            while (RegexRunner.CharInClass(char.ToLower(runtext[runtextpos], CultureInfo.InvariantCulture), "\x0001\0\t\0\x0002\x0004\x0005\x0003\x0001\t\x0013\0"));
            runtextpos--;
        Label_0164:
            if (num5 > start)
            {
                runtrack[--runtrackpos] = (num5 - start) - 1;
                runtrack[--runtrackpos] = runtextpos - 1;
                runtrack[--runtrackpos] = 2;
            }
        Label_0197:
            if ((((6 > (runtextend - runtextpos)) || (char.ToLower(runtext[runtextpos], CultureInfo.InvariantCulture) != 's')) || ((char.ToLower(runtext[runtextpos + 1], CultureInfo.InvariantCulture) != 'e') || (char.ToLower(runtext[runtextpos + 2], CultureInfo.InvariantCulture) != 'r'))) || (((char.ToLower(runtext[runtextpos + 3], CultureInfo.InvariantCulture) != 'v') || (char.ToLower(runtext[runtextpos + 4], CultureInfo.InvariantCulture) != 'e')) || (char.ToLower(runtext[runtextpos + 5], CultureInfo.InvariantCulture) != 'r')))
            {
                goto Label_028E;
            }
            runtextpos += 6;
            start = runstack[runstackpos++];
            this.Capture(0, start, runtextpos);
            runtrack[--runtrackpos] = start;
            runtrack[--runtrackpos] = 3;
        Label_0285:
            base.runtextpos = runtextpos;
            return;
        Label_028E:
            base.runtrackpos = runtrackpos;
            base.runstackpos = runstackpos;
            this.EnsureStorage();
            runtrackpos = base.runtrackpos;
            runstackpos = base.runstackpos;
            runtrack = base.runtrack;
            runstack = base.runstack;
            switch (runtrack[runtrackpos++])
            {
                case 1:
                    runstackpos++;
                    goto Label_028E;

                case 2:
                    runtextpos = runtrack[runtrackpos++];
                    start = runtrack[runtrackpos++];
                    if (start > 0)
                    {
                        runtrack[--runtrackpos] = start - 1;
                        runtrack[--runtrackpos] = runtextpos - 1;
                        runtrack[--runtrackpos] = 2;
                    }
                    goto Label_0197;

                case 3:
                    runstack[--runstackpos] = runtrack[runtrackpos++];
                    this.Uncapture();
                    goto Label_028E;
            }
            runtextpos = runtrack[runtrackpos++];
            goto Label_0285;
        }

        public override void InitTrackCount()
        {
            base.runtrackcount = 4;
        }
    }
}

