namespace Optica.Domain.Comun;

/// <summary>
/// Nombre con el que una cuenta se identifica al ingresar.
///
/// Es un tipo de valor y no una cadena suelta porque tiene comportamiento asociado: una regla de
/// longitud que viene de la columna, y una comparación que **no distingue mayúsculas de
/// minúsculas**, alineada con la colación de la base de datos (regla de data-model.md, ítem CHK006
/// del checklist de modelo de datos). Dejarlo como `string` obliga a recordar esa comparación en
/// cada punto de uso, y basta olvidarla una vez para que `JPEREZ` y `jperez` sean cuentas distintas
/// en la aplicación y la misma en la base.
/// </summary>
public sealed class NombreUsuario : IEquatable<NombreUsuario>
{
    /// <summary>Longitud mínima admitida.</summary>
    public const int LongitudMinima = 1;

    /// <summary>Longitud máxima admitida, tomada de <c>NOMBRE_USUARIO nvarchar(100)</c>.</summary>
    public const int LongitudMaxima = 100;

    private NombreUsuario(string valor) => Valor = valor;

    /// <summary>El nombre tal como se escribió, sin alterar la caja.</summary>
    public string Valor { get; }

    /// <summary>
    /// Crea el nombre de usuario validando la regla de longitud.
    /// </summary>
    /// <param name="valor">Texto presentado. Se recortan los espacios de los extremos.</param>
    /// <returns>
    /// El nombre, o un fallo con el código <c>nombre-usuario-invalido</c> si está vacío o excede la
    /// longitud de la columna.
    /// </returns>
    public static Resultado<NombreUsuario> Crear(string? valor)
    {
        var recortado = valor?.Trim() ?? string.Empty;

        if (recortado.Length < LongitudMinima || recortado.Length > LongitudMaxima)
        {
            return Resultado.Fallo<NombreUsuario>(ErroresDeNombreUsuario.LongitudInvalida);
        }

        return Resultado.Correcto(new NombreUsuario(recortado));
    }

    /// <inheritdoc />
    public bool Equals(NombreUsuario? otro) =>
        otro is not null && string.Equals(Valor, otro.Valor, StringComparison.OrdinalIgnoreCase);

    /// <inheritdoc />
    public override bool Equals(object? obj) => Equals(obj as NombreUsuario);

    /// <inheritdoc />
    public override int GetHashCode() => StringComparer.OrdinalIgnoreCase.GetHashCode(Valor);

    /// <inheritdoc />
    public override string ToString() => Valor;
}

/// <summary>Fallos que puede producir la creación de un <see cref="NombreUsuario"/>.</summary>
public static class ErroresDeNombreUsuario
{
    /// <summary>
    /// El nombre está vacío o excede la longitud de la columna.
    ///
    /// El mensaje habla de formato, no de credenciales: este fallo produce un 400 y es
    /// deliberadamente distinguible del 401 genérico de FR-004. Revelar que un nombre de 101
    /// caracteres es inválido no dice nada sobre qué cuentas existen.
    /// </summary>
    public static readonly ErrorDeDominio LongitudInvalida = new(
        "nombre-usuario-invalido",
        $"El nombre de usuario debe tener entre {NombreUsuario.LongitudMinima} y {NombreUsuario.LongitudMaxima} caracteres.");
}
