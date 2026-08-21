using System.Diagnostics.CodeAnalysis;

namespace Optica.Domain.Comun;

/// <summary>
/// Error de programación detectado en el dominio: una invariante que el código debería haber
/// garantizado antes de llegar aquí.
///
/// No se usa para fallos de negocio previstos —credenciales incorrectas, cuenta bloqueada, rol
/// inválido—; esos son una de las dos salidas normales de la operación y viajan en
/// <see cref="Resultado"/>, como exige el principio I. Lanzar esta excepción significa que hay un
/// defecto que corregir, no una situación que manejar.
/// </summary>
[SuppressMessage(
    "Naming",
    "CA1710:Identifiers should have correct suffix",
    Justification = "Todo el dominio está nombrado en español, y el principio I exige nombres que " +
                    "revelen intención. 'ExcepcionDeDominio' lo hace; 'DominioException' mezcla dos " +
                    "idiomas en un identificador y 'ExcepcionDeDominioException' es redundante. La " +
                    "regla busca el sufijo inglés y aquí choca con la convención del proyecto, igual " +
                    "que CA1707 choca con el nombrado de las pruebas.")]
public sealed class ExcepcionDeDominio : Exception
{
    /// <summary>Crea la excepción con un mensaje descriptivo del defecto.</summary>
    public ExcepcionDeDominio(string mensaje)
        : base(mensaje)
    {
    }

    /// <summary>Crea la excepción encadenando la causa original.</summary>
    public ExcepcionDeDominio(string mensaje, Exception causa)
        : base(mensaje, causa)
    {
    }

    /// <summary>Constructor sin mensaje. Existe por convención; preferir los otros dos.</summary>
    public ExcepcionDeDominio()
    {
    }

    /// <summary>
    /// Lanza la excepción si la condición no se cumple. Atajo para afirmar invariantes sin
    /// ensuciar el flujo con bloques condicionales de una línea.
    /// </summary>
    /// <param name="condicion">Lo que debe ser cierto.</param>
    /// <param name="mensaje">Qué defecto revela que no lo sea.</param>
    /// <exception cref="ExcepcionDeDominio">Si <paramref name="condicion"/> es falsa.</exception>
    public static void Exigir(bool condicion, string mensaje)
    {
        if (!condicion)
        {
            throw new ExcepcionDeDominio(mensaje);
        }
    }
}
