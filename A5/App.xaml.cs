using A5.Services;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;

namespace A5
{
    public partial class App : Application
    {
        public IServiceProvider Services { get; private set; } = null!;

        protected override void OnStartup(StartupEventArgs e)
        {
            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);

            Services = serviceCollection.BuildServiceProvider();

            base.OnStartup(e);

            var mainWindow = Services.GetRequiredService<MainWindow>();
            mainWindow.Show();
        }

        private void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IConverterService, ConverterService>();
            services.AddSingleton<IDecoderService, DecoderService>();
            services.AddSingleton<IEncoderService, EncoderService>();
            services.AddSingleton<IMatrixService, MatrixService>();
            services.AddSingleton<INoisyChannelService, NoisyChannelService>();

            services.AddSingleton<MainWindow>();
        }
    }

}
