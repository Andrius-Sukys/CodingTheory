using System.Diagnostics;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace A5.UI;

// Control class for Help and About window
public partial class HelpAboutWindow : UserControl
{
    // Initialization of the window
    public HelpAboutWindow()
    {
        InitializeComponent();
    }

    // Method used to navigate to a given link
    private void NavigateToLink(object sender, RequestNavigateEventArgs e)
    {
        Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });
        e.Handled = true;
    }
}
