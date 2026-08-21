using Optica.Web.Components;

var builder = WebApplication.CreateBuilder(args);

// Render en servidor para las pantallas de consulta e islas WebAssembly donde se requiere
// interactividad, según docs/adr/ADR-001-modelo-presentacion-blazor-web-app.md. La pantalla
// de ingreso se resuelve en servidor: no necesita interactividad y así evita descargar el
// runtime antes de autenticar.
builder.Services.AddRazorComponents()
    .AddInteractiveWebAssemblyComponents();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(Optica.Web.Client._Imports).Assembly);

app.Run();
