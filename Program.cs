using ERPtask.servcies;
using Microsoft.Extensions.Configuration;

namespace ERPtask
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            // Get the connection string from appsettings.json
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

            // Add logging
            builder.Services.AddLogging(logging =>
            {
                logging.AddConsole();
                logging.AddDebug();
            });

            // Register services with proper DI
            builder.Services.AddSingleton(new CostService(connectionString));
            builder.Services.AddSingleton<TaxCalculationService>();
            builder.Services.AddSingleton<InvoiceService>(provider =>
                new InvoiceService(connectionString, provider.GetRequiredService<TaxCalculationService>()));
            builder.Services.AddSingleton<ClientService>(provider =>
                new ClientService(connectionString));
            builder.Services.AddSingleton<NotificationService>(provider =>
                new NotificationService(connectionString));
            builder.Services.AddHostedService<ReminderBackgroundService>();

            var app = builder.Build();
            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.UseAuthorization();
            app.MapControllers();
            app.Run();
        }
    }
}
