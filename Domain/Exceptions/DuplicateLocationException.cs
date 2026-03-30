namespace Domain.Exceptions
{
    public class DuplicateLocationException : DomainException
    {
        public DuplicateLocationException(string location)
            : base($"{location} konumuna son 1 saat içinde zaten bildirim açılmış.")
        {
        }
    }
}
