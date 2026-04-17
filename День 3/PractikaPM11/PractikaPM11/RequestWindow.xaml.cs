using System.Windows;
using System.Text.RegularExpressions;
using Microsoft.Win32;

namespace PractikaPM11
{
    public partial class RequestWindow : Window
    {
        private int _currentUserId;
        private string _scanPath = "";
        private List<GroupMember> _groupMembers = new List<GroupMember>();

        public RequestWindow(int userId)
        {
            InitializeComponent();
            _currentUserId = userId;
            LoadData();
            dgGroupMembers.ItemsSource = _groupMembers;
        }

        private void LoadData()
        {
            try
            {
                var departments = DatabaseHelper.GetDepartments();
                if (departments != null && departments.Count > 0)
                    cmbDepartment.ItemsSource = departments;

                var purposes = DatabaseHelper.GetPurposes();
                if (purposes != null && purposes.Count > 0)
                    cmbPurpose.ItemsSource = purposes;

                dpStartDate.SelectedDate = DateTime.Now.AddDays(1);
                dpEndDate.SelectedDate = DateTime.Now.AddDays(1);
                dpStartDate.DisplayDateStart = DateTime.Now.AddDays(1);
                dpEndDate.DisplayDateStart = DateTime.Now.AddDays(1);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}");
            }
        }

