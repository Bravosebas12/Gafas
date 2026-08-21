namespace Optica.Application.Abstracciones;

/// <summary>
/// Declara que un cambio debe auditarse. El registro lo materializa un interceptor de persistencia
/// durante el guardado, de modo que queda en la **misma transacción** que la operación auditada
/// (regla R-A1, compuerta G7).
///
/// **Por qué la acción la declara quien la ejecuta y no se deduce del cambio.** Dos de los eventos
/// de FR-038 son indistinguibles mirando solo las filas modificadas: cerrar sesión y revocar la
/// cadena por reutilización de credencial producen exactamente lo mismo en la base, una marca de
/// revocación sobre `RefreshTokens`. Lo que los separa es la intención, y esa solo la conoce el caso
/// de uso. El interceptor aporta los valores antes y después; el handler aporta el significado.
/// </summary>
public interface IAuditor
{
    /// <summary>
    /// Anota que el cambio pendiente sobre la entidad indicada debe auditarse.
    /// </summary>
    /// <param name="accion">Código de <c>AccionAuditada</c>. La lista es cerrada por FR-038.</param>
    /// <param name="entidad">
    /// Instancia afectada, ya modificada y con el cambio pendiente de guardar. El interceptor lee de
    /// ella el estado previo y el posterior.
    /// </param>
    void Anotar(string accion, object entidad);

    /// <summary>
    /// Anota un cambio ejecutado **sin usuario responsable**, como la creación del primer
    /// Administrador, que ejecuta un operador con acceso a la máquina y no un usuario del sistema.
    /// </summary>
    /// <param name="accion">Código de <c>AccionAuditada</c>.</param>
    /// <param name="entidad">Instancia afectada.</param>
    void AnotarComoAccionDeOperador(string accion, object entidad);
}
