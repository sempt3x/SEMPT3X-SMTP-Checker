using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.ComponentModel;
using SEMPT3X_SMTP_CHECKER.Models;
using SEMPT3X_SMTP_CHECKER.Services;

namespace SEMPT3X_SMTP_CHECKER;

public partial class ErrorCodesWindow : Window
{
    private readonly ICollectionView _view;
    public ErrorCodesWindow()
    {
        InitializeComponent();
        _view = CollectionViewSource.GetDefaultView(ErrorCatalog.All);
        ErrorGrid.ItemsSource = _view;
    }
    private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        var query = SearchBox.Text.Trim();
        _view.Filter = item => string.IsNullOrEmpty(query) || item is ErrorDefinition error &&
            new[] { error.Code, error.Name, error.Cause, error.Solution, error.Category }.Any(value => value.Contains(query, StringComparison.OrdinalIgnoreCase));
    }
}
