using System;
using System.Collections.Generic;

namespace Lab3.Models;

public partial class Review
{
    public int Id { get; set; }

    public int? ClientId { get; set; }

    public int? ServiceId { get; set; }

    public string? ReviewText { get; set; }

    public DateOnly? ReviewDate { get; set; }

    public virtual Client? Client { get; set; }

    public virtual Services? Service { get; set; }
}
