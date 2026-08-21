using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Optica.Application.Abstracciones;
using Optica.Domain.Comun;

namespace Optica.Infrastructure.Persistencia.Interceptores;

/// <summary>
/// Puebla las cuatro columnas de auditoría de toda entidad auditable: usuario y fecha de creación,
/// usuario y fecha de actualización.
///
/// Se hace en un interceptor y no en cada caso de uso por dos razones. La primera es que el
/// principio VII lo exige en **todas** las tablas, y cuarenta tablas por veintiséis features son
/// demasiados sitios donde olvidarlo. La segunda es la fuente de tiempo: aquí se toma una sola vez
/// desde <see cref="TimeProvider"/>, de modo que todas las filas de un mismo guardado comparten
/// exactamente la misma marca, en lugar de diferir en milisegundos según el orden en que se
/// escribieron.
/// </summary>
/// <param name="usuarioActual">Quién ejecuta la petición en curso.</param>
/// <param name="reloj">Única fuente de tiempo (D-07).</param>
public sealed class ColumnasDeAuditoriaInterceptor(IUsuarioActual usuarioActual, TimeProvider reloj)
    : SaveChangesInterceptor
{
    /// <inheritdoc />
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(eventData);

        Marcar(eventData.Context);

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    /// <inheritdoc />
    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        ArgumentNullException.ThrowIfNull(eventData);

        Marcar(eventData.Context);

        return base.SavingChanges(eventData, result);
    }

    private void Marcar(DbContext? contexto)
    {
        if (contexto is null)
        {
            return;
        }

        var ahora = reloj.GetUtcNow();
        var usuario = usuarioActual.Id;

        foreach (var entrada in contexto.ChangeTracker.Entries<IEntidadAuditable>())
        {
            switch (entrada.State)
            {
                case EntityState.Added:
                    entrada.Entity.MarcarCreacion(usuario, ahora);
                    entrada.Entity.MarcarActualizacion(usuario, ahora);
                    break;

                case EntityState.Modified:
                    entrada.Entity.MarcarActualizacion(usuario, ahora);
                    break;

                default:
                    // Sin cambios, desasociada o eliminada: no hay nada que marcar. Y en una tabla
                    // de solo inserción no debería haber eliminaciones en primer lugar.
                    break;
            }
        }
    }
}
