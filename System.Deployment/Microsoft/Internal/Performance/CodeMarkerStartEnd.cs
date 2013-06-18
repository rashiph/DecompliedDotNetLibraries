namespace Microsoft.Internal.Performance
{
    using System;

    internal sealed class CodeMarkerStartEnd : IDisposable
    {
        private CodeMarkerEvent _end;

        public CodeMarkerStartEnd(CodeMarkerEvent begin, CodeMarkerEvent end)
        {
            CodeMarkers.Instance.CodeMarker(begin);
            this._end = end;
        }

        public void Dispose()
        {
            if (this._end != ((CodeMarkerEvent) 0))
            {
                CodeMarkers.Instance.CodeMarker(this._end);
                this._end = (CodeMarkerEvent) 0;
            }
        }
    }
}

