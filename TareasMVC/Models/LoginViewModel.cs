using System.ComponentModel.DataAnnotations;

namespace TareasMVC.Models;

public class LoginViewModel
{
    [Required(ErrorMessage = "Error.Requerido")]
    [EmailAddress(ErrorMessage = "Error.Email")]
    public string Email { get; set; }

    [Required(ErrorMessage = "Error.Requerido")]
    [DataType(DataType.Password)]
    public string Password { get; set; }

    public bool Recuerdame { get; set; }
}