using A5.Repositories;
using A5.Services;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;

namespace A5
{
    public partial class App : Application
    {
        public IServiceProvider Services { get; private set; } = null!;

        // Configuration of services and opening the main window
        protected override void OnStartup(StartupEventArgs e)
        {
            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);

            Services = serviceCollection.BuildServiceProvider();

            base.OnStartup(e);

            var mainWindow = Services.GetRequiredService<MainWindow>();
            mainWindow.Show();
        }

        // Configuration of services by tying interfaces to their implementations
        private void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IConverterService, ConverterService>();
            services.AddSingleton<IDecoderService, DecoderService>();
            services.AddSingleton<IEncoderService, EncoderService>();
            services.AddSingleton<IMatrixService, MatrixService>();
            services.AddSingleton<INoisyChannelService, NoisyChannelService>();
            services.AddSingleton<IValidationService, ValidationService>();

            services.AddSingleton<IMatrixRepository, MatrixRepository>();

            services.AddSingleton<MainWindow>();
        }
    }

}
