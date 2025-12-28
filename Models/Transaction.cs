using Nass.Models;

public class Transaction
{
    public int Trans_id { get; set; }         // PK for business
    public Guid Trans_uid { get; set; }
    public DateTime Trans_date { get; set; }
    public string? Trans_categories { get; set; }
    public string? Trans_description { get; set; }
    public string? Trans_url { get; set; } //customer upload url
    public int TransStatus { get; set; }      // 0 = new, 1 = confirmed

    public int Customer_id { get; set; }
    public Customer Customer { get; set; } = null!;

    public int? Agency_id { get; set; }
    public Agencies? Agency { get; set; }

    public string? Agency_tenat { get; set; }
    public DateTime? trans_recived_date { get; set; }
    public ICollection<Notification> NotificationList { get; set; } = new List<Notification>();

    public ICollection<NotificationRecipient> Notifications { get; set; } = new List<NotificationRecipient>();
}
