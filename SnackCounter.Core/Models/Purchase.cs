
using System;
using SnackCounter.Core.Enums;

namespace SnackCounter.Core.Models
{
    public class Purchase
    {
        public int PurchaseId { get; set; }

        public int RefillRequestId { get; set; }

        public int VolunteerId { get; set; }

        public decimal Amount { get; set; }

        public string Currency { get; set; }

        public string Description { get; set; }

        public string ReceiptImagePath { get; set; }

        public DateTime PurchasedAt { get; set; }

        public ReimbursementStatus ReimbursementStatus { get; set; }
        public string OfficeCode { get; set; }

        public string? HrReferenceId { get; set; }

    }
}
