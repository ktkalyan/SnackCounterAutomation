using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SnackCounter.Core.Models;
using SnackCounter.Core.Services;

namespace SnackCounter.Tests
{
    [TestClass]
    public class BudgetServiceTests
    {
        #region Budget Validation Tests

        [TestMethod]
        public void CanPurchase_WithinBudget_ShouldReturnTrue()
        {
            // Arrange
            BudgetService service = new BudgetService();

            Office office = new Office
            {
                Code = "BLR",
                Name = "Bengaluru",
                Currency = "INR",
                MonthlyBudget = 15000
            };

            // Act
            bool result = service.CanPurchase(
                office, 2000, new DateTime(2026, 11, 19));

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void RecordPurchase_ExceedsBudget_ShouldThrowException()
        {
            // Arrange
            BudgetService service = new BudgetService();

            Office office = new Office
            {
                Code = "BLR",
                Currency = "INR",
                MonthlyBudget = 15000
            };

            Purchase firstPurchase = new Purchase
            {
                PurchaseId = 1,
                Amount = 12000,
                Currency = "INR",
                PurchasedAt = new DateTime(2026, 11, 19)
            };

            Purchase secondPurchase = new Purchase
            {
                PurchaseId = 2,
                Amount = 4000,
                Currency = "INR",
                PurchasedAt = new DateTime(2026, 11, 19)
            };

            // Act
            service.RecordPurchase(office, firstPurchase);

            // Assert
            Assert.Throws<InvalidOperationException>(() =>
                service.RecordPurchase(office, secondPurchase));
        }

        [TestMethod]
        public void RecordPurchase_DifferentOffices_ShouldMaintainSeparateBudgets()
        {
            // Arrange
            BudgetService service = new BudgetService();

            Office bengaluru = new Office
            {
                Code = "BLR",
                Currency = "INR",
                MonthlyBudget = 15000
            };

            Office oslo = new Office
            {
                Code = "OSL",
                Currency = "NOK",
                MonthlyBudget = 6000
            };

            // Act
            service.RecordPurchase(bengaluru, new Purchase
            {
                PurchaseId = 1,
                Amount = 14000,
                Currency = "INR",
                PurchasedAt = new DateTime(2026, 11, 19)
            });

            service.RecordPurchase(oslo, new Purchase
            {
                PurchaseId = 2,
                Amount = 5000,
                Currency = "NOK",
                PurchasedAt = new DateTime(2026, 11, 19)
            });

            // Assert
            Assert.IsFalse(service.CanPurchase(
                bengaluru, 2000, new DateTime(2026, 11, 19)));

            Assert.IsTrue(service.CanPurchase(
                oslo, 1000, new DateTime(2026, 11, 19)));
        }

        [TestMethod]
        public void RecordPurchase_NewMonth_ShouldResetBudgetCalculation()
        {
            // Arrange
            BudgetService service = new BudgetService();

            Office office = new Office
            {
                Code = "BLR",
                Currency = "INR",
                MonthlyBudget = 15000
            };

            service.RecordPurchase(office, new Purchase
            {
                PurchaseId = 1,
                Amount = 15000,
                Currency = "INR",
                PurchasedAt = new DateTime(2026, 11, 19)
            });

            // Act
            bool result = service.CanPurchase(
                office, 15000, new DateTime(2026, 12, 1));

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void RecordPurchase_WrongCurrency_ShouldThrowException()
        {
            // Arrange
            BudgetService service = new BudgetService();

            Office office = new Office
            {
                Code = "BLR",
                Currency = "INR",
                MonthlyBudget = 15000
            };

            Purchase purchase = new Purchase
            {
                PurchaseId = 1,
                Amount = 1000,
                Currency = "NOK",
                PurchasedAt = new DateTime(2026, 11, 19)
            };

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() =>
                service.RecordPurchase(office, purchase));
        }

        #endregion



        #region Budget Reservation Tests

        [TestMethod]
        public void ReserveBudget_WithinBudget_ShouldCreateReservation()
        {
            // Arrange
            BudgetService service = new BudgetService();

            Office office = new Office
            {
                Code = "BLR",
                Currency = "INR",
                MonthlyBudget = 15000
            };

            // Act
            BudgetReservation reservation = service.ReserveBudget(
                office, 101, 3000, new DateTime(2026, 11, 19));

            // Assert
            Assert.AreEqual(101, reservation.RefillRequestId);
            Assert.AreEqual(3000m, reservation.Amount);
            Assert.AreEqual("INR", reservation.Currency);
            Assert.IsTrue(reservation.IsActive);
        }

        [TestMethod]
        public void ReserveBudget_DuplicateRequest_ShouldThrowException()
        {
            // Arrange
            BudgetService service = new BudgetService();

            Office office = new Office
            {
                Code = "BLR",
                Currency = "INR",
                MonthlyBudget = 15000
            };

            service.ReserveBudget(
                office, 101, 3000, new DateTime(2026, 11, 19));

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() =>
                service.ReserveBudget(
                    office, 101, 2000, new DateTime(2026, 11, 19)));
        }

