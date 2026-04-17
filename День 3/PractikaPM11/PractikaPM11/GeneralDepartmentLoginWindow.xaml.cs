using System.Windows;

namespace PractikaPM11
{
    public partial class GeneralDepartmentLoginWindow : Window
    {
        public GeneralDepartmentLoginWindow()
        {
            InitializeComponent();
        }

        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            string employeeCode = txtEmployeeCode.Text.Trim();

            if (string.IsNullOrWhiteSpace(employeeCode))
            {
                MessageBox.Show("Введите код сотрудника!", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(employeeCode, out int code))
            {
                MessageBox.Show("Код сотрудника должен состоять из цифр!", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var employee = DatabaseHelper.GetEmployeeByCode(code);

            if (employee != null && employee.Section == "Общий отдел")
            {
                var mainWindow = new GeneralDepartmentWindow(employee);
                mainWindow.Show();
                this.Close();
            }
            else
            {
                MessageBox.Show("Код сотрудника не найден или сотрудник не относится к общему отделу!",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}