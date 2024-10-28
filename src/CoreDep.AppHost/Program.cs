var builder = DistributedApplication.CreateBuilder(args);

var app = builder.AddProject<Projects.CoreDep>("app");

builder.Build().Run();