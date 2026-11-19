using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SnackCounter.Core.Enums;
using SnackCounter.Core.Models;
using SnackCounter.Core.Services;

namespace SnackCounter.Tests
{
    [TestClass]
    public class RefillServiceTests
    {
        [TestMethod]
        public void CreateRequest_FirstRequest_ShouldCreatePendingRequest()
        {
            // Arrange
            RefillService service = new RefillService();

            // Act
            RefillRequest request =
                service.CreateRequest("BLR", "EMP001");

            // Assert
            Assert.AreEqual(1, request.RequestId);
            Assert.AreEqual("BLR", request.OfficeCode);
            Assert.AreEqual(RefillStatus.Pending, request.Status);
            Assert.IsNull(request.AssignedVolunteerId);
        }

        [TestMethod]
        public void CreateRequest_SameOffice_ShouldNotCreateDuplicate()
        {
            // Arrange
            RefillService service = new RefillService();

            // Act
            RefillRequest firstRequest =
                service.CreateRequest("BLR", "EMP001");

            RefillRequest secondRequest =
                service.CreateRequest("BLR", "EMP002");

            // Assert
            Assert.AreEqual(
                firstRequest.RequestId,
                secondRequest.RequestId);

            Assert.AreSame(firstRequest, secondRequest);
        }

        [TestMethod]
        public void CreateRequest_DifferentOffices_ShouldCreateSeparateRequests()
        {
            // Arrange
            RefillService service = new RefillService();

            // Act
            RefillRequest bengaluruRequest =
                service.CreateRequest("BLR", "EMP001");

            RefillRequest osloRequest =
                service.CreateRequest("OSL", "EMP002");

            // Assert
            Assert.AreNotEqual(
                bengaluruRequest.RequestId,
                osloRequest.RequestId);
        }




        #region Volunteer Assignment Tests

        [TestMethod]
        public void AssignVolunteer_AvailableVolunteer_ShouldAssignSuccessfully()
        {
            // Arrange
            RefillService service = new RefillService();

            RefillRequest request =
                service.CreateRequest("BLR", "EMP001");

            List<Volunteer> volunteers = new List<Volunteer>
            {
                new Volunteer
                {
                    VolunteerId = 101,
                    EmployeeId = "EMP002",
                    Name = "Rahul",
                    HomeOfficeCode = "BLR",
                    CurrentOfficeCode = "BLR",
                    IsAvailable = true
                }
            };

            // Act
            RefillRequest result =
                service.AssignVolunteer(request.RequestId, volunteers);

            // Assert
            Assert.AreEqual(101, result.AssignedVolunteerId);
            Assert.AreEqual(RefillStatus.Assigned, result.Status);
        }

        [TestMethod]
        public void AssignVolunteer_NoAvailableVolunteer_ShouldThrowException()
        {
            // Arrange
            RefillService service = new RefillService();

            RefillRequest request =
                service.CreateRequest("BLR", "EMP001");

            List<Volunteer> volunteers = new List<Volunteer>
            {
                new Volunteer
                {
                    VolunteerId = 102,
                    EmployeeId = "EMP003",
                    Name = "Amit",
                    HomeOfficeCode = "BLR",
                    CurrentOfficeCode = "BLR",
                    IsAvailable = false
                }
            };

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() =>
                service.AssignVolunteer(request.RequestId, volunteers));
        }

