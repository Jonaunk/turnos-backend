namespace Application.Common.Interfaces
{
    public interface IDateTimeService
    {
        //Creamos una interfaz para que se pueda inyectar automaticamente la fecha cuando se hace
        //un cambio desde un comando y la usamos en el dbcontext
        DateTime NowUtc { get; }
        DateTime Now { get; }
    }
}