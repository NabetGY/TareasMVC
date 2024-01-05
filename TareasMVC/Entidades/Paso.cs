namespace TareasMVC.Entidades;

public class Paso
{
    public Guid Id { get; set; }

    public int TareaId { get; set; }

    //propiedad de navegacion; como un inner join
    // un paso le corresponde a una tarea
    public Tarea Tarea { get; set; }

    public string Descripcion { get; set; }

    public bool Realizado { get; set; }

    public int Orden { get; set; }
}