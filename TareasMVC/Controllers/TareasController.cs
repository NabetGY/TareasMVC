using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TareasMVC.Entidades;
using TareasMVC.Models;
using TareasMVC.Servicios;

namespace TareasMVC.Controllers;

[Route("api/tareas")]
public class TareasController: ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IServicioUsuarios _servicioUsuarios;
    private readonly IMapper _mapper;

    public TareasController(ApplicationDbContext context, IServicioUsuarios servicioUsuarios,
    IMapper mapper)
    {
        _mapper = mapper;
        _servicioUsuarios = servicioUsuarios;
        _context = context;
    }

    [HttpPost]
    public async Task<ActionResult<Tarea>> Post([FromBody] string titulo)
    {
        var usuarioId = _servicioUsuarios.ObtenerUsuarioId();

        var existenTareas = await _context.Tareas
            .AnyAsync(tarea => tarea.UsuarioCreacionId == usuarioId);

        var ordenMayor = 0;

        if (existenTareas)
        {
            ordenMayor = await _context.Tareas
                .Where(tarea => tarea.UsuarioCreacionId == usuarioId)
                .Select(tarea => tarea.Orden)
                .MaxAsync();
        }

        var tarea = new Tarea
            {
                Titulo = titulo,
                UsuarioCreacionId = usuarioId,
                FechaCreacion = DateTime.UtcNow,
                Orden = ordenMayor + 1
            };

        _context.Add(tarea);
        await _context.SaveChangesAsync();

        return tarea;
        
    }

    [HttpGet]
    public async Task<List<TareaDTO>> Get()
    {
        
        var usuarioId = _servicioUsuarios.ObtenerUsuarioId();
        var tareas = await _context.Tareas
            .Where(tarea => tarea.UsuarioCreacionId == usuarioId)
            .OrderBy(tarea => tarea.Orden)
            .ProjectTo<TareaDTO>(_mapper.ConfigurationProvider)
            .ToListAsync();

        return tareas;
    }

    [HttpPost("ordenar")]
    public async Task<IActionResult> Ordenar([FromBody] int[] ids)
    {
        var usuarioId = _servicioUsuarios.ObtenerUsuarioId();

        var tareas = await _context.Tareas
            .Where(tarea => tarea.UsuarioCreacionId == usuarioId)
            .ToListAsync();

        var tareasId = tareas.Select(tarea => tarea.Id);

        var idsTareasNoPertenecenAlUsuario = ids.Except(tareasId).ToList();

        if (idsTareasNoPertenecenAlUsuario.Any())
        {
            return Forbid();
        }

        var tareasDiccionario = tareas.ToDictionary(tarea => tarea.Id);

        for (int i = 0; i < ids.Length; i++)
        {
            var id = ids[i];
            var tarea = tareasDiccionario[id];
            tarea.Orden = i + 1;
        }

        await _context.SaveChangesAsync();

        return Ok();
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Tarea>> Get(int id)
    {
        var usuarioId = _servicioUsuarios.ObtenerUsuarioId();

        var tarea = await _context.Tareas
            .Include(tarea => tarea.Pasos.OrderBy(pasos => pasos.Orden))
            .Include(tarea => tarea.ArchivosAdjuntos.OrderBy(a => a.Orden ))
            .FirstOrDefaultAsync(tarea =>
                tarea.UsuarioCreacionId == usuarioId
                && tarea.Id == id);

        if (tarea is null)
        {
            return NotFound();
        }

        return tarea;

    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> EditarTarea(int id, [FromBody] TareaEditarDTO tareaEditarDto)
    {
        var usuarioId = _servicioUsuarios.ObtenerUsuarioId();

        var tarea = await _context.Tareas
            .FirstOrDefaultAsync(tarea =>
                tarea.UsuarioCreacionId == usuarioId
                && tarea.Id == id);

        if (tarea is null)
        {
            return NotFound();
        }

        tarea.Titulo = tareaEditarDto.Titulo;
        tarea.Descripcion = tareaEditarDto.Descripcion;

        await _context.SaveChangesAsync();

        return Ok();
    }
    
    [HttpDelete("{id:int}")]
    public async Task<ActionResult> Delete(int id)
    {
        var usuarioId = _servicioUsuarios.ObtenerUsuarioId();

        var tarea = await _context.Tareas
            .FirstOrDefaultAsync(tarea =>
                tarea.UsuarioCreacionId == usuarioId
                && tarea.Id == id);

        if (tarea is null)
        {
            return NotFound();
        }
        
        _context.Remove(tarea);
        await _context.SaveChangesAsync();

        return Ok();
    }

    
    
}