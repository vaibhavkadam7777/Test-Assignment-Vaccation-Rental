using System;
using System.Collections.Generic;
using System.Linq;
using VacationRental.Api.Models;

namespace VacationRental.Api.RentalHelperService
{
    public class COVIDFunctionalityRentalHelperService : IRentalHelperService
    {
        private readonly IDictionary<int, RentalViewModel> _rentals;
        private readonly IDictionary<int, BookingViewModel> _bookings;

        public COVIDFunctionalityRentalHelperService(IDictionary<int, RentalViewModel> rentals, IDictionary<int, BookingViewModel> bookings)
        {
            _rentals = rentals;
            _bookings = bookings;
        }

        public bool DoesBookingDateOverLap(BookingViewModel ExistingBooking, BookingBindingModel newBooking)
        {
            if (ExistingBooking.RentalId != newBooking.RentalId)
                return false;

            int preparationTimeInDays = _rentals[newBooking.RentalId].PreparationTimeInDays;

            var bookedStartDate = ExistingBooking.Start;
            var bookedEndDate = ExistingBooking.Start.AddDays(ExistingBooking.Nights).AddDays(preparationTimeInDays);

            var newBookStartDate = newBooking.Start.Date;
            var newBookEndDate = newBooking.Start.Date.AddDays(newBooking.Nights).AddDays(preparationTimeInDays);

            bool isOverlap = (bookedStartDate <= newBookStartDate && bookedEndDate > newBookStartDate)          // newBookStartDate lie between existing booking 
                       || (bookedStartDate < newBookEndDate && bookedEndDate >= newBookEndDate)                 // newBookEndDate lie between existing booking
                       || (bookedStartDate > newBookStartDate && bookedEndDate < newBookEndDate);               // new Booking overshadows existing booking

            return isOverlap;
        }


        public List<CalendarDateViewModel> CalculateCalender(int rentalId, DateTime start, int nights)
        {
            List<CalendarDateViewModel> calendar = new List<CalendarDateViewModel>();

            var preparationTimeInDays = _rentals[rentalId].PreparationTimeInDays;

            // Array of size of units 
            int[] bookingIds = new int[_rentals[rentalId].Units];

            for (var i = 0; i < nights; i++)
            {
                var date = new CalendarDateViewModel
                {
                    Date = start.Date.AddDays(i),
                    Bookings = new List<CalendarBookingViewModel>(),
                    PreparationTimes = new List<PreparationTimeViewModel>()
                };

                foreach (var booking in _bookings.Values.Where(x => x.RentalId == rentalId))
                {
                    var bookingStartDate = booking.Start;
                    var bookingEndDate = booking.Start.AddDays(booking.Nights);
                    var endPreparationDate = bookingEndDate.AddDays(preparationTimeInDays);

                    if (bookingStartDate <= date.Date && endPreparationDate > date.Date)
                    {
                        int unit = GetAssignedUnitNumber(bookingIds, booking.Id);

                        if (bookingEndDate > date.Date)
                        {
                            date.Bookings.Add(new CalendarBookingViewModel { Id = booking.Id, Unit = unit });
                        }
                        else
                        {
                            date.PreparationTimes.Add(new PreparationTimeViewModel { Unit = unit });
                        }

                        // check if date is last preparationDay 
                        if (endPreparationDate.AddDays(-1) == date.Date)
                        {
                            bookingIds[unit - 1] = 0;        //empty the unit 
                        }
                    }
                }

                calendar.Add(date);
            }
            return calendar;
        }

        private int GetAssignedUnitNumber(int[] bookingIds, int bookingId)
        {
            var index = Array.IndexOf(bookingIds, bookingId);   // get unit index for booking Id 
            if (index < 0)
            {
                index = Array.IndexOf(bookingIds, 0);    // check next unit is available  for booking
                bookingIds[index] = bookingId;           //assign the unit to specific booking Id
            }
            return index + 1;
        }


        public RentalViewModel UpdateExistingRental(RentalViewModel existingRental, RentalBindingModel newRental)
        {
            // If units are unchanged/increased And preparationTime unchanged/decreased its updatable
            if (existingRental.Units <= newRental.Units && existingRental.PreparationTimeInDays >= newRental.PreparationTimeInDays)
            {
                existingRental.Units = newRental.Units;
                existingRental.PreparationTimeInDays = newRental.PreparationTimeInDays;
                return existingRental;
            }
            else
            {
                var min_date = _bookings.Values.Where(x => x.RentalId == existingRental.Id).Select(x => x.Start.Date).Min();
                var max_date = _bookings.Values.Where(x => x.RentalId == existingRental.Id).
                    Select(x => x.Start.AddDays(x.Nights).Date).Max().AddDays(newRental.PreparationTimeInDays);

                var daysRentalOccupied = (max_date - min_date).TotalDays;

                for (var i = 0; i < daysRentalOccupied; i++)
                {
                    var unitsAllocated = 0;
                    var date = min_date.Date.AddDays(i);

                    foreach (var booking in _bookings.Values.Where(x => x.RentalId == existingRental.Id))
                    {
                        if (booking.Start <= date.Date && booking.Start.AddDays(booking.Nights).AddDays(newRental.PreparationTimeInDays) > date.Date)
                        {
                            unitsAllocated++;
                        }
                    }

                    if (unitsAllocated > newRental.Units)
                        return null;
                }
                existingRental.Units = newRental.Units;
                existingRental.PreparationTimeInDays = newRental.PreparationTimeInDays;
                return existingRental;
            }
        }
    }
}
