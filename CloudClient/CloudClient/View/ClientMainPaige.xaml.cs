using System.Windows;
using CloudClient.ViewModel;

namespace CloudClient.View;

public partial class ClientMainPaige : Window
{
    public ClientMainPaige()
    {
        InitializeComponent();
        DataContext = new ClientViewModel();
    }
}