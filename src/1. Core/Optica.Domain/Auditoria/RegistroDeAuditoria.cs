namespace Optica.Domain.Auditoria;

/// <summary>
/// Traza inalterable de un cambio sensible, mapeada a <c>AUDITORIA.LogUsuarios</c>.
///
/// Es de **solo inserción** por la regla R-A2 y el principio VII: no tiene ningún método que
/// modifique su estado después de crearse, y esa ausencia es la garantía. Una entidad con
/// propiedades de escritura pública se acaba modificando, por más que un comentario lo prohíba.
/// </summary>
public sealed class RegistroDeAuditoria
{
    private RegistroDeAuditoria(
        string tablaOrigen,
        long registroId,
        string accion,
        string? datosAntes,
        string? datosDespues,
        long? usuarioAccion,
        DateTimeOffset fechaAccion)
    {
        TablaOrigen = tablaOrigen;
        RegistroId = registroId;
        Accion = accion;
        DatosAntes = datosAntes;
        DatosDespues = datosDespues;
        UsuarioAccion = usuarioAccion;
        FechaAccion = fechaAccion;
    }

    /// <summary>Requerido por el proveedor de datos para materializar la entidad.</summary>
    private RegistroDeAuditoria()
    {
        TablaOrigen = string.Empty;
        Accion = string.Empty;
    }

    /// <summary>Identificador de la fila de auditoría.</summary>
    public long Id { get; private set; }

    /// <summary>Tabla donde ocurrió el cambio.</summary>
    public string TablaOrigen { get; private set; }

    /// <summary>Identificador de la fila afectada en esa tabla.</summary>
    public long RegistroId { get; private set; }

    /// <summary>Código de la acción auditada. Uno de <see cref="AccionAuditada"/>.</summary>
    public string Accion { get; private set; }

    /// <summary>Estado previo en JSON, o nulo si la fila no existía.</summary>
    public string? DatosAntes { get; private set; }

    /// <summary>Estado posterior en JSON, o nulo si la fila se eliminó.</summary>
    public string? DatosDespues { get; private set; }

    /// <summary>
    /// Quién ejecutó la acción, o **nulo** cuando no hay usuario responsable, como en la creación
    /// del primer Administrador, que la ejecuta un operador en la máquina.
    /// </summary>
    public long? UsuarioAccion { get; private set; }

    /// <summary>Momento de la acción, en UTC y desde la única fuente de tiempo (D-07).</summary>
    public DateTimeOffset FechaAccion { get; private set; }

    /// <summary>
    /// Crea el registro de auditoría.
    /// </summary>
    /// <param name="tablaOrigen">Tabla donde ocurrió el cambio.</param>
    /// <param name="registroId">Identificador de la fila afectada.</param>
    /// <param name="accion">Código de la acción, de <see cref="AccionAuditada"/>.</param>
    /// <param name="datosAntes">Estado previo en JSON, sin columnas secretas (regla R-A3).</param>
    /// <param name="datosDespues">Estado posterior en JSON, sin columnas secretas.</param>
    /// <param name="usuarioAccion">Responsable, o nulo si es una acción de operador.</param>
    /// <param name="fechaAccion">Momento en UTC.</param>
    /// <returns>El registro listo para insertar.</returns>
    public static RegistroDeAuditoria Crear(
        string tablaOrigen,
        long registroId,
        string accion,
        string? datosAntes,
        string? datosDespues,
        long? usuarioAccion,
        DateTimeOffset fechaAccion) =>
        new(tablaOrigen, registroId, accion, datosAntes, datosDespues, usuarioAccion, fechaAccion);
}
