
using System;
using System.Collections.Generic;
using System.Linq;
using SnackCounter.Core.Models;

namespace SnackCounter.Core.Services
{
    public class BudgetService
    {
        #region Private Fields

        private readonly List<Purchase> _purchases =
            new List<Purchase>();

        private readonly object _lock = new object();

        private readonly List<BudgetReservation> _reservations =
    new List<BudgetReservation>();

        private int _nextReservationId = 1;

        #endregion

        #region Budget Validation


        public bool CanPurchase(Office office, decimal amount, DateTime purchaseDate)
        {
            if (office == null)
                throw new ArgumentNullException(nameof(office));

            if (amount <= 0)
                throw new ArgumentOutOfRangeException(nameof(amount));

            lock (_lock)
            {
                decimal monthlySpent = _purchases
                    .Where(p =>
                        string.Equals(
                            p.OfficeCode,
                            office.Code,
                            StringComparison.OrdinalIgnoreCase) &&
                        string.Equals(
                            p.Currency,
                            office.Currency,
                            StringComparison.OrdinalIgnoreCase) &&
                        p.PurchasedAt.Year == purchaseDate.Year &&
                        p.PurchasedAt.Month == purchaseDate.Month)
                    .Sum(p => p.Amount);

                decimal monthlyReserved = _reservations
                    .Where(r =>
                        r.IsActive &&
                        string.Equals(
                            r.OfficeCode,
                            office.Code,
                            StringComparison.OrdinalIgnoreCase) &&
                        string.Equals(
                            r.Currency,
                            office.Currency,
                            StringComparison.OrdinalIgnoreCase) &&
                        r.ReservedAt.Year == purchaseDate.Year &&
                        r.ReservedAt.Month == purchaseDate.Month)
                    .Sum(r => r.Amount);

                decimal availableBudget =
                    office.MonthlyBudget - monthlySpent - monthlyReserved;

                return amount <= availableBudget;
            }
        }


        #endregion

        #region Purchase Management

        public void RecordPurchase(Office office, Purchase purchase)
        {
            if (office == null)
                throw new ArgumentNullException(nameof(office));

            if (purchase == null)
                throw new ArgumentNullException(nameof(purchase));

            if (!string.Equals(
                office.Currency,
                purchase.Currency,
                StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    "Purchase currency does not match office currency.");
            }

            lock (_lock)
            {
                if (!CanPurchase(
                    office,
                    purchase.Amount,
                    purchase.PurchasedAt))
                {
                    throw new InvalidOperationException(
                        "Monthly office budget exceeded.");
                }

                purchase.OfficeCode = office.Code;

                _purchases.Add(purchase);
            }
        }

        #endregion




        #region Budget Reservation Management

        public BudgetReservation ReserveBudget(Office office, int refillRequestId, decimal amount, DateTime reservationDate)
        {
            if (office == null)
                throw new ArgumentNullException(nameof(office));

            if (refillRequestId <= 0)
                throw new ArgumentOutOfRangeException(nameof(refillRequestId));

            if (amount <= 0)
                throw new ArgumentOutOfRangeException(nameof(amount));

            lock (_lock)
            {
                bool alreadyReserved = _reservations.Any(r =>
                    r.RefillRequestId == refillRequestId &&
                    r.IsActive);

                if (alreadyReserved)
                    throw new InvalidOperationException(
                        "An active reservation already exists for this request.");

                decimal monthlySpent = _purchases
                    .Where(p =>
                        string.Equals(
                            p.OfficeCode,
                            office.Code,
                            StringComparison.OrdinalIgnoreCase) &&
                        p.PurchasedAt.Year == reservationDate.Year &&
                        p.PurchasedAt.Month == reservationDate.Month)
                    .Sum(p => p.Amount);

                decimal monthlyReserved = _reservations
                    .Where(r =>
                        r.IsActive &&
                        string.Equals(
                            r.OfficeCode,
                            office.Code,
                            StringComparison.OrdinalIgnoreCase) &&
                        r.ReservedAt.Year == reservationDate.Year &&
                        r.ReservedAt.Month == reservationDate.Month)
                    .Sum(r => r.Amount);

                decimal availableBudget =
                    office.MonthlyBudget - monthlySpent - monthlyReserved;

                if (amount > availableBudget)
                    throw new InvalidOperationException(
                        "Insufficient monthly budget.");

                BudgetReservation reservation = new BudgetReservation
                {
                    ReservationId = _nextReservationId++,
                    RefillRequestId = refillRequestId,
                    OfficeCode = office.Code,
                    Amount = amount,
                    Currency = office.Currency,
                    ReservedAt = reservationDate,
                    IsActive = true
                };

                _reservations.Add(reservation);

                return reservation;
            }
        }



