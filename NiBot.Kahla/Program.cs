using System;
using Aiursoft.Scanner;
using Kahla.SDK.Abstract;
using Microsoft.Extensions.DependencyInjection;

namespace NiBot.Kahla
{
    class Program
    {
        static void Main(string[] args)
        {
            StartUp.ConfigureServices()
                .GetService<StartUp>()
                .Bot
                .Start().Wait();
        }
    }
}
