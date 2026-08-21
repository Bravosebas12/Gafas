using Microsoft.Extensions.Configuration;

namespace Optica.Infrastructure.Configuracion;

/// <summary>
/// Falla el arranque si la configuración declara cualquier proveedor de identidad externo.
///
/// FR-002 lo prohíbe y la compuerta G5 lo verifica. La comprobación se hace en el arranque y no en
/// el ingreso porque la única respuesta correcta a esa configuración es no levantar la aplicación:
/// permitir que arranque y rechazar el proveedor en tiempo de ejecución dejaría un sistema que
/// contradice su configuración, y alguien acabaría "arreglando" la contradicción por el lado
/// equivocado.
/// </summary>
public static class ValidadorDeProveedoresExternos
{
    /// <summary>
    /// Secciones donde un proveedor externo tendría sentido y por tanto se busca en profundidad.
    /// </summary>
    private static readonly string[] SeccionesDeAutenticacion =
    [
        "Authentication",
        "Autenticacion",
        "Identity",
        "IdentityServer",
        "ExternalProviders",
        "ProveedoresExternos",
        "OpenIdConnect",
        "OAuth",
    ];

    /// <summary>
    /// Nombres de proveedor que se rechazan dentro de una sección de autenticación.
    /// </summary>
    private static readonly string[] ProveedoresProhibidos =
    [
        "Google", "Microsoft", "MicrosoftAccount", "Facebook", "Apple", "Twitter", "GitHub",
        "Okta", "Auth0", "AzureAd", "AzureAD", "EntraId", "Cognito", "Firebase", "Keycloak",
        "LinkedIn", "Saml", "OpenIdConnect", "OAuth", "Ldap", "ActiveDirectory",
    ];

    /// <summary>
    /// Nombres que se rechazan incluso como sección de primer nivel.
    ///
    /// Es una lista más corta a propósito. `Microsoft` y `Apple` no están: `Microsoft` aparece de
    /// forma legítima en las claves de registro, como <c>Logging:LogLevel:Microsoft</c>, y rechazarlo
    /// en cualquier posición convertiría el validador en un estorbo que alguien desactivaría.
    /// </summary>
    private static readonly string[] ProveedoresProhibidosEnRaiz =
    [
        "Google", "Facebook", "Okta", "Auth0", "AzureAd", "AzureAD", "EntraId", "Cognito",
        "Keycloak", "IdentityServer",
    ];

    /// <summary>
    /// Valida la configuración completa.
    /// </summary>
    /// <param name="configuracion">Configuración de la aplicación.</param>
    /// <exception cref="ConfiguracionProhibidaException">
    /// Si se declara un proveedor de identidad externo. El mensaje nombra la clave exacta, para que
    /// quien lo vea sepa qué quitar en lugar de tener que buscarlo.
    /// </exception>
    public static void Validar(IConfiguration configuracion)
    {
        ArgumentNullException.ThrowIfNull(configuracion);

        foreach (var seccion in configuracion.GetChildren())
        {
            if (ProveedoresProhibidosEnRaiz.Contains(seccion.Key, StringComparer.OrdinalIgnoreCase))
            {
                throw new ConfiguracionProhibidaException(seccion.Path);
            }

            if (SeccionesDeAutenticacion.Contains(seccion.Key, StringComparer.OrdinalIgnoreCase))
            {
                ValidarEnProfundidad(seccion);
            }
        }
    }

    private static void ValidarEnProfundidad(IConfigurationSection seccion)
    {
        foreach (var hija in seccion.GetChildren())
        {
            if (ProveedoresProhibidos.Contains(hija.Key, StringComparer.OrdinalIgnoreCase))
            {
                throw new ConfiguracionProhibidaException(hija.Path);
            }

            ValidarEnProfundidad(hija);
        }
    }
}
