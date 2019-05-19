using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PFC.WebApp
{
    public static class ControllerExtensions
    {
        public static void DisallowFormEdition(this Controller controller, string reason)
        {
            controller.ViewData["FormEditionDisabled"] = reason;
        }
    }
}
