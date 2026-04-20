using System.Windows;

namespace DepartmentTerminal
{
    public partial class AddToBlacklistWindow : Window
    {
        private string _fullName;
        private string _passportNumber;

        public AddToBlacklistWindow(string fullName, string passportNumber)
        {
            InitializeComponent();
            _fullName = fullName;
            _passportNumber = passportNumber;

            txtVisitorInfo.Text = $"Посетитель: {fullName}";
            txtPassport.Text = $"Паспорт: {passportNumber}";
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            string reason = txtReason.Text.Trim();

            if (string.IsNullOrWhiteSpace(reason))
            {
                MessageBox.Show("Укажите причину добавления в чёрный список!", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (reason.Length > 5000)
            {
                MessageBox.Show("Причина не должна превышать 5000 символов!", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            bool success = DatabaseHelper.AddToBlacklist(_passportNumber, _fullName, reason);

            if (success)
            {
                DialogResult = true;
                Close();
            }
            else
            {
                MessageBox.Show("Ошибка при добавлении в чёрный список!", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}