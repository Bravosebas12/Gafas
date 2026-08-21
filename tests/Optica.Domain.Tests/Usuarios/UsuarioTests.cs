using Optica.Domain.Comun;
using Optica.Domain.Usuarios;

namespace Optica.Domain.Tests.Usuarios;

/// <summary>
/// Reglas R-U1 a R-U5 de la entidad Usuario (data-model.md).
///
/// Tarea T027. Estas pruebas se escriben ANTES de la entidad, según exige el principio IV:
/// definen la superficie que T033 debe implementar. Hasta que exista
/// <c>Optica.Domain.Usuarios.Usuario</c> este archivo no compila, y eso es el estado rojo
/// esperado, no un defecto.
///
/// Superficie que estas pruebas requieren de la entidad:
///   Usuario.Crear(empleadoId, nombreUsuario, hash, sal) -> Resultado&lt;Usuario&gt;
///   Resultado VerificarQuePuedeAutenticarse(DateTimeOffset ahora)
///   void RegistrarIngresoExitoso()
///   void Desactivar()
///   void Bloquear(DateTimeOffset hasta)
///   Propiedades: Activo, BloqueadoHasta, IntentosFallidos, NombreUsuario, EmpleadoId
///
/// Lo que estas pruebas NO cubren, deliberadamente: el incremento del contador de fallos y la
/// decisión del bloqueo al llegar a cinco. La decisión D-06 resuelve ambos en una sola sentencia
/// UPDATE con OUTPUT, precisamente para que dos intentos simultáneos no pierdan un incremento.
/// Modelarlo en la entidad significaría leer y escribir por separado desde la aplicación, que es
/// lo que D-06 prohíbe. Su prueba vive en las de integración, con FR-023.
/// </summary>
public sealed class UsuarioTests
{
    private static readonly DateTimeOffset Ahora = new(2026, 8, 20, 9, 0, 0, TimeSpan.Zero);

    private static Usuario UsuarioActivo() =>
        Usuario.Crear(
            empleadoId: 1,
            nombreUsuario: "jperez",
            hashContrasena: new byte[32],
            salContrasena: new byte[16]).Valor;

    // ---------------------------------------------------------------------
    // R-U1 (FR-005): una cuenta desactivada no autentica
    // ---------------------------------------------------------------------

    [Fact]
    public void Usuario_activo_sin_bloqueo_puede_autenticarse()
    {
        var usuario = UsuarioActivo();

        var resultado = usuario.VerificarQuePuedeAutenticarse(Ahora);

        Assert.True(resultado.EsCorrecto);
    }

    [Fact]
    public void Usuario_desactivado_no_puede_autenticarse_aunque_la_contrasena_sea_correcta()
    {
        var usuario = UsuarioActivo();
        usuario.Desactivar();

        var resultado = usuario.VerificarQuePuedeAutenticarse(Ahora);

        Assert.True(resultado.EsFallo);
        Assert.Equal(ErroresDeUsuario.CuentaInactiva.Codigo, resultado.Error.Codigo);
    }

    [Fact]
    public void Desactivar_no_borra_la_fila_porque_la_baja_es_logica()
    {
        var usuario = UsuarioActivo();

        usuario.Desactivar();

        Assert.False(usuario.Activo);
        Assert.Equal("jperez", usuario.NombreUsuario);
        Assert.Equal(1, usuario.EmpleadoId);
    }

    // ---------------------------------------------------------------------
    // R-U2 (FR-017, FR-019): el bloqueo vigente rechaza incluso la contraseña correcta.
    // El borde está en el instante exacto del vencimiento.
    // ---------------------------------------------------------------------

    [Fact]
    public void Usuario_bloqueado_no_puede_autenticarse_aunque_la_contrasena_sea_correcta()
    {
        var usuario = UsuarioActivo();
        usuario.Bloquear(hasta: Ahora.AddMinutes(15));

        var resultado = usuario.VerificarQuePuedeAutenticarse(Ahora);

        Assert.True(resultado.EsFallo);
        Assert.Equal(ErroresDeUsuario.CuentaBloqueada.Codigo, resultado.Error.Codigo);
    }

