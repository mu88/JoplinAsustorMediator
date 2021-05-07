using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace JoplinAsustorMediator
{
    public class Program
    {
        public static void Main(string[] args)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                System.IO.File.Copy("mu88_root_CA.crt", "/usr/local/share/ca-certificates/mu88_root_CA.crt");
                
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "update-ca-certificates",
                        RedirectStandardOutput = true,
                        UseShellExecute = false,
                        CreateNoWindow = false,
                    }
                };
                process.Start();
                var standardOutput = process.StandardOutput.ReadToEnd();
                process.WaitForExit();

                Console.WriteLine(standardOutput);
            }
            
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((_, config) => { config.AddEnvironmentVariables(); })
                .ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); });
        }
    }
}