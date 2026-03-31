namespace Application.DTOs
{
    public class OutageReportDto
    {
        public Guid Id { get; init; }
        public string Title { get; init; } = default!;
        public string Description { get; init; } = default!;
        public string Location { get; init; } = default!;
        public string Priority { get; init; } = default!; 
        public string Status { get; init; } = default!;  
        public string CreatedByName { get; init; } = default!;
        public DateTime CreatedAt { get; init; }
        public DateTime UpdatedAt { get; init; }
    }
}
