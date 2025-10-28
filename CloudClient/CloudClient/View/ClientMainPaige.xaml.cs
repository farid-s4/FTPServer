using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;
using CloudClient.Model;
using CloudClient.ViewModel;

namespace CloudClient.View;

public partial class ClientMainPaige : Window
{
    public ClientMainPaige(Client loggedInClient)
    {
        InitializeComponent();
        DataContext = new ClientViewModel(loggedInClient);
    }

    private void FileList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (DataContext is ClientViewModel clientViewModel)
        {
            clientViewModel.OpenSelectedDirCommand.Execute(null);
        }
    }
}