using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace VacationRental.Api.Models
{
    public class CalendarDateViewModel
    {
        public DateTime Date { get; set; }
        public List<CalendarBookingViewModel> Bookings { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<PreparationTimeViewModel> PreparationTimes { get; set; }
    }
}
