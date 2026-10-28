namespace FlotaVehicular.Domain.Entities;

// Estado en que se encuentra una unidad dentro de su ciclo de mantenimiento.
public enum VehicleStatus
{
    // La unidad está en operación normal y al día de su mantenimiento preventivo.
    Operativo = 1,

    // La unidad está recibiendo mantenimiento; solo admite órdenes de tipo emergencia.
    EnMantenimiento = 2,

    // La unidad está fuera de servicio y no recibe órdenes de mantenimiento.
    FueraDeServicio = 3,

    // Baja lógica: el registro se conserva y ya no admite cambios ni órdenes.
    DadoDeBaja = 4
}
