var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.HtmxBlazor_Web>("htmxblazor-web");

builder.Build().Run();
