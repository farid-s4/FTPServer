using System.Collections.ObjectModel;
using System.Windows;
using CloudClient.Model;
using CloudClient.Services;
using CloudClient.ViewModel;

namespace CloudClient.View;

public partial class UploadFilePage : Window
{
    public UploadFilePage(ObservableCollection<string> roots, Client currentClient, AuthService authService)
    {
        InitializeComponent();
        var vm = new UploadFileViewModel(roots, currentClient, authService);
        DataContext = vm;
    }
}