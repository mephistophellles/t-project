using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using t_project.DB.Context;
using t_project.Models;

namespace t_project.Views.Pages
{
    public partial class DirectionStatusPage : Page, INotifyPropertyChanged
    {
        private ObservableCollection<Direction> _directionList;
        private ObservableCollection<Status> _statusList;
        private ICollectionView _directionView;
        private ICollectionView _statusView;
        private readonly DirectionContext _db;
        private readonly StatusContext _dbstatus;

        private string _searchText = "";
        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                OnPropertyChanged(nameof(SearchText));
                DirectionsView?.Refresh(); // Обновляем отображение при изменении текста
                StatusesView?.Refresh();
            }
        }

        public ObservableCollection<Direction> DirectionItems
        {
            get => _directionList;
            set
            {
                _directionList = value;
                OnPropertyChanged(nameof(DirectionItems));
            }
        }
        public ObservableCollection<Status> StatusItems
        {
            get => _statusList;
            set
            {
                _statusList = value;
                OnPropertyChanged(nameof(StatusItems));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        public ICollectionView DirectionsView => _directionView;
        public ICollectionView StatusesView => _statusView;

        public DirectionStatusPage()
        {
            InitializeComponent();
            _db = new DirectionContext();
            _dbstatus = new StatusContext();

            // Создаем пустые коллекции
            _directionList = new ObservableCollection<Direction>();
            _statusList = new ObservableCollection<Status>();

            _directionView = CollectionViewSource.GetDefaultView(_directionList);
            DirectionsView.Filter = FilterDirections;

            _statusView = CollectionViewSource.GetDefaultView(_statusList);
            StatusesView.Filter = FilterStatuses;

            _ = LoadDirectionStatusesAsync();
            this.DataContext = this;
        }
        private bool FilterDirections(object item)
        {
            if (item is Direction direction)
            {
                return direction.NameDirection.Contains(SearchText, StringComparison.OrdinalIgnoreCase);
            }
            return false;
        }

        private bool FilterStatuses(object item)
        {
            if (item is Status status)
            {
                return status.NameStatus.Contains(SearchText, StringComparison.OrdinalIgnoreCase);
            }
            return false;
        }

        private async Task LoadDirectionStatusesAsync()
        {
            // Загрузка направлений и статусов из базы данных
            var directions = await _db.Direction.ToListAsync();
            var statuses = await _dbstatus.Status.ToListAsync();

            // Добавление данных в ObservableCollection
            DirectionItems = new ObservableCollection<Direction>(directions);
            StatusItems = new ObservableCollection<Status>(statuses);

            // Обновляем представление
            _directionView = CollectionViewSource.GetDefaultView(DirectionItems);
            _directionView.Refresh(); // Обновление представления

            _statusView = CollectionViewSource.GetDefaultView(StatusItems);
            _statusView.Refresh(); // Обновление представления
        }

        private bool DirectionStatusFilter(object item)
        {
            if (string.IsNullOrEmpty(SearchTextBox.Text))
                return true;

            if (item is DirectionsAndStatuses directionStatus)
            {
                string searchQuery = SearchTextBox.Text.ToLower();
                return directionStatus.NameDirection.ToLower().Contains(searchQuery) ||
                       directionStatus.NameStatus.ToLower().Contains(searchQuery);
            }
            return false;
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Применить фильтрацию к данным направлений
            _directionView.Filter = item =>
            {
                if (item is Direction direction)
                {
                    return direction.NameDirection.ToLower().Contains(SearchTextBox.Text.ToLower());
                }
                return false;
            };

            // Применить фильтрацию к данным статусов
            _statusView.Filter = item =>
            {
                if (item is Status status)
                {
                    return status.NameStatus.ToLower().Contains(SearchTextBox.Text.ToLower());
                }
                return false;
            };

            _directionView.Refresh();
            _statusView.Refresh();
        }

        private void SearchTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(SearchTextBox.Text))
                SearchPlaceholder.Visibility = Visibility.Hidden;
        }

        private void SearchTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(SearchTextBox.Text))
                SearchPlaceholder.Visibility = Visibility.Visible;
        }

        private void EditItem_Click(object sender, RoutedEventArgs e)
        {

        }

        private async void DeleteItem_Click(object sender, RoutedEventArgs e)
        {
            if (DirectionsGrid.SelectedItem is Direction selectedDirection)
            {
                if (MessageBox.Show($"Удалить: {selectedDirection.NameDirection}?", "Удаление", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    _db.Direction.Remove(selectedDirection);
                    await _db.SaveChangesAsync();
                    DirectionItems.Remove(selectedDirection);
                }
            }
            else if (StatusesGrid.SelectedItem is Status selectedStatus)
            {
                bool isStatusUsed;
                using (var db = new EquipmentContext())
                {
                    isStatusUsed = await db.Equipment.AnyAsync(e => e.Status == selectedStatus.NameStatus);
                }

                if (isStatusUsed)
                {
                    MessageBox.Show("Этот статус используется в других модулях. Удаление может вызвать ошибки!",
                                    "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                }

                if (MessageBox.Show($"Удалить: {selectedStatus.NameStatus}?", "Удаление", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    _dbstatus.Status.Remove(selectedStatus);
                    await _dbstatus.SaveChangesAsync();
                    StatusItems.Remove(selectedStatus);
                }
            }
        }

        private async void AddNewRowButton_Click(object sender, RoutedEventArgs e)
        {
            if (MainTabControl.SelectedItem is TabItem selectedTab)
            {
                if (selectedTab.Header.ToString() == "Направления")
                {
                    var newItem = new Direction { NameDirection = "Новое направление" };
                    DirectionItems.Add(newItem);
                    await _db.Direction.AddAsync(newItem);
                    await _db.SaveChangesAsync();
                }
                else if (selectedTab.Header.ToString() == "Статусы")
                {
                    var newItem = new Status { NameStatus = "Новый статус" };
                    StatusItems.Add(newItem);
                    await _dbstatus.Status.AddAsync(newItem);
                    await _dbstatus.SaveChangesAsync();
                }
            }
        }

        private async void DirectionsGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (e.EditAction == DataGridEditAction.Commit && e.Row.Item is Direction editedItem)
            {
                await _db.SaveChangesAsync();
            }
        }

        private async void StatusesGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (e.EditAction == DataGridEditAction.Commit && e.Row.Item is Status editedItem)
            {
                await _dbstatus.SaveChangesAsync();
            }
        }
    }
}
