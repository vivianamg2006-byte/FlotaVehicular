namespace FlotaVehicular.Domain.Entities;

// El catálogo está completo; este avance solo utiliza Solicitada.
public enum OrderStatus
{
    Solicitada = 1,
    Diagnosticada = 2,
    Aprobada = 3,
    Rechazada = 4,
    EnEjecucion = 5,
    EnEsperaDeRepuesto = 6,
    Completada = 7,
    Cancelada = 8
}
