using Microsoft.EntityFrameworkCore;
using PFC.WebApp.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PFC.WebApp.Data
{
    /// <summary>
    /// Contine la entrada a los repositorios de persistencia de datos de la aplicación
    /// </summary>
    /// <seealso cref="Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityDbContext{PFC.WebApp.Models.ApplicationUser}" />
    public partial class ApplicationDbContext
    {
        /// <summary>
        /// Acceso al repositorio de datos de los eventos telefónicos.
        /// </summary>
        public DbSet<PhoneEvent> PhoneEvents { get; set; }

        /// <summary>
        /// Acceso al repositorio de datos del estado de los ficheros CSV.
        /// </summary>
        public DbSet<MasMovilFileState> MasMovilFileStates { get; set; }

        /// <summary>
        /// Acceso al repositorio de datos de las tarifas móviles.
        /// </summary>
        public DbSet<Plan> Plan { get; set; }

        /// <summary>
        /// Acceso al repositorio de datos de las tarifas móviles.
        /// </summary>
        public DbSet<Subscription> Subscriptions { get; set; }


        /// <summary>
        /// Acceso al repositorio de datos de facturas.
        /// </summary>
        public DbSet<Invoice> Invoice { get; set; }
    }
}
