namespace Optica.Application.Abstracciones;

/// <summary>
/// Quién está ejecutando la petición en curso.
///
/// Devuelve el identificador y nada más. No expone nombre, roles ni la entidad: la capa de
/// aplicación necesita saber **quién** para poblar las columnas de auditoría, y darle más de eso
/// invita a tomar decisiones de autorización aquí, que es justo lo que el principio VI pone en el
/// servidor y en las políticas, no en el código de negocio.
/// </summary>
public interface IUsuarioActual
{
    /// <summary>
    /// Identificador del usuario autenticado, o **nulo** cuando no hay sesión: el ingreso, la
    /// renovación y la creación del primer Administrador ocurren sin usuario establecido.
    /// </summary>
    long? Id { get; }
}
