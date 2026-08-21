namespace Optica.Domain.Auditoria;

/// <summary>
/// Los códigos de acción que esta feature audita. La lista es **cerrada y exhaustiva** por FR-038 y
/// la decisión D-14.
///
/// Que sea cerrada es lo que hace auditable la compuerta G7: se puede comprobar que el interceptor
/// escribe estos y solo estos, en lugar de deducir requisito por requisito qué debería auditarse.
///
/// **La renovación de sesión no está aquí a propósito.** Ocurre unas 30 veces por usuario y jornada
/// y cada fila diría lo mismo, que la sesión siguió viva. Queda en el registro estructurado de
/// FR-037 y en la marca de rotación de la propia credencial.
///
/// **El ingreso, exitoso o fallido, tampoco.** Su tabla natural es `LoginAttempts`, que el modelo
/// previó para eso. Auditar lo mismo en dos tablas produce dos fuentes de verdad que se
/// desincronizan.
/// </summary>
public static class AccionAuditada
{
    /// <summary>Bloqueo automático tras cinco intentos fallidos (FR-021).</summary>
    public const string BloqueoCuenta = "BLOQUEO_CUENTA";

    /// <summary>Asignación de un rol a un usuario (FR-028).</summary>
    public const string AsignarRol = "ASIGNAR_ROL";

    /// <summary>Retiro de un rol a un usuario (FR-028).</summary>
    public const string QuitarRol = "QUITAR_ROL";

    /// <summary>Cierre de sesión iniciado por el usuario (FR-038).</summary>
    public const string CierreSesion = "CIERRE_SESION";

    /// <summary>
    /// Revocación de todas las credenciales de un usuario por reutilización de una ya rotada
    /// (FR-013). Es el indicio más fuerte de compromiso que el sistema sabe detectar.
    /// </summary>
    public const string RevocacionCadena = "REVOCACION_CADENA";

    /// <summary>Creación del primer Administrador, sin usuario responsable (FR-035).</summary>
    public const string CrearPrimerAdmin = "CREAR_PRIMER_ADMIN";

    /// <summary>Todos los códigos admitidos, para validar y para probar que la lista es cerrada.</summary>
    public static IReadOnlyCollection<string> Todas { get; } =
    [
        BloqueoCuenta,
        AsignarRol,
        QuitarRol,
        CierreSesion,
        RevocacionCadena,
        CrearPrimerAdmin,
    ];
}
