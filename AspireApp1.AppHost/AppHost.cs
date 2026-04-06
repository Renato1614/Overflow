using Projects;

var builder = DistributedApplication.CreateBuilder(args);

#pragma warning disable ASPIRECERTIFICATES001 // O tipo é apenas para fins de avaliação e está sujeito a alterações ou remoção em atualizações futuras. Suprima este diagnóstico para continuar.
var keycloak = builder.AddKeycloak("keycloak", 6001)
                                                    .WithoutHttpsCertificate()
                                                    .WithDataVolume("keycloak-data");
#pragma warning restore ASPIRECERTIFICATES001 // O tipo é apenas para fins de avaliação e está sujeito a alterações ou remoção em atualizações futuras. Suprima este diagnóstico para continuar.

var postgress = builder.AddPostgres("postgres", port: 5432)
    .WithDataVolume("postgres-data")
    .WithPgAdmin();

var typesenseApiKey = builder.AddParameter("typesense-api-key", secret: true);

var typesense = builder
        .AddContainer("typesense", "typesense/typesense","29.0")
        .WithArgs("--data-dir", "/data","--api-key", typesenseApiKey, "--enable-cors")
        .WithVolume("typesense-data","/data")
        .WithHttpEndpoint(8108,8108,name:"typesense");

var typesenseContainer = typesense.GetEndpoint("typesense");

var rabbitMq = builder.AddRabbitMQ("messaging")
    .WithDataVolume("rabbitmq-data")
    .WithManagementPlugin(port:1567);


var questionDb = postgress.AddDatabase("questionDb");

var questionService = builder.AddProject<QuestionService>("question-svc")
                             .WithReference(keycloak)
                             .WithReference(questionDb)
                             .WithReference(rabbitMq)
                             .WaitFor(keycloak)
                             .WaitFor(questionDb)
                             .WaitFor(rabbitMq);

var searchService = builder.AddProject<SearchService>("search-svc")
                            .WithEnvironment("typesense-api-key", typesenseApiKey)
                             .WithReference(typesenseContainer)
                             .WithReference(rabbitMq)
                             .WaitFor(typesense)
                             .WaitFor(rabbitMq);

builder.Build().Run();
