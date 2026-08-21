namespace Optica.Domain.Comun;

/// <summary>
/// Entidad cuya tabla lleva las cuatro columnas de auditoría que el principio VII exige en **todas**
/// las tablas: quién creó, cuándo, quién actualizó por última vez y cuándo.
///
/// Las propiedades tienen escritura interna, no pública. Es deliberado: quien las puebla es el
/// interceptor de persistencia, no el código de negocio. Si un handler pudiera escribir la fecha de
/// actualización, tarde o temprano alguien pondría ahí un valor calculado a mano y el sistema
/// tendría dos fuentes de tiempo, que es justo lo que la decisión D-07 evita.
/// </summary>
public interface IEntidadAuditable
{
    /// <summary>Quién creó la fila. Nulo para filas creadas sin usuario en sesión.</summary>
    long? UsuarioCreacion { get; }

    /// <summary>Cuándo se creó, en UTC.</summary>
    DateTimeOffset FechaCreacion { get; }

    /// <summary>Quién la actualizó por última vez.</summary>
    long? UsuarioActualizacion { get; }

    /// <summary>Cuándo se actualizó por última vez, en UTC.</summary>
    DateTimeOffset FechaActualizacion { get; }

    /// <summary>
    /// Marca la fila como creada. Lo invoca el interceptor de persistencia, nunca un caso de uso.
    /// </summary>
    /// <param name="usuario">Responsable, o nulo si no hay sesión.</param>
    /// <param name="momento">Instante en UTC, desde la única fuente de tiempo.</param>
    void MarcarCreacion(long? usuario, DateTimeOffset momento);

    /// <summary>
    /// Marca la fila como actualizada. Lo invoca el interceptor de persistencia.
    /// </summary>
    /// <param name="usuario">Responsable, o nulo si no hay sesión.</param>
    /// <param name="momento">Instante en UTC.</param>
    void MarcarActualizacion(long? usuario, DateTimeOffset momento);
}
