namespace System.Web.RegularExpressions
{
    using System;
    using System.Text.RegularExpressions;

    internal class CommentRegexRunner7 : RegexRunner
    {
        public override bool FindFirstChar()
        {
            if (base.runtextpos > base.runtextstart)
            {
                base.runtextpos = base.runtextend;
                return false;
            }
            return true;
        }

        public override void Go()
        {
            int num4;
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
            if ((((runtextpos != base.runtextstart) || (4 > (runtextend - runtextpos))) || ((runtext[runtextpos] != '<') || (runtext[runtextpos + 1] != '%'))) || ((runtext[runtextpos + 2] != '-') || (runtext[runtextpos + 3] != '-')))
            {
                goto Label_02F8;
            }
            runtextpos += 4;
            runstack[--runstackpos] = -1;
            runtrack[--runtrackpos] = 1;
            goto Label_0213;
        Label_0161:
            if (num5 > num4)
            {
                runtrack[--runtrackpos] = (num5 - num4) - 1;
                runtrack[--runtrackpos] = runtextpos - 1;
                runtrack[--runtrackpos] = 2;
            }
        Label_0194:
            num4 = runstack[runstackpos++];
            this.Capture(2, num4, runtextpos);
            runtrack[--runtrackpos] = num4;
            runtrack[--runtrackpos] = 3;
            if (runtextpos >= runtextend)
            {
                goto Label_02F8;
            }
            runtextpos++;
            if (runtext[runtextpos] != '-')
            {
                goto Label_02F8;
            }
            num4 = runstack[runstackpos++];
            this.Capture(1, num4, runtextpos);
            runtrack[--runtrackpos] = num4;
            runtrack[--runtrackpos] = 3;
        Label_0213:
            if (num4 != -1)
            {
                runtrack[--runtrackpos] = num4;
            }
            else
            {
                runtrack[--runtrackpos] = runtextpos;
            }
            if ((num4 = runstack[runstackpos++]) != runtextpos)
            {
                runtrack[--runtrackpos] = runtextpos;
                runtrack[--runtrackpos] = 4;
            }
            else
            {
                runstack[--runstackpos] = num4;
                runtrack[--runtrackpos] = 5;
            }
            if (((3 > (runtextend - runtextpos)) || (runtext[runtextpos] != '-')) || ((runtext[runtextpos + 1] != '%') || (runtext[runtextpos + 2] != '>')))
            {
                goto Label_02F8;
            }
            runtextpos += 3;
            num4 = runstack[runstackpos++];
            this.Capture(0, num4, runtextpos);
            runtrack[--runtrackpos] = num4;
            runtrack[--runtrackpos] = 3;
        Label_02EF:
            base.runtextpos = runtextpos;
            return;
        Label_02F8:
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
                    goto Label_02F8;

                case 2:
                    runtextpos = runtrack[runtrackpos++];
                    num4 = runtrack[runtrackpos++];
                    if (num4 > 0)
                    {
                        runtrack[--runtrackpos] = num4 - 1;
                        runtrack[--runtrackpos] = runtextpos - 1;
                        runtrack[--runtrackpos] = 2;
                    }
                    goto Label_0194;

                case 3:
                    runstack[--runstackpos] = runtrack[runtrackpos++];
                    this.Uncapture();
                    goto Label_02F8;

                case 4:
                    runtextpos = runtrack[runtrackpos++];
                    runstack[--runstackpos] = runtextpos;
                    runtrack[--runtrackpos] = 5;
                    if ((runtrackpos > 40) && (runstackpos > 30))
                    {
                        runstack[--runstackpos] = runtextpos;
                        runtrack[--runtrackpos] = 1;
                        runstack[--runstackpos] = runtextpos;
                        runtrack[--runtrackpos] = 1;
                        num4 = (num5 = runtextend - runtextpos) + 1;
                        do
                        {
                            if (--num4 <= 0)
                            {
                                goto Label_0161;
                            }
                            runtextpos++;
                        }
                        while (runtext[runtextpos] != '-');
                        runtextpos--;
                        goto Label_0161;
                    }
                    runtrack[--runtrackpos] = 6;
                    goto Label_02F8;

                case 5:
                    runstack[runstackpos] = runtrack[runtrackpos++];
                    goto Label_02F8;
            }
            runtextpos = runtrack[runtrackpos++];
            goto Label_02EF;
        }

        public override void InitTrackCount()
        {
            base.runtrackcount = 10;
        }
    }
}

