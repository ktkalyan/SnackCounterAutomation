
using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SnackCounter.Core.Services;

namespace SnackCounter.Tests
{
    [TestClass]
    public class FakeChatNotificationServiceTests
    {
        #region Notification Tests

        [TestMethod]
        public async Task SendRefillAssignment_ValidDetails_ShouldCreateMessage()
        {
            // Arrange
            FakeChatNotificationService service =
                new FakeChatNotificationService();

            // Act
            await service.SendRefillAssignmentAsync(
                "EMP001", 101, "BLR");

            // Assert
            var messages = service.GetSentMessages();

            Assert.AreEqual(1, messages.Count);
            StringAssert.Contains(messages[0], "EMP001");
            StringAssert.Contains(messages[0], "101");
            StringAssert.Contains(messages[0], "BLR");
        }

        [TestMethod]
        public async Task SendRefillAssignment_MissingEmployeeId_ShouldThrowException()
        {
            // Arrange
            FakeChatNotificationService service =
                new FakeChatNotificationService();

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(
                () => service.SendRefillAssignmentAsync(
                    "", 101, "BLR"));

            Assert.AreEqual(
                0, service.GetSentMessages().Count);
        }

        [TestMethod]
        public async Task SendRefillAssignment_InvalidRequestId_ShouldThrowException()
        {
            // Arrange
            FakeChatNotificationService service =
                new FakeChatNotificationService();

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
                () => service.SendRefillAssignmentAsync(
                    "EMP001", 0, "BLR"));

            Assert.AreEqual(
                0, service.GetSentMessages().Count);
        }

        [TestMethod]
        public async Task GetSentMessages_MultipleNotifications_ShouldReturnAllMessages()
        {
            // Arrange
            FakeChatNotificationService service =
                new FakeChatNotificationService();

            // Act
            await service.SendRefillAssignmentAsync(
                "EMP001", 101, "BLR");

            await service.SendRefillAssignmentAsync(
                "EMP002", 102, "OSL");

            var messages = service.GetSentMessages();

            // Assert
            Assert.AreEqual(2, messages.Count);

            StringAssert.Contains(messages[0], "EMP001");
            StringAssert.Contains(messages[1], "EMP002");

            // Verify returned history is a copy.
            messages.Clear();

            Assert.AreEqual(
                2, service.GetSentMessages().Count);
        }

        #endregion
    }
}
