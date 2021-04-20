using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MINEM.MIGI.Web.Models
{
    public class LoginViewModel
    {
        [Required]
        [StringLength(100)]
        [RegularExpression(@"^(([^<>()\[\]\\.,;:\s@" + "\u0022" + @"]+(\.[^<>()\[\]\\.,;:\s@\" + "\u0022" + @"]+)*)|(\" + "\u0022" + @".+\" + "\u0022" + @"))@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\])|(([a-zA-Z\-0-9]+\.)+[a-zA-Z]{2,}))$", ErrorMessage = "Debe ingresar un formato válido de correo")]
        [Display(Name = "Correo electrónico")]
        public string CORREO { get; set; }

        [Required]
        [StringLength(20, MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña")]
        public string CONTRASENA { get; set; }
    }
}