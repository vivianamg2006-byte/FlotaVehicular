namespace FlotaVehicular.Domain.Entities;

// Ciclo de vida de una orden de mantenimiento.
public enum OrderStatus
{
    // Recién creada; falta asignar técnico y ejecutar el trabajo.
    Solicitada = 1,

    // Se diagnosticó la falla y se definió el plan de reparación.
    Diagnosticada = 2,

    // El plan fue aceptado y se programó la reparación.
    Aprobada = 3,

    // Se canceló el trabajo por decisión del cliente o del taller.
    Rechazada = 4,

    // La reparación está en curso.
    EnEjecucion = 5,

    // La reparación está esperando un repuesto.
    EnEsperaDeRepuesto = 6,

    // La reparación se completó y se registró el cierre.
    Completada = 7,

    // La orden fue cancelada antes o durante su ejecución.
    Cancelada = 8
}
