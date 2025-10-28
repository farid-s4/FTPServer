using System.Collections.ObjectModel;
using System.Windows;
using CloudClient.Model;
using CloudClient.ViewModel;

namespace CloudClient.View;

public partial class RenameDirPage : Window
{
    public RenameDirPage(ObservableCollection<string> roots, Client currentClient)
    {
        InitializeComponent();
        var vm = new RenameDirViewModel( roots, currentClient);
        DataContext = vm;
    }
}