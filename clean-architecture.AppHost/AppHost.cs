var builder = DistributedApplication.CreateBuilder(args);

var webApi = builder.AddProject<Projects.clean_architecture_WebApi>("clean-architecture-webapi");

builder.AddJavaScriptApp("clean-architecture-web", "../clean-architecture.Web")
    .WithReference(webApi)
    .WaitFor(webApi)
    .WithEnvironment("NEXT_PUBLIC_API_URL", webApi.GetEndpoint("https"))
    .WithHttpEndpoint(port: 3000, env: "PORT")
    .WithExternalHttpEndpoints();

await builder.Build().RunAsync();
