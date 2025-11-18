using Syncfusion.Licensing;
using System.Configuration;
using System.Data;
using System.Windows;

namespace _301428777_GuertaLigo__Lab_2
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            // Syncfusion License
            SyncfusionLicenseProvider.RegisterLicense("Ngo9BigBOggjHTQxAR8/V1JFaF5cXGRCf1NpQHxbf1x1ZFRMYVlbQHJPIiBoS35Rc0VqW3dfc3VQQmRdUUN0VEFc");

            base.OnStartup(e);
        }
    }

}
