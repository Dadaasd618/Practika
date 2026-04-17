using System.Windows;

namespace PractikaPM11
{
    public partial class MemberWindow : Window
    {
        public GroupMember NewMember { get; private set; }

        public MemberWindow()
        {
            InitializeComponent();
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            // Проверка обязательных полей
            if (string.IsNullOrWhiteSpace(txtLastName.Text))
            {
                MessageBox.Show("Введите фамилию!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (string.IsNullOrWhiteSpace(txtFirstName.Text))
            {
                MessageBox.Show("Введите имя!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (string.IsNullOrWhiteSpace(txtEmail.Text) || !txtEmail.Text.Contains("@"))
            {
                MessageBox.Show("Введите корректный email!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (dpBirthDate.SelectedDate == null)
            {
                MessageBox.Show("Введите дату рождения!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Проверка возраста (16+)
            var birthDate = DateOnly.FromDateTime(dpBirthDate.SelectedDate.Value);
            var today = DateOnly.FromDateTime(DateTime.Now);
            int age = today.Year - birthDate.Year;
            if (birthDate > today.AddYears(-age)) age--;

            if (age < 16)
            {
                MessageBox.Show("Посетитель должен быть не моложе 16 лет!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Проверка паспорта
            if (txtPassportSeries.Text.Length != 4 || !int.TryParse(txtPassportSeries.Text, out _))
            {
                MessageBox.Show("Серия паспорта должна содержать 4 цифры!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (txtPassportNumber.Text.Length != 6 || !int.TryParse(txtPassportNumber.Text, out _))
            {
                MessageBox.Show("Номер паспорта должен содержать 6 цифр!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Создаём участника
            NewMember = new GroupMember
            {
                LastName = txtLastName.Text,
                FirstName = txtFirstName.Text,
                MiddleName = string.IsNullOrWhiteSpace(txtMiddleName.Text) ? null : txtMiddleName.Text,
                Phone = string.IsNullOrWhiteSpace(txtPhone.Text) ? null : txtPhone.Text,
                Email = txtEmail.Text,
                BirthDate = DateOnly.FromDateTime(dpBirthDate.SelectedDate.Value),
                PassportSeries = txtPassportSeries.Text,
                PassportNumber = txtPassportNumber.Text
            };

            DialogResult = true;
            Close();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}