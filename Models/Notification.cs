using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Nass.Models
{
    //public class Notification
    //{
    //    [Key]
    //    public int NotificationId { get; set; }

    //    public int Trans_Id { get; set; } // FK → Transactions.Trans_id
    //    public Transaction Transaction { get; set; } = null!;

    //    public string Title { get; set; } = null!;
    //    public string Message { get; set; } = null!;

    //    public bool IsPublished { get; set; }
    //    public DateTime CreatedAt { get; set; }

    //    // Optional collection of recipients via Transaction link
    //    public ICollection<NotificationRecipient>? Recipients { get; set; }
    //}
    public class Notification
    {
        public int NotificationId { get; set; } // PK, identity
        public int Trans_Id { get; set; } // FK to Transaction
        public string Title { get; set; }
        public string Message { get; set; }
        public bool IsPublished { get; set; }
        public DateTime CreatedAt { get; set; }

        // Navigation
        public Transaction Transaction { get; set; }
        public ICollection<NotificationRecipient> Recipients { get; set; } = new List<NotificationRecipient>();
    }

}
