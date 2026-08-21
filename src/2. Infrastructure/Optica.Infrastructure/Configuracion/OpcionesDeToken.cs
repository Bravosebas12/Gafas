namespace Optica.Infrastructure.Configuracion;

/// <summary>
/// Parámetros de emisión y validación del token de acceso.
/// </summary>
public sealed class OpcionesDeToken
{
    /// <summary>Sección de configuración de donde se leen.</summary>
    public const string Seccion = "Autenticacion";

    /// <summary>
    /// Longitud mínima de la clave de firma, en bytes.
    ///
    /// HS256 usa HMAC-SHA256, cuyo bloque es de 32 bytes: una clave más corta no aporta más
    /// entropía que su propia longitud y debilita la firma sin que nada lo advierta. Decisión D-08.
    /// </summary>
    public const int BytesMinimosDeClave = 32;

    /// <summary>
    /// Clave simétrica de firma. **Nunca** se versiona: llega del gestor de secretos en desarrollo y
    /// de variable de entorno en despliegue (decisión D-08, principio VI).
    /// </summary>
    public string ClaveDeFirma { get; set; } = string.Empty;

    /// <summary>Emisor que se estampa en el token y se exige al validarlo.</summary>
    public string Emisor { get; set; } = "optica";

    /// <summary>Audiencia que se estampa en el token y se exige al validarlo.</summary>
    public string Audiencia { get; set; } = "optica";

    /// <summary>Vigencia del token de acceso. Quince minutos por FR-008.</summary>
    public TimeSpan VigenciaDelTokenDeAcceso { get; set; } = TimeSpan.FromMinutes(15);

    /// <summary>Vigencia absoluta de la sesión. Ocho horas por FR-009 y FR-009a.</summary>
    public TimeSpan VigenciaDeLaSesion { get; set; } = TimeSpan.FromHours(8);
}
