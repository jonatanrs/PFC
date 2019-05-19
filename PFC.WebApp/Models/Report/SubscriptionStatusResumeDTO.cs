using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PFC.WebApp.Models.Report
{
    public class PhoneEventsWithoutSubscriptionDTO
    {
        public string MSISDN { get; set; }

        public int EventsCount { get; set; }

        public DateTime First { get; set; }

        public DateTime Last { get; set; }
    }
}
