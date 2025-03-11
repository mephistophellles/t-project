using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Media.Imaging;
using t_project.DB.Context;
using t_project.Models;

namespace t_project.Views.Windows
{
    public partial class EditEquipmentWindow : Window
    {
        public Equipment Equipment { get; private set; }
        public ObservableCollection<Auditorium> Auditoriums { get; set; }

        public EditEquipmentWindow(Equipment equipment)
        {
            InitializeComponent();
            Equipment = equipment;

            // Заполняем поля данными
            NameTextBox.Text = Equipment.Name;
            NumberTextBox.Text = Equipment.EquipmentNumber;
            RoomComboBox.Text = Equipment.RoomNumber;
            ResponsibleTextBox.Text = Equipment.ResponsibleUser;
            TemporaryUserTextBox.Text = Equipment.TemporaryUser;
            DirectionTextBox.Text = Equipment.Direction;
            ModelTextBox.Text = Equipment.Model;
            PriceTextBox.Text = Equipment.Price.ToString();
            CommentTextBox.Text = Equipment.Comment;
            StatusComboBox.Text = Equipment.Status;

            // Загружаем изображение
            LoadImage();
            LoadAuditoriums(equipment.RoomNumber);
        }
        private async void LoadAuditoriums(string currentRoom)
        {
            using (var context = new AuditoriumContext())
            {
                var auditoriums = await context.Auditorium.ToListAsync();
                Auditoriums = new ObservableCollection<Auditorium>(auditoriums);
                Auditoriums = new ObservableCollection<Auditorium>(await context.Auditorium.ToListAsync());
                RoomComboBox.ItemsSource = Auditoriums;
                RoomComboBox.SelectedValue = auditoriums.FirstOrDefault(a => a.ShortName.ToString() == currentRoom)?.ShortName;
            }
        }
        private void LoadImage()
        {
            byte[] imageBytes = null;

            using (var db = new EquipmentContext())
            {
                var dbEquipment = db.Equipment.Find(Equipment.Id);
                if (dbEquipment != null && dbEquipment.ImagePath != null)
                {
                    imageBytes = dbEquipment.ImagePath;
                }
            }
            // Проверяем, есть ли изображение
            if (imageBytes != null && imageBytes.Length > 0)
            {
                try
                {
                    using (MemoryStream stream = new MemoryStream(Equipment.ImagePath))
                    {
                        BitmapImage bitmap = new BitmapImage();
                        bitmap.BeginInit();
                        bitmap.StreamSource = stream;
                        bitmap.CacheOption = BitmapCacheOption.OnLoad;
                        bitmap.EndInit();
                        bitmap.Freeze(); // Замораживаем изображение для безопасного использования в UI

                        EquipmentImage.Source = bitmap; // Обновляем изображение на главном потоке

                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка загрузки изображения: " + ex.Message);
                }
            }
            else
            {
                EquipmentImage.Source = null; // Если нет данных, очищаем изображение
            }
        }

        private void SelectImage_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Файлы изображений (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg";

            if (openFileDialog.ShowDialog() == true)
            {
                byte[] imageBytes = File.ReadAllBytes(openFileDialog.FileName);

                // Обновляем в БД
                using (var db = new EquipmentContext())
                {
                    var dbEquipment = db.Equipment.Find(Equipment.Id);
                    if (dbEquipment != null)
                    {
                        dbEquipment.ImagePath = imageBytes;
                        db.SaveChanges();
                    }
                }
                Equipment.ImagePath = imageBytes;
                // Обновляем отображение
                LoadImage();
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            // Сохраняем изменения
            Equipment.Name = NameTextBox.Text;
            Equipment.EquipmentNumber = NumberTextBox.Text;
            Equipment.RoomNumber = RoomComboBox.SelectedValue?.ToString(); // Сохраняем выбранную аудиторию
            Equipment.ResponsibleUser = ResponsibleTextBox.Text;
            Equipment.TemporaryUser = TemporaryUserTextBox.Text;
            Equipment.Direction = DirectionTextBox.Text;
            Equipment.Model = ModelTextBox.Text;
            Equipment.Price = decimal.TryParse(PriceTextBox.Text, out decimal price) ? price : 0;
            Equipment.Comment = CommentTextBox.Text;
            Equipment.Status = StatusComboBox.Text;

            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void OnlyNumbers_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            e.Handled = !Regex.IsMatch(e.Text, "^[0-9]+$"); // Только цифры
        }
    }
}
