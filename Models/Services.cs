using System;
using System.Collections.Generic;

namespace Lab3.Models;

public partial class Services
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public int? ServiceTypeId { get; set; }

    public string? Description { get; set; }

    public string? Innovations { get; set; }

    public decimal? Price { get; set; }

    public virtual ICollection<PerformedService> PerformedServices { get; set; } = new List<PerformedService>();

    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();

    public virtual ServiceType? ServiceType { get; set; }
}
