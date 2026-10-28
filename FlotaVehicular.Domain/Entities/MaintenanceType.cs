namespace FlotaVehicular.Domain.Entities;

// Tipo de trabajo que se le pide realizar al vehículo.
public enum MaintenanceType
{
    // Mantenimiento programado según el intervalo de la categoría.
    Preventivo = 1,

    // Falla o daño reportado por el operador o el cliente.
    Correctivo = 2,

    // Avería que interrumpe la operación y se atiende de inmediato.
    Emergencia = 3
}
