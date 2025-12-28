using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Nass.Models
{
    public class Agencies
    {
        [Key]
        public int AgencyId { get; set; }

        public string? AgencyName { get; set; }
        public string? AgencyPhone { get; set; }
        public string? AgencyEmail { get; set; }
        public string? AgencyWebsite { get; set; }
        public string? AgencyAddress { get; set; }
        public string? AgencyLocation { get; set; }
        public string? AgencyTaxId { get; set; }
        public string? AgencyTenet { get; set; }
        public string? AgencyUsername { get; set; }
        public string? AgencyPassword { get; set; }
        public DateTime AgencyJoinedDate { get; set; }
        public int AgencyStatus { get; set; } = 0;
        public string? AgencyLogo { get; set; }
        public Guid AgencyUid { get; set; }

        public ICollection<Transaction>? Transactions { get; set; }
        public ICollection<NotificationRecipient>? NotificationRecipients { get; set; }
    }
}
