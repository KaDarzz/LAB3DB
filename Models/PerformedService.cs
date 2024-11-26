using System;
using System.Collections.Generic;

namespace Lab3.Models;

public partial class PerformedService
{
    public int Id { get; set; }

    public int? ClientId { get; set; }

    public int? ServiceId { get; set; }

    public int? EmployeeId { get; set; }

    public DateOnly? ServiceDate { get; set; }

    public decimal? Cost { get; set; }

    public virtual Client? Client { get; set; }

    public virtual Employee? Employee { get; set; }

    public virtual Services? Service { get; set; }
}
