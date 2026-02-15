using ElBruno.AgentsOrchestration.Agents;
using ElBruno.AgentsOrchestration.Orchestration;
using ElBruno.AgentsOrchestration.Workspace;
using AgentsOrchestration.Web.Components;
using AgentsOrchestration.Web.Hubs;
using GitHub.Copilot.SDK;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddSignalR();

builder.Services.AddSingleton(sp =>
{
    return new AgentConfigurationStore(InstructionLoader.LoadInstructions());
});
builder.Services.AddSingleton<CopilotClient>();
builder.Services.AddSingleton<IAgentClient, CopilotAgentClient>();
builder.Services.AddSingleton<AgentFactory>();
builder.Services.AddScoped(sp =>
{
    var configuredRoot = builder.Configuration["Workspace:RootPath"] ?? "../../workspaces";
    var rootPath = Path.GetFullPath(Path.Combine(builder.Environment.ContentRootPath, configuredRoot));
    return new WorkspaceManager(rootPath);
});
builder.Services.AddScoped<IWorkspace>(sp => sp.GetRequiredService<WorkspaceManager>());
builder.Services.AddScoped(sp =>
{
    var maxFix = builder.Configuration.GetValue("Orchestration:MaxFixAttempts", 3);
    return new OrchestrationService(
        sp.GetRequiredService<AgentFactory>(),
        sp.GetRequiredService<IWorkspace>(),
        maxFix);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapDefaultEndpoints();
app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();
app.MapHub<OrchestrationHub>("/hubs/orchestration");

app.Run();
