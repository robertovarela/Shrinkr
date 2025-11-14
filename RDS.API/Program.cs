using RDS.API;
using RDS.API.Common;

var builder = WebApplication.CreateBuilder(args);

builder.AddConfiguration();
builder.AddServiceContainer();
builder.AddDataContexts();
builder.AddDocumentation();
builder.AddCrossOrigin();
builder.AddRepositories();
builder.AddServices();

var app = builder.Build();

app.UseCors(ConfigurationApi.CorsPolicyName);

app.LoadConfiguration();
app.ConfigureEnvironment();
app.ConfigureComponents();

app.Run();
