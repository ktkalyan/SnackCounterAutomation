
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SnackCounter.Core.Enums;
using SnackCounter.Core.Models;
using SnackCounter.Core.Services;

namespace SnackCounter.Demo
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            #region Application Header

            Console.WriteLine("==========================================");
            Console.WriteLine(" FRAMSIKT SNACK COUNTER AUTOMATION");
            Console.WriteLine("==========================================");
            Console.WriteLine();

            #endregion

            #region Office Configuration

            Office bengaluru = new Office
            {
                Code = "BLR",
                Name = "Bengaluru",
                Currency = "INR",
                MonthlyBudget = 15000m
            };

            Office oslo = new Office
            {
                Code = "OSL",
                Name = "Oslo",
                Currency = "NOK",
                MonthlyBudget = 6000m
            };

            #endregion

            #region Volunteer Configuration

            List<Volunteer> volunteers = new List<Volunteer>
            {
                new Volunteer
                {
                    VolunteerId = 101,
                    EmployeeId = "EMP001",
                    Name = "Rahul",
                    HomeOfficeCode = "BLR",
                    CurrentOfficeCode = "BLR",
                    IsAvailable = true
                },
                new Volunteer
                {
                    VolunteerId = 102,
                    EmployeeId = "EMP002",
                    Name = "Priya",
                    HomeOfficeCode = "BLR",
                    CurrentOfficeCode = "BLR",
                    IsAvailable = true
                },
                new Volunteer
                {
                    VolunteerId = 201,
                    EmployeeId = "EMP201",
                    Name = "Erik",
                    HomeOfficeCode = "OSL",
                    CurrentOfficeCode = "OSL",
                    IsAvailable = true
                }
            };

            #endregion

            #region Service Initialization

            RefillService refillService =
                new RefillService();

            BudgetService budgetService =
                new BudgetService();

            FakeChatNotificationService chatService =
                new FakeChatNotificationService();

            FakeHrReimbursementService hrService =
                new FakeHrReimbursementService();

            SnackRefillWorkflowService workflow =
                new SnackRefillWorkflowService(
                    refillService,
                    budgetService,
                    chatService);

            FinancialReportService reportService =
                new FinancialReportService(budgetService);

            DateTime purchaseDate = DateTime.UtcNow;

            #endregion

            #region Bengaluru Refill Request

            Console.WriteLine("BENGALURU OFFICE REFILL");
            Console.WriteLine("-----------------------");

            RefillRequest blrRequest =
                refillService.CreateRequest(
                    "BLR",
                    "EMP100");

            Console.WriteLine(
                $"Refill request created: {blrRequest.RequestId}");

            await workflow.AssignAndNotifyAsync(
                blrRequest.RequestId,
                volunteers);

            Console.WriteLine(
                $"Assigned volunteer: {blrRequest.AssignedVolunteerId}");

            #endregion

            #region Bengaluru Budget Reservation

            BudgetReservation blrReservation =
                budgetService.ReserveBudget(
                    bengaluru,
                    blrRequest.RequestId,
                    3000m,
                    purchaseDate);

            Console.WriteLine(
                $"Budget reserved: {blrReservation.Amount} INR");

            #endregion

            #region Bengaluru Purchase

            Purchase blrPurchase = new Purchase
            {
                PurchaseId = 1,
                RefillRequestId = blrRequest.RequestId,
                VolunteerId = blrRequest.AssignedVolunteerId.Value,
                OfficeCode = "BLR",
                Amount = 2800m,
                Currency = "INR",
                Description = "Office snacks and beverages",
                ReceiptImagePath = "receipts/blr-1.jpg",
                PurchasedAt = purchaseDate,
                ReimbursementStatus =
                    ReimbursementStatus.NotSubmitted
            };

            budgetService.SettlePurchase(
                bengaluru,
                blrReservation.ReservationId,
                blrPurchase);

            Console.WriteLine(
                $"Purchase recorded: {blrPurchase.Amount} INR");

            refillService.CompleteRequest(
                blrRequest.RequestId,
                blrPurchase.VolunteerId);

            Console.WriteLine(
                $"Refill status: {blrRequest.Status}");

            #endregion

            #region Bengaluru HR Reimbursement

            string blrHrReference =
                await hrService.SubmitReimbursementAsync(
                    "EMP001",
                    blrPurchase);

            Console.WriteLine(
                $"HR reimbursement submitted: {blrHrReference}");

            hrService.UpdateReimbursementStatus(
                blrHrReference,
                ReimbursementStatus.Approved);

            Console.WriteLine(
                $"Reimbursement status: " +
                $"{blrPurchase.ReimbursementStatus}");

            Console.WriteLine();

            #endregion

            #region Oslo Refill Request

            Console.WriteLine("OSLO OFFICE REFILL");
            Console.WriteLine("------------------");

            RefillRequest osloRequest =
                refillService.CreateRequest(
                    "OSL",
                    "EMP202");

            Console.WriteLine(
                $"Refill request created: {osloRequest.RequestId}");

            await workflow.AssignAndNotifyAsync(
                osloRequest.RequestId,
                volunteers);

            Console.WriteLine(
                $"Assigned volunteer: {osloRequest.AssignedVolunteerId}");

            #endregion

            #region Oslo Budget Reservation

            BudgetReservation osloReservation =
                budgetService.ReserveBudget(
                    oslo,
                    osloRequest.RequestId,
                    1200m,
                    purchaseDate);

            Console.WriteLine(
                $"Budget reserved: {osloReservation.Amount} NOK");

            #endregion

            #region Oslo Purchase

            Purchase osloPurchase = new Purchase
            {
                PurchaseId = 2,
                RefillRequestId = osloRequest.RequestId,
                VolunteerId = osloRequest.AssignedVolunteerId.Value,
                OfficeCode = "OSL",
                Amount = 1100m,
                Currency = "NOK",
                Description = "Oslo office snacks and beverages",
                ReceiptImagePath = "receipts/oslo-2.jpg",
                PurchasedAt = purchaseDate,
                ReimbursementStatus =
                    ReimbursementStatus.NotSubmitted
            };

            budgetService.SettlePurchase(
                oslo,
                osloReservation.ReservationId,
                osloPurchase);

            Console.WriteLine(
                $"Purchase recorded: {osloPurchase.Amount} NOK");

            refillService.CompleteRequest(
                osloRequest.RequestId,
                osloPurchase.VolunteerId);

            Console.WriteLine(
                $"Refill status: {osloRequest.Status}");

            #endregion

            #region Oslo HR Reimbursement

            string osloHrReference =
                await hrService.SubmitReimbursementAsync(
                    "EMP201",
                    osloPurchase);

            Console.WriteLine(
                $"HR reimbursement submitted: {osloHrReference}");

            hrService.UpdateReimbursementStatus(
                osloHrReference,
                ReimbursementStatus.Approved);

            Console.WriteLine(
                $"Reimbursement status: " +
                $"{osloPurchase.ReimbursementStatus}");

            Console.WriteLine();

            #endregion

            #region Monthly Financial Reporting

            Console.WriteLine("==========================================");
            Console.WriteLine(" MONTHLY FINANCIAL REPORT");
            Console.WriteLine("==========================================");

            // Illustrative exchange rate:
            // 1 INR = 0.11 NOK.
            // This is not a live exchange rate.
            decimal inrToNokRate = 0.11m;

            CombinedMonthlyReport report =
                reportService.GenerateCombinedReport(
                    bengaluru,
                    oslo,
                    purchaseDate.Year,
                    purchaseDate.Month,
                    inrToNokRate);

            Console.WriteLine(
                $"Reporting Period: {report.Month}/{report.Year}");

            Console.WriteLine();

            foreach (MonthlyOfficeReport officeReport
                in report.OfficeReports)
            {
                Console.WriteLine(
                    $"Office: {officeReport.OfficeCode}");

                Console.WriteLine(
                    $"Monthly Budget: " +
                    $"{officeReport.MonthlyBudget:F2} " +
                    $"{officeReport.Currency}");

                Console.WriteLine(
                    $"Total Spent: " +
                    $"{officeReport.TotalSpent:F2} " +
                    $"{officeReport.Currency}");

                Console.WriteLine(
                    $"Remaining Budget: " +
                    $"{officeReport.RemainingBudget:F2} " +
                    $"{officeReport.Currency}");

                Console.WriteLine(
                    $"Total Purchases: {officeReport.PurchaseCount}");

                Console.WriteLine("------------------------------------------");
            }

            Console.WriteLine(
                $"INR to NOK Rate: {report.InrToNokRate}");

            Console.WriteLine(
                $"Combined Monthly Budget: " +
                $"{report.TotalBudgetInNok:F2} NOK");

            Console.WriteLine(
                $"Combined Total Spent: " +
                $"{report.TotalSpentInNok:F2} NOK");

            Console.WriteLine(
                $"Combined Remaining Budget: " +
                $"{report.RemainingBudgetInNok:F2} NOK");

            Console.WriteLine();

            #endregion

            #region Chat Notifications

            Console.WriteLine("==========================================");
            Console.WriteLine(" CHAT NOTIFICATIONS");
            Console.WriteLine("==========================================");

            List<string> messages =
                chatService.GetSentMessages();

            foreach (string message in messages)
            {
                Console.WriteLine(message);
                Console.WriteLine();
            }

            #endregion

            #region Final Summary

            Console.WriteLine("==========================================");
            Console.WriteLine(" AUTOMATION SUMMARY");
            Console.WriteLine("==========================================");

            Console.WriteLine(
                $"Bengaluru Request: {blrRequest.Status}");

            Console.WriteLine(
                $"Oslo Request: {osloRequest.Status}");

            Console.WriteLine(
                $"Bengaluru Reimbursement: " +
                $"{blrPurchase.ReimbursementStatus}");

            Console.WriteLine(
                $"Oslo Reimbursement: " +
                $"{osloPurchase.ReimbursementStatus}");

            Console.WriteLine(
                $"Total Chat Notifications: {messages.Count}");

            Console.WriteLine();
            Console.WriteLine("Demo completed successfully.");

            #endregion
        }
    }
}
