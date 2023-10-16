using HandJob.WebApi.Data;
using HandJob.WebApi.Filters;
using HandJob.WebApi.Setups;
using Soda.AutoMapper;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllersWithViews(setup =>
{
    setup.ReturnHttpNotAcceptable = true;
    setup.Filters.Add<GlobalExceptionFilter>();
}).AddJson();


// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddRazorPages();

builder.Services.AddHttpContextAccessor();

builder.Services.AddHandJobServices();

var app = builder.Build();


using var scoped = app.Services.CreateScope();
using var db = scoped.ServiceProvider.GetRequiredService<HandJobDbContext>();
try
{
    await db.Database.EnsureDeletedAsync();
    await db.Database.EnsureCreatedAsync();
}
catch { }

app.InitSodaMapper();

// Configure the HTTP request pipeline.
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseCors();

app.UseBlazorFrameworkFiles();
app.UseStaticFiles(new StaticFileOptions
{
    ServeUnknownFileTypes = true
    //ContentTypeProvider = new FileExtensionContentTypeProvider(new Dictionary<string, string>
    //        {
    //                { ".apk", "application/vnd.android.package-archive" }
    //        })
});


// app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

var defaultFilesOptions = new DefaultFilesOptions();
defaultFilesOptions.DefaultFileNames.Clear();
defaultFilesOptions.DefaultFileNames.Add("index.html");
app.UseDefaultFiles(defaultFilesOptions);
app.UseStaticFiles();

app.Run();
