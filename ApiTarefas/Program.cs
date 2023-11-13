using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//para usar o EF CORE em memória 
builder.Services.AddDbContext<AppDbContext>(
    opt => opt.UseInMemoryDatabase("TarefasDb") );



//iniciando o projeto

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.MapGet("/", () => "ola mundo");

app.MapGet("frases", async () => 
    await new HttpClient().GetStreamAsync("https://ron-swanson-quotes.herokuapp.com/v2/quotes")
);


//rota de tarefas 
app.MapGet("/tarefas" ,  async(AppDbContext db) =>{
    return await db.Tarefas.ToListAsync();
});

app.MapPost("/tarefas" , async(Tarefa tarefa , AppDbContext db) =>
{
    db.Tarefas.Add(tarefa);
    await db.SaveChangesAsync();
    return Results.Created($"/tarefas/{tarefa.Id}" , tarefa);
} );


app.Run();

class Tarefa
{
    public int Id { get; set; }

    public string? Nome { get; set; }

    public bool IsConcluida { get; set; }

}

class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {}

    public DbSet<Tarefa> Tarefas => Set<Tarefa>();
}

