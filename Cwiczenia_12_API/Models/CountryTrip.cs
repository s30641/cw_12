using System;
using System.Collections.Generic;

namespace Cwiczenia_12_API.Models
{
    public partial class CountryTrip
    {
        public int IdCountry { get; set; }
        public int IdTrip { get; set; }

        public virtual Country IdCountryNavigation { get; set; } = null!;
        public virtual Trip IdTripNavigation { get; set; } = null!;
    }
}