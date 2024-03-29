﻿namespace Geonorge.GmlKart.Application.Exceptions
{
    public class MapDocumentException : Exception
    {
        public MapDocumentException()
        {
        }

        public MapDocumentException(string message) : base(message)
        {
        }

        public MapDocumentException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
