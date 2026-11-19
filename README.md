

\# Framsikt Snack Counter Automation



A .NET 9 prototype that coordinates office snack refills,

volunteer assignments, monthly budgets, reimbursements,

and financial reporting across Framsikt's Bengaluru and

Oslo offices.



\## Problem



Snack counters may remain empty because employees do not

know who is responsible for restocking. Volunteers can

duplicate purchases, abandon assignments, or exceed

monthly budgets. Reimbursement tracking also requires

coordination with HR.



\## Offices



| Office | Currency | Monthly Budget |

|--------|----------|----------------|

| Bengaluru | INR | 15,000 |

| Oslo | NOK | 6,000 |



\## Solution Structure



SnackCounterAutomation/

\- SnackCounter.Core/

&#x20; - Models/

&#x20; - Enums/

&#x20; - Interfaces/

&#x20; - Services/

\- SnackCounter.Demo/

\- SnackCounter.Tests/

\- SnackCounterAutomation.sln

\- README.md

\- SYSTEM\_DESIGN.md



\## Implemented Features



\- Create refill requests for an office.

\- Reuse an existing active refill request for the same office.

\- Assign an available volunteer at their current office.

\- Send assignment notifications through a fake chat service.

\- Reserve office budget before purchasing.

\- Settle purchases against active budget reservations.

\- Prevent duplicate purchase settlement for the same refill request.

\- Release abandoned assignments and budget reservations.

\- Reassign abandoned or expired assignments when triggered.

\- Track assignment expiration using UTC timestamps.

\- Mark successfully fulfilled requests as completed.

\- Submit fake HR reimbursement requests.

\- Track reimbursement approval and payment status.

\- Generate monthly spending reports per office.

\- Convert combined financial totals into NOK using an

&#x20; explicitly supplied INR-to-NOK exchange rate.



\## Technology Stack



\- C#

\- .NET 9

\- MSTest

\- In-memory collections

\- Fake chat and HR integrations

\- Visual Studio



\## Requirements



\- .NET 9 SDK

\- Visual Studio with .NET development support,

&#x20; or another compatible .NET development environment



\## Build



From the solution directory:



&#x20;   dotnet restore

&#x20;   dotnet build



\## Run the Demo



&#x20;   dotnet run --project SnackCounter.Demo



The demo demonstrates:



1\. Bengaluru refill request and volunteer assignment.

2\. Bengaluru budget reservation and purchase settlement.

3\. Fake HR reimbursement submission and approval.

4\. Oslo refill request and purchase workflow.

5\. Monthly multi-currency financial reporting.

6\. Fake chat assignment notifications.



\## Run Automated Tests



&#x20;   dotnet test



The test suite covers refill management, budget operations,

workflow behavior, fake integrations, and financial reporting.



\## Example Financial Report



For an illustrative INR-to-NOK rate of 0.11:



| Office | Spent | Remaining |

|--------|-------|-----------|

| Bengaluru | 2,800 INR | 12,200 INR |

| Oslo | 1,100 NOK | 4,900 NOK |



Combined spending: 1,408 NOK.



The exchange rate is illustrative, not a live market rate.



\## Design Decisions



\- Business logic is separated from the console application.

\- Chat and HR integrations use interfaces or fake implementations.

\- Budget operations use locking to protect shared in-memory state.

\- Office budgets are maintained in their local currencies.

\- Reporting converts totals only after per-office aggregation.

\- Volunteer selection uses the current office, supporting

&#x20; visitors working temporarily at another office.



\## Current Limitations



\- State is stored in memory and is lost when the application exits.

\- There is no database or distributed transaction management.

\- Assignment expiration requires an explicit workflow invocation;

&#x20; there is no background scheduler.

\- Volunteer selection currently chooses the first eligible volunteer;

&#x20; workload balancing is not implemented.

\- The application does not enforce a global one-active-task-per-

&#x20; volunteer constraint.

\- Budget settlement and refill completion are separate operations.

\- Chat and HR integrations are simulated.

\- Receipt image paths are placeholders, not uploaded files.

\- Exchange rates must be supplied by the caller.

\- Concurrency protection is not transactional across services.



\## Production Improvements



For a production implementation:



\- Use a persistent relational database.

\- Enforce uniqueness and assignment constraints transactionally.

\- Add a scheduled background worker for expired assignments.

\- Add retries, idempotency keys, and integration error handling.

\- Connect real chat and HR APIs.

\- Store receipt images securely.

\- Add audit logging and operational monitoring.

\- Introduce volunteer workload tracking and fair rotation.

\- Use a reliable exchange-rate provider.



\## Project Status



Working console prototype with automated unit tests.



This is a demonstration implementation, not a

production-ready deployment.



