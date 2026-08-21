using Optica.Application.Abstracciones;
using Optica.Application.Autenticacion.Comandos.IniciarSesion;
using Optica.Domain.Usuarios;

namespace Optica.Application.Tests.Autenticacion;

/// <summary>
/// Handler del comando de ingreso. Escenarios 1.2 y 1.4 de la especificación, más el mensaje
/// genérico único de FR-004 y el registro de intentos de FR-006.
///
/// Tarea T028. Se escriben antes del handler, que llega en T037. Definen su superficie:
///   IniciarSesionCommand(NombreUsuario, Contrasena, DireccionOrigen)
///   IniciarSesionHandler(IRepositorioDeUsuarios, IHasheadorDeContrasena,
///                        IRegistradorDeIntentos, IEmisorDeToken, TimeProvider)
///   Task&lt;Resultado&lt;SesionIniciada&gt;&gt; Handle(comando, cancelacion)
///
/// Los dobles se escriben a mano en lugar de usar una biblioteca de simulación. No hay ninguna
/// referenciada en el proyecto, y la constitución solo exige simular interfaces, no una
/// herramienta concreta. Un doble a mano de cuatro líneas se lee mejor que una configuración
/// de tres llamadas encadenadas.
/// </summary>
public sealed class IniciarSesionHandlerTests
{
    private static readonly DateTimeOffset Ahora = new(2026, 8, 20, 9, 0, 0, TimeSpan.Zero);
    private const string ContrasenaCorrecta = "Optica2026#Segura";
    private const string ContrasenaIncorrecta = "ClaveEquivocada#1";

    // ---------------------------------------------------------------------
    // Escenario 1.1: credenciales correctas sobre cuenta activa
    // ---------------------------------------------------------------------

    [Fact]
    public async Task Credenciales_correctas_sobre_cuenta_activa_inician_sesion()
    {
        var contexto = Contexto.ConUsuarioActivo();

        var resultado = await contexto.Ejecutar("jperez", ContrasenaCorrecta);

        Assert.True(resultado.EsCorrecto);
    }

    [Fact]
    public async Task Un_ingreso_exitoso_emite_token_de_acceso_y_credencial_de_renovacion()
    {
        var contexto = Contexto.ConUsuarioActivo();

        await contexto.Ejecutar("jperez", ContrasenaCorrecta);

        Assert.Equal(1, contexto.Emisor.TokensEmitidos);
    }

    // ---------------------------------------------------------------------
    // Escenario 1.2: contraseña incorrecta
    // ---------------------------------------------------------------------

    [Fact]
    public async Task Contrasena_incorrecta_rechaza_el_ingreso()
    {
        var contexto = Contexto.ConUsuarioActivo();

        var resultado = await contexto.Ejecutar("jperez", ContrasenaIncorrecta);

        Assert.True(resultado.EsFallo);
    }

    [Fact]
    public async Task Contrasena_incorrecta_no_emite_ningun_token()
    {
        var contexto = Contexto.ConUsuarioActivo();

        await contexto.Ejecutar("jperez", ContrasenaIncorrecta);

        Assert.Equal(0, contexto.Emisor.TokensEmitidos);
    }

    // ---------------------------------------------------------------------
    // Escenario 1.4: cuenta desactivada lógicamente
    // ---------------------------------------------------------------------

    [Fact]
    public async Task Una_cuenta_desactivada_no_ingresa_aunque_la_contrasena_sea_correcta()
    {
        var contexto = Contexto.ConUsuarioDesactivado();

        var resultado = await contexto.Ejecutar("mlopez", ContrasenaCorrecta);

        Assert.True(resultado.EsFallo);
        Assert.Equal(0, contexto.Emisor.TokensEmitidos);
    }

    [Fact]
    public async Task Una_cuenta_bloqueada_no_ingresa_aunque_la_contrasena_sea_correcta()
    {
        var contexto = Contexto.ConUsuarioBloqueadoHasta(Ahora.AddMinutes(10));

        var resultado = await contexto.Ejecutar("rgomez", ContrasenaCorrecta);

        Assert.True(resultado.EsFallo);
        Assert.Equal(0, contexto.Emisor.TokensEmitidos);
    }

    // ---------------------------------------------------------------------
    // FR-004: las cuatro causas producen el mismo error hacia el exterior.
    // Es la prueba central del handler: si alguna causa devuelve un error distinto,
    // el sistema permite enumerar cuentas.
    // ---------------------------------------------------------------------

