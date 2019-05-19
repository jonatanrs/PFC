using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PFC.WebApp.Models.InvoicesViewModel
{
    public class InvoiceViewModel
    {
        public virtual int SubscriptionId { get; set; }

        public virtual DateTime Period { get; set; }
    }
}
