using System;

namespace SkiaSharp.FontSubset
{
    public class FontSubsetException : Exception
    {
        public FontSubsetException()
        {
        }

        public FontSubsetException(string message)
            : base(message)
        {
        }

        public FontSubsetException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}