    [Fact]
    public async Task Las_cuatro_causas_de_rechazo_devuelven_el_mismo_error()
    {
        var errores = new[]
        {
            (await Contexto.SinUsuarios().Ejecutar("fantasma", ContrasenaCorrecta)).Error,
            (await Contexto.ConUsuarioActivo().Ejecutar("jperez", ContrasenaIncorrecta)).Error,
            (await Contexto.ConUsuarioDesactivado().Ejecutar("mlopez", ContrasenaCorrecta)).Error,
            (await Contexto.ConUsuarioBloqueadoHasta(Ahora.AddMinutes(10))
                .Ejecutar("rgomez", ContrasenaCorrecta)).Error,
        };

        Assert.Single(errores.Select(error => error.Codigo).Distinct());
        Assert.Single(errores.Select(error => error.Mensaje).Distinct());
    }

    // ---------------------------------------------------------------------
    // SC-004 y decisión D-02: la derivación señuelo iguala el tiempo de respuesta.
    // Se afirma que el señuelo se invoca, no cuánto tarda: medir tiempo en una prueba
    // unitaria produce una prueba intermitente. La medición real es T031, de integración.
    // ---------------------------------------------------------------------

    [Fact]
    public async Task Un_usuario_inexistente_dispara_la_derivacion_senuelo()
    {
        var contexto = Contexto.SinUsuarios();

        await contexto.Ejecutar("fantasma", ContrasenaCorrecta);

        Assert.Equal(1, contexto.Hasheador.SenuelosDerivados);
    }

    [Fact]
    public async Task Un_usuario_existente_no_dispara_la_derivacion_senuelo()
    {
        var contexto = Contexto.ConUsuarioActivo();

        await contexto.Ejecutar("jperez", ContrasenaIncorrecta);

        Assert.Equal(0, contexto.Hasheador.SenuelosDerivados);
    }

    // ---------------------------------------------------------------------
    // FR-006 y FR-022: se registra todo intento, exista o no la cuenta
    // ---------------------------------------------------------------------

    [Fact]
    public async Task Un_ingreso_exitoso_se_registra_como_exitoso()
    {
        var contexto = Contexto.ConUsuarioActivo();

        await contexto.Ejecutar("jperez", ContrasenaCorrecta);

        var intento = Assert.Single(contexto.Registrador.Intentos);
        Assert.Equal("jperez", intento.NombreUsuario);
        Assert.True(intento.Exitoso);
    }

    [Fact]
    public async Task Un_intento_fallido_se_registra_como_fallido()
    {
        var contexto = Contexto.ConUsuarioActivo();

        await contexto.Ejecutar("jperez", ContrasenaIncorrecta);

        var intento = Assert.Single(contexto.Registrador.Intentos);
        Assert.False(intento.Exitoso);
    }

    [Fact]
    public async Task Un_intento_contra_una_cuenta_inexistente_se_registra_sin_identificador()
    {
        var contexto = Contexto.SinUsuarios();

        await contexto.Ejecutar("fantasma", ContrasenaCorrecta);

        var intento = Assert.Single(contexto.Registrador.Intentos);
        Assert.Equal("fantasma", intento.NombreUsuario);
        Assert.Null(intento.UsuarioId);
        Assert.False(intento.Exitoso);
    }

    [Fact]
    public async Task El_intento_registra_la_direccion_de_origen()
    {
        var contexto = Contexto.ConUsuarioActivo();

        await contexto.Ejecutar("jperez", ContrasenaCorrecta, direccionOrigen: "192.168.1.40");

        var intento = Assert.Single(contexto.Registrador.Intentos);
        Assert.Equal("192.168.1.40", intento.DireccionOrigen);
    }

    [Fact]
    public async Task El_intento_registra_la_marca_de_tiempo_del_reloj_inyectado_en_UTC()
    {
        var contexto = Contexto.ConUsuarioActivo();

        await contexto.Ejecutar("jperez", ContrasenaCorrecta);

        var intento = Assert.Single(contexto.Registrador.Intentos);
        Assert.Equal(Ahora, intento.Momento);
        Assert.Equal(TimeSpan.Zero, intento.Momento.Offset);
    }

    // ---------------------------------------------------------------------
    // R-U5 y compuerta G8: la contraseña no viaja a la traza de intentos
    // ---------------------------------------------------------------------

    [Fact]
    public async Task La_contrasena_presentada_no_llega_al_registro_de_intentos()
    {
        var contexto = Contexto.ConUsuarioActivo();

        await contexto.Ejecutar("jperez", ContrasenaIncorrecta);

        var intento = Assert.Single(contexto.Registrador.Intentos);
        var contenido = $"{intento.NombreUsuario}|{intento.DireccionOrigen}";
        Assert.DoesNotContain(ContrasenaIncorrecta, contenido);
    }

    // ---------------------------------------------------------------------
    // Cancelación: principio de rendimiento, el token se propaga hasta el repositorio
    // ---------------------------------------------------------------------

    [Fact]
    public async Task El_token_de_cancelacion_se_propaga_al_repositorio()
    {
        var contexto = Contexto.ConUsuarioActivo();
        using var origen = new CancellationTokenSource();

        await contexto.Ejecutar("jperez", ContrasenaCorrecta, cancelacion: origen.Token);

        Assert.Equal(origen.Token, contexto.Repositorio.UltimaCancelacionRecibida);
    }

    // =====================================================================
    // Dobles de prueba
    // =====================================================================

    private sealed class Contexto
    {
        private readonly IniciarSesionHandler handler;

        private Contexto(Usuario? usuario)
        {
            Repositorio = new RepositorioFalso(usuario);
            Hasheador = new HasheadorFalso();
            Registrador = new RegistradorFalso();
            Emisor = new EmisorFalso();
            handler = new IniciarSesionHandler(
                Repositorio, Hasheador, Registrador, Emisor, new RelojFijo(Ahora));
        }

        public RepositorioFalso Repositorio { get; }

        public HasheadorFalso Hasheador { get; }

        public RegistradorFalso Registrador { get; }

        public EmisorFalso Emisor { get; }

        public static Contexto SinUsuarios() => new(null);

        public static Contexto ConUsuarioActivo() => new(CrearUsuario("jperez"));

        public static Contexto ConUsuarioDesactivado()
        {
            var usuario = CrearUsuario("mlopez");
            usuario.Desactivar();
            return new Contexto(usuario);
        }

        public static Contexto ConUsuarioBloqueadoHasta(DateTimeOffset hasta)
        {
            var usuario = CrearUsuario("rgomez");
            usuario.Bloquear(hasta);
            return new Contexto(usuario);
        }

        public Task<Optica.Domain.Comun.Resultado<SesionIniciada>> Ejecutar(
            string nombreUsuario,
            string contrasena,
            string direccionOrigen = "192.168.1.1",
            CancellationToken cancelacion = default) =>
            handler.Handle(
                new IniciarSesionCommand(nombreUsuario, contrasena, direccionOrigen), cancelacion);

        private static Usuario CrearUsuario(string nombreUsuario) =>
            Usuario.Crear(
                empleadoId: 1,
                nombreUsuario: nombreUsuario,
                hashContrasena: HasheadorFalso.HashDeLaContrasenaCorrecta,
                salContrasena: new byte[16]).Valor;
    }

    private sealed class RepositorioFalso(Usuario? usuario) : IRepositorioDeUsuarios
    {
        public CancellationToken UltimaCancelacionRecibida { get; private set; }

        public Task<Usuario?> ObtenerPorNombreDeUsuario(
            string nombreUsuario, CancellationToken cancelacion)
        {
            UltimaCancelacionRecibida = cancelacion;
            var coincide = usuario is not null
                && string.Equals(
                    usuario.NombreUsuario, nombreUsuario, StringComparison.OrdinalIgnoreCase);
            return Task.FromResult(coincide ? usuario : null);
        }
    }

    private sealed class HasheadorFalso : IHasheadorDeContrasena
    {
        public static readonly byte[] HashDeLaContrasenaCorrecta = [42];

        public int SenuelosDerivados { get; private set; }

        public (byte[] Hash, byte[] Salt) Derivar(string contrasena) =>
            (HashDeLaContrasenaCorrecta, new byte[16]);

        public bool Coincide(string contrasena, byte[] hashAlmacenado, byte[] salAlmacenada) =>
            contrasena == ContrasenaCorrecta
            && hashAlmacenado.SequenceEqual(HashDeLaContrasenaCorrecta);

        public void DerivarSenuelo() => SenuelosDerivados++;
    }

    private sealed class RegistradorFalso : IRegistradorDeIntentos
    {
        private readonly List<IntentoDeIngreso> intentos = [];

        public IReadOnlyList<IntentoDeIngreso> Intentos => intentos;

        public Task Registrar(IntentoDeIngreso intento, CancellationToken cancelacion)
        {
            intentos.Add(intento);
            return Task.CompletedTask;
        }
    }

    private sealed class EmisorFalso : IEmisorDeToken
    {
        public int TokensEmitidos { get; private set; }

        public SesionIniciada Emitir(Usuario usuario, DateTimeOffset ahora)
        {
            TokensEmitidos++;
            return new SesionIniciada(
                ExpiraEn: ahora.AddMinutes(15),
                SesionExpiraEn: ahora.AddHours(8),
                Roles: []);
        }
    }

    private sealed class RelojFijo(DateTimeOffset ahora) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => ahora;
    }
}
