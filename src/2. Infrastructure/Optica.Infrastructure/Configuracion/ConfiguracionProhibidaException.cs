namespace Optica.Infrastructure.Configuracion;

/// <summary>
/// La configuración declara algo que la especificación prohíbe, y por tanto la aplicación no debe
/// arrancar.
///
/// A diferencia de <c>ExcepcionDeDominio</c>, esta sí termina en <c>Exception</c>: no es un tipo del
/// dominio en español, es un fallo de arranque que se lee en una consola junto a las excepciones del
/// marco, y ahí la convención del ecosistema pesa más que la del dominio.
/// </summary>
public sealed class ConfiguracionProhibidaException : Exception
{
    /// <summary>
    /// Crea la excepción nombrando la clave de configuración rechazada.
    /// </summary>
    /// <param name="claveRechazada">Ruta completa de la clave, para que se pueda localizar y quitar.</param>
    public ConfiguracionProhibidaException(string claveRechazada)
        : base($"La clave de configuración '{claveRechazada}' declara un proveedor de identidad " +
               "externo. FR-002 lo prohíbe: la autenticación se valida exclusivamente contra la base " +
               "de datos propia. Quite esa clave para que la aplicación pueda arrancar.")
        => ClaveRechazada = claveRechazada;

    /// <summary>Crea la excepción con un mensaje libre.</summary>
    public ConfiguracionProhibidaException()
        : base("La configuración declara un proveedor de identidad externo, prohibido por FR-002.")
        => ClaveRechazada = string.Empty;

    /// <summary>Crea la excepción encadenando la causa.</summary>
    public ConfiguracionProhibidaException(string mensaje, Exception causa)
        : base(mensaje, causa)
        => ClaveRechazada = string.Empty;

    /// <summary>Clave de configuración que provocó el rechazo.</summary>
    public string ClaveRechazada { get; }
}
