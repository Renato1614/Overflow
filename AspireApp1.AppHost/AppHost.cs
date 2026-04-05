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

var questionDb = postgress.AddDatabase("questionDb");

var questionService = builder.AddProject<QuestionService>("question-svc")
                             .WithReference(keycloak)
                             .WithReference(questionDb)
                             .WaitFor(keycloak)
                             .WaitFor(questionDb);


builder.Build().Run();
