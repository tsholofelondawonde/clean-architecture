var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.clean_architecture_WebApi>("clean-architecture-webapi");

builder.Build().Run();