        private void CmbDepartment_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (cmbDepartment.SelectedItem != null)
            {
                dynamic selected = cmbDepartment.SelectedItem;
                int deptId = selected.DepartmentId;
                var employees = DatabaseHelper.GetEmployeesByDepartment(deptId);
                cmbEmployee.ItemsSource = employees;
            }
        }

        private void CmbRequestType_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            bool isGroup = cmbRequestType.SelectedIndex == 1;

            if (borderIndividual != null)
                borderIndividual.Visibility = isGroup ? Visibility.Collapsed : Visibility.Visible;

            if (borderGroup != null)
                borderGroup.Visibility = isGroup ? Visibility.Visible : Visibility.Collapsed;
        }

        private void DatePicker_SelectedDateChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            ValidateDates();
        }

        private void ValidateDates()
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

        private void BtnSelectScan_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog { Filter = "PDF файлы (*.pdf)|*.pdf", Title = "Выберите скан паспорта" };
            if (dialog.ShowDialog() == true)
            {
                _scanPath = dialog.FileName;
                txtScanPath.Text = System.IO.Path.GetFileName(_scanPath);
            }
        }

        private void BtnAddMember_Click(object sender, RoutedEventArgs e)
        {
            var memberWindow = new MemberWindow();
            if (memberWindow.ShowDialog() == true && memberWindow.NewMember != null)
            {
                _groupMembers.Add(memberWindow.NewMember);
                RefreshGroupGrid();
            }
        }

        private void BtnRemoveMember_Click(object sender, RoutedEventArgs e)
        {
            if (dgGroupMembers.SelectedItem is GroupMember selected)
            {
                _groupMembers.Remove(selected);
                RefreshGroupGrid();
            }
        }

        private void RefreshGroupGrid()
        {
            dgGroupMembers.ItemsSource = null;
            dgGroupMembers.ItemsSource = _groupMembers;
            ValidateGroup();
        }

        private void ValidateGroup()
        {
            if (cmbRequestType.SelectedIndex == 1)
            {
                if (_groupMembers.Count < 5)
                {
                    txtGroupError.Text = $"Минимум 5 участников. Добавлено: {_groupMembers.Count}";
                    btnSubmit.IsEnabled = false;
                }
                else
                {
                    txtGroupError.Text = "";
                    btnSubmit.IsEnabled = true;
                }
            }
            else
            {
                btnSubmit.IsEnabled = true;
            }
        }

        private bool ValidateIndividualForm()
        {
            if (string.IsNullOrWhiteSpace(txtLastName.Text)) { ShowError("Введите фамилию!"); return false; }
            if (string.IsNullOrWhiteSpace(txtFirstName.Text)) { ShowError("Введите имя!"); return false; }
            if (string.IsNullOrWhiteSpace(txtEmail.Text) || !txtEmail.Text.Contains("@")) { ShowError("Введите корректный email!"); return false; }
            if (dpBirthDate.SelectedDate == null) { ShowError("Введите дату рождения!"); return false; }

            var birthDate = DateOnly.FromDateTime(dpBirthDate.SelectedDate.Value);
            var today = DateOnly.FromDateTime(DateTime.Now);
            int age = today.Year - birthDate.Year;
            if (birthDate > today.AddYears(-age)) age--;
            if (age < 16) { ShowError("Посетитель должен быть не моложе 16 лет!"); return false; }

            if (txtPassportSeries.Text.Length != 4 || !int.TryParse(txtPassportSeries.Text, out _))
            { ShowError("Серия паспорта должна содержать 4 цифры!"); return false; }
            if (txtPassportNumber.Text.Length != 6 || !int.TryParse(txtPassportNumber.Text, out _))
            { ShowError("Номер паспорта должен содержать 6 цифр!"); return false; }

            if (string.IsNullOrEmpty(_scanPath)) { ShowError("Прикрепите скан паспорта (PDF)!"); return false; }
            if (string.IsNullOrWhiteSpace(txtComment.Text)) { ShowError("Введите примечание!"); return false; }
            if (txtDateError.Visibility == Visibility.Visible) { ShowError("Исправьте ошибки в датах!"); return false; }

            return true;
        }

        private bool ValidateGroupForm()
        {
            if (_groupMembers.Count < 5) { ShowError($"Минимум 5 участников. Добавлено: {_groupMembers.Count}"); return false; }
            if (string.IsNullOrWhiteSpace(txtGroupComment.Text)) { ShowError("Введите примечание!"); return false; }
            if (txtDateError.Visibility == Visibility.Visible) { ShowError("Исправьте ошибки в датах!"); return false; }
            if (cmbDepartment.SelectedItem == null) { ShowError("Выберите подразделение!"); return false; }
            if (cmbEmployee.SelectedItem == null) { ShowError("Выберите сотрудника!"); return false; }
            if (cmbPurpose.SelectedItem == null) { ShowError("Выберите цель посещения!"); return false; }

            return true;
        }

        private void ShowError(string message)
        {
            txtError.Text = message;
            txtError.Visibility = Visibility.Visible;
        }

        private void BtnSubmit_Click(object sender, RoutedEventArgs e)
        {
            txtError.Visibility = Visibility.Collapsed;

            try
            {
                bool isGroup = cmbRequestType.SelectedIndex == 1;

                if (cmbDepartment.SelectedItem == null) { ShowError("Выберите подразделение!"); return; }
                if (cmbEmployee.SelectedItem == null) { ShowError("Выберите сотрудника!"); return; }
                if (cmbPurpose.SelectedItem == null) { ShowError("Выберите цель посещения!"); return; }
                if (dpStartDate.SelectedDate == null) { ShowError("Выберите дату начала!"); return; }
                if (dpEndDate.SelectedDate == null) { ShowError("Выберите дату окончания!"); return; }

                if (!isGroup && !ValidateIndividualForm()) return;
                if (isGroup && !ValidateGroupForm()) return;

                string type = isGroup ? "group" : "individual";
                DateOnly startDate = DateOnly.FromDateTime(dpStartDate.SelectedDate.Value);
                DateOnly endDate = DateOnly.FromDateTime(dpEndDate.SelectedDate.Value);

                dynamic purpose = cmbPurpose.SelectedItem;
                int purposeId = purpose.PurposeId;

                dynamic dept = cmbDepartment.SelectedItem;
                int departmentId = dept.DepartmentId;

                dynamic emp = cmbEmployee.SelectedItem;
                int employeeId = emp.EmployeeId;

                string comment = isGroup ? txtGroupComment.Text : txtComment.Text;
                int method = cmbCreateMethod.SelectedIndex;
                bool success = false;

                if (!isGroup)
                {
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
                        PassportScanPath = _scanPath ?? ""
                    };

                    switch (method)
                    {
                        case 0:
                            int requestId = DatabaseHelper.CreateRequest(_currentUserId, type, startDate, endDate,
                                purposeId, departmentId, employeeId, comment);
                            success = DatabaseHelper.CreateVisitorAndIndividualRequest(requestId, visitor);
                            break;
                        case 1:
                            MessageBox.Show("Хранимая процедура для индивидуальной заявки в разработке");
                            return;
                        case 2:
                            success = DatabaseHelper.CreateIndividualRequestORM(_currentUserId, type, startDate, endDate,
                                purposeId, departmentId, employeeId, comment, visitor);
                            break;
                    }
                }
                else
                {
                    switch (method)
                    {
                        case 0:
                            success = DatabaseHelper.CreateGroupRequest(_currentUserId, type, startDate, endDate,
                                purposeId, departmentId, employeeId, comment, _groupMembers);
                            break;
                        case 1:
                            MessageBox.Show("Хранимая процедура для групповой заявки в разработке");
                            return;
                        case 2:
                            success = DatabaseHelper.CreateGroupRequestORM(_currentUserId, type, startDate, endDate,
                                purposeId, departmentId, employeeId, comment, _groupMembers);
                            break;
                    }
                }

                if (success)
                {
                    MessageBox.Show("Заявка успешно отправлена!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    Close();
                }
                else
                {
                    MessageBox.Show("Ошибка при создании заявки!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}