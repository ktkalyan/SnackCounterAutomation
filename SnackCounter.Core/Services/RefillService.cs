
using System;
using System.Collections.Generic;
using System.Linq;
using SnackCounter.Core.Enums;
using SnackCounter.Core.Models;

namespace SnackCounter.Core.Services
{
    public class RefillService
    {
        #region Private Fields

        private readonly List<RefillRequest> _requests =
            new List<RefillRequest>();

        private readonly object _lock = new object();

        private int _nextRequestId = 1;

        #endregion


        #region Refill Request Management

        public RefillRequest CreateRequest(string officeCode, string employeeId)
        {
            if (string.IsNullOrWhiteSpace(officeCode))
                throw new ArgumentException(
                    "Office code is required.");

            if (string.IsNullOrWhiteSpace(employeeId))
                throw new ArgumentException(
                    "Employee ID is required.");

            officeCode = officeCode.Trim().ToUpperInvariant();

            lock (_lock)
            {
                RefillRequest existingRequest = _requests
                    .FirstOrDefault(r =>
                        r.OfficeCode == officeCode &&
                        (r.Status == RefillStatus.Pending ||
                         r.Status == RefillStatus.Assigned));

                if (existingRequest != null)
                    return existingRequest;

                RefillRequest newRequest = new RefillRequest
                {
                    RequestId = _nextRequestId++,
                    OfficeCode = officeCode,
                    RequestedByEmployeeId = employeeId,
                    CreatedAt = DateTime.UtcNow,
                    Status = RefillStatus.Pending,
                    AssignedVolunteerId = null
                };

                _requests.Add(newRequest);

                return newRequest;
            }
        }



        public RefillRequest ReleaseVolunteer(int requestId,int volunteerId)
        {
            lock (_lock)
            {
                RefillRequest request = _requests
                    .FirstOrDefault(r => r.RequestId == requestId);

                if (request == null)
                    throw new InvalidOperationException(
                        "Refill request not found.");

                if (request.Status != RefillStatus.Assigned)
                    throw new InvalidOperationException(
                        "Only assigned requests can be released.");

                if (request.AssignedVolunteerId != volunteerId)
                    throw new InvalidOperationException(
                        "Volunteer is not assigned to this request.");


                request.AssignedVolunteerId = null;
                request.AssignedAt = null;
                request.AssignmentExpiresAt = null;
                request.Status = RefillStatus.Pending;


                return request;
            }
        }



        #endregion


        #region Volunteer Assignment

        public RefillRequest AssignVolunteer(int requestId, List<Volunteer> volunteers)
        {
            if (volunteers == null)
                throw new ArgumentNullException(
                    nameof(volunteers));

            lock (_lock)
            {
                RefillRequest request = _requests
                    .FirstOrDefault(r =>
                        r.RequestId == requestId);

                if (request == null)
                    throw new InvalidOperationException(
                        "Refill request not found.");

                if (request.Status != RefillStatus.Pending)
                    throw new InvalidOperationException(
                        "Only pending requests can be assigned.");

                Volunteer volunteer = volunteers
                    .FirstOrDefault(v =>
                        v.IsAvailable &&
                        string.Equals(
                            v.CurrentOfficeCode,
                            request.OfficeCode,
                            StringComparison.OrdinalIgnoreCase));

                if (volunteer == null)
                    throw new InvalidOperationException(
                        "No available volunteer found.");


                request.AssignedVolunteerId = volunteer.VolunteerId;
                request.AssignedAt = DateTime.UtcNow;
                request.AssignmentExpiresAt =
                    request.AssignedAt.Value.AddHours(2);

                request.Status = RefillStatus.Assigned;


                return request;
            }
        }

        #region Assignment Timeout

        public bool IsAssignmentExpired(
            int requestId,
            DateTime currentTimeUtc)
        {
            lock (_lock)
            {
                RefillRequest request = _requests
                    .FirstOrDefault(r => r.RequestId == requestId);

                if (request == null)
                    throw new InvalidOperationException(
                        "Refill request not found.");

                return request.Status == RefillStatus.Assigned
                    && request.AssignmentExpiresAt.HasValue
                    && currentTimeUtc >=
                       request.AssignmentExpiresAt.Value;
            }
        }

        #endregion

        #endregion



        #region Request Retrieval

        public RefillRequest GetRequest(int requestId)
        {
            lock (_lock)
            {
                RefillRequest request = _requests
                    .FirstOrDefault(r => r.RequestId == requestId);

                if (request == null)
                    throw new InvalidOperationException(
                        "Refill request not found.");

                return request;
            }
        }

        #endregion

        #region Complete Refill Request

        public RefillRequest CompleteRequest(
            int requestId,
            int volunteerId)
        {
            lock (_lock)
            {
                RefillRequest request = _requests
                    .FirstOrDefault(r => r.RequestId == requestId);

                if (request == null)
                    throw new InvalidOperationException(
                        "Refill request not found.");

                if (request.Status != RefillStatus.Assigned)
                    throw new InvalidOperationException(
                        "Only assigned requests can be completed.");

                if (request.AssignedVolunteerId != volunteerId)
                    throw new InvalidOperationException(
                        "Only the assigned volunteer can complete this request.");

                request.Status = RefillStatus.Completed;
                request.AssignmentExpiresAt = null;

                return request;
            }
        }

        #endregion

    }
}
