using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using t_project.DB.Context;
using t_project.Models;
using t_project.Views.Windows;

namespace t_project.Views.Pages
{
    public partial class AuditoriumsPage : Page
    {
        private ObservableCollection<Auditorium> _auditoriumList;
        private ICollectionView AuditoriumView; // Представление коллекции с фильтрацией
        private readonly AuditoriumContext _db;

        public ObservableCollection<Auditorium> AuditoriumItems
        {
            get => _auditoriumList;
            set
            {
                _auditoriumList = value;
                OnPropertyChanged(nameof(AuditoriumItems));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public AuditoriumsPage()
        {
            InitializeComponent();
            _db = new AuditoriumContext();
            _ = LoadAuditoriumsAsync();
            DataContext = this;
        }
        private async Task LoadAuditoriumsAsync()
        {
            var items = await _db.Auditorium.AsNoTracking().ToListAsync();
            AuditoriumItems = new ObservableCollection<Auditorium>(items);
            AuditoriumView = CollectionViewSource.GetDefaultView(AuditoriumItems);
            AuditoriumView.Filter = AuditoriumFilter;
            AuditoriumsGrid.ItemsSource = AuditoriumView;
        }
        private bool AuditoriumFilter(object item)
        {
            if (string.IsNullOrEmpty(SearchTextBox.Text))
                return true;

            if (item is Auditorium auditorium)
            {
                string searchQuery = SearchTextBox.Text.ToLower();
                return (auditorium.Name?.ToLower().Contains(searchQuery) == true ||
                        auditorium.ShortName?.ToLower().Contains(searchQuery) == true ||
                        auditorium.ResponsibleUser?.ToLower().Contains(searchQuery) == true ||
                        auditorium.TemporaryResponsibleUser?.ToLower().Contains(searchQuery) == true);
            }

            return false;
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            AuditoriumView.Refresh(); // Обновляем фильтр
        }

        private void SearchTextBox_GotFocus(object sender, System.Windows.RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(SearchTextBox.Text))
            {
                SearchPlaceholder.Visibility = Visibility.Hidden;
            }
        }

        private void SearchTextBox_LostFocus(object sender, System.Windows.RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(SearchTextBox.Text))
            {
                SearchPlaceholder.Visibility = Visibility.Visible;
            }
        }

        private void EditItem_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (AuditoriumsGrid.SelectedItem is Auditorium selectedAuditorium)
            {
                EditAuditoriumWindows editWindow = new EditAuditoriumWindows(selectedAuditorium);
                if (editWindow.ShowDialog() == true)
                {
                    // Обновляем отображение и сохраняем в БД
                    AuditoriumsGrid.Items.Refresh();
                    SaveChangesToDatabase(selectedAuditorium);
                }
            }
        }
        private void SaveChangesToDatabase(Auditorium updatedAuditorium)
        {
            using (var db = new AuditoriumContext())
            {
                var existingAuditorium = db.Auditorium.Find(updatedAuditorium.Id);
                if (existingAuditorium != null)
                {
                    existingAuditorium.Name = updatedAuditorium.Name;
                    existingAuditorium.ShortName = updatedAuditorium.ShortName;
                    existingAuditorium.ResponsibleUser = updatedAuditorium.ResponsibleUser;
                    existingAuditorium.TemporaryResponsibleUser = updatedAuditorium.TemporaryResponsibleUser;
                    db.SaveChanges();
                }
            }
        }

        private async void DeleteItem_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (AuditoriumsGrid.SelectedItem is Auditorium selectedAuditorium)
            {
                // Подтверждение удаления
                var result = MessageBox.Show($"Вы уверены, что хотите удалить аудиторию: {selectedAuditorium.Name}?",
                                              "Удаление аудитории",
                                              MessageBoxButton.YesNo,
                                              MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    using (var db = new EquipmentContext())
                    {
                        // Проверяем, связано ли оборудование с этой аудиторией
                        bool isUsed = await db.Equipment.AnyAsync(e => e.RoomNumber == selectedAuditorium.ShortName);

                        if (isUsed)
                        {
                            MessageBox.Show("Внимание! Аудитория используется в оборудовании, но будет удалена.",
                            "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                        }
                    }
                    using (var db = new AuditoriumContext())
                    {
                        // Загружаем аудиторию для удаления
                        var auditoriumToDelete = await db.Auditorium.FindAsync(selectedAuditorium.Id);
                        if (auditoriumToDelete != null)
                        {
                            db.Auditorium.Remove(auditoriumToDelete);
                            await db.SaveChangesAsync();

                            // Удаляем из коллекции
                            AuditoriumItems.Remove(selectedAuditorium);
                        }
                    }

                }
            }
        }

        private void AddNewRowButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var newAuditorium = new Auditorium
            {
                Name = "123",
                ShortName = "123",
                ResponsibleUser = "123",
                TemporaryResponsibleUser = "123"
            };
            AuditoriumItems.Add(newAuditorium);
            // Сохраняем новую запись в базу данных
            using (var db = new AuditoriumContext())
            {
                db.Auditorium.Add(newAuditorium);
                db.SaveChanges();
            }
        }
    }
}
