using System.Windows;
using t_project.Models;

namespace t_project.Views.Windows
{
    public partial class EditAuditoriumWindows : Window
    {
        public Auditorium Auditorium { get; private set; }
        public EditAuditoriumWindows(Auditorium auditorium)
        {
            InitializeComponent();
            Auditorium = auditorium;
            NameTextBox.Text = auditorium.Name;
            ShortNameTextBox.Text = auditorium.ShortName;
            ResponsibleUserTextBox.Text = auditorium.ResponsibleUser;
            TemporaryResponsibleUserTextBox.Text = auditorium.TemporaryResponsibleUser;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(NameTextBox.Text))
            {
                MessageBox.Show("Наименование аудитории не может быть пустым!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            Auditorium.Name = NameTextBox.Text;
            Auditorium.ShortName = ShortNameTextBox.Text;
            Auditorium.ResponsibleUser = ResponsibleUserTextBox.Text;
            Auditorium.TemporaryResponsibleUser = TemporaryResponsibleUserTextBox.Text;
            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
