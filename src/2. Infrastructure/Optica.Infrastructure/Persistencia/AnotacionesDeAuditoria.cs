using Optica.Application.Abstracciones;
using Optica.Domain.Auditoria;
using Optica.Domain.Comun;

namespace Optica.Infrastructure.Persistencia;

/// <summary>
/// Una anotación de auditoría pendiente de materializar.
/// </summary>
/// <param name="Accion">Código de <see cref="AccionAuditada"/>.</param>
/// <param name="Entidad">Instancia afectada, con su cambio pendiente de guardar.</param>
/// <param name="EsAccionDeOperador">
/// Cierto cuando no hay usuario responsable, como en la creación del primer Administrador.
/// </param>
public sealed record AnotacionDeAuditoria(string Accion, object Entidad, bool EsAccionDeOperador);

/// <summary>
/// Acumula las anotaciones de auditoría de la petición en curso, hasta que el interceptor las
/// materializa en el guardado.
///
/// Vive por petición y es la vía por la que el caso de uso, que conoce el **significado** del
/// cambio, se comunica con el interceptor, que conoce los **valores** antes y después. Ninguno de
/// los dos podría producir el registro completo por su cuenta.
/// </summary>
public sealed class AnotacionesDeAuditoria : IAuditor
{
    private readonly List<AnotacionDeAuditoria> pendientes = [];

    /// <summary>Anotaciones aún no materializadas.</summary>
    public IReadOnlyList<AnotacionDeAuditoria> Pendientes => pendientes;

    /// <inheritdoc />
    public void Anotar(string accion, object entidad) =>
        Agregar(accion, entidad, esAccionDeOperador: false);

    /// <inheritdoc />
    public void AnotarComoAccionDeOperador(string accion, object entidad) =>
        Agregar(accion, entidad, esAccionDeOperador: true);

    /// <summary>Descarta las anotaciones ya materializadas.</summary>
    public void Limpiar() => pendientes.Clear();

    private void Agregar(string accion, object entidad, bool esAccionDeOperador)
    {
        ArgumentNullException.ThrowIfNull(entidad);

        // La lista de FR-038 es cerrada, así que un código fuera de ella es un defecto de
        // programación y no un dato de entrada: falla ruidosamente en lugar de escribir una fila de
        // auditoría con una acción que nadie podrá interpretar después.
        ExcepcionDeDominio.Exigir(
            AccionAuditada.Todas.Contains(accion),
            $"La acción de auditoría '{accion}' no pertenece a la lista cerrada de FR-038.");

        pendientes.Add(new AnotacionDeAuditoria(accion, entidad, esAccionDeOperador));
    }
}
