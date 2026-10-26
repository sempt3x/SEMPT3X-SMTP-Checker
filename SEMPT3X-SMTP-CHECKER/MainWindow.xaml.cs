using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Microsoft.Win32;
using SEMPT3X_SMTP_CHECKER.ViewModels;

namespace SEMPT3X_SMTP_CHECKER;

public partial class MainWindow : Window
{
    private readonly MainViewModel _viewModel = new();
    private bool _logScrollPending;
    private bool? _usesCompactFormLayout;
    private bool? _usesCompactHeaderLayout;

    public MainWindow()
    {
        InitializeComponent();
        DataContext = _viewModel;
        _viewModel.SmtpPasswordSource = () => SmtpPasswordBox.Password;
        _viewModel.ClientSecretSource = () => ClientSecretBox.Password;
        _viewModel.Logs.CollectionChanged += Logs_CollectionChanged;
        Closed += (_, _) => _viewModel.Logs.CollectionChanged -= Logs_CollectionChanged;
    }
    private void AddAttachments_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog { Multiselect = true, Title = "Select email attachments", CheckFileExists = true };
        if (dialog.ShowDialog(this) == true) _viewModel.AddAttachments(dialog.FileNames);
    }
    private void OpenErrors_Click(object sender, RoutedEventArgs e) => new ErrorCodesWindow { Owner = this }.ShowDialog();
    private void OpenSmtpPortHelp_Click(object sender, RoutedEventArgs e) => new SmtpPortInfoWindow { Owner = this }.ShowDialog();

    private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        var useCompactLayout = e.NewSize.Width < 1000;
        if (_usesCompactHeaderLayout == useCompactLayout) return;

        _usesCompactHeaderLayout = useCompactLayout;
        HeaderTitle.Visibility = useCompactLayout ? Visibility.Collapsed : Visibility.Visible;
        ConnectionTypeCombo.Width = useCompactLayout ? 220 : 280;
    }

    private void ConfigurationPanel_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        var useCompactLayout = e.NewSize.Width < 540;
        if (_usesCompactFormLayout == useCompactLayout) return;

        _usesCompactFormLayout = useCompactLayout;
        SetResponsiveFieldLayout(SmtpPortPanel, SmtpEndpointGap, SmtpEndpointSecondColumn, useCompactLayout);
        SetResponsiveFieldLayout(RecipientEmailPanel, EmailAddressGap, EmailAddressSecondColumn, useCompactLayout);
    }

    private static void SetResponsiveFieldLayout(
        FrameworkElement secondField,
        ColumnDefinition gap,
        ColumnDefinition secondColumn,
        bool useCompactLayout)
    {
        Grid.SetRow(secondField, useCompactLayout ? 1 : 0);
        Grid.SetColumn(secondField, useCompactLayout ? 0 : 2);
        gap.Width = useCompactLayout ? new GridLength(0) : new GridLength(14);
        secondColumn.Width = useCompactLayout ? new GridLength(0) : new GridLength(1, GridUnitType.Star);
    }

    private void Logs_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        // Scroll only after WPF has finished applying all pending collection changes.
        // Calling ScrollIntoView directly from CollectionChanged re-enters layout and can
        // make the ItemContainerGenerator inconsistent when log entries arrive rapidly.
        if (e.Action == NotifyCollectionChangedAction.Reset || _logScrollPending) return;

        _logScrollPending = true;
        Dispatcher.BeginInvoke(DispatcherPriority.ContextIdle, () =>
        {
            _logScrollPending = false;
            if (LogList.Items.Count > 0)
                LogList.ScrollIntoView(LogList.Items[^1]);
        });
    }
}
