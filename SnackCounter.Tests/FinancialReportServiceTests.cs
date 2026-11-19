
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SnackCounter.Core.Models;
using SnackCounter.Core.Services;

namespace SnackCounter.Tests
{
    [TestClass]
    public class FinancialReportServiceTests
    {
        #region Combined Reporting Tests

        [TestMethod]
        public void GenerateCombinedReport_TwoCurrencies_ShouldConvertToNok()
        {
            // Arrange
            BudgetService budgetService = new BudgetService();

            Office blr = new Office
            {
                Code = "BLR",
                Currency = "INR",
                MonthlyBudget = 15000
            };

            Office osl = new Office
            {
                Code = "OSL",
                Currency = "NOK",
                MonthlyBudget = 6000
            };

            budgetService.RecordPurchase(blr, new Purchase
            {
                PurchaseId = 1,
                Amount = 5000,
                Currency = "INR",
                PurchasedAt = new DateTime(2026, 11, 10)
            });

            budgetService.RecordPurchase(osl, new Purchase
            {
                PurchaseId = 2,
                Amount = 2000,
                Currency = "NOK",
                PurchasedAt = new DateTime(2026, 11, 12)
            });

            FinancialReportService service =
                new FinancialReportService(budgetService);

            // Act
            CombinedMonthlyReport report =
                service.GenerateCombinedReport(
                    blr, osl, 2026, 11, 0.11m);

            // Assert
            Assert.AreEqual(2550m, report.TotalSpentInNok);
            Assert.AreEqual(7650m, report.TotalBudgetInNok);
            Assert.AreEqual(5100m, report.RemainingBudgetInNok);
            Assert.AreEqual(2, report.OfficeReports.Count);
            Assert.AreEqual("NOK", report.ReportingCurrency);
        }

        #endregion
    }
}
