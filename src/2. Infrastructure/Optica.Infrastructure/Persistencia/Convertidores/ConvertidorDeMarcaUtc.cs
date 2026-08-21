using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Optica.Infrastructure.Persistencia.Convertidores;

/// <summary>
/// Convierte entre <see cref="DateTimeOffset"/> del dominio y <c>datetime2</c> de la base.
///
/// Hace falta porque el esquema usa <c>datetime2(7)</c>, que **no guarda desplazamiento**, mientras
/// el dominio usa <see cref="DateTimeOffset"/> para que el tipo obligue a pensar en la zona. Sin
/// este convertidor, Entity Framework mapearía a <c>datetimeoffset</c> y el modelo dejaría de
/// coincidir con el esquema congelado, lo que viola el principio X.
///
/// La conversión asume UTC en los dos sentidos, que es lo que el principio X exige y lo que los
/// valores por omisión del esquema producen con <c>SYSUTCDATETIME()</c>. Si algún día entra un valor
/// con otro desplazamiento, se normaliza a UTC al escribir en lugar de perderlo en silencio.
/// </summary>
public sealed class ConvertidorDeMarcaUtc : ValueConverter<DateTimeOffset, DateTime>
{
    /// <summary>Crea el convertidor.</summary>
    public ConvertidorDeMarcaUtc()
        : base(
            marca => marca.UtcDateTime,
            fecha => new DateTimeOffset(DateTime.SpecifyKind(fecha, DateTimeKind.Utc)))
    {
    }
}

/// <summary>
/// Igual que <see cref="ConvertidorDeMarcaUtc"/> para columnas que admiten nulo.
/// </summary>
public sealed class ConvertidorDeMarcaUtcOpcional : ValueConverter<DateTimeOffset?, DateTime?>
{
    /// <summary>Crea el convertidor.</summary>
    public ConvertidorDeMarcaUtcOpcional()
        : base(
            marca => marca.HasValue ? marca.Value.UtcDateTime : null,
            fecha => fecha.HasValue
                ? new DateTimeOffset(DateTime.SpecifyKind(fecha.Value, DateTimeKind.Utc))
                : null)
    {
    }
}
