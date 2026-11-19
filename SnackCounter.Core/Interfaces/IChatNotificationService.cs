
using System.Threading.Tasks;

namespace SnackCounter.Core.Interfaces
{
    public interface IChatNotificationService
    {
        #region Notification Methods

        Task SendRefillAssignmentAsync(
            string employeeId,
            int refillRequestId,
            string officeCode);

        #endregion
    }
}
