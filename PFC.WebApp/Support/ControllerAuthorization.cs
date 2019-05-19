using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.Mvc
{
    public static class ControllerAuthorization
    {
        static IDictionary<string, string[]> ControllerRoles;
        static ControllerAuthorization() {

            ControllerRoles = Assembly.GetExecutingAssembly().GetTypes()
                .Where(t => typeof(Controller).IsAssignableFrom(t))
                .Select(t => new { t.Name, Roles = t.GetCustomAttribute<AuthorizeAttribute>()?.Roles?.Split(",") ?? new string[0] })
                .ToDictionary(x => x.Name, x => x.Roles);

        }

        public static string[] ControllerAuthorizedRoles(string controllerName)
        {
            controllerName = controllerName.EndsWith("Controller") ? controllerName : controllerName + "Controller";
            return ControllerRoles[controllerName];
        }
    }
}
