using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace DepartmentTerminal
{
    public partial class MainWindow : Window
    {
        private Employee _currentEmployee;
        private List<ApprovedRequestForDepartment> _allRequests;

        public MainWindow(Employee employee)
        {
            InitializeComponent();
            _currentEmployee = employee;
            txtWelcome.Text = $"Добро пожаловать, {employee.FullName}\nПодразделение: {employee.DepartmentName}";
            LoadRequests();
        }

        private void LoadRequests()
        {
            _allRequests = DatabaseHelper.GetApprovedRequestsForDepartment(_currentEmployee.DepartmentId.Value);
            ApplyFilter();
        }

        private void ApplyFilter()
        {
            if (_allRequests == null) return;

            var filtered = _allRequests.AsEnumerable();

            if (dpStartDate.SelectedDate != null)
            {
                DateOnly start = DateOnly.FromDateTime(dpStartDate.SelectedDate.Value);
                filtered = filtered.Where(r => r.StartDate >= start);
            }

            if (dpEndDate.SelectedDate != null)
            {
                DateOnly end = DateOnly.FromDateTime(dpEndDate.SelectedDate.Value);
                filtered = filtered.Where(r => r.StartDate <= end);
            }

            dgRequests.ItemsSource = filtered.ToList();
        }

        private void DpStartDate_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilter();
        }

        private void DpEndDate_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilter();
        }

        private void BtnClearFilter_Click(object sender, RoutedEventArgs e)
        {
            dpStartDate.SelectedDate = null;
            dpEndDate.SelectedDate = null;
            LoadRequests();
        }

        private void DgRequests_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgRequests.SelectedItem is ApprovedRequestForDepartment selected)
            {
                var detailWindow = new RequestDetailWindow(selected, _currentEmployee);
                detailWindow.ShowDialog();
                LoadRequests();
            }
        }

        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}