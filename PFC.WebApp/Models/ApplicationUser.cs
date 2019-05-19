using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace PFC.WebApp.Models
{
    // Add profile data for application users by adding properties to the ApplicationUser class
    public class ApplicationUser : IdentityUser
    {

        /// <summary>
        /// Comercial asignado al usuario.
        /// </summary>
        [ForeignKey(nameof(ComercialId))]
        public virtual ApplicationUser Comercial { get; set; }

        /// <summary>
        /// Identificador del comercial asignado al usuario.
        /// </summary>
        public virtual string ComercialId { get; set; }

    }
}
