using AutoMapper;
using TareasMVC.Entidades;
using TareasMVC.Models;

namespace TareasMVC.Servicios;

public class AutoMapperProfiles: Profile
{
    public AutoMapperProfiles()
    {
        CreateMap<Tarea, TareaDTO>()
            .ForMember(dto => dto.PasosTotal,
                ent => 
                    ent.MapFrom(tarea=> tarea.Pasos.Count()) )
            .ForMember(dto => dto.PasosRealizados,
                ent => 
                    ent.MapFrom(tarea => tarea.Pasos
                        .Where(pasos => pasos.Realizado).Count()));
    }
}