using System.Windows;
using CloudClient.ViewModel;

namespace CloudClient.View;

public partial class RegisterPage : Window
{
    public RegisterPage()
    {
        InitializeComponent();
        DataContext = new RegisterViewModel();
    }
}