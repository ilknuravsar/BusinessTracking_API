using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs
{
    public class CreateReportRequest
    {
        public string Title { get; init; } = default!;
        public string Description { get; init; } = default!;
        public string Location { get; init; } = default!;
        public Priority Priority { get; init; }
    }
}
