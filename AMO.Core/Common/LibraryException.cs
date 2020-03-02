using System;
using System.Runtime.Serialization;

namespace AMO.Core
{
    [Serializable]
    public class LibraryException : Exception
    {
        public LibraryException() : base()
        {
        }

        public LibraryException(string message) : base(message)
        {
        }

        public LibraryException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected LibraryException(SerializationInfo serializationInfo, StreamingContext streamingContext)
            : base(serializationInfo, streamingContext)
        {
        }
    }
}