        [TestMethod]
        public void AssignVolunteer_DifferentOffice_ShouldNotAssign()
        {
            // Arrange
            RefillService service = new RefillService();

            RefillRequest request =
                service.CreateRequest("BLR", "EMP001");

            List<Volunteer> volunteers = new List<Volunteer>
            {
                new Volunteer
                {
                    VolunteerId = 103,
                    EmployeeId = "EMP004",
                    Name = "John",
                    HomeOfficeCode = "OSL",
                    CurrentOfficeCode = "OSL",
                    IsAvailable = true
                }
            };

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() =>
                service.AssignVolunteer(request.RequestId, volunteers));
        }

        [TestMethod]
        public void AssignVolunteer_AlreadyAssigned_ShouldThrowException()
        {
            // Arrange
            RefillService service = new RefillService();

            RefillRequest request =
                service.CreateRequest("BLR", "EMP001");

            List<Volunteer> volunteers = new List<Volunteer>
            {
                new Volunteer
                {
                    VolunteerId = 104,
                    EmployeeId = "EMP005",
                    Name = "Kiran",
                    HomeOfficeCode = "BLR",
                    CurrentOfficeCode = "BLR",
                    IsAvailable = true
                }
            };

            service.AssignVolunteer(request.RequestId, volunteers);

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() =>
                service.AssignVolunteer(request.RequestId, volunteers));
        }

        #endregion




        #region Volunteer Reassignment Tests

        [TestMethod]
        public void ReleaseVolunteer_AssignedVolunteer_ShouldReleaseSuccessfully()
        {
            // Arrange
            RefillService service = new RefillService();

            RefillRequest request =
                service.CreateRequest("BLR", "EMP001");

            List<Volunteer> volunteers = new List<Volunteer>
    {
        new Volunteer
        {
            VolunteerId = 101,
            EmployeeId = "EMP002",
            Name = "Rahul",
            CurrentOfficeCode = "BLR",
            IsAvailable = true
        }
    };

            service.AssignVolunteer(request.RequestId, volunteers);

            // Act
            RefillRequest result =
                service.ReleaseVolunteer(request.RequestId, 101);

            // Assert
            Assert.AreEqual(RefillStatus.Pending, result.Status);
            Assert.IsNull(result.AssignedVolunteerId);
        }

        [TestMethod]
        public void ReleaseVolunteer_WrongVolunteer_ShouldThrowException()
        {
            // Arrange
            RefillService service = new RefillService();

            RefillRequest request =
                service.CreateRequest("BLR", "EMP001");

            List<Volunteer> volunteers = new List<Volunteer>
    {
        new Volunteer
        {
            VolunteerId = 101,
            CurrentOfficeCode = "BLR",
            IsAvailable = true
        }
    };

            service.AssignVolunteer(request.RequestId, volunteers);

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() =>
                service.ReleaseVolunteer(request.RequestId, 999));
        }

        [TestMethod]
        public void ReleaseVolunteer_PendingRequest_ShouldThrowException()
        {
            // Arrange
            RefillService service = new RefillService();

            RefillRequest request =
                service.CreateRequest("BLR", "EMP001");

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() =>
                service.ReleaseVolunteer(request.RequestId, 101));
        }

        [TestMethod]
        public void ReleaseVolunteer_AfterRelease_ShouldAllowReassignment()
        {
            // Arrange
            RefillService service = new RefillService();

            RefillRequest request =
                service.CreateRequest("BLR", "EMP001");

            List<Volunteer> firstVolunteers = new List<Volunteer>
            {
                new Volunteer
                {
                    VolunteerId = 101,
                    Name = "Rahul",
                    CurrentOfficeCode = "BLR",
                    IsAvailable = true
                }
            };

            List<Volunteer> secondVolunteers = new List<Volunteer>
            {
                new Volunteer
                {
                    VolunteerId = 102,
                    Name = "Amit",
                    CurrentOfficeCode = "BLR",
                    IsAvailable = true
                }
            };

            service.AssignVolunteer(request.RequestId, firstVolunteers);

            // Act
            service.ReleaseVolunteer(request.RequestId, 101);

            RefillRequest reassignedRequest =
                service.AssignVolunteer(request.RequestId, secondVolunteers);

            // Assert
            Assert.AreEqual(102, reassignedRequest.AssignedVolunteerId);
            Assert.AreEqual(RefillStatus.Assigned, reassignedRequest.Status);
        }

        #endregion



        [TestMethod]
        public void IsAssignmentExpired_AfterDeadline_ShouldReturnTrue()
        {
            // Arrange
            RefillService service = new RefillService();

            RefillRequest request =
                service.CreateRequest("BLR", "EMP100");

            List<Volunteer> volunteers = new List<Volunteer>
    {
        new Volunteer
        {
            VolunteerId = 101,
            EmployeeId = "EMP001",
            CurrentOfficeCode = "BLR",
            IsAvailable = true
        }
    };

            service.AssignVolunteer(request.RequestId, volunteers);

            // Act
            bool expired = service.IsAssignmentExpired(
                request.RequestId,
                request.AssignmentExpiresAt.Value.AddMinutes(1));

            // Assert
            Assert.IsTrue(expired);
        }


        [TestMethod]
        public void CompleteRequest_AssignedVolunteer_ShouldMarkCompleted()
        {
            // Arrange
            RefillService service = new RefillService();

            RefillRequest request =
                service.CreateRequest("BLR", "EMP100");

            List<Volunteer> volunteers = new List<Volunteer>
    {
        new Volunteer
        {
            VolunteerId = 101,
            EmployeeId = "EMP001",
            CurrentOfficeCode = "BLR",
            IsAvailable = true
        }
    };

            service.AssignVolunteer(request.RequestId, volunteers);

            // Act
            RefillRequest completed =
                service.CompleteRequest(request.RequestId, 101);

            // Assert
            Assert.AreEqual(RefillStatus.Completed, completed.Status);
            Assert.AreEqual(101, completed.AssignedVolunteerId);
            Assert.IsNull(completed.AssignmentExpiresAt);
        }

    }
}
