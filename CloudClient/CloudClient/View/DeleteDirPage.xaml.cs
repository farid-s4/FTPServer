using System.Collections.ObjectModel;
using System.Windows;
using CloudClient.Model;
using CloudClient.ViewModel;

namespace CloudClient.View;

public partial class DeleteDirPage : Window
{
    public DeleteDirPage(ObservableCollection<string> roots, Client currentClient)
    {
        InitializeComponent();
        var vm = new DeleteDirViewModel(roots, currentClient);
        DataContext = vm;
    }
}