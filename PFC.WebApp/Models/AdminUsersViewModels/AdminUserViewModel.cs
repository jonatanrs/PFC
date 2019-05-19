namespace PFC.WebApp.Models.AdminUsersViewModels
{
    class AdminUserViewModel
    {
        /// <summary>
        /// Identificador del usuario
        /// </summary>
        public string ID { get; set; }

        /// <summary>
        /// Nombre del usuario.
        /// </summary>
        public string Usuario { get; set; }

        /// <summary>
        /// Roles del usuario.
        /// </summary>
        public string Roles { get; set; }

        /// <summary>
        /// Comercial asignado al usuario.
        /// </summary>
        public string Comercial { get; set; }


        /// <summary>
        /// Indica si el usuario está bloqueado.
        /// </summary>
        public bool IsLocked { get; set; }
    }
}