using System.Text;
using Microsoft.Extensions.Options;

namespace Optica.Infrastructure.Configuracion;

/// <summary>
/// Valida las opciones del token en el arranque y falla de forma explícita si no sirven.
///
/// Se valida al arrancar y no al emitir el primer token. La diferencia: con validación diferida, un
/// despliegue sin la clave de firma configurada arranca sin ruido y falla en el primer ingreso de un
/// empleado, con un error que no dice que falta una variable de entorno. Decisión D-08.
/// </summary>
public sealed class ValidadorDeOpcionesDeToken : IValidateOptions<OpcionesDeToken>
{
    /// <inheritdoc />
    public ValidateOptionsResult Validate(string? name, OpcionesDeToken options)
    {
        ArgumentNullException.ThrowIfNull(options);

        var fallos = new List<string>();

        if (string.IsNullOrWhiteSpace(options.ClaveDeFirma))
        {
            fallos.Add(
                $"Falta la clave de firma del token. Se configura en " +
                $"'{OpcionesDeToken.Seccion}:{nameof(OpcionesDeToken.ClaveDeFirma)}', desde el gestor " +
                "de secretos en desarrollo o una variable de entorno en despliegue. Nunca en un " +
                "archivo versionado.");
        }
        else if (Encoding.UTF8.GetByteCount(options.ClaveDeFirma) < OpcionesDeToken.BytesMinimosDeClave)
        {
            fallos.Add(
                $"La clave de firma mide menos de {OpcionesDeToken.BytesMinimosDeClave} bytes. " +
                "HS256 usa HMAC-SHA256, cuyo bloque es de 32 bytes: una clave más corta debilita la " +
                "firma sin que nada lo advierta.");
        }

        if (options.VigenciaDelTokenDeAcceso <= TimeSpan.Zero)
        {
            fallos.Add("La vigencia del token de acceso debe ser positiva. FR-008 la fija en 15 minutos.");
        }

        if (options.VigenciaDeLaSesion <= options.VigenciaDelTokenDeAcceso)
        {
            fallos.Add(
                "La vigencia de la sesión debe superar la del token de acceso; si no, la renovación " +
                "de FR-009a no tiene ninguna ventana en la que ocurrir.");
        }

        return fallos.Count > 0
            ? ValidateOptionsResult.Fail(fallos)
            : ValidateOptionsResult.Success;
    }
}
