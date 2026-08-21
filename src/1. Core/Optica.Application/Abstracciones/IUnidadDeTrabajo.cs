namespace Optica.Application.Abstracciones;

/// <summary>
/// Confirma los cambios pendientes y permite agrupar varias escrituras en una sola transacción.
///
/// Existe para que la capa de aplicación pueda exigir atomicidad sin conocer la tecnología de
/// persistencia, como manda el principio II. Es lo que hace posible la compuerta G7: el registro de
/// auditoría se escribe **dentro** de la transacción de la operación auditada, no después y con los
/// dedos cruzados.
/// </summary>
public interface IUnidadDeTrabajo
{
    /// <summary>
    /// Persiste los cambios pendientes.
    /// </summary>
    /// <param name="cancelacion">Token de cancelación, propagado hasta el proveedor de datos.</param>
    /// <returns>Número de filas afectadas.</returns>
    Task<int> GuardarCambios(CancellationToken cancelacion);

    /// <summary>
    /// Ejecuta la operación dentro de una transacción explícita: confirma si termina bien, revierte
    /// por completo si lanza.
    ///
    /// El "por completo" incluye la auditoría. Si la escritura del cambio falla, no puede quedar
    /// una fila de auditoría afirmando que ocurrió algo que no ocurrió.
    /// </summary>
    /// <typeparam name="TResultado">Tipo que devuelve la operación.</typeparam>
    /// <param name="operacion">Trabajo a ejecutar de forma atómica.</param>
    /// <param name="cancelacion">Token de cancelación.</param>
    /// <returns>Lo que devuelva la operación.</returns>
    Task<TResultado> EjecutarEnTransaccion<TResultado>(
        Func<CancellationToken, Task<TResultado>> operacion,
        CancellationToken cancelacion);
}
