using System.Windows;
using System.Windows.Controls;
using CloudClient.ViewModel;

namespace CloudClient.View;

public partial class RegisterPage : Window
{
    public RegisterPage()
    {
        InitializeComponent();
        DataContext = new RegisterViewModel();
    }

    private void PasswordBox_OnPasswordChanged(object sender, RoutedEventArgs e)
    {
        if (DataContext is RegisterViewModel vm)
        {
            vm.PasswordBox = ((PasswordBox)sender).Password;
        }
    }
}