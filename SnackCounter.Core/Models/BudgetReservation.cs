
using System;

namespace SnackCounter.Core.Models
{
    public class BudgetReservation
    {
        public int ReservationId { get; set; }

        public int RefillRequestId { get; set; }

        public string OfficeCode { get; set; } = string.Empty;

        public decimal Amount { get; set; }

        public string Currency { get; set; } = string.Empty;

        public DateTime ReservedAt { get; set; }

        public bool IsActive { get; set; }
    }
}
