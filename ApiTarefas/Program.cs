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


//Tarefas concluidas 
app.MapGet("/tarefas/concluidas", async (AppDbContext db) => {
    return await db.Tarefas.Where(t => t.IsConcluida).ToListAsync();
});


//Busca de tarefas por id
app.MapGet("/tarefas/{id}" , async (int id , AppDbContext db) =>
{
   return await db.Tarefas.FindAsync(id) is Tarefa tarefa ? Results.Ok(tarefa) : Results.NotFound();
});

//Insere tarefas 
app.MapPost("/tarefas" , async(Tarefa tarefa , AppDbContext db) =>
{
    db.Tarefas.Add(tarefa);
    await db.SaveChangesAsync();
    return Results.Created($"/tarefas/{tarefa.Id}" , tarefa);
} );



//Atualizar tarefas 
app.MapPut("/tarefas/{id}" , async (int id , Tarefa inputTarefa , AppDbContext db ) =>
{
    var tarefa = await db.Tarefas.FindAsync( id );
    if(tarefa is null)
        return Results.NotFound("Tarefa não encontrada ");

    tarefa.Nome = inputTarefa.Nome;
    tarefa.IsConcluida = inputTarefa.IsConcluida;

    await db.SaveChangesAsync();
    return Results.NoContent();

});

app.MapDelete("/tarefas/{id}" , async ( int id , AppDbContext db) =>
{
    if ( await db.Tarefas.FindAsync(id) is Tarefa tarefa)
    {
        //remove
        db.Tarefas.Remove(tarefa);
        //persiste no banco de dados 
        await db.SaveChangesAsync();

        //retorna a tarefa que foi removida 
        return Results.Ok(tarefa);
    }
    return Results.NotFound("Tarefa não encontrada");

});

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

