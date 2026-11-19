
# Snack Counter Automation — System Design

## 1. Objective

Framsikt operates snack counters in Bengaluru and Oslo,
supported by volunteer employees.

The system aims to reduce empty counters, duplicate
purchases, abandoned assignments, volunteer overload,
and delays in coordinating reimbursements.

Office budgets are 15,000 INR per month in Bengaluru
and 6,000 NOK per month in Oslo.

## 2. Architecture

The prototype consists of three .NET 9 projects:

**SnackCounter.Core**
Contains business models, service interfaces, and
the core application logic.

**SnackCounter.Demo**
A console application demonstrating complete
refill-to-reimbursement workflows.

**SnackCounter.Tests**
MSTest automated tests validating core behaviors.

### Core Components

- RefillService: Request creation, assignment,
  expiration checks, release, and completion.
- BudgetService: Budget validation, reservations,
  settlement, and monthly spending summaries.
- SnackRefillWorkflowService: Coordinates assignments,
  chat notifications, and triggered reassignment.
- FakeChatNotificationService: Simulates volunteer
  notifications.
- FakeHrReimbursementService: Simulates reimbursement
  submission and status changes.
- FinancialReportService: Combines office reports
  using an explicit currency conversion rate.

## 3. Main Workflow

1. An employee reports that snacks need refilling.
2. The system creates or reuses an active office request.
3. An available volunteer at that office is assigned.
4. The fake chat service sends an assignment notification.
5. Budget is reserved before a purchase.
6. The volunteer purchases snacks and provides receipt details.
7. The purchase is settled against the reservation.
8. The refill request is marked completed.
9. The fake HR service receives a reimbursement submission.
10. Financial reports aggregate the purchase by office and month.

### Abandoned Assignments

Assignments include a UTC expiration timestamp.

When abandonment or expiration is explicitly detected,
the workflow releases the assignment and its budget
reservation, excludes the previous volunteer, and
attempts to assign another eligible volunteer.

A replacement purchase requires a new budget reservation.

The prototype does not include a continuously running
scheduler for detecting expired assignments.

## 4. Duplicate Prevention and Budget Safety

Only one active refill request is created per office
within a RefillService instance.

Assignments and budget operations use in-memory locks.

Budget reservations account for planned spending,
reducing the risk of exceeding monthly limits.

Purchase settlement validates the active reservation,
office, currency, refill request, and amount.

Duplicate settlement for a refill request is rejected.

These protections operate within a single process.
Cross-service transactions and database-level
constraints would be required in production.

## 5. Multi-Office and Multi-Currency Support

Each volunteer has a home office and a current office.

Assignment uses the current office, allowing employees
visiting another office to participate locally.

Bengaluru purchases are tracked in INR.
Oslo purchases are tracked in NOK.

Monthly reports calculate local spending and remaining
budgets separately.

For consolidated reporting, INR values are converted
into NOK using an explicitly provided rate.

The system never directly adds INR and NOK amounts.

## 6. Integrations and Reliability

Chat and HR services are simulated so the core logic
can be tested without external credentials or APIs.

The fake HR service accepts employee identification,
purchase amount, currency, description, and receipt path.

Reimbursement statuses support submission, approval,
rejection, and payment transitions.

Assignment notification failures trigger an attempt
to release the assignment.

Production integration would require persistent state,
retries, idempotency, audit records, and monitoring.

## 7. Testing and Trade-offs

Automated tests cover request handling, volunteer
assignment, abandonment, timeout detection, budgets,
reimbursement status transitions, and reporting.

The in-memory design keeps the prototype simple and
easy to demonstrate, but it does not provide durable
storage or distributed concurrency guarantees.

The first-eligible-volunteer strategy is simple but
does not guarantee fair distribution of work.

Future improvements include a database, background
scheduler, workload balancing, secure receipt storage,
real integrations, and transactional consistency.

## Conclusion

The prototype demonstrates the main operational flow
from snack refill request through purchase, reimbursement,
and multi-currency reporting.

It provides a testable foundation for a more reliable,
production-grade office automation system.
