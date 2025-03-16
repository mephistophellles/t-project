using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
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
    /// <summary>
    /// Логика взаимодействия для TypePage.xaml
    /// </summary>
    public partial class TypePage : Page
    {
        private ObservableCollection<EquipmentType> _typeList;
        private ICollectionView TypeView; // Представление коллекции с фильтрацией
        private readonly TypeContext _db;
        public ObservableCollection<EquipmentType> TypesItems
        {
            get => _typeList;
            set
            {
                _typeList = value;
                OnPropertyChanged(nameof(TypesItems));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public TypePage()
        {
            InitializeComponent();
            _db = new TypeContext();

        }
        private async Task LoadTypesAsync()
        {
            var items = await _db.Type.AsNoTracking().ToListAsync();
            TypesItems = new ObservableCollection<EquipmentType>(items);
            TypeView = CollectionViewSource.GetDefaultView(TypesItems);
            TypeView.Filter = TypeFilter;
            TypesGrid.ItemsSource = TypeView;
        }
        private bool TypeFilter(object item)
        {
            if (string.IsNullOrEmpty(SearchTextBox.Text))
                return true;

            if (item is EquipmentType type)
            {
                string searchQuery = SearchTextBox.Text.ToLower();
                return (type.NameType?.ToLower().Contains(searchQuery) == true ||
                        type.Id.ToString().ToLower().Contains(searchQuery) == true);
            }

            return false;
        }

        private void AddNewRowButton_Click(object sender, RoutedEventArgs e)
        {
            var newType = new EquipmentType
            {
                NameType = "Персональный компьютер"
            };
            TypesItems.Add(newType);
            // Сохраняем новую запись в базу данных
            using (var db = new TypeContext())
            {
                db.Type.Add(newType);
                db.SaveChanges();
            }
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TypeView.Refresh();
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

        private void EditItem_Click(object sender, RoutedEventArgs e)
        {

        }

        private void DeleteItem_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
