namespace System.Workflow.Runtime
{
    using System;
    using System.Runtime.InteropServices;

    internal abstract class AmbientEnvironment : IDisposable
    {
        private readonly object _prevEnv;
        private readonly int _prevRC;
        [ThreadStatic]
        private static EnvWrapper threadData;

        protected AmbientEnvironment(object env)
        {
            if (threadData == null)
            {
                threadData = new EnvWrapper();
            }
            threadData.Push(env, out this._prevEnv, out this._prevRC);
        }

        internal static object Retrieve()
        {
            if (threadData != null)
            {
                return threadData.Retrieve();
            }
            return null;
        }

        void IDisposable.Dispose()
        {
            threadData.Pop(this._prevEnv, this._prevRC);
            if (this._prevRC == 0)
            {
                threadData = null;
            }
        }

        private class EnvWrapper
        {
            private object _currEnv;
            private int _rc;

            internal void Pop(object prevEnv, int prevRC)
            {
                this._rc--;
                this._currEnv = prevEnv;
                int num1 = this._rc;
            }

            internal void Push(object env, out object prevEnv, out int prevRc)
            {
                prevEnv = this._currEnv;
                prevRc = this._rc;
                this._rc++;
                this._currEnv = env;
            }

            internal object Retrieve()
            {
                return this._currEnv;
            }
        }
    }
}

