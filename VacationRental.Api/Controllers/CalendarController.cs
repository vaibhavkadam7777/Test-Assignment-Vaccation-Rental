using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using VacationRental.Api.Models;

namespace VacationRental.Api.Controllers
{
    [Route("api/v1/calendar")]
    [ApiController]
    public class CalendarController : ControllerBase
    {
        private readonly IDictionary<int, RentalViewModel> _rentals;
        private readonly IDictionary<int, BookingViewModel> _bookings;
        private readonly RentalHelperService.IRentalHelperService _bookingHelper;
        public CalendarController(
            IDictionary<int, RentalViewModel> rentals,
            IDictionary<int, BookingViewModel> bookings,
             RentalHelperService.IRentalHelperService bookingHelper)
        {
            _rentals = rentals;
            _bookings = bookings;
            _bookingHelper = bookingHelper;
        }

        [HttpGet]
        public CalendarViewModel Get(int rentalId, DateTime start, int nights)
        {
            if (nights < 0)
                throw new ApplicationException("Nights must be positive");
            if (!_rentals.ContainsKey(rentalId))
                throw new ApplicationException("Rental not found");


            var result = new CalendarViewModel
            {
                RentalId = rentalId,
                Dates = _bookingHelper.CalculateCalender(rentalId, start, nights)
            };

            return result;
        }

    }
}
