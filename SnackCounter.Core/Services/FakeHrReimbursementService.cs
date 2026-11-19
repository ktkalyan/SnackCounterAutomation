
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SnackCounter.Core.Interfaces;
using SnackCounter.Core.Models;
using SnackCounter.Core.Enums;

namespace SnackCounter.Core.Services
{
    public class FakeHrReimbursementService : IHrReimbursementService
    {
        #region Private Fields

        private readonly Dictionary<string, Purchase> _submissions =
            new Dictionary<string, Purchase>();

        private readonly object _lock = new object();

        private int _nextReferenceId = 1;

        #endregion

        #region Reimbursement Submission

        public Task<string> SubmitReimbursementAsync(string employeeId, Purchase purchase)
        {
            if (string.IsNullOrWhiteSpace(employeeId))
                throw new ArgumentException(
                    "Employee ID is required.");

            if (purchase == null)
                throw new ArgumentNullException(nameof(purchase));

            if (purchase.Amount <= 0)
                throw new ArgumentOutOfRangeException(
                    nameof(purchase.Amount));

            if (string.IsNullOrWhiteSpace(purchase.Currency))
                throw new ArgumentException(
                    "Purchase currency is required.");

            if (string.IsNullOrWhiteSpace(purchase.Description))
                throw new ArgumentException(
                    "Purchase description is required.");

            if (string.IsNullOrWhiteSpace(purchase.ReceiptImagePath))
                throw new ArgumentException(
                    "Receipt image is required.");


            lock (_lock)
            {
                // Prevent the same purchase from being submitted twice.
                if (!string.IsNullOrWhiteSpace(purchase.HrReferenceId))
                {
                    throw new InvalidOperationException(
                        "This purchase has already been submitted for reimbursement.");
                }

                string referenceId =
                    $"HR-{_nextReferenceId++:D5}";

                purchase.HrReferenceId = referenceId;
                purchase.ReimbursementStatus =
                    ReimbursementStatus.Submitted;

                _submissions.Add(referenceId, purchase);

                return Task.FromResult(referenceId);
            }

        }

        #endregion



        #region Reimbursement Status Management

        public ReimbursementStatus GetReimbursementStatus(
            string referenceId)
        {
            if (string.IsNullOrWhiteSpace(referenceId))
                throw new ArgumentException(
                    "HR reference ID is required.");

            lock (_lock)
            {
                if (!_submissions.TryGetValue(
                    referenceId, out Purchase? purchase))
                {
                    throw new InvalidOperationException(
                        "Reimbursement request not found.");
                }

                return purchase.ReimbursementStatus;
            }
        }

        public void UpdateReimbursementStatus(string referenceId, ReimbursementStatus newStatus)
        {
            if (string.IsNullOrWhiteSpace(referenceId))
                throw new ArgumentException(
                    "HR reference ID is required.");

            lock (_lock)
            {
                if (!_submissions.TryGetValue(
                    referenceId, out Purchase? purchase))
                {
                    throw new InvalidOperationException(
                        "Reimbursement request not found.");
                }

                ReimbursementStatus currentStatus =
                    purchase.ReimbursementStatus;

                bool validTransition =
                    (currentStatus == ReimbursementStatus.Submitted &&
                     (newStatus == ReimbursementStatus.Approved ||
                      newStatus == ReimbursementStatus.Rejected)) ||
                    (currentStatus == ReimbursementStatus.Approved &&
                     newStatus == ReimbursementStatus.Paid);

                if (!validTransition)
                {
                    throw new InvalidOperationException(
                        $"Invalid reimbursement status transition: " +
                        $"{currentStatus} to {newStatus}.");
                }

                purchase.ReimbursementStatus = newStatus;
            }
        }

        #endregion

    }
}
