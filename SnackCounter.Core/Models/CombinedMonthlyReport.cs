
using System.Collections.Generic;

namespace SnackCounter.Core.Models
{
    public class CombinedMonthlyReport
    {
        #region Properties

        public int Year { get; set; }

        public int Month { get; set; }

        public string ReportingCurrency { get; set; } = "NOK";

        public decimal TotalSpentInNok { get; set; }

        public decimal TotalBudgetInNok { get; set; }

        public decimal RemainingBudgetInNok { get; set; }

        public decimal InrToNokRate { get; set; }

        public List<MonthlyOfficeReport> OfficeReports { get; set; }
            = new List<MonthlyOfficeReport>();

        #endregion
    }
}
