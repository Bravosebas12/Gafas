namespace Optica.Application.Abstracciones;

/// <summary>
/// Token de acceso emitido, con el instante en que deja de ser válido.
/// </summary>
/// <param name="Valor">El token firmado. Viaja en cookie <c>HttpOnly</c> (decisión D-04).</param>
/// <param name="ExpiraEn">Vencimiento en UTC, 15 minutos después de la emisión (FR-008).</param>
public sealed record TokenDeAcceso(string Valor, DateTimeOffset ExpiraEn);

/// <summary>
/// Emite el token de acceso de una sesión.
///
/// **Recibe identificador y roles, no la entidad de usuario.** Es deliberado: FR-015 limita el
/// contenido del token a identificador, roles y metadatos de vigencia, así que pasarle la entidad
/// completa le daría acceso a datos que tiene prohibido incluir. La firma del método es la que
/// impide el error, no un comentario pidiendo que no lo haga.
/// </summary>
public interface IEmisorDeToken
{
    /// <summary>
    /// Emite un token de acceso para el usuario indicado.
    /// </summary>
    /// <param name="usuarioId">Identificador de la cuenta.</param>
    /// <param name="codigosDeRol">
    /// Códigos de rol, no nombres visibles. El código es <c>Optometra</c> sin tilde; el nombre
    /// <c>Optómetra</c> con tilde no sirve para autorizar.
    /// </param>
    /// <param name="ahora">Instante de emisión, tomado de la única fuente de tiempo (D-07).</param>
    /// <returns>El token y su vencimiento.</returns>
    TokenDeAcceso Emitir(
        long usuarioId,
        IReadOnlyCollection<string> codigosDeRol,
        DateTimeOffset ahora);
}
