using System.Windows;
using CloudClient.ViewModel;

namespace CloudClient.View;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class LoginPage : Window
{
    public LoginPage()
    {
        InitializeComponent();
        DataContext = new LoginViewModel();
    }
}