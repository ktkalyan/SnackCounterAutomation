
namespace SnackCounter.Core.Models
{
    public class MonthlyOfficeReport
    {
        #region Properties

        public string OfficeCode { get; set; } = string.Empty;

        public string Currency { get; set; } = string.Empty;

        public decimal MonthlyBudget { get; set; }

        public decimal TotalSpent { get; set; }

        public decimal RemainingBudget { get; set; }

        public int PurchaseCount { get; set; }

        #endregion
    }
}
