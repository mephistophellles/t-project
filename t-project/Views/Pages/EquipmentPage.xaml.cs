using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using t_project.DB.Context;
using t_project.Models;
using t_project.Views.Windows;

namespace t_project.Views // Необходимо добавить поиск
{
    public partial class EquipmentPage : Page
    {
        public ObservableCollection<Auditorium> Auditoriums;
        private ObservableCollection<Equipment> _equipmentList;
        private ICollectionView EquipmentView; // Представление коллекции с фильтрацией
        private readonly EquipmentContext _db;
        private readonly AuditoriumContext auditoriumContext;

        public ObservableCollection<Equipment> EquipmentItems
        {
            get => _equipmentList;
            set
            {
                _equipmentList = value;
                OnPropertyChanged(nameof(EquipmentItems));
            }
        }

        public EquipmentPage()
        {
            InitializeComponent();
            _db = new EquipmentContext();
            auditoriumContext = new AuditoriumContext();
            _ = LoadEquipmentAsync();
            DataContext = this;
        }

        private async Task LoadEquipmentAsync()
        {
            // Загрузка аудиторий
            var auditoriums = await auditoriumContext.Auditorium.AsNoTracking().ToListAsync();
            Auditoriums = new ObservableCollection<Auditorium>(auditoriums);
            // Загрузка оборудования
            var items = await _db.Equipment.AsNoTracking().ToListAsync();
            EquipmentItems = new ObservableCollection<Equipment>(items);

            // Применяем представление коллекции с фильтрацией
            EquipmentView = CollectionViewSource.GetDefaultView(EquipmentItems);
            EquipmentView.Filter = EquipmentFilter;
            EquipmentGrid.ItemsSource = EquipmentView;
        }
        private bool EquipmentFilter(object item)
        {
            if (string.IsNullOrEmpty(SearchTextBox.Text))
                return true;

            var equipment = item as Equipment;
            string searchQuery = SearchTextBox.Text.ToLower();

            return equipment != null &&
                   (equipment.Name?.ToLower().Contains(searchQuery) == true ||
                    equipment.EquipmentNumber?.ToLower().Contains(searchQuery) == true ||
                    equipment.RoomNumber?.ToLower().Contains(searchQuery) == true ||
                    equipment.ResponsibleUser?.ToLower().Contains(searchQuery) == true ||
                    equipment.TemporaryUser?.ToLower().Contains(searchQuery) == true ||
                    equipment.Direction?.ToLower().Contains(searchQuery) == true ||
                    equipment.Model?.ToLower().Contains(searchQuery) == true ||
                    equipment.Price.ToString().ToLower().Contains(searchQuery) ||
                    equipment.Comment?.ToLower().Contains(searchQuery) == true ||
                    equipment.Status?.ToLower().Contains(searchQuery) == true);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        private void EditItem_Click(object sender, RoutedEventArgs e)
        {
            if (EquipmentGrid.SelectedItem is Equipment selectedEquipment)
            {
                EditEquipmentWindow editWindow = new EditEquipmentWindow(selectedEquipment);
                if (editWindow.ShowDialog() == true)
                {
                    // Обновляем отображение и сохраняем в БД
                    EquipmentGrid.Items.Refresh();
                    SaveChangesToDatabase(selectedEquipment);
                }
            }
        }

        // Сохранение изменений в базу
        private void SaveChangesToDatabase(Equipment updatedEquipment)
        {
            if (!ValidateEquipment(updatedEquipment))
                return;

            using (var db = new EquipmentContext())
            {
                var existingEquipment = db.Equipment.Find(updatedEquipment.Id);
                if (existingEquipment != null)
                {
                    existingEquipment.Name = updatedEquipment.Name;
                    existingEquipment.EquipmentNumber = updatedEquipment.EquipmentNumber;
                    existingEquipment.RoomNumber = updatedEquipment.RoomNumber;
                    existingEquipment.ResponsibleUser = updatedEquipment.ResponsibleUser;
                    existingEquipment.TemporaryUser = updatedEquipment.TemporaryUser;
                    existingEquipment.Direction = updatedEquipment.Direction;
                    existingEquipment.Model = updatedEquipment.Model;
                    existingEquipment.Price = updatedEquipment.Price;
                    existingEquipment.Comment = updatedEquipment.Comment;
                    existingEquipment.Status = updatedEquipment.Status;
                    existingEquipment.ImagePath = updatedEquipment.ImagePath;

                    db.SaveChanges();
                }
            }
        }
        /// <summary>
        /// Валидация ввода данных
        /// </summary>
        /// <param name="equipment"></param>
        /// <returns></returns>
        private bool ValidateEquipment(Equipment equipment)
        {
            if (string.IsNullOrWhiteSpace(equipment.Name))
            {
                MessageBox.Show("Поле 'Наименование' обязательно для заполнения!",
                                "Ошибка валидации", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (string.IsNullOrWhiteSpace(equipment.EquipmentNumber))
            {
                MessageBox.Show("Поле 'Инвентарный номер' обязательно для заполнения!",
                                "Ошибка валидации", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (!Regex.IsMatch(equipment.EquipmentNumber, "^[0-9]+$"))
            {
                MessageBox.Show("Инвентарный номер должен содержать только цифры!", "Ошибка валидации", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (!decimal.TryParse(equipment.Price.ToString(), out _))
            {
                MessageBox.Show("Стоимость должна быть числом!", "Ошибка валидации", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            return true;
        }
        private async void DeleteItem_Click(object sender, RoutedEventArgs e)
        {
            if (EquipmentGrid.SelectedItem is Equipment selectedEquipment)
            {
                // Подтверждение удаления
                var result = MessageBox.Show($"Вы уверены, что хотите удалить оборудование: {selectedEquipment.Name}?",
                                              "Удаление оборудования",
                                              MessageBoxButton.YesNo,
                                              MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    // Удаляем элемент из коллекции
                    EquipmentItems.Remove(selectedEquipment);

                    // Удаляем элемент из базы данных
                    using (var db = new EquipmentContext())
                    {
                        var equipmentToDelete = await db.Equipment.FindAsync(selectedEquipment.Id);
                        if (equipmentToDelete != null)
                        {
                            db.Equipment.Remove(equipmentToDelete);
                            await db.SaveChangesAsync();
                        }
                    }
                }
            }
        }

        private void AddNewRowButton_Click(object sender, RoutedEventArgs e)
        {
            var newEquipment = new Equipment
            {
                Name = "Новое оборудование",
                EquipmentNumber = "12345",
                RoomNumber = "101",
                ResponsibleUser = "Иванов",
                TemporaryUser = "Сидоров",
                Direction = "IT",
                Model = "Model-X",
                Price = 1000,
                Comment = "Нет комментариев",
                Status = "Активен"
            };

            if (!ValidateEquipment(newEquipment))
                return;

            // Добавляем новую строку в коллекцию
            EquipmentItems.Add(newEquipment);

            // Сохраняем новую запись в базу данных
            using (var db = new EquipmentContext())
            {
                db.Equipment.Add(newEquipment);
                db.SaveChanges();
            }
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            EquipmentView.Refresh(); // Обновляем фильтр
        }

        private void SearchTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(SearchTextBox.Text))
            {
                SearchPlaceholder.Visibility = Visibility.Hidden;
            }
        }

        private void SearchTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(SearchTextBox.Text))
            {
                SearchPlaceholder.Visibility = Visibility.Visible;
            }
        }
    }
}
