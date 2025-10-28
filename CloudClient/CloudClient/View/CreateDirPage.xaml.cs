using System.Collections.ObjectModel;
using System.Windows;
using CloudClient.Model;
using CloudClient.ViewModel;

namespace CloudClient.View;

public partial class CreateDirPage : Window
{
    public CreateDirPage(ObservableCollection<string> roots, Client currentClient)
    {
        InitializeComponent();
        var vm = new CreateDirViewModel(roots, currentClient);
        DataContext = vm;
        vm.RequestClose += () => this.Close();
    }
}