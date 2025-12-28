using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Nass.Models
{
    public class Customer
    {
        [Key]
        public int CustomerId { get; set; }
        public string? CustomerName { get; set; }
        public string? CustomerPhone { get; set; }
        public string? CustomerEmail { get; set; }
        public string? CustomerAddress { get; set; }
        public string? CustomerLocation { get; set; }
        public string? CustomerTaxId { get; set; }
        public string? CustomerTenet { get; set; }
        public string? CustomerUsername { get; set; }
        public string? CustomerPassword { get; set; }
        public DateTime CustomerJoinedDate { get; set; }
        public Guid CustomerUid { get; set; } = Guid.NewGuid();
        public int CustomerStatus { get; set; } = 0;
        public ICollection<Transaction>? Transactions { get; set; }
    }
}
