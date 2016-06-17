using System;
using System.Runtime.Serialization;

namespace Fly.Blackbox.Console.Riff
{
    public class RiffParserException : ApplicationException
    {
        public RiffParserException()
            : base()
        {
        }

        public RiffParserException(string message)
            : base(message)
        {
        }

        public RiffParserException(string message, Exception inner)
            : base(message, inner)
        {
        }

        public RiffParserException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
