using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using ACTA.Data;
using ACTA.Data.Repositories;
using ACTA.Models;

namespace ACTA.Views;

public partial class HistoryWindow : Window
{
    private readonly MeetingActRepository _repository;

    private readonly ObservableCollection<MeetingActSummary> _meetingActs = [];

    private List<MeetingActSummary> _allMeetingActs = [];

    public long? SelectedMeetingActId { get; private set; }

    public HistoryWindow(Database database)
    {
        InitializeComponent();

        _repository = new MeetingActRepository(database);

        HistoryDataGrid.ItemsSource = _meetingActs;

        Loaded += HistoryWindow_Loaded;
    }

    private async void HistoryWindow_Loaded(object sender, RoutedEventArgs e)
    {
        await LoadMeetingActsAsync();
    }

    private async Task LoadMeetingActsAsync()
    {
        try
        {
            _allMeetingActs = await _repository.GetAllAsync();

            ApplyFilter();
        }
        catch (Exception exception)
        {
            MessageBox.Show(
                $"No fue posible cargar el historial.\n\n{exception.Message}",
                "ACTA",
                MessageBoxButton.OK,
                MessageBoxImage.Error
            );
        }
    }

    private void ApplyFilter()
    {
        string searchText =
            SearchTextBox.Text.Trim();

        IEnumerable<MeetingActSummary> filtered = _allMeetingActs;

        if (!string.IsNullOrWhiteSpace(searchText))
        {
            filtered = _allMeetingActs.Where(
                act =>
                    act.Motives.Contains(searchText, StringComparison.OrdinalIgnoreCase) ||
                    act.Id.ToString().Contains(searchText, StringComparison.OrdinalIgnoreCase) ||
                    act.DateTime.ToString("dd/MM/yyyy").Contains(searchText,StringComparison.OrdinalIgnoreCase)
            );
        }

        _meetingActs.Clear();

        foreach (MeetingActSummary meetingAct in filtered)
        {
            _meetingActs.Add(meetingAct);
        }

        UpdateRecordCount();
    }

    private void UpdateRecordCount()
    {
        RecordCountTextBlock.Text =
            _meetingActs.Count == 1
                ? "1 acta"
                : $"{_meetingActs.Count} actas";
    }

    private void SearchTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
    {
        ApplyFilter();
    }

    private async void RefreshButton_Click(object sender, RoutedEventArgs e)
    {
        await LoadMeetingActsAsync();
    }
       

    private void HistoryDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        OpenSelectedMeetingAct();
    }

    private void OpenSelectedMeetingAct()
    {
        if (HistoryDataGrid.SelectedItem
            is not MeetingActSummary selected)
        {
            return;
        }

        SelectedMeetingActId = selected.Id;

        DialogResult = true;
    }

    private async void DeleteButton_Click(object sender, RoutedEventArgs e)
    {
        if (HistoryDataGrid.SelectedItem is not MeetingActSummary selected)
        {
            MessageBox.Show(
                "Seleccione un acta.",
                "ACTA",
                MessageBoxButton.OK,
                MessageBoxImage.Information
            );

            return;
        }

        MessageBoxResult result = MessageBox.Show(
            $"¿Desea eliminar el acta #{selected.Id}?\n\n" +
            "Esta acción no se puede deshacer.",
            "Eliminar acta",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning
        );

        if (result != MessageBoxResult.Yes)
        {
            return;
        }

        try
        {
            bool deleted = await _repository.DeleteAsync(selected.Id);

            if (!deleted)
            {
                MessageBox.Show(
                    "El acta ya no existe.",
                    "ACTA",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                );
            }

            await LoadMeetingActsAsync();
        }
        catch (Exception exception)
        {
            MessageBox.Show(
                $"No fue posible eliminar el acta.\n\n{exception.Message}",
                "ACTA",
                MessageBoxButton.OK,
                MessageBoxImage.Error
            );
        }
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}