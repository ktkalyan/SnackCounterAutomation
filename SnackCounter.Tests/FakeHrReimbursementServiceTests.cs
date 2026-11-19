
using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SnackCounter.Core.Models;
using SnackCounter.Core.Services;
using SnackCounter.Core.Enums;

namespace SnackCounter.Tests
{
    [TestClass]
    public class FakeHrReimbursementServiceTests
    {
        #region Reimbursement Submission Tests

        [TestMethod]
        public async Task SubmitReimbursement_ValidPurchase_ShouldReturnReferenceId()
        {
            // Arrange
            FakeHrReimbursementService service =
                new FakeHrReimbursementService();

            Purchase purchase = new Purchase
            {
                PurchaseId = 1,
                RefillRequestId = 101,
                Amount = 2800,
                Currency = "INR",
                Description = "Snacks and beverages",
                ReceiptImagePath = "receipts/101.jpg"
            };

            // Act
            string referenceId =
                await service.SubmitReimbursementAsync(
                    "EMP001", purchase);

            // Assert
            Assert.AreEqual("HR-00001", referenceId);
        }

        [TestMethod]
        public async Task SubmitReimbursement_MissingReceipt_ShouldThrowException()
        {
            // Arrange
            FakeHrReimbursementService service =
                new FakeHrReimbursementService();

            Purchase purchase = new Purchase
            {
                PurchaseId = 1,
                Amount = 1000,
                Currency = "INR",
                Description = "Office snacks",
                ReceiptImagePath = ""
            };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(
                () => service.SubmitReimbursementAsync(
                    "EMP001", purchase));
        }

        [TestMethod]
        public async Task SubmitReimbursement_InvalidAmount_ShouldThrowException()
        {
            // Arrange
            FakeHrReimbursementService service =
                new FakeHrReimbursementService();

            Purchase purchase = new Purchase
            {
                PurchaseId = 1,
                Amount = 0,
                Currency = "INR",
                Description = "Office snacks",
                ReceiptImagePath = "receipts/101.jpg"
            };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
                () => service.SubmitReimbursementAsync(
                    "EMP001", purchase));
        }

        [TestMethod]
        public async Task SubmitReimbursement_MultiplePurchases_ShouldGenerateUniqueReferences()
        {
            // Arrange
            FakeHrReimbursementService service =
                new FakeHrReimbursementService();

            Purchase firstPurchase = new Purchase
            {
                PurchaseId = 1,
                Amount = 1000,
                Currency = "INR",
                Description = "Biscuits",
                ReceiptImagePath = "receipts/1.jpg"
            };

            Purchase secondPurchase = new Purchase
            {
                PurchaseId = 2,
                Amount = 2000,
                Currency = "INR",
                Description = "Coffee",
                ReceiptImagePath = "receipts/2.jpg"
            };

            // Act
            string firstReference =
                await service.SubmitReimbursementAsync(
                    "EMP001", firstPurchase);

            string secondReference =
                await service.SubmitReimbursementAsync(
                    "EMP002", secondPurchase);

            // Assert
            Assert.AreNotEqual(firstReference, secondReference);
            Assert.AreEqual("HR-00001", firstReference);
            Assert.AreEqual("HR-00002", secondReference);
        }

        #endregion



        #region Reimbursement Status Tests

        [TestMethod]
        public async Task SubmitReimbursement_ValidPurchase_ShouldSetSubmittedStatus()
        {
            // Arrange
            FakeHrReimbursementService service =
                new FakeHrReimbursementService();

            Purchase purchase = new Purchase
            {
                PurchaseId = 101,
                Amount = 2800,
                Currency = "INR",
                Description = "Office snacks",
                ReceiptImagePath = "receipts/101.jpg"
            };

            // Act
            string referenceId =
                await service.SubmitReimbursementAsync(
                    "EMP001", purchase);

            // Assert
            Assert.AreEqual(referenceId, purchase.HrReferenceId);

            Assert.AreEqual(
                ReimbursementStatus.Submitted,
                purchase.ReimbursementStatus);

            Assert.AreEqual(
                ReimbursementStatus.Submitted,
                service.GetReimbursementStatus(referenceId));
        }

        [TestMethod]
        public async Task UpdateReimbursementStatus_ApprovedToPaid_ShouldSucceed()
        {
            // Arrange
            FakeHrReimbursementService service =
                new FakeHrReimbursementService();

            Purchase purchase = new Purchase
            {
                PurchaseId = 102,
                Amount = 1500,
                Currency = "INR",
                Description = "Biscuits and coffee",
                ReceiptImagePath = "receipts/102.jpg"
            };

            string referenceId =
                await service.SubmitReimbursementAsync(
                    "EMP002", purchase);

            // Act
            service.UpdateReimbursementStatus(
                referenceId, ReimbursementStatus.Approved);

            service.UpdateReimbursementStatus(
                referenceId, ReimbursementStatus.Paid);

            // Assert
            Assert.AreEqual(
                ReimbursementStatus.Paid,
                service.GetReimbursementStatus(referenceId));

            Assert.AreEqual(
                ReimbursementStatus.Paid,
                purchase.ReimbursementStatus);
        }

        [TestMethod]
        public async Task UpdateReimbursementStatus_SubmittedToRejected_ShouldSucceed()
        {
            // Arrange
            FakeHrReimbursementService service =
                new FakeHrReimbursementService();

            Purchase purchase = new Purchase
            {
                PurchaseId = 103,
                Amount = 1000,
                Currency = "INR",
                Description = "Snack purchase",
                ReceiptImagePath = "receipts/103.jpg"
            };

            string referenceId =
                await service.SubmitReimbursementAsync(
                    "EMP003", purchase);

            // Act
            service.UpdateReimbursementStatus(
                referenceId, ReimbursementStatus.Rejected);

            // Assert
            Assert.AreEqual(
                ReimbursementStatus.Rejected,
                service.GetReimbursementStatus(referenceId));
        }

        [TestMethod]
        public async Task UpdateReimbursementStatus_SubmittedDirectlyToPaid_ShouldThrowException()
        {
            // Arrange
            FakeHrReimbursementService service =
                new FakeHrReimbursementService();

            Purchase purchase = new Purchase
            {
                PurchaseId = 104,
                Amount = 2000,
                Currency = "INR",
                Description = "Office refreshments",
                ReceiptImagePath = "receipts/104.jpg"
            };

            string referenceId =
                await service.SubmitReimbursementAsync(
                    "EMP004", purchase);

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() =>
                service.UpdateReimbursementStatus(
                    referenceId, ReimbursementStatus.Paid));

            Assert.AreEqual(
                ReimbursementStatus.Submitted,
                purchase.ReimbursementStatus);
        }

        #endregion

        [TestMethod]
        public async Task SubmitReimbursement_SamePurchaseTwice_ShouldThrowException()
        {
            // Arrange
            FakeHrReimbursementService service =
                new FakeHrReimbursementService();

            Purchase purchase = new Purchase
            {
                PurchaseId = 105,
                Amount = 2800,
                Currency = "INR",
                Description = "Office snacks",
                ReceiptImagePath = "receipts/105.jpg"
            };

            // Act
            string firstReference =
                await service.SubmitReimbursementAsync(
                    "EMP001", purchase);

            // Assert
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => service.SubmitReimbursementAsync(
                    "EMP001", purchase));

            Assert.AreEqual("HR-00001", firstReference);

            Assert.AreEqual(
                ReimbursementStatus.Submitted,
                purchase.ReimbursementStatus);
        }


    }
}
