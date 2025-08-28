using ABC_Retail.Services;

namespace ABC_Retail
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            var configuration= builder.Configuration;




            // Add services to the container.
            builder.Services.AddControllersWithViews();

            builder.Services.AddSingleton(new TableStorageService(configuration.GetConnectionString("AzureStorage")));

            builder.Services.AddSingleton(new BlobService(configuration.GetConnectionString("AzureStorage")));

            builder.Services.AddSingleton<QueueService>(sp =>
            {
                var connectionString = configuration.GetConnectionString("AzureStorage");
                return new QueueService(connectionString,"abcretail");
            }


            );

            builder.Services.AddSingleton<AzureFileShareServices>(sp =>
            {
                var connectionString = configuration.GetConnectionString("AzureStorage");
                return new AzureFileShareServices(connectionString, "abcretailshare");

            }

            );

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseRouting();

            app.UseAuthorization();

            app.MapStaticAssets();
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}")
                .WithStaticAssets();

            app.Run();
        }
    }
}
