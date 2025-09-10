using System.ComponentModel.DataAnnotations;

namespace APP.Models
{
    public class Usuario
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre de usuario es obligatorio")]
        [StringLength(16, ErrorMessage = "El nombre de usuario no puede exceder 16 caracteres")]
        [Display(Name = "Usuario")]
        public string Username { get; set; }

        [Required(ErrorMessage = "La contraseña es obligatoria")]
        [StringLength(32, ErrorMessage = "La contraseña no puede exceder 32 caracteres")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required(ErrorMessage = "El rol es obligatorio")]
        [Display(Name = "Rol")]
        [RegularExpression("^(ADMIN|ENCARGADO|EMPLEADO)$", ErrorMessage = "Rol inválido")]
        public string Rol { get; set; }

        // ================= Datos de la tabla Generales =================
        [Required(ErrorMessage = "El nombre es obligatorio")]
        [Display(Name = "Nombre")]
        public string Nombre { get; set; }

        [Required(ErrorMessage = "El apellido es obligatorio")]
        [Display(Name = "Apellido")]
        public string Apellido { get; set; }

        [Required(ErrorMessage = "El sexo es obligatorio")]
        [Display(Name = "Sexo")]
        [RegularExpression("^(M|F)$", ErrorMessage = "Sexo inválido")]
        public string Sexo { get; set; }

        // ================= Datos de Nivel Académico =================
        [Required(ErrorMessage = "El ID del nivel académico es obligatorio")]
        [Display(Name = "Nivel Académico ID")]
        public int NivelAcademicoId { get; set; } // ID para relacionar con la tabla Nivel_Academico

        [Required(ErrorMessage = "El nivel académico es obligatorio")]
        [Display(Name = "Nivel Académico")]
        public string NivelAcademico { get; set; } // Nombre del nivel para combobox estático

        [Display(Name = "Institución")]
        public string Institucion { get; set; } // Permite que el usuario agregue su institución

        // Campo auxiliar (para mostrar el nombre del nivel en vistas)
        public string? NivelAcademicoNombre { get; set; }
    }
}
