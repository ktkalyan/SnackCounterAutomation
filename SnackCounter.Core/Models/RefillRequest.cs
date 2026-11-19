
using System;
using SnackCounter.Core.Enums;

namespace SnackCounter.Core.Models
{
    public class RefillRequest
    {
        public int RequestId { get; set; }

        public string OfficeCode { get; set; }

        public string RequestedByEmployeeId { get; set; }

        public int? AssignedVolunteerId { get; set; }

        public DateTime CreatedAt { get; set; }

        public RefillStatus Status { get; set; }


        public DateTime? AssignedAt { get; set; }

        public DateTime? AssignmentExpiresAt { get; set; }

    }
}
