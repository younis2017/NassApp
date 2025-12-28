using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Nass.Models
{
    //public class NotificationRecipient
    //{
    //    public int NotificationRecipientId { get; set; }

    //    public int Trans_id { get; set; }       // FK to Transaction.Trans_id
    //    public int AgencyId { get; set; }       // FK to Agency.AgencyId
    //    public bool IsRead { get; set; } = false;
    //    public DateTime? ReadAt { get; set; }
    //    public string Status { get; set; } = "New";

    //    // Navigations
    //    public Transaction Transaction { get; set; } = null!;
    //    public Agency Agency { get; set; } = null!;
    //}
    public class NotificationRecipient
    {
        public int NotificationRecipientId { get; set; } // PK, identity
        public int NotificationId { get; set; } // FK to Notification
        public int Trans_id { get; set; } // FK to Transaction
        public int AgencyId { get; set; } // FK to Agency

        public string Status { get; set; }
        public bool IsRead { get; set; }
        public DateTime? ReadAt { get; set; }

        // Navigation properties
        public Notification Notification { get; set; }
        public Transaction Transaction { get; set; }
        public Agencies Agency { get; set; }
    }

}