        public void ReleaseReservation(int reservationId)
        {
            if (reservationId <= 0)
                throw new ArgumentOutOfRangeException(
                    nameof(reservationId));

            lock (_lock)
            {
                BudgetReservation reservation = _reservations
                    .FirstOrDefault(r =>
                        r.ReservationId == reservationId);

                if (reservation == null)
                    throw new InvalidOperationException(
                        "Budget reservation not found.");

                if (!reservation.IsActive)
                    throw new InvalidOperationException(
                        "Budget reservation is already released.");

                reservation.IsActive = false;
            }
        }




        public void SettlePurchase(Office office, int reservationId, Purchase purchase)
        {
            if (office == null)
                throw new ArgumentNullException(nameof(office));

            if (purchase == null)
                throw new ArgumentNullException(nameof(purchase));

            if (purchase.Amount <= 0)
                throw new ArgumentOutOfRangeException(
                    nameof(purchase.Amount));

            lock (_lock)
            {
                BudgetReservation reservation = _reservations
                    .FirstOrDefault(r =>
                        r.ReservationId == reservationId);

                if (reservation == null || !reservation.IsActive)
                    throw new InvalidOperationException(
                        "Active reservation not found.");

                if (!string.Equals(
                    reservation.OfficeCode,
                    office.Code,
                    StringComparison.OrdinalIgnoreCase))
                    throw new InvalidOperationException(
                        "Reservation belongs to another office.");

                if (!string.Equals(
                    reservation.Currency,
                    purchase.Currency,
                    StringComparison.OrdinalIgnoreCase))
                    throw new InvalidOperationException(
                        "Purchase currency does not match reservation.");

                if (!string.Equals(
                    office.Currency,
                    purchase.Currency,
                    StringComparison.OrdinalIgnoreCase))
                    throw new InvalidOperationException(
                        "Purchase currency does not match office.");

                if (purchase.RefillRequestId != reservation.RefillRequestId)
                    throw new InvalidOperationException(
                        "Purchase belongs to another refill request.");

                if (purchase.Amount > reservation.Amount)
                    throw new InvalidOperationException(
                        "Purchase exceeds reserved amount.");

                if (purchase.PurchasedAt.Year != reservation.ReservedAt.Year ||
                    purchase.PurchasedAt.Month != reservation.ReservedAt.Month)
                    throw new InvalidOperationException(
                        "Purchase and reservation must be in the same month.");

                if (_purchases.Any(p =>
                    p.RefillRequestId == purchase.RefillRequestId))
                    throw new InvalidOperationException(
                        "Purchase already recorded for this refill request.");

                purchase.OfficeCode = office.Code;

                // Convert reserved spending into actual spending.
                _purchases.Add(purchase);

                reservation.IsActive = false;
            }
        }




        public void ValidateActiveReservation(int reservationId, int refillRequestId)
        {
            lock (_lock)
            {
                BudgetReservation reservation = _reservations
                    .FirstOrDefault(r =>
                        r.ReservationId == reservationId);

                if (reservation == null)
                    throw new InvalidOperationException(
                        "Budget reservation not found.");

                if (!reservation.IsActive)
                    throw new InvalidOperationException(
                        "Budget reservation is not active.");

                if (reservation.RefillRequestId != refillRequestId)
                    throw new InvalidOperationException(
                        "Reservation does not belong to this refill request.");
            }
        }


        #endregion

        #region Monthly Financial Reporting

        public MonthlyOfficeReport GetMonthlyReport(
            Office office,
            int year,
            int month)
        {
            if (office == null)
                throw new ArgumentNullException(nameof(office));

            if (year < 1 || year > 9999)
                throw new ArgumentOutOfRangeException(nameof(year));

            if (month < 1 || month > 12)
                throw new ArgumentOutOfRangeException(nameof(month));

            lock (_lock)
            {
                var monthlyPurchases = _purchases
                    .Where(p =>
                        string.Equals(
                            p.OfficeCode,
                            office.Code,
                            StringComparison.OrdinalIgnoreCase) &&
                        string.Equals(
                            p.Currency,
                            office.Currency,
                            StringComparison.OrdinalIgnoreCase) &&
                        p.PurchasedAt.Year == year &&
                        p.PurchasedAt.Month == month)
                    .ToList();

                decimal totalSpent =
                    monthlyPurchases.Sum(p => p.Amount);

                return new MonthlyOfficeReport
                {
                    OfficeCode = office.Code,
                    Currency = office.Currency,
                    MonthlyBudget = office.MonthlyBudget,
                    TotalSpent = totalSpent,
                    RemainingBudget =
                        office.MonthlyBudget - totalSpent,
                    PurchaseCount = monthlyPurchases.Count
                };
            }
        }

        #endregion

    }
}
