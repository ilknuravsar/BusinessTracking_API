using Microsoft.AspNetCore.Identity;


namespace Domain.Entities
{
    public  class AppUser : IdentityUser<Guid>
    {
        public ICollection<OutageReport> Reports { get; set; } = new List<OutageReport>();
    }
}


