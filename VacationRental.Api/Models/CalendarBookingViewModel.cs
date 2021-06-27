using Newtonsoft.Json;

namespace VacationRental.Api.Models
{
    public class CalendarBookingViewModel
    {
        public int Id { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public int Unit { get; set; }
    }
}
