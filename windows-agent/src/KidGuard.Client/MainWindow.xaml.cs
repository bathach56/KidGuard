using System.Windows;

namespace KidGuard.Client;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void ParentModeButton_Click(object sender, RoutedEventArgs e)
    {
        ShowPanel(ParentPanel);
    }

    private void ChildModeButton_Click(object sender, RoutedEventArgs e)
    {
        ShowPanel(ChildPanel);
    }

    private void BackButton_Click(object sender, RoutedEventArgs e)
    {
        ShowPanel(RoleSelectionPanel);
    }

    private void ShowPanel(UIElement activePanel)
    {
        RoleSelectionPanel.Visibility = Visibility.Collapsed;
        ParentPanel.Visibility = Visibility.Collapsed;
        ChildPanel.Visibility = Visibility.Collapsed;

        activePanel.Visibility = Visibility.Visible;
    }
}
