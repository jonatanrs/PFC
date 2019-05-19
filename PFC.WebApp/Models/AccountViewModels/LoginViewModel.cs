using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace PFC.WebApp.Models.AccountViewModels
{
    public class LoginViewModel
    {
        [Required]
        public string NIF { get; set; }

        [Required]
        [DataType(DataType.Password), Display(Name = "Contraseña")]
        public string Password { get; set; }

        [Display(Name = "¿Mantener sesión abierta?")]
        public bool RememberMe { get; set; }
    }
}
