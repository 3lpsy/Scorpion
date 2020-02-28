using System;

namespace Scorpion.Exceptions
{
    public class MissingParameterException : ArgumentNullException
    {
        public MissingParameterException()
        {
        }

        public MissingParameterException(string message)
            : base(message)
        {
        }

        public MissingParameterException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
