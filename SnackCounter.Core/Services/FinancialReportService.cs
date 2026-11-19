
using System;
using System.Collections.Generic;
using SnackCounter.Core.Models;

namespace SnackCounter.Core.Services
{
    public class FinancialReportService
    {
        #region Private Fields

        private readonly BudgetService _budgetService;

        #endregion

        #region Constructor

        public FinancialReportService(BudgetService budgetService)
        {
            _budgetService = budgetService
                ?? throw new ArgumentNullException(
                    nameof(budgetService));
        }

        #endregion

        #region Combined Monthly Reporting

        public CombinedMonthlyReport GenerateCombinedReport(
            Office bengaluruOffice,
            Office osloOffice,
            int year,
            int month,
            decimal inrToNokRate)
        {
            if (bengaluruOffice == null)
                throw new ArgumentNullException(
                    nameof(bengaluruOffice));

            if (osloOffice == null)
                throw new ArgumentNullException(
                    nameof(osloOffice));

            if (!string.Equals(
                bengaluruOffice.Currency, "INR",
                StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException(
                    "Bengaluru office must use INR.");

            if (!string.Equals(
                osloOffice.Currency, "NOK",
                StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException(
                    "Oslo office must use NOK.");

            if (inrToNokRate <= 0)
                throw new ArgumentOutOfRangeException(
                    nameof(inrToNokRate));

            MonthlyOfficeReport blrReport =
                _budgetService.GetMonthlyReport(
                    bengaluruOffice, year, month);

            MonthlyOfficeReport oslReport =
                _budgetService.GetMonthlyReport(
                    osloOffice, year, month);

            decimal totalSpentNok =
                (blrReport.TotalSpent * inrToNokRate)
                + oslReport.TotalSpent;

            decimal totalBudgetNok =
                (blrReport.MonthlyBudget * inrToNokRate)
                + oslReport.MonthlyBudget;

            return new CombinedMonthlyReport
            {
                Year = year,
                Month = month,
                ReportingCurrency = "NOK",
                InrToNokRate = inrToNokRate,
                TotalSpentInNok = totalSpentNok,
                TotalBudgetInNok = totalBudgetNok,
                RemainingBudgetInNok =
                    totalBudgetNok - totalSpentNok,
                OfficeReports = new List<MonthlyOfficeReport>
                {
                    blrReport,
                    oslReport
                }
            };
        }

        #endregion
    }
}
