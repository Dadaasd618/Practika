using System.Windows;
using System.Text.RegularExpressions;

namespace PractikaPM11
{
    public partial class RequestWindow : Window
    {
        private int _currentUserId;
        private string _scanPath = "";

        public RequestWindow(int userId)
        {
            InitializeComponent();
            _currentUserId = userId;
            LoadData();
        }

        private void LoadData()
        {
            try
            {
                // Загрузка подразделений
                var departments = DatabaseHelper.GetDepartments();
                if (departments != null && departments.Count > 0)
                {
                    cmbDepartment.ItemsSource = departments;
                }

                // Загрузка целей посещения
                var purposes = DatabaseHelper.GetPurposes();
                if (purposes != null && purposes.Count > 0)
                {
                    cmbPurpose.ItemsSource = purposes;
                }

                // Установка дат по умолчанию
                dpStartDate.SelectedDate = DateTime.Now.AddDays(1);
                dpEndDate.SelectedDate = DateTime.Now.AddDays(1);

                // Устанавливаем минимальные даты
                dpStartDate.DisplayDateStart = DateTime.Now.AddDays(1);
                dpEndDate.DisplayDateStart = DateTime.Now.AddDays(1);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CmbDepartment_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            try
            {
                if (cmbDepartment.SelectedItem != null)
                {
                    dynamic selected = cmbDepartment.SelectedItem;
                    int deptId = selected.DepartmentId;
                    var employees = DatabaseHelper.GetEmployeesByDepartment(deptId);
                    cmbEmployee.ItemsSource = employees;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки сотрудников: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CmbRequestType_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            try
            {
                bool isGroup = cmbRequestType.SelectedIndex == 1;
                if (borderGroup != null)
                {
                    borderGroup.Visibility = isGroup ? Visibility.Visible : Visibility.Collapsed;
                }
            }
            catch (Exception ex)
            {
                // Игнорируем
            }
        }

        private void DatePicker_SelectedDateChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            ValidateDates();
        }

        private void ValidateDates()
        {
            try
            {
                if (dpStartDate.SelectedDate == null || dpEndDate.SelectedDate == null) return;

                DateOnly startDate = DateOnly.FromDateTime(dpStartDate.SelectedDate.Value);
                DateOnly endDate = DateOnly.FromDateTime(dpEndDate.SelectedDate.Value);
                DateOnly minDate = DateOnly.FromDateTime(DateTime.Now.AddDays(1));
                DateOnly maxEndDate = startDate.AddDays(15);

                if (startDate < minDate)
                {
                    txtDateError.Text = $"Дата начала не может быть раньше {minDate}";
                    txtDateError.Visibility = Visibility.Visible;
                }
                else if (endDate < startDate)
                {
                    txtDateError.Text = "Дата окончания не может быть раньше даты начала";
                    txtDateError.Visibility = Visibility.Visible;
                }
                else if (endDate > maxEndDate)
                {
                    txtDateError.Text = $"Дата окончания не может быть позже {maxEndDate}";
                    txtDateError.Visibility = Visibility.Visible;
                }
                else
                {
                    txtDateError.Visibility = Visibility.Collapsed;
                }
            }
            catch (Exception ex)
            {
                // Игнорируем
            }
        }

        private void BtnSelectScan_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var dialog = new Microsoft.Win32.OpenFileDialog
                {
                    Filter = "PDF файлы (*.pdf)|*.pdf",
                    Title = "Выберите скан паспорта"
                };

                if (dialog.ShowDialog() == true)
                {
                    _scanPath = dialog.FileName;
                    txtScanPath.Text = System.IO.Path.GetFileName(_scanPath);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка выбора файла: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool ValidateForm()
        {
            try
            {
                // Проверка обязательных полей
                if (string.IsNullOrWhiteSpace(txtLastName.Text))
                {
                    ShowError("Введите фамилию!");
                    return false;
                }
                if (string.IsNullOrWhiteSpace(txtFirstName.Text))
                {
                    ShowError("Введите имя!");
                    return false;
                }
                if (string.IsNullOrWhiteSpace(txtEmail.Text) || !txtEmail.Text.Contains("@"))
                {
                    ShowError("Введите корректный email!");
                    return false;
                }
                if (dpBirthDate.SelectedDate == null)
                {
                    ShowError("Введите дату рождения!");
                    return false;
                }

                // Проверка возраста (не моложе 16 лет)
                var birthDate = DateOnly.FromDateTime(dpBirthDate.SelectedDate.Value);
                var today = DateOnly.FromDateTime(DateTime.Now);
                int age = today.Year - birthDate.Year;
                if (birthDate > today.AddYears(-age)) age--;

                if (age < 16)
                {
                    ShowError("Посетитель должен быть не моложе 16 лет!");
                    return false;
                }

                // Проверка паспорта
                if (txtPassportSeries.Text.Length != 4 || !int.TryParse(txtPassportSeries.Text, out _))
                {
                    ShowError("Серия паспорта должна содержать 4 цифры!");
                    return false;
                }
                if (txtPassportNumber.Text.Length != 6 || !int.TryParse(txtPassportNumber.Text, out _))
                {
                    ShowError("Номер паспорта должен содержать 6 цифр!");
                    return false;
                }

                // Проверка скана
                if (string.IsNullOrEmpty(_scanPath))
                {
                    ShowError("Прикрепите скан паспорта (PDF)!");
                    return false;
                }

                // Проверка примечания
                if (string.IsNullOrWhiteSpace(txtComment.Text))
                {
                    ShowError("Введите примечание!");
                    return false;
                }

                // Проверка дат
                if (txtDateError.Visibility == Visibility.Visible)
                {
                    ShowError("Исправьте ошибки в датах!");
                    return false;
                }

                // Проверка выбора подразделения и сотрудника
                if (cmbDepartment.SelectedItem == null)
                {
                    ShowError("Выберите подразделение!");
                    return false;
                }
                if (cmbEmployee.SelectedItem == null)
                {
                    ShowError("Выберите сотрудника!");
                    return false;
                }

                // Для групповой заявки проверяем количество
                if (cmbRequestType.SelectedIndex == 1)
                {
                    if (!int.TryParse(txtGroupCount.Text, out int count) || count < 5)
                    {
                        ShowError("Групповая заявка должна содержать не менее 5 человек!");
                        return false;
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                ShowError($"Ошибка валидации: {ex.Message}");
                return false;
            }
        }

        private void ShowError(string message)
        {
            txtError.Text = message;
            txtError.Visibility = Visibility.Visible;
        }

        private void BtnSubmit_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                txtError.Visibility = Visibility.Collapsed;

                if (!ValidateForm()) return;

                string type = cmbRequestType.SelectedIndex == 0 ? "individual" : "group";
                DateOnly startDate = DateOnly.FromDateTime(dpStartDate.SelectedDate.Value);
                DateOnly endDate = DateOnly.FromDateTime(dpEndDate.SelectedDate.Value);
                int purposeId = (int)cmbPurpose.SelectedValue;
                int departmentId = (int)cmbDepartment.SelectedValue;
                int employeeId = (int)cmbEmployee.SelectedValue;
                string comment = txtComment.Text;

                // Создаём заявку
                int requestId = DatabaseHelper.CreateRequest(_currentUserId, type, startDate, endDate,
                    purposeId, departmentId, employeeId, comment);

                if (requestId > 0)
                {
                    // Создаём посетителя и связываем с заявкой
                    var visitor = new Visitor
                    {
                        LastName = txtLastName.Text,
                        FirstName = txtFirstName.Text,
                        MiddleName = string.IsNullOrWhiteSpace(txtMiddleName.Text) ? null : txtMiddleName.Text,
                        Phone = string.IsNullOrWhiteSpace(txtPhone.Text) ? null : txtPhone.Text,
                        Email = txtEmail.Text,
                        Organization = string.IsNullOrWhiteSpace(txtOrganization.Text) ? null : txtOrganization.Text,
                        BirthDate = DateOnly.FromDateTime(dpBirthDate.SelectedDate.Value),
                        PassportSeries = txtPassportSeries.Text,
                        PassportNumber = txtPassportNumber.Text,
                        PassportScanPath = _scanPath
                    };

                    bool success = DatabaseHelper.CreateVisitorAndIndividualRequest(requestId, visitor);

                    if (success)
                    {
                        MessageBox.Show("Заявка успешно отправлена!", "Успех",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                        this.Close();
                    }
                    else
                    {
                        MessageBox.Show("Ошибка при сохранении данных посетителя!", "Ошибка",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                else
                {
                    MessageBox.Show("Ошибка при создании заявки!", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}