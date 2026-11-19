
using System.Threading.Tasks;
using SnackCounter.Core.Models;

namespace SnackCounter.Core.Interfaces
{
    public interface IHrReimbursementService
    {
        Task<string> SubmitReimbursementAsync(
            string employeeId,
            Purchase purchase);
    }
}
