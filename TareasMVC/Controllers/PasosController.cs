using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TareasMVC.Entidades;
using TareasMVC.Models;
using TareasMVC.Servicios;

namespace TareasMVC.Controllers;

[Route("api/pasos")]
public class PasosController: ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IServicioUsuarios _servicioUsuarios;

    public PasosController(ApplicationDbContext context, IServicioUsuarios servicioUsuarios)
    {
        _servicioUsuarios = servicioUsuarios;
        _context = context;
    }

    [HttpPost("{tareaId:int}")]
    public async Task<ActionResult<Paso>> Post(int tareaId, [FromBody] PasoCrearDTO pasoCrearDto)
    {
        var usuarioId = _servicioUsuarios.ObtenerUsuarioId();

        var tarea = await _context.Tareas.FirstOrDefaultAsync(tarea => tarea.Id == tareaId);

        if (tarea is null)
        {
            return NotFound();
        }

        if (tarea.UsuarioCreacionId != usuarioId)
        {
            return Forbid();
        }

        var existenPasos = await _context.Pasos.AnyAsync(pasos => pasos.TareaId == tareaId);

        var ordenMayor = 0;
        if (existenPasos)
        {
            ordenMayor = await _context.Pasos
                .Where(pasos => pasos.TareaId == tareaId)
                .Select(pasos => pasos.Orden).MaxAsync();
        }

        var paso = new Paso();
        paso.TareaId = tareaId;
        paso.Orden = ordenMayor;
        paso.Descripcion = pasoCrearDto.Descripcion;
        paso.Realizado = pasoCrearDto.Realizado;

        _context.Add(paso);
        await _context.SaveChangesAsync();

        return paso;
    
            
    }
    
    [HttpPut("{id}")]
    public async Task<ActionResult> Put(Guid id, [FromBody] PasoCrearDTO pasoCrearDto)
    {
        var usuarioId = _servicioUsuarios.ObtenerUsuarioId();

        var paso = await _context.Pasos
            .Include(paso => paso.Tarea)
            .FirstOrDefaultAsync(paso => paso.Id == id);

        if (paso is null)
        {
            return NotFound();
        }

        if (paso.Tarea.UsuarioCreacionId != usuarioId)
        {
            return Forbid();
        }

        paso.Descripcion = pasoCrearDto.Descripcion;
        paso.Realizado = pasoCrearDto.Realizado;

        await _context.SaveChangesAsync();

        return Ok();

    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(Guid id)
    {
        var usuarioId = _servicioUsuarios.ObtenerUsuarioId();

        var paso = await _context.Pasos
            .Include(paso => paso.Tarea)
            .FirstOrDefaultAsync(paso => paso.Id == id);

        if (paso is null)
        {
            return NotFound();
        }

        if (paso.Tarea.UsuarioCreacionId != usuarioId)
        {
            return Forbid();
        }

        _context.Remove(paso);
        await _context.SaveChangesAsync();

        return Ok();
    }

    [HttpPost("ordenar/{tareaId:int}")]
    public async Task<IActionResult> Ordenar(int tareaId, [FromBody] Guid[] ids)
    {
        var usuarioId = _servicioUsuarios.ObtenerUsuarioId();

        var tarea = await _context.Tareas.FirstOrDefaultAsync(tarea => tarea.Id == tareaId);

        if (tarea is null)
        {
            return NotFound();
        }

        var pasos = await _context.Pasos.Where(paso => paso.TareaId == tareaId)
            .ToListAsync();

        var pasosIds = pasos.Select(paso => paso.Id);

        var idsPasosNoPertenecenALaTarea = ids.Except(pasosIds).ToList();

        if (idsPasosNoPertenecenALaTarea.Any())

        {
            return BadRequest("No todos los pasos estan presentes");
        }

        var pasosDiccionario = pasos.ToDictionary(paso => paso.Id);

        for (int i = 0; i < ids.Length; i++)
        {
            var pasoId = ids[i];
            var paso = pasosDiccionario[pasoId];
            paso.Orden = i + 1;
        }

        await _context.SaveChangesAsync();

        return Ok();
    }
}