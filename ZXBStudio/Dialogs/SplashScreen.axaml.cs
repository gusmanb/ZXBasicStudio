using Avalonia.Controls;
using System.Reflection;

namespace ZXBasicStudio.Dialogs
{
    public partial class SplashScreen : Window
    {
        public SplashScreen()
        {
            InitializeComponent();

            Assembly assembly = Assembly.GetExecutingAssembly();
            // Obtiene la información de versión del ensamblado
            var version = assembly.GetName().Version.ToString();
            txtVersion.Text = "Version " + version + "-beta";
        }
    }
}
