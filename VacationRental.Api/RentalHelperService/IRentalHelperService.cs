using System;
using System.Collections.Generic;
using VacationRental.Api.Models;

namespace VacationRental.Api.RentalHelperService
{
    public interface IRentalHelperService
    {
        bool DoesBookingDateOverLap(BookingViewModel existingBooking, BookingBindingModel newBooking);

        List<CalendarDateViewModel> CalculateCalender(int rentalId, DateTime start, int nights);

        RentalViewModel UpdateExistingRental(RentalViewModel existingRental, RentalBindingModel newRental);
    }
}
