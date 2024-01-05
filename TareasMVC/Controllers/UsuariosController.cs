using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TareasMVC.Models;
using TareasMVC.Servicios;

namespace TareasMVC.Controllers;

public class UsuariosController: Controller
{
    
    private readonly UserManager<IdentityUser> userManager;
    private readonly SignInManager<IdentityUser> signInManager;
    private readonly ApplicationDbContext context;

    public UsuariosController(UserManager<IdentityUser> userManager,
        SignInManager<IdentityUser> signInManager,
        ApplicationDbContext context)
    {
        this.signInManager = signInManager;
        this.context = context;
        this.userManager = userManager;
    }
    
    // GET
    [AllowAnonymous]
    public IActionResult Registro()
    {
        return View();
    }
    
    [AllowAnonymous]
    [HttpPost]
    public async Task<IActionResult> Registro(RegistroViewModel modelo)
    {
        if (!ModelState.IsValid)
        {
            return View(modelo);
        }

        var usuario = new IdentityUser()
        {
            Email = modelo.Email,
            UserName = modelo.Email
        };

        var resultado = await userManager.CreateAsync(
            usuario, 
            password: modelo.Password
            );

        if (resultado.Succeeded)
        {
            await signInManager.SignInAsync(usuario, isPersistent: true);
            return RedirectToAction("Index", "Home");

        }
        else
        {
            foreach (var error in resultado.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(modelo);
        }
    }
    
    [HttpGet]
    [AllowAnonymous]
    public IActionResult Login()
    {
        return View();
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> Login(LoginViewModel modelo)
    {
        if (!ModelState.IsValid)
        {
            return View(modelo);
        }

        var resultado = await signInManager.PasswordSignInAsync(
            modelo.Email, 
            modelo.Password, 
            modelo.Recuerdame,
            lockoutOnFailure: false
        );

        if (resultado.Succeeded)
        {
            return RedirectToAction("Index", "Home");
        }
        else
        {
            ModelState.AddModelError(string.Empty, "Nombre del usuario o password incorrecto.");
            return View(modelo);
        }
        
    }
    
    [HttpPost]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(IdentityConstants.ApplicationScheme);
        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    [Authorize(Roles = Constantes.RolAdmin)]
    public async Task<IActionResult> Listado(string mensaje = null)
    {
        var usuarios = await context.Users
            .Select(user => new UsuarioViewModel()
            {
                Email = user.Email,
            })
            .ToListAsync();

        var modelo = new UsuariosListadoViewModel();
        modelo.Usuarios = usuarios;
        modelo.Mensaje = mensaje;

        return View(modelo);
    }


    [HttpPost]
    [Authorize(Roles = Constantes.RolAdmin)]
    public async Task<IActionResult> HacerAdmin(string email)
    {
        var usuario = await context.Users
            .Where(user => user.Email == email).FirstOrDefaultAsync();

        if (usuario is null)
        {
            return NotFound();
        }

        await userManager.AddToRoleAsync(usuario, Constantes.RolAdmin);

        return RedirectToAction("Listado",
            routeValues: new {mensaje = "Rol asignado correctamente a "+ email});
        
    }
    
    [HttpPost]
    [Authorize(Roles = Constantes.RolAdmin)]
    public async Task<IActionResult> RemoverAdmin(string email)
    {
        var usuario = await context.Users
            .Where(user => user.Email == email).FirstOrDefaultAsync();

        if (usuario is null)
        {
            return NotFound();
        }

        await userManager.RemoveFromRoleAsync(usuario, Constantes.RolAdmin);

        return RedirectToAction("Listado",
            routeValues: new {mensaje = "Rol removido correctamente a "+ email});
        
    }

}