using System.Windows;
using System.Windows.Controls;
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

    private void PasswordBox_OnPasswordChanged(object sender, RoutedEventArgs e)
    {
        if (DataContext is LoginViewModel vm)
        {
            vm.PasswordBox = ((PasswordBox)sender).Password;
        }
    }
}