    [Fact]
    public void El_bloqueo_sigue_vigente_un_segundo_antes_de_cumplirse()
    {
        var usuario = UsuarioActivo();
        var hasta = Ahora.AddMinutes(15);
        usuario.Bloquear(hasta);

        var resultado = usuario.VerificarQuePuedeAutenticarse(hasta.AddSeconds(-1));

        Assert.True(resultado.EsFallo);
        Assert.Equal(ErroresDeUsuario.CuentaBloqueada.Codigo, resultado.Error.Codigo);
    }

    [Fact]
    public void El_bloqueo_se_libera_en_el_instante_exacto_del_vencimiento()
    {
        var usuario = UsuarioActivo();
        var hasta = Ahora.AddMinutes(15);
        usuario.Bloquear(hasta);

        var resultado = usuario.VerificarQuePuedeAutenticarse(hasta);

        Assert.True(resultado.EsCorrecto);
    }

    [Fact]
    public void El_desbloqueo_es_implicito_y_no_requiere_ningun_proceso_que_limpie_el_estado()
    {
        var usuario = UsuarioActivo();
        var hasta = Ahora.AddMinutes(15);
        usuario.Bloquear(hasta);

        var resultado = usuario.VerificarQuePuedeAutenticarse(hasta.AddMinutes(1));

        Assert.True(resultado.EsCorrecto);
        Assert.NotNull(usuario.BloqueadoHasta);
    }

    [Fact]
    public void Un_usuario_desactivado_y_bloqueado_a_la_vez_se_rechaza_por_inactivo()
    {
        var usuario = UsuarioActivo();
        usuario.Desactivar();
        usuario.Bloquear(hasta: Ahora.AddMinutes(15));

        var resultado = usuario.VerificarQuePuedeAutenticarse(Ahora);

        Assert.True(resultado.EsFallo);
        Assert.Equal(ErroresDeUsuario.CuentaInactiva.Codigo, resultado.Error.Codigo);
    }

    // ---------------------------------------------------------------------
    // R-U3 (FR-020): el ingreso correcto reinicia contador y bloqueo
    // ---------------------------------------------------------------------

    [Fact]
    public void El_ingreso_exitoso_deja_el_contador_de_fallos_en_cero()
    {
        var usuario = UsuarioActivo();
        usuario.Bloquear(hasta: Ahora.AddMinutes(15));

        usuario.RegistrarIngresoExitoso();

        Assert.Equal(0, usuario.IntentosFallidos);
    }

    [Fact]
    public void El_ingreso_exitoso_limpia_el_bloqueo()
    {
        var usuario = UsuarioActivo();
        usuario.Bloquear(hasta: Ahora.AddMinutes(15));

        usuario.RegistrarIngresoExitoso();

        Assert.Null(usuario.BloqueadoHasta);
    }

    // ---------------------------------------------------------------------
    // R-U4 (FR-004): las causas se distinguen por código, nunca por mensaje.
    // Esta es la prueba que impide la fuga de información por el texto del error.
    // ---------------------------------------------------------------------

    [Fact]
    public void Las_causas_de_rechazo_comparten_exactamente_el_mismo_mensaje()
    {
        var mensajes = new[]
        {
            ErroresDeUsuario.CuentaInactiva.Mensaje,
            ErroresDeUsuario.CuentaBloqueada.Mensaje,
            ErroresDeUsuario.CredencialesIncorrectas.Mensaje,
            ErroresDeUsuario.CuentaInexistente.Mensaje,
        };

        Assert.Single(mensajes.Distinct());
    }

    [Fact]
    public void Las_causas_de_rechazo_se_distinguen_por_codigo_para_el_log_y_la_traza()
    {
        var codigos = new[]
        {
            ErroresDeUsuario.CuentaInactiva.Codigo,
            ErroresDeUsuario.CuentaBloqueada.Codigo,
            ErroresDeUsuario.CredencialesIncorrectas.Codigo,
            ErroresDeUsuario.CuentaInexistente.Codigo,
        };

        Assert.Equal(4, codigos.Distinct().Count());
    }

    [Theory]
    [InlineData("inexistente")]
    [InlineData("no existe")]
    [InlineData("inactiv")]
    [InlineData("bloquead")]
    [InlineData("desactivad")]
    public void El_mensaje_expuesto_no_revela_la_causa_del_rechazo(string terminoProhibido)
    {
        var mensaje = ErroresDeUsuario.CredencialesIncorrectas.Mensaje;

        Assert.DoesNotContain(terminoProhibido, mensaje, StringComparison.OrdinalIgnoreCase);
    }

    // ---------------------------------------------------------------------
    // R-U5 (FR-003, principio VI): la contraseña no se expone por ninguna vía
    // ---------------------------------------------------------------------

    [Fact]
    public void La_entidad_no_expone_ninguna_propiedad_con_la_contrasena_en_claro()
    {
        var propiedades = typeof(Usuario)
            .GetProperties()
            .Select(propiedad => propiedad.Name)
            .ToArray();

        Assert.DoesNotContain("Contrasena", propiedades);
        Assert.DoesNotContain("ContrasenaEnClaro", propiedades);
        Assert.DoesNotContain("Password", propiedades);
    }

    [Fact]
    public void La_representacion_en_texto_no_incluye_el_hash_ni_la_sal()
    {
        var usuario = Usuario.Crear(
            empleadoId: 1,
            nombreUsuario: "jperez",
            hashContrasena: [1, 2, 3, 4],
            salContrasena: [5, 6, 7, 8]).Valor;

        var texto = usuario.ToString() ?? string.Empty;

        Assert.DoesNotContain("1234", texto);
        Assert.DoesNotContain("5678", texto);
        Assert.DoesNotContain(Convert.ToBase64String([1, 2, 3, 4]), texto);
    }

    // ---------------------------------------------------------------------
    // Creación: invariantes de la entidad
    // ---------------------------------------------------------------------

    [Fact]
    public void Un_usuario_nuevo_nace_activo_sin_bloqueo_y_sin_fallos()
    {
        var usuario = UsuarioActivo();

        Assert.True(usuario.Activo);
        Assert.Null(usuario.BloqueadoHasta);
        Assert.Equal(0, usuario.IntentosFallidos);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void No_se_crea_un_usuario_sin_nombre(string nombreUsuario)
    {
        var resultado = Usuario.Crear(
            empleadoId: 1,
            nombreUsuario: nombreUsuario,
            hashContrasena: new byte[32],
            salContrasena: new byte[16]);

        Assert.True(resultado.EsFallo);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(100)]
    public void Un_nombre_de_usuario_en_los_bordes_de_longitud_se_acepta(int longitud)
    {
        var resultado = Usuario.Crear(
            empleadoId: 1,
            nombreUsuario: new string('u', longitud),
            hashContrasena: new byte[32],
            salContrasena: new byte[16]);

        Assert.True(resultado.EsCorrecto);
    }

    [Fact]
    public void Un_nombre_de_usuario_mas_largo_que_la_columna_se_rechaza()
    {
        var resultado = Usuario.Crear(
            empleadoId: 1,
            nombreUsuario: new string('u', 101),
            hashContrasena: new byte[32],
            salContrasena: new byte[16]);

        Assert.True(resultado.EsFallo);
    }

    [Fact]
    public void No_existe_usuario_sin_empleado()
    {
        var resultado = Usuario.Crear(
            empleadoId: 0,
            nombreUsuario: "jperez",
            hashContrasena: new byte[32],
            salContrasena: new byte[16]);

        Assert.True(resultado.EsFallo);
    }
}
