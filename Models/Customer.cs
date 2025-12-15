using System;
using System.Collections.Generic;

namespace Nass.Models;

public partial class Customer
{
    public int CustomerId { get; set; }

    public string CustomerName { get; set; } = null!;

    public string? CustomerPhone { get; set; }

    public string? CustomerEmail { get; set; }

    public string? CustomerAddress { get; set; }

    public string? CustomerLocation { get; set; }

    public string? CustomerTaxId { get; set; }

    public string? CustomerTenet { get; set; }

    public string CustomerUsername { get; set; } = null!;

    public string CustomerPassword { get; set; } = null!;

    public DateTime CustomerJoinedDate { get; set; }

    public Guid CustomerUid { get; set; }

    public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
}
