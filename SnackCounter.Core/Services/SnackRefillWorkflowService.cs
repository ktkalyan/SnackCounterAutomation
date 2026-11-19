
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SnackCounter.Core.Interfaces;
using SnackCounter.Core.Models;

namespace SnackCounter.Core.Services
{
    public class SnackRefillWorkflowService
    {
        #region Private Fields

        private readonly RefillService _refillService;
        private readonly BudgetService _budgetService;
        private readonly IChatNotificationService _chatService;

        #endregion

        #region Constructors

        public SnackRefillWorkflowService(
            RefillService refillService,
            BudgetService budgetService,
            IChatNotificationService chatService)
        {
            _refillService = refillService
                ?? throw new ArgumentNullException(nameof(refillService));

            _budgetService = budgetService
                ?? throw new ArgumentNullException(nameof(budgetService));

            _chatService = chatService
                ?? throw new ArgumentNullException(nameof(chatService));
        }

        // Existing two-parameter constructor.
        public SnackRefillWorkflowService(
            RefillService refillService,
            BudgetService budgetService)
            : this(
                refillService,
                budgetService,
                new FakeChatNotificationService())
        {
        }

        #endregion

        #region Volunteer Abandonment

        public RefillRequest AbandonAssignment(
            int requestId,
            int volunteerId,
            int reservationId)
        {
            // Validate the reservation first.
            _budgetService.ValidateActiveReservation(
                reservationId, requestId);

            // Release the assigned volunteer.
            RefillRequest request =
                _refillService.ReleaseVolunteer(
                    requestId, volunteerId);

            // Release the reserved budget.
            _budgetService.ReleaseReservation(
                reservationId);

            return request;
        }

        #endregion

        #region Volunteer Assignment Workflow


        public async Task<RefillRequest> AssignAndNotifyAsync(
            int requestId,
            List<Volunteer> volunteers)
        {
            if (volunteers == null)
                throw new ArgumentNullException(nameof(volunteers));

            // Step 1: Assign an available volunteer.
            RefillRequest request =
                _refillService.AssignVolunteer(requestId, volunteers);

            try
            {
                // Step 2: Find the assigned volunteer.
                Volunteer volunteer = volunteers.Find(v =>
                    v.VolunteerId == request.AssignedVolunteerId);

                if (volunteer == null)
                    throw new InvalidOperationException(
                        "Assigned volunteer not found.");

                // Step 3: Send notification.
                await _chatService.SendRefillAssignmentAsync(
                    volunteer.EmployeeId,
                    request.RequestId,
                    request.OfficeCode);

                return request;
            }
            catch
            {
                // Step 4: Roll back the assignment if notification fails.
                if (request.AssignedVolunteerId.HasValue)
                {
                    _refillService.ReleaseVolunteer(
                        request.RequestId,
                        request.AssignedVolunteerId.Value);
                }

                throw;
            }
        }


        #endregion




        #region Automatic Volunteer Reassignment

        public async Task<RefillRequest> ReassignAbandonedTaskAsync(
            int requestId,
            int abandonedVolunteerId,
            int reservationId,
            List<Volunteer> volunteers)
        {
            if (volunteers == null)
                throw new ArgumentNullException(nameof(volunteers));

            // Step 1: Release the abandoned assignment
            // and its reserved budget.
            RefillRequest request = AbandonAssignment(
                requestId,
                abandonedVolunteerId,
                reservationId);

            // Step 2: Exclude the volunteer who abandoned the task.
            List<Volunteer> eligibleVolunteers =
                volunteers.FindAll(v =>
                    v.VolunteerId != abandonedVolunteerId);

            // Step 3: Assign another volunteer and notify them.
            return await AssignAndNotifyAsync(
                request.RequestId,
                eligibleVolunteers);
        }

        #endregion

        #region Automatic Timeout Reassignment

        public async Task<RefillRequest> ReassignExpiredTaskAsync(
            int requestId,
            int reservationId,
            List<Volunteer> volunteers,
            DateTime currentTimeUtc)
        {
            if (volunteers == null)
                throw new ArgumentNullException(nameof(volunteers));

            // Step 1: Check whether the assignment has expired.
            bool isExpired = _refillService.IsAssignmentExpired(
                requestId, currentTimeUtc);

            if (!isExpired)
                throw new InvalidOperationException(
                    "The assignment has not expired.");

            // Step 2: Find the current assigned volunteer.
            // The request is expected to be in Assigned status.
            RefillRequest request = _refillService.GetRequest(requestId);

            if (!request.AssignedVolunteerId.HasValue)
                throw new InvalidOperationException(
                    "No volunteer is assigned to this request.");

            int expiredVolunteerId =
                request.AssignedVolunteerId.Value;

            // Step 3: Release old assignment and reassign.
            return await ReassignAbandonedTaskAsync(
                requestId,
                expiredVolunteerId,
                reservationId,
                volunteers);
        }

        #endregion

    }
}
