using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using VacationRental.Api.Models;

namespace VacationRental.Api.Controllers
{
    [Route("api/v1/rentals")]
    [ApiController]
    public class RentalsController : ControllerBase
    {
        private readonly IDictionary<int, RentalViewModel> _rentals;

        private readonly RentalHelperService.IRentalHelperService _bookingHelper;

        public RentalsController(IDictionary<int, RentalViewModel> rentals,
            RentalHelperService.IRentalHelperService bookingHelper)
        {
            _rentals = rentals;
            _bookingHelper = bookingHelper;
        }

        [HttpGet]
        [Route("{rentalId:int}")]
        public RentalViewModel Get(int rentalId)
        {
            if (!_rentals.ContainsKey(rentalId))
                throw new ApplicationException("Rental not found");

            return _rentals[rentalId];
        }

        [HttpPost]
        public ResourceIdViewModel Post(RentalBindingModel model)
        {
            var key = new ResourceIdViewModel { Id = _rentals.Keys.Count + 1 };

            _rentals.Add(key.Id, new RentalViewModel
            {
                Id = key.Id,
                Units = model.Units,
                PreparationTimeInDays = model.PreparationTimeInDays
            });

            return key;
        }

        [HttpPut("{rentalId}")]
        public RentalViewModel Put(int rentalId, RentalBindingModel model)
        {
            if (!_rentals.ContainsKey(rentalId))
                throw new ApplicationException("Rental not found");

            var existingRental = _rentals[rentalId];

            var updatedRental = _bookingHelper.UpdateExistingRental(existingRental, model);

            return updatedRental ?? throw new ApplicationException($"Existing Rental with Id : {rentalId} can not updated"); ;
        }
    }
}
