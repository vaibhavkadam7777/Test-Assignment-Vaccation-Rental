using System;
using System.Collections.Generic;
using System.Linq;
using VacationRental.Api.Models;

namespace VacationRental.Api.RentalHelperService
{
    public class BasicRentalHelperService : IRentalHelperService
    {
        private readonly IDictionary<int, RentalViewModel> _rentals;
        private readonly IDictionary<int, BookingViewModel> _bookings;

        public BasicRentalHelperService(IDictionary<int, RentalViewModel> rentals, IDictionary<int, BookingViewModel> bookings)
        {
            _rentals = rentals;
            _bookings = bookings;
        }

        public bool DoesBookingDateOverLap(BookingViewModel ExistingBooking, BookingBindingModel newBooking)
        {
            if (ExistingBooking.RentalId != newBooking.RentalId)
                return false;

            var bookedStartDate = ExistingBooking.Start;
            var bookedEndDate = ExistingBooking.Start.AddDays(ExistingBooking.Nights);

            var newBookStartDate = newBooking.Start.Date;
            var newBookEndDate = newBooking.Start.Date.AddDays(newBooking.Nights);

            bool isOverlap = (bookedStartDate <= newBookStartDate && bookedEndDate > newBookStartDate)          // newBookStartDate lie between existing booking
                       || (bookedStartDate < newBookEndDate && bookedEndDate >= newBookEndDate)               // newBookEndDate lie between existing booking 
                       || (bookedStartDate > newBookStartDate && bookedEndDate < newBookEndDate);             // new Booking overshadows existing booking 

            return isOverlap;
        }

        public List<CalendarDateViewModel> CalculateCalender(int rentalId, DateTime start, int nights)
        {
            List<CalendarDateViewModel> calendar = new List<CalendarDateViewModel>();

            for (var i = 0; i < nights; i++)
            {
                var date = new CalendarDateViewModel
                {
                    Date = start.Date.AddDays(i),
                    Bookings = new List<CalendarBookingViewModel>()
                };

                foreach (var booking in _bookings.Values.Where(x => x.RentalId == rentalId))
                {
                    // Check wether date sits been between booking start and endDate
                    if (booking.Start <= date.Date && booking.Start.AddDays(booking.Nights) > date.Date)          
                    {
                        date.Bookings.Add(new CalendarBookingViewModel { Id = booking.Id });
                    }
                }

                calendar.Add(date);
            }

            return calendar;
        }

        public RentalViewModel UpdateExistingRental(RentalViewModel existingRental, RentalBindingModel newRental)
        {
            if (existingRental.Units <= newRental.Units)   // if new units are equal or greater than existing units 
            {
                existingRental.Units = newRental.Units;
                return existingRental;
            }
            else
            {
                var min_date = _bookings.Values.Where(x => x.RentalId == existingRental.Id).Select(x => x.Start.Date).Min();
                var max_date = _bookings.Values.Where(x => x.RentalId == existingRental.Id).Select(x => x.Start.AddDays(x.Nights).Date).Max();

                var daysRentalOccupied = (max_date - min_date).TotalDays;

                for (var i = 0; i < daysRentalOccupied; i++)
                {
                    var unitsAllocated = 0;
                    var date = min_date.Date.AddDays(i);

                    foreach (var booking in _bookings.Values.Where(x => x.RentalId == existingRental.Id))
                    {
                        if (booking.Start <= date.Date && booking.Start.AddDays(booking.Nights) > date.Date)
                        {
                            unitsAllocated++;
                        }
                    }

                    if (unitsAllocated > newRental.Units)
                        return null;
                }

                existingRental.Units = newRental.Units;
                return existingRental;
            }
        }
    }
}
