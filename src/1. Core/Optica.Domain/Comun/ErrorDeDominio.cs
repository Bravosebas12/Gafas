namespace Optica.Domain.Comun;

/// <summary>
/// Un fallo de negocio, identificado por un código estable y acompañado de un mensaje legible.
///
/// El código es lo que la capa de presentación traduce a HTTP y lo que las pruebas afirman; el
/// mensaje es para quien lo lee. Se separan a propósito: afirmar sobre el texto de un mensaje
/// convierte cualquier mejora de redacción en una prueba roja.
/// </summary>
/// <param name="Codigo">Identificador estable del fallo, en minúsculas y separado por guiones.</param>
/// <param name="Mensaje">Descripción legible del fallo.</param>
public sealed record ErrorDeDominio(string Codigo, string Mensaje)
{
    /// <summary>Ausencia de error. Se usa como valor neutro en un resultado correcto.</summary>
    public static readonly ErrorDeDominio Ninguno = new(string.Empty, string.Empty);
}
