using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;

var builder = WebApplication.CreateBuilder(args);

// Adiciona serviços ao contêiner.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Pega a string de conexão do environment variable (que o docker-compose injeta)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// Registra a conexão com o banco (IDbConnection) para injeção de dependência
// Isso é "Scoped", o que significa que uma nova conexão é criada para cada request HTTP.
builder.Services.AddScoped<IDbConnection>(sp => new SqlConnection(connectionString));

var app = builder.Build();

// Configura o pipeline de request HTTP.
app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthorization();

app.MapControllers();

// ---- INICIALIZAÇÃO DO BANCO ----
// Tenta conectar ao banco e criar a tabela quando a API sobe
// Isso é crucial para o Dapper funcionar, já que ele não tem "migrations"
InitializeDatabase(app);
// ---------------------------------

app.Run();


// ---- FUNÇÃO DE AJUDA PARA CRIAR A TABELA ----
void InitializeDatabase(IHost app)
{
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        var logger = services.GetRequiredService<ILogger<Program>>();
        try
        {
            logger.LogInformation("Tentando conectar ao banco de dados para inicialização...");
            
            using (var db = services.GetRequiredService<IDbConnection>())
            {
                db.Open();
                
                // Cria a tabela Products se ela não existir
                string createTableQuery = @"
                IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Products' AND xtype='U')
                CREATE TABLE Products (
                    Id INT PRIMARY KEY IDENTITY(1,1),
                    Nome NVARCHAR(100) NOT NULL
                );";
                
                db.Execute(createTableQuery);
                logger.LogInformation("Tabela 'Products' verificada/criada com sucesso.");

                // Opcional: Adiciona dados iniciais se a tabela estiver vazia
                db.Execute("IF NOT EXISTS (SELECT 1 FROM Products) INSERT INTO Products (Nome) VALUES ('Caneta'), ('Caderno')");
                
                db.Close();
            }
            
            logger.LogInformation("Banco de dados inicializado com sucesso.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Ocorreu um erro durante a inicialização do banco de dados.");
            // Você pode decidir se quer parar a aplicação se o banco não subir
            // Environment.Exit(-1);
        }
    }
}