        [TestMethod]
        public void ReserveBudget_InsufficientBudget_ShouldThrowException()
        {
            // Arrange
            BudgetService service = new BudgetService();

            Office office = new Office
            {
                Code = "BLR",
                Currency = "INR",
                MonthlyBudget = 15000
            };

            service.ReserveBudget(
                office, 101, 12000, new DateTime(2026, 11, 19));

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() =>
                service.ReserveBudget(
                    office, 102, 4000, new DateTime(2026, 11, 19)));
        }

        [TestMethod]
        public void ReleaseReservation_ActiveReservation_ShouldReleaseSuccessfully()
        {
            // Arrange
            BudgetService service = new BudgetService();

            Office office = new Office
            {
                Code = "BLR",
                Currency = "INR",
                MonthlyBudget = 15000
            };

            BudgetReservation reservation = service.ReserveBudget(
                office, 101, 3000, new DateTime(2026, 11, 19));

            // Act
            service.ReleaseReservation(reservation.ReservationId);

            // Assert
            Assert.IsFalse(reservation.IsActive);

            BudgetReservation newReservation = service.ReserveBudget(
                office, 102, 15000, new DateTime(2026, 11, 19));

            Assert.IsTrue(newReservation.IsActive);
        }

        [TestMethod]
        public void ReleaseReservation_AlreadyReleased_ShouldThrowException()
        {
            // Arrange
            BudgetService service = new BudgetService();

            Office office = new Office
            {
                Code = "BLR",
                Currency = "INR",
                MonthlyBudget = 15000
            };

            BudgetReservation reservation = service.ReserveBudget(
                office, 101, 3000, new DateTime(2026, 11, 19));

            service.ReleaseReservation(reservation.ReservationId);

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() =>
                service.ReleaseReservation(reservation.ReservationId));
        }

        [TestMethod]
        public void ReserveBudget_ConcurrentRequests_ShouldNotExceedBudget()
        {
            // Arrange
            BudgetService service = new BudgetService();

            Office office = new Office
            {
                Code = "BLR",
                Currency = "INR",
                MonthlyBudget = 15000
            };

            int successfulReservations = 0;

            // Act
            System.Threading.Tasks.Parallel.For(1, 11, requestId =>
            {
                try
                {
                    service.ReserveBudget(
                        office,
                        requestId,
                        5000,
                        new DateTime(2026, 11, 19));

                    System.Threading.Interlocked.Increment(
                        ref successfulReservations);
                }
                catch (InvalidOperationException)
                {
                    // Expected when the budget is exhausted.
                }
            });

            // Assert
            Assert.AreEqual(3, successfulReservations);
        }

        #endregion



        #region Purchase Settlement Tests

        [TestMethod]
        public void SettlePurchase_ValidPurchase_ShouldCompleteSuccessfully()
        {
            // Arrange
            BudgetService service = new BudgetService();

            Office office = new Office
            {
                Code = "BLR",
                Currency = "INR",
                MonthlyBudget = 15000
            };

            DateTime date = new DateTime(2026, 11, 19);

            BudgetReservation reservation =
                service.ReserveBudget(office, 101, 3000, date);

            Purchase purchase = new Purchase
            {
                PurchaseId = 1,
                RefillRequestId = 101,
                VolunteerId = 5,
                Amount = 2800,
                Currency = "INR",
                PurchasedAt = date,
                Description = "Snacks and beverages"
            };

            // Act
            service.SettlePurchase(
                office, reservation.ReservationId, purchase);

            // Assert
            Assert.IsFalse(reservation.IsActive);

            // 15000 - 2800 = 12200 remaining.
            Assert.IsTrue(service.CanPurchase(office, 12200, date));
            Assert.IsFalse(service.CanPurchase(office, 12201, date));
        }

        [TestMethod]
        public void SettlePurchase_AmountExceedsReservation_ShouldThrowException()
        {
            // Arrange
            BudgetService service = new BudgetService();

            Office office = new Office
            {
                Code = "BLR",
                Currency = "INR",
                MonthlyBudget = 15000
            };

            DateTime date = new DateTime(2026, 11, 19);

            BudgetReservation reservation =
                service.ReserveBudget(office, 101, 3000, date);

            Purchase purchase = new Purchase
            {
                PurchaseId = 1,
                RefillRequestId = 101,
                Amount = 3500,
                Currency = "INR",
                PurchasedAt = date
            };

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() =>
                service.SettlePurchase(
                    office, reservation.ReservationId, purchase));

            Assert.IsTrue(reservation.IsActive);
        }

        [TestMethod]
        public void SettlePurchase_WrongCurrency_ShouldThrowException()
        {
            // Arrange
            BudgetService service = new BudgetService();

            Office office = new Office
            {
                Code = "BLR",
                Currency = "INR",
                MonthlyBudget = 15000
            };

            DateTime date = new DateTime(2026, 11, 19);

            BudgetReservation reservation =
                service.ReserveBudget(office, 101, 3000, date);

            Purchase purchase = new Purchase
            {
                PurchaseId = 1,
                RefillRequestId = 101,
                Amount = 2500,
                Currency = "NOK",
                PurchasedAt = date
            };

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() =>
                service.SettlePurchase(
                    office, reservation.ReservationId, purchase));

            Assert.IsTrue(reservation.IsActive);
        }

        [TestMethod]
        public void SettlePurchase_DuplicateSettlement_ShouldThrowException()
        {
            // Arrange
            BudgetService service = new BudgetService();

            Office office = new Office
            {
                Code = "BLR",
                Currency = "INR",
                MonthlyBudget = 15000
            };

            DateTime date = new DateTime(2026, 11, 19);

            BudgetReservation reservation =
                service.ReserveBudget(office, 101, 3000, date);

            Purchase purchase = new Purchase
            {
                PurchaseId = 1,
                RefillRequestId = 101,
                Amount = 2800,
                Currency = "INR",
                PurchasedAt = date
            };

            service.SettlePurchase(
                office, reservation.ReservationId, purchase);

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() =>
                service.SettlePurchase(
                    office, reservation.ReservationId, purchase));
        }

        [TestMethod]
        public void SettlePurchase_UnusedAmount_ShouldBecomeAvailable()
        {
            // Arrange
            BudgetService service = new BudgetService();

            Office office = new Office
            {
                Code = "BLR",
                Currency = "INR",
                MonthlyBudget = 15000
            };

            DateTime date = new DateTime(2026, 11, 19);

            BudgetReservation reservation =
                service.ReserveBudget(office, 101, 5000, date);

            Purchase purchase = new Purchase
            {
                PurchaseId = 1,
                RefillRequestId = 101,
                Amount = 3000,
                Currency = "INR",
                PurchasedAt = date
            };

            // Act
            service.SettlePurchase(
                office, reservation.ReservationId, purchase);

            // Assert
            BudgetReservation secondReservation =
                service.ReserveBudget(office, 102, 12000, date);

            Assert.IsTrue(secondReservation.IsActive);
        }

        #endregion


        [TestMethod]
        public void RecordPurchase_ActiveReservation_ShouldRespectRemainingBudget()
        {
            // Arrange
            BudgetService service = new BudgetService();

            Office office = new Office
            {
                Code = "BLR",
                Currency = "INR",
                MonthlyBudget = 15000
            };

            DateTime date = new DateTime(2026, 11, 19);

            service.ReserveBudget(
                office, 101, 12000, date);

            Purchase purchase = new Purchase
            {
                PurchaseId = 1,
                RefillRequestId = 102,
                Amount = 5000,
                Currency = "INR",
                PurchasedAt = date
            };

            // Act & Assert
            Assert.IsFalse(
                service.CanPurchase(office, purchase.Amount, date));

            Assert.Throws<InvalidOperationException>(() =>
                service.RecordPurchase(office, purchase));
        }



        [TestMethod]
        public void GetMonthlyReport_MultiplePurchases_ShouldCalculateTotals()
        {
            // Arrange
            BudgetService service = new BudgetService();

            Office office = new Office
            {
                Code = "BLR",
                Currency = "INR",
                MonthlyBudget = 15000
            };

            service.RecordPurchase(office, new Purchase
            {
                PurchaseId = 1,
                Amount = 2000,
                Currency = "INR",
                PurchasedAt = new DateTime(2026, 11, 5)
            });

            service.RecordPurchase(office, new Purchase
            {
                PurchaseId = 2,
                Amount = 3000,
                Currency = "INR",
                PurchasedAt = new DateTime(2026, 11, 10)
            });

            // Act
            MonthlyOfficeReport report =
                service.GetMonthlyReport(office, 2026, 11);

            // Assert
            Assert.AreEqual("BLR", report.OfficeCode);
            Assert.AreEqual("INR", report.Currency);
            Assert.AreEqual(5000m, report.TotalSpent);
            Assert.AreEqual(10000m, report.RemainingBudget);
            Assert.AreEqual(2, report.PurchaseCount);
        }

    }
}
