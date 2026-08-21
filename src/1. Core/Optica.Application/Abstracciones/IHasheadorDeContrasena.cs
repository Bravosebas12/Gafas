namespace Optica.Application.Abstracciones;

/// <summary>
/// Deriva y verifica hashes de contraseña.
///
/// Se declara en la capa de aplicación e se implementa en infraestructura, según exige el
/// principio II: cuando una capa interna necesita una capacidad de una externa, la interfaz
/// vive en la interna.
/// </summary>
public interface IHasheadorDeContrasena
{
    /// <summary>
    /// Deriva el hash de una contraseña con un salt nuevo y aleatorio.
    /// </summary>
    /// <param name="contrasena">Contraseña en claro. No se registra ni se persiste.</param>
    /// <returns>El hash y el salt, para persistirlos en columnas separadas.</returns>
    (byte[] Hash, byte[] Salt) Derivar(string contrasena);

    /// <summary>
    /// Comprueba si una contraseña corresponde a un hash y salt almacenados.
    /// </summary>
    /// <param name="contrasena">Contraseña presentada por quien intenta autenticarse.</param>
    /// <param name="hashAlmacenado">Hash persistido del usuario.</param>
    /// <param name="salAlmacenada">Salt persistido del usuario.</param>
    /// <returns>Cierto si coinciden.</returns>
    bool Coincide(string contrasena, byte[] hashAlmacenado, byte[] salAlmacenada);

    /// <summary>
    /// Ejecuta una derivación señuelo y descarta el resultado.
    ///
    /// Se invoca cuando el nombre de usuario presentado no existe, para que el tiempo de
    /// respuesta no delate la ausencia de la cuenta. Sin esto, la diferencia de tiempo es el
    /// costo completo de la derivación —cientos de milisegundos— y permite enumerar cuentas.
    /// Requisito de SC-004; decisión D-02 en research.md.
    /// </summary>
    void DerivarSenuelo();
}
