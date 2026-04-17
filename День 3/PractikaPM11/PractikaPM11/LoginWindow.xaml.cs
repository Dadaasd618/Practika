using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;

namespace PractikaPM11
{
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
        }

        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            string email = txtLoginEmail.Text;
            string password = txtLoginPassword.Password;
            int method = cmbLoginMethod.SelectedIndex;

            int userId = -1;

            switch (method)
            {
                case 0:
                    userId = DatabaseHelper.LoginSQL(email, password);
                    break;
                case 1:
                    userId = DatabaseHelper.LoginProcedure(email, password);
                    break;
                case 2:
                    var user = DatabaseHelper.LoginORM(email, password);
                    userId = user?.UserId ?? -1;
                    break;
            }

            if (userId > 0)
            {
                var mainWindow = new MainWindow(userId);
                mainWindow.Show();
                this.Close();
            }
            else
            {
                MessageBox.Show("Неверный email или пароль!", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnRegister_Click(object sender, RoutedEventArgs e)
        {
            string email = txtRegEmail.Text;
            string password = txtRegPassword.Password;
            string confirmPassword = txtRegConfirmPassword.Password;
            int method = cmbRegMethod.SelectedIndex;

            // Проверка пароля
            var passwordRegex = new Regex(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$");

            if (!passwordRegex.IsMatch(password))
            {
                MessageBox.Show("Пароль должен содержать минимум 8 символов,\n" +
                                "заглавную и строчную буквы, цифру и спецсимвол (@$!%*?&)",
                                "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (password != confirmPassword)
            {
                MessageBox.Show("Пароли не совпадают!", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(email) || !email.Contains("@"))
            {
                MessageBox.Show("Введите корректный email!", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            bool success = false;

            switch (method)
            {
                case 0:
                    success = DatabaseHelper.RegisterSQL(email, password);
                    break;
                case 1:
                    success = DatabaseHelper.RegisterProcedure(email, password);
                    break;
                case 2:
                    success = DatabaseHelper.RegisterORM(email, password);
                    break;
            }

            if (success)
            {
                MessageBox.Show("Регистрация успешна! Теперь вы можете войти.",
                    "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                // Очистка полей регистрации
                txtRegEmail.Clear();
                txtRegPassword.Clear();
                txtRegConfirmPassword.Clear();

                // ИСПРАВЛЕННЫЙ СПОСОБ ПЕРЕКЛЮЧЕНИЯ НА ВКЛАДКУ "ВХОД"
                // Находим родительский TabControl через визуальное дерево
                var tabControl = FindParent<TabControl>(cmbRegMethod);
                if (tabControl != null)
                {
                    tabControl.SelectedIndex = 0; // переключаем на вкладку "Вход"
                }

                // Очистка полей входа
                txtLoginEmail.Clear();
                txtLoginPassword.Clear();
            }
            else
            {
                MessageBox.Show("Ошибка регистрации. Возможно, email уже существует.",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void BtnGeneralDepartment_Click(object sender, RoutedEventArgs e)
        {
            var loginWindow = new GeneralDepartmentLoginWindow();
            loginWindow.ShowDialog();
        }
        // Вспомогательный метод для поиска родительского элемента
        private T FindParent<T>(DependencyObject child) where T : DependencyObject
        {
            DependencyObject parent = System.Windows.Media.VisualTreeHelper.GetParent(child);

            while (parent != null && !(parent is T))
            {
                parent = System.Windows.Media.VisualTreeHelper.GetParent(parent);
            }

            return parent as T;
        }
    }
}