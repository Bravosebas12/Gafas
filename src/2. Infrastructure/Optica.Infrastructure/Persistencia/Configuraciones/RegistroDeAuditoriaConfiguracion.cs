using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Optica.Domain.Auditoria;
using Optica.Infrastructure.Persistencia.Convertidores;

namespace Optica.Infrastructure.Persistencia.Configuraciones;

/// <summary>
/// Mapea <see cref="RegistroDeAuditoria"/> a <c>AUDITORIA.LogUsuarios</c>, tal como la tabla existe
/// hoy en el esquema congelado. Cada nombre de columna se declara de forma explícita en lugar de
/// confiar en una convención: la convención de Entity Framework produce `TablaOrigen`, y la columna
/// se llama `TABLA_ORIGEN`.
/// </summary>
public sealed class RegistroDeAuditoriaConfiguracion : IEntityTypeConfiguration<RegistroDeAuditoria>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<RegistroDeAuditoria> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("LogUsuarios", schema: "AUDITORIA");

        builder.HasKey(registro => registro.Id);
        builder.Property(registro => registro.Id).HasColumnName("ID");

        builder.Property(registro => registro.TablaOrigen)
            .HasColumnName("TABLA_ORIGEN")
            .HasMaxLength(128)
            .IsRequired();

        // La columna del esquema se llama `RegistroID`, no `REGISTRO_ID`: es la única de la tabla que
        // rompe la convención de mayúsculas con guion bajo. Se respeta tal cual está.
        builder.Property(registro => registro.RegistroId).HasColumnName("RegistroID").IsRequired();

        builder.Property(registro => registro.Accion)
            .HasColumnName("ACCION")
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(registro => registro.DatosAntes).HasColumnName("DATOS_ANTES");
        builder.Property(registro => registro.DatosDespues).HasColumnName("DATOS_DESPUES");
        builder.Property(registro => registro.UsuarioAccion).HasColumnName("USUARIO_ACCION");

        builder.Property(registro => registro.FechaAccion)
            .HasColumnName("FECHA_ACCION")
            .HasConversion(new ConvertidorDeMarcaUtc())
            .IsRequired();
    }
}
