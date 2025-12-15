using Nass.Models;
using System.Text.Json.Serialization;

public partial class Transaction
{
    public int TransId { get; set; }
    public Guid TransUid { get; set; }
    public int CustomerId { get; set; }
    public DateTime TransDate { get; set; }
    public byte[]? TransBlobAttachmenet { get; set; }
    public string? TransUrlAttachment { get; set; }
    public string TransCategories { get; set; } = null!;
    public string? TransDescription { get; set; }
    public int? AgencyId { get; set; }
    public DateTime? TransRecivedDate { get; set; }
    public int TransMaxAgency { get; set; }
    public string TransStatus { get; set; } = null!;
    public string? AgencyTenat { get; set; }

    [JsonIgnore]
    public virtual Agency? Agency { get; set; }

    [JsonIgnore]
    public virtual Customer Customer { get; set; } = null!;
}
