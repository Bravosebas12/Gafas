using Microsoft.EntityFrameworkCore;
using Optica.Application.Abstracciones;

namespace Optica.Infrastructure.Persistencia;

/// <summary>
/// Implementa la unidad de trabajo sobre <see cref="OpticaDbContext"/>.
/// </summary>
/// <param name="contexto">Contexto de persistencia de la petición en curso.</param>
public sealed class UnidadDeTrabajo(OpticaDbContext contexto) : IUnidadDeTrabajo
{
    /// <inheritdoc />
    public Task<int> GuardarCambios(CancellationToken cancelacion) =>
        contexto.SaveChangesAsync(cancelacion);

    /// <inheritdoc />
    public async Task<TResultado> EjecutarEnTransaccion<TResultado>(
        Func<CancellationToken, Task<TResultado>> operacion,
        CancellationToken cancelacion)
    {
        ArgumentNullException.ThrowIfNull(operacion);

        // Si ya hay una transacción abierta, no se abre otra: se participa en la existente. Sin
        // esto, un comando que despacha a otro comando anidaría transacciones y el proveedor de SQL
        // Server no lo admite.
        if (contexto.Database.CurrentTransaction is not null)
        {
            return await operacion(cancelacion).ConfigureAwait(false);
        }

        var transaccion = await contexto.Database
            .BeginTransactionAsync(cancelacion)
            .ConfigureAwait(false);

        await using (transaccion.ConfigureAwait(false))
        {
            var resultado = await operacion(cancelacion).ConfigureAwait(false);

            // Confirma siempre que la operación no haya lanzado, incluso si devolvió un resultado
            // fallido. Es deliberado: un fallo de negocio previsto suele necesitar persistir lo que
            // ya escribió, como el intento fallido que FR-006 obliga a registrar. Solo una excepción
            // revierte.
            await transaccion.CommitAsync(cancelacion).ConfigureAwait(false);

            return resultado;
        }
    }
}
