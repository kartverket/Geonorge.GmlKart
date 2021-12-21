namespace Geonorge.GmlKart.Application.Exceptions
{
    public class CouldNotLoadXDocumentException : Exception
    {
        public CouldNotLoadXDocumentException()
        {
        }

        public CouldNotLoadXDocumentException(string message) : base(message)
        {
        }

        public CouldNotLoadXDocumentException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
