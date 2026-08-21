using Optica.Application.Abstracciones;

namespace Optica.Infrastructure.Persistencia;

/// <summary>
/// Usuario actual para los flujos que corren **sin sesión**: la herramienta de creación del primer
/// Administrador y cualquier proceso de línea de comandos.
///
/// Devuelve nulo siempre, y eso es exactamente lo correcto en esos flujos: no hay usuario del
/// sistema ejecutando, hay un operador con acceso a la máquina. El registro de auditoría lo refleja
/// dejando el responsable en nulo, que es lo que FR-035 y la tabla del modelo de datos prevén.
///
/// Se registra con <c>TryAdd</c>, de modo que la capa web sustituya esta implementación por la que
/// lee la sesión de la petición sin que aquí haya que saber nada de HTTP.
/// </summary>
public sealed class UsuarioActualDeOperador : IUsuarioActual
{
    /// <inheritdoc />
    public long? Id => null;
}
