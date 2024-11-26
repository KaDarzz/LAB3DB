using System;
using System.Collections.Generic;

namespace Lab3.Models;

public partial class Client
{
    public int Id { get; set; }

    public string FullName { get; set; } = null!;

    public string? Address { get; set; }

    public string? Phone { get; set; }

    public decimal? Discount { get; set; }

    public decimal? TotalServicesCost { get; set; }

    public virtual ICollection<PerformedService> PerformedServices { get; set; } = new List<PerformedService>();

    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
}
