using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Optica.Application.Abstracciones;
using Optica.Domain.Auditoria;

namespace Optica.Infrastructure.Persistencia.Interceptores;

/// <summary>
/// Materializa los registros de auditoría anotados por los casos de uso, justo antes de guardar.
///
/// Escribir aquí y no después es lo que satisface la compuerta G7: las filas de auditoría entran en
/// el mismo <c>SaveChanges</c> y por tanto en la misma transacción que el cambio auditado. Si la
/// operación se revierte, la auditoría se revierte con ella y no queda una fila afirmando algo que
/// nunca ocurrió.
/// </summary>
/// <param name="anotaciones">Anotaciones acumuladas durante la petición en curso.</param>
/// <param name="usuarioActual">Quién ejecuta, para poblar el responsable.</param>
/// <param name="reloj">Única fuente de tiempo (D-07).</param>
public sealed class AuditoriaInterceptor(
    AnotacionesDeAuditoria anotaciones,
    IUsuarioActual usuarioActual,
    TimeProvider reloj)
    : SaveChangesInterceptor
{
    /// <summary>
    /// Columnas que **nunca** aparecen en el JSON de auditoría, por la regla R-A3 y el principio
    /// VIII. Se excluyen por nombre de columna y de forma explícita: confiar en que nadie las incluya
    /// por descuido es exactamente lo que la regla prohíbe.
    /// </summary>
    private static readonly HashSet<string> ColumnasProhibidas = new(StringComparer.OrdinalIgnoreCase)
    {
        "PASSWORD_HASH",
        "PASSWORD_SALT",
        "TOKEN_HASH",
    };

    /// <inheritdoc />
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(eventData);

        if (eventData.Context is not null)
        {
            MaterializarRegistros(eventData.Context);
        }

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    /// <inheritdoc />
    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        ArgumentNullException.ThrowIfNull(eventData);

        if (eventData.Context is not null)
        {
            MaterializarRegistros(eventData.Context);
        }

        return base.SavingChanges(eventData, result);
    }

    private void MaterializarRegistros(DbContext contexto)
    {
        if (anotaciones.Pendientes.Count == 0)
        {
            return;
        }

        var ahora = reloj.GetUtcNow();

        // Se copia la lista antes de recorrerla: añadir entidades al contexto altera el rastreador
        // de cambios, y recorrer una colección que muta en el camino es un defecto que aparece solo
        // cuando hay más de una anotación.
        foreach (var anotacion in anotaciones.Pendientes.ToArray())
        {
            var entrada = contexto.Entry(anotacion.Entidad);

            contexto.Add(RegistroDeAuditoria.Crear(
                tablaOrigen: entrada.Metadata.GetTableName() ?? entrada.Metadata.Name,
                registroId: LeerIdentificador(entrada),
                accion: anotacion.Accion,
                datosAntes: SerializarValoresPrevios(entrada),
                datosDespues: SerializarValoresActuales(entrada),
                usuarioAccion: anotacion.EsAccionDeOperador ? null : usuarioActual.Id,
                fechaAccion: ahora));
        }

        anotaciones.Limpiar();
    }

    private static long LeerIdentificador(EntityEntry entrada)
    {
        var propiedades = entrada.Metadata.FindPrimaryKey()?.Properties;
        var clave = propiedades is { Count: > 0 } ? propiedades[0] : null;

        if (clave is null)
        {
            return 0;
        }

        var valor = entrada.Property(clave.Name).CurrentValue;

        return valor is null ? 0 : Convert.ToInt64(valor, System.Globalization.CultureInfo.InvariantCulture);
    }

    private static string? SerializarValoresPrevios(EntityEntry entrada)
    {
        if (entrada.State == EntityState.Added)
        {
            return null;
        }

        return Serializar(entrada, propiedad => propiedad.OriginalValue);
    }

    private static string? SerializarValoresActuales(EntityEntry entrada)
    {
        if (entrada.State == EntityState.Deleted)
        {
            return null;
        }

        return Serializar(entrada, propiedad => propiedad.CurrentValue);
    }

    private static string Serializar(
        EntityEntry entrada,
        Func<PropertyEntry, object?> leerValor)
    {
        var valores = new Dictionary<string, object?>(StringComparer.Ordinal);

        foreach (var propiedad in entrada.Properties)
        {
            var columna = propiedad.Metadata.GetColumnName();

            if (ColumnasProhibidas.Contains(columna))
            {
                continue;
            }

            valores[columna] = leerValor(propiedad);
        }

        return JsonSerializer.Serialize(valores);
    }
}
