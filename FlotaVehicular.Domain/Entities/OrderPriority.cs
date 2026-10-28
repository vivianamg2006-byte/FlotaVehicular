namespace FlotaVehicular.Domain.Entities;

// Prioridad con la que se clasifica una orden de mantenimiento.
public enum OrderPriority
{
    // Afecta la operación pero puede esperar el próximo ciclo programado.
    Baja = 1,

    // Requiere atención ordenada, sin urgencia.
    Media = 2,

    // La unidad no puede operar con seguridad hasta resolverla.
    Alta = 3,

    // Riesgo inmediato para la operación o los pasajeros.
    Critica = 4
}
