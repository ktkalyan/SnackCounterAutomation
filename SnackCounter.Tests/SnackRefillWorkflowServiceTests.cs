
using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SnackCounter.Core.Enums;
using SnackCounter.Core.Models;
using SnackCounter.Core.Services;
using System.Threading.Tasks;

namespace SnackCounter.Tests
{
    [TestClass]
    public class SnackRefillWorkflowServiceTests
    {
        #region Volunteer Abandonment Tests

        [TestMethod]
        public void AbandonAssignment_ValidRequest_ShouldReleaseVolunteerAndBudget()
        {
            // Arrange
            RefillService refillService = new RefillService();
            BudgetService budgetService = new BudgetService();

            SnackRefillWorkflowService workflow =
                new SnackRefillWorkflowService(
                    refillService, budgetService);

            Office office = new Office
            {
                Code = "BLR",
                Currency = "INR",
                MonthlyBudget = 15000
            };

            RefillRequest request =
                refillService.CreateRequest("BLR", "EMP001");

            List<Volunteer> volunteers = new List<Volunteer>
            {
                new Volunteer
                {
                    VolunteerId = 101,
                    CurrentOfficeCode = "BLR",
                    IsAvailable = true
                }
            };

            refillService.AssignVolunteer(
                request.RequestId, volunteers);

            BudgetReservation reservation =
                budgetService.ReserveBudget(
                    office,
                    request.RequestId,
                    3000,
                    new DateTime(2026, 11, 19));

            // Act
            RefillRequest result = workflow.AbandonAssignment(
                request.RequestId,
                101,
                reservation.ReservationId);

            // Assert
            Assert.AreEqual(RefillStatus.Pending, result.Status);
            Assert.IsNull(result.AssignedVolunteerId);
            Assert.IsFalse(reservation.IsActive);

            Assert.IsTrue(budgetService.CanPurchase(
                office, 15000, new DateTime(2026, 11, 19)));
        }

        [TestMethod]
        public void AbandonAssignment_InvalidReservation_ShouldNotReleaseVolunteer()
        {
            // Arrange
            RefillService refillService = new RefillService();
            BudgetService budgetService = new BudgetService();

            SnackRefillWorkflowService workflow =
                new SnackRefillWorkflowService(
                    refillService, budgetService);

            RefillRequest request =
                refillService.CreateRequest("BLR", "EMP001");

            List<Volunteer> volunteers = new List<Volunteer>
            {
                new Volunteer
                {
                    VolunteerId = 101,
                    CurrentOfficeCode = "BLR",
                    IsAvailable = true
                }
            };

            refillService.AssignVolunteer(
                request.RequestId, volunteers);

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() =>
                workflow.AbandonAssignment(
                    request.RequestId, 101, 999));

            Assert.AreEqual(RefillStatus.Assigned, request.Status);
            Assert.AreEqual(101, request.AssignedVolunteerId);
        }

        [TestMethod]
        public void AbandonAssignment_WrongRequestReservation_ShouldThrowException()
        {
            // Arrange
            RefillService refillService = new RefillService();
            BudgetService budgetService = new BudgetService();

            SnackRefillWorkflowService workflow =
                new SnackRefillWorkflowService(
                    refillService, budgetService);

            Office office = new Office
            {
                Code = "BLR",
                Currency = "INR",
                MonthlyBudget = 15000
            };

            RefillRequest firstRequest =
                refillService.CreateRequest("BLR", "EMP001");

            RefillRequest secondRequest =
                refillService.CreateRequest("OSL", "EMP002");

            List<Volunteer> volunteers = new List<Volunteer>
            {
                new Volunteer
                {
                    VolunteerId = 101,
                    CurrentOfficeCode = "BLR",
                    IsAvailable = true
                }
            };

            refillService.AssignVolunteer(
                firstRequest.RequestId, volunteers);

            BudgetReservation reservation =
                budgetService.ReserveBudget(
                    office,
                    secondRequest.RequestId,
                    3000,
                    new DateTime(2026, 11, 19));

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() =>
                workflow.AbandonAssignment(
                    firstRequest.RequestId,
                    101,
                    reservation.ReservationId));

            Assert.AreEqual(
                RefillStatus.Assigned,
                firstRequest.Status);

            Assert.IsTrue(reservation.IsActive);
        }

        #endregion



        #region Assignment Notification Tests

        [TestMethod]
        public async Task AssignAndNotify_ValidVolunteer_ShouldSendNotification()
        {
            // Arrange
            RefillService refillService = new RefillService();
            BudgetService budgetService = new BudgetService();
            FakeChatNotificationService chatService =
                new FakeChatNotificationService();

            SnackRefillWorkflowService workflow =
                new SnackRefillWorkflowService(
                    refillService, budgetService, chatService);

            RefillRequest request =
                refillService.CreateRequest("BLR", "EMP100");

            List<Volunteer> volunteers = new List<Volunteer>
    {
        new Volunteer
        {
            VolunteerId = 101,
            EmployeeId = "EMP001",
            Name = "Rahul",
            CurrentOfficeCode = "BLR",
            IsAvailable = true
        }
    };

            // Act
            RefillRequest result =
                await workflow.AssignAndNotifyAsync(
                    request.RequestId, volunteers);

            // Assert
            Assert.AreEqual(RefillStatus.Assigned, result.Status);
            Assert.AreEqual(101, result.AssignedVolunteerId);

            List<string> messages = chatService.GetSentMessages();

            Assert.AreEqual(1, messages.Count);
            StringAssert.Contains(messages[0], "EMP001");
            StringAssert.Contains(messages[0], "BLR");
            StringAssert.Contains(messages[0], request.RequestId.ToString());
        }

        [TestMethod]
        public async Task AssignAndNotify_NoAvailableVolunteer_ShouldNotSendNotification()
        {
            // Arrange
            RefillService refillService = new RefillService();
            BudgetService budgetService = new BudgetService();
            FakeChatNotificationService chatService =
                new FakeChatNotificationService();

            SnackRefillWorkflowService workflow =
                new SnackRefillWorkflowService(
                    refillService, budgetService, chatService);

            RefillRequest request =
                refillService.CreateRequest("BLR", "EMP100");

            List<Volunteer> volunteers = new List<Volunteer>
            {
                new Volunteer
                {
                    VolunteerId = 102,
                    EmployeeId = "EMP002",
                    CurrentOfficeCode = "OSL",
                    IsAvailable = true
                }
            };

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => workflow.AssignAndNotifyAsync(
                    request.RequestId, volunteers));

            Assert.AreEqual(RefillStatus.Pending, request.Status);
            Assert.IsNull(request.AssignedVolunteerId);
            Assert.AreEqual(0, chatService.GetSentMessages().Count);
        }

        #endregion

        [TestMethod]
        public async Task AssignAndNotify_MissingEmployeeId_ShouldRollbackAssignment()
        {
            // Arrange
            RefillService refillService = new RefillService();
            BudgetService budgetService = new BudgetService();
            FakeChatNotificationService chatService =
                new FakeChatNotificationService();

            SnackRefillWorkflowService workflow =
                new SnackRefillWorkflowService(
                    refillService, budgetService, chatService);

            RefillRequest request =
                refillService.CreateRequest("BLR", "EMP100");

            List<Volunteer> volunteers = new List<Volunteer>
    {
        new Volunteer
        {
            VolunteerId = 101,
            EmployeeId = "", // Chat service will reject this.
            CurrentOfficeCode = "BLR",
            IsAvailable = true
        }
    };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(
                () => workflow.AssignAndNotifyAsync(
                    request.RequestId, volunteers));

            // Assignment must be rolled back.
            Assert.AreEqual(
                RefillStatus.Pending,
                request.Status);

            Assert.IsNull(request.AssignedVolunteerId);

            // No notification should be stored.
            Assert.AreEqual(
                0, chatService.GetSentMessages().Count);
        }


        [TestMethod]
        public async Task ReassignAbandonedTask_ValidVolunteers_ShouldAssignAnotherVolunteer()
        {
            // Arrange
            RefillService refillService = new RefillService();
            BudgetService budgetService = new BudgetService();
            FakeChatNotificationService chatService =
                new FakeChatNotificationService();

            SnackRefillWorkflowService workflow =
                new SnackRefillWorkflowService(
                    refillService, budgetService, chatService);

            Office office = new Office
            {
                Code = "BLR",
                Currency = "INR",
                MonthlyBudget = 15000
            };

            RefillRequest request =
                refillService.CreateRequest("BLR", "EMP100");

            List<Volunteer> volunteers = new List<Volunteer>
    {
        new Volunteer
        {
            VolunteerId = 101,
            EmployeeId = "EMP001",
            CurrentOfficeCode = "BLR",
            IsAvailable = true
        },
        new Volunteer
        {
            VolunteerId = 102,
            EmployeeId = "EMP002",
            CurrentOfficeCode = "BLR",
            IsAvailable = true
        }
    };

            // Assign first volunteer.
            await workflow.AssignAndNotifyAsync(
                request.RequestId, volunteers);

            BudgetReservation reservation =
                budgetService.ReserveBudget(
                    office,
                    request.RequestId,
                    3000,
                    new DateTime(2026, 11, 19));

            // Act
            RefillRequest result =
                await workflow.ReassignAbandonedTaskAsync(
                    request.RequestId,
                    101,
                    reservation.ReservationId,
                    volunteers);

            // Assert
            Assert.AreEqual(RefillStatus.Assigned, result.Status);
            Assert.AreEqual(102, result.AssignedVolunteerId);
            Assert.IsFalse(reservation.IsActive);

            List<string> messages = chatService.GetSentMessages();

            Assert.AreEqual(2, messages.Count);
            StringAssert.Contains(messages[1], "EMP002");
        }



        [TestMethod]
        public async Task ReassignExpiredTask_AfterDeadline_ShouldAssignAnotherVolunteer()
        {
            // Arrange
            RefillService refillService = new RefillService();
            BudgetService budgetService = new BudgetService();
            FakeChatNotificationService chatService =
                new FakeChatNotificationService();

            SnackRefillWorkflowService workflow =
                new SnackRefillWorkflowService(
                    refillService, budgetService, chatService);

            Office office = new Office
            {
                Code = "BLR",
                Currency = "INR",
                MonthlyBudget = 15000
            };

            RefillRequest request =
                refillService.CreateRequest("BLR", "EMP100");

            List<Volunteer> volunteers = new List<Volunteer>
    {
        new Volunteer
        {
            VolunteerId = 101,
            EmployeeId = "EMP001",
            CurrentOfficeCode = "BLR",
            IsAvailable = true
        },
        new Volunteer
        {
            VolunteerId = 102,
            EmployeeId = "EMP002",
            CurrentOfficeCode = "BLR",
            IsAvailable = true
        }
    };

            await workflow.AssignAndNotifyAsync(
                request.RequestId, volunteers);

            BudgetReservation reservation =
                budgetService.ReserveBudget(
                    office,
                    request.RequestId,
                    3000,
                    DateTime.UtcNow);

            // Simulate time after the assignment deadline.
            DateTime simulatedTime =
                request.AssignmentExpiresAt.Value.AddMinutes(1);

            // Act
            RefillRequest result =
                await workflow.ReassignExpiredTaskAsync(
                    request.RequestId,
                    reservation.ReservationId,
                    volunteers,
                    simulatedTime);

            // Assert
            Assert.AreEqual(RefillStatus.Assigned, result.Status);
            Assert.AreEqual(102, result.AssignedVolunteerId);
            Assert.IsFalse(reservation.IsActive);
            Assert.AreEqual(2, chatService.GetSentMessages().Count);
        }


    }
}
