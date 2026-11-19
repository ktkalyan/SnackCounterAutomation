
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SnackCounter.Core.Interfaces;

namespace SnackCounter.Core.Services
{
    public class FakeChatNotificationService : IChatNotificationService
    {
        #region Private Fields

        private readonly List<string> _messages =
            new List<string>();

        private readonly object _lock = new object();

        #endregion

        #region Notification Methods

        public Task SendRefillAssignmentAsync(string employeeId, int refillRequestId, string officeCode)
        {
            if (string.IsNullOrWhiteSpace(employeeId))
                throw new ArgumentException(
                    "Employee ID is required.");

            if (refillRequestId <= 0)
                throw new ArgumentOutOfRangeException(
                    nameof(refillRequestId));

            if (string.IsNullOrWhiteSpace(officeCode))
                throw new ArgumentException(
                    "Office code is required.");

            string message =
                $"Hello {employeeId}, " +
                $"you have been assigned refill request " +
                $"#{refillRequestId} for office {officeCode}. " +
                $"Please purchase snacks and upload the receipt.";

            lock (_lock)
            {
                _messages.Add(message);
            }

            return Task.CompletedTask;
        }

        #endregion

        #region Message History

        public List<string> GetSentMessages()
        {
            lock (_lock)
            {
                return new List<string>(_messages);
            }
        }

        #endregion
    }
}
