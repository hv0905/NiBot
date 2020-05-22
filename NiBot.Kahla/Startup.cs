using Aiursoft.Scanner;
using Aiursoft.Scanner.Interfaces;
using Kahla.SDK.Abstract;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using Microsoft.EntityFrameworkCore;

namespace NiBot.Kahla
{
    public class StartUp : IScopedDependency
    {
        public BotBase Bot { get; set; }

        public StartUp(BotSelector botConfigurer, NiDbContext dbContext)
        {
            // init db
            dbContext.Database.Migrate();
            Bot = botConfigurer.SelectBot();
        }

        public static IServiceProvider ConfigureServices()
        {
            JsonConvert.DefaultSettings = () => new JsonSerializerSettings()
            {
                DateTimeZoneHandling = DateTimeZoneHandling.Utc,
                DateFormatHandling = DateFormatHandling.IsoDateFormat,
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
            };
            

            return new ServiceCollection()
                .AddHttpClient()
                .AddScannedDependencies()
                .AddDbContext<NiDbContext>(options =>
                {
                    options.UseSqlite("Data Source=userData.db");
                })
                .AddBots();
        }
    }
}
