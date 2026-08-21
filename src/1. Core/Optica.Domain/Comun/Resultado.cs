namespace Optica.Domain.Comun;

/// <summary>
/// Resultado de una operación de negocio: correcto, o fallido con un <see cref="ErrorDeDominio"/>.
///
/// Existe porque el principio I prohíbe usar excepciones como control de flujo. Un fallo de
/// negocio previsto —credenciales incorrectas, cuenta bloqueada, rol inválido— no es una
/// condición excepcional: es una de las dos salidas normales de la operación, y el tipo de
/// retorno debe decirlo.
/// </summary>
public class Resultado
{
    /// <summary>Crea un resultado. Usar <see cref="Correcto"/> o <see cref="Fallo"/>.</summary>
    protected Resultado(bool esCorrecto, ErrorDeDominio error)
    {
        EsCorrecto = esCorrecto;
        Error = error;
    }

    /// <summary>Indica si la operación terminó bien.</summary>
    public bool EsCorrecto { get; }

    /// <summary>Indica si la operación falló.</summary>
    public bool EsFallo => !EsCorrecto;

    /// <summary>El fallo, o <see cref="ErrorDeDominio.Ninguno"/> si fue correcta.</summary>
    public ErrorDeDominio Error { get; }

    /// <summary>Resultado correcto sin valor asociado.</summary>
    public static Resultado Correcto() => new(true, ErrorDeDominio.Ninguno);

    /// <summary>Resultado fallido con el error indicado.</summary>
    public static Resultado Fallo(ErrorDeDominio error) => new(false, error);

    /// <summary>Resultado correcto que transporta un valor.</summary>
    public static Resultado<TValor> Correcto<TValor>(TValor valor) =>
        new(valor, true, ErrorDeDominio.Ninguno);

    /// <summary>Resultado fallido de una operación que habría devuelto un valor.</summary>
    public static Resultado<TValor> Fallo<TValor>(ErrorDeDominio error) =>
        new(default, false, error);
}

/// <summary>
/// Resultado de una operación que devuelve un valor cuando termina bien.
/// </summary>
/// <typeparam name="TValor">Tipo del valor devuelto.</typeparam>
public sealed class Resultado<TValor> : Resultado
{
    private readonly TValor? valor;

    internal Resultado(TValor? valor, bool esCorrecto, ErrorDeDominio error)
        : base(esCorrecto, error)
    {
        this.valor = valor;
    }

    /// <summary>
    /// El valor producido. Solo es válido cuando <see cref="Resultado.EsCorrecto"/> es cierto.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Si se lee el valor de un resultado fallido. Es un error de programación, no de negocio:
    /// significa que alguien omitió comprobar el resultado antes de usarlo.
    /// </exception>
    public TValor Valor => EsCorrecto
        ? valor!
        : throw new InvalidOperationException(
            $"No se puede leer el valor de un resultado fallido. Error: {Error.Codigo}");
}
