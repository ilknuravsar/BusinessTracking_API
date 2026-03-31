using Domain.Enums;

namespace Application.DTOs
{
    public class UpdateOutageRequest
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public Priority Priority { get; set; } 
    }
}
