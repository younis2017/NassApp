using System;
using System.Collections.Generic;

namespace Nass.Models;

public partial class Agency
{
    public int AgencyId { get; set; }

    public string AgencyName { get; set; } = null!;

    public string? AgencyPhone { get; set; }

    public string? AgencyEmail { get; set; }

    public string? AgencyWebsite { get; set; }

    public string? AgencyAddress { get; set; }

    public string? AgencyLocation { get; set; }

    public string? AgencyTaxId { get; set; }

    public string? AgencyTenet { get; set; }

    public string AgencyUsername { get; set; } = null!;

    public string AgencyPassword { get; set; } = null!;

    public DateTime AgencyJoinedDate { get; set; }

    public string? AgencyLogo { get; set; }

    public Guid AgencyUid { get; set; }

    public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
}
