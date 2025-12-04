using Microsoft.Win32;
using Shilenko_wpf1.Models;
using System;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace Shilenko_wpf1.Pages
{
    public partial class EmployeeEdit : Page
    {
        private Employees _currentEmployee;
        private AutobaseEntities _db;

        public EmployeeEdit(Employees employee)
        {
            InitializeComponent();
            _currentEmployee = employee ?? new Employees();
            _db = new AutobaseEntities();
            LoadPositions();
            LoadEmployeeData();
        }

        private void LoadPositions()
        {
            try
            {
                var positions = _db.EmployeePositions.ToList();
                cmbPosition.ItemsSource = positions;
                cmbPosition.DisplayMemberPath = "PositionName";
                cmbPosition.SelectedValuePath = "PositionID";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки должностей: {ex.Message}");
            }
        }

        private void LoadEmployeeData()
        {
            if (_currentEmployee.EmployeeID == 0) // Новый сотрудник
            {
                tbTitle.Text = "Добавление сотрудника";
                btnDelete.Visibility = Visibility.Collapsed;
                dpHireDate.SelectedDate = DateTime.Today;
            }
            else // Редактирование существующего
            {
                tbTitle.Text = "Редактирование сотрудника";
                btnDelete.Visibility = Visibility.Visible;

                // Загружаем данные сотрудника
                txtLastName.Text = _currentEmployee.LastName;
                txtFirstName.Text = _currentEmployee.FirstName;
                cmbPosition.SelectedValue = _currentEmployee.PositionID;
                dpHireDate.SelectedDate = _currentEmployee.HireDate;
                txtPhone.Text = _currentEmployee.Phone;
                txtEmail.Text = _currentEmployee.Email;

                // Загружаем изображение если есть
                // В данном примере просто используем заглушку
            }
        }

        private void btnSelectImage_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image files (*.png;*.jpeg;*.jpg)|*.png;*.jpeg;*.jpg|All files (*.*)|*.*";

            if (openFileDialog.ShowDialog() == true)
            {
                txtImagePath.Text = openFileDialog.FileName;
                try
                {
                    BitmapImage bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(openFileDialog.FileName);
                    bitmap.EndInit();
                    imgPhoto.Source = bitmap;
                }
                catch
                {
                    MessageBox.Show("Не удалось загрузить изображение");
                }
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            StringBuilder errors = new StringBuilder();

            if (string.IsNullOrWhiteSpace(txtLastName.Text))
                errors.AppendLine("Введите фамилию");
            if (string.IsNullOrWhiteSpace(txtFirstName.Text))
                errors.AppendLine("Введите имя");
            if (cmbPosition.SelectedItem == null)
                errors.AppendLine("Выберите должность");
            if (dpHireDate.SelectedDate == null)
                errors.AppendLine("Выберите дату приёма");
            if (string.IsNullOrWhiteSpace(txtEmail.Text))
                errors.AppendLine("Введите email");

            if (errors.Length > 0)
            {
                MessageBox.Show(errors.ToString(), "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                // Заполняем данные сотрудника
                _currentEmployee.LastName = txtLastName.Text;
                _currentEmployee.FirstName = txtFirstName.Text;
                _currentEmployee.PositionID = (int)cmbPosition.SelectedValue;
                _currentEmployee.HireDate = dpHireDate.SelectedDate.Value;
                _currentEmployee.Phone = txtPhone.Text;
                _currentEmployee.Email = txtEmail.Text;

                if (_currentEmployee.EmployeeID == 0) // Новый сотрудник
                {
                    _db.Employees.Add(_currentEmployee);
                }
                else // Редактирование
                {
                    var employeeInDb = _db.Employees.Find(_currentEmployee.EmployeeID);
                    if (employeeInDb != null)
                    {
                        employeeInDb.LastName = _currentEmployee.LastName;
                        employeeInDb.FirstName = _currentEmployee.FirstName;
                        employeeInDb.PositionID = _currentEmployee.PositionID;
                        employeeInDb.HireDate = _currentEmployee.HireDate;
                        employeeInDb.Phone = _currentEmployee.Phone;
                        employeeInDb.Email = _currentEmployee.Email;
                    }
                }

                _db.SaveChanges();
                MessageBox.Show("Данные сохранены успешно", "Успех",
                    MessageBoxButton.OK, MessageBoxImage.Information);

                NavigationService.GoBack();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Вы действительно хотите удалить этого сотрудника?",
                "Подтверждение удаления", MessageBoxButton.YesNo,
                MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                try
                {
                    var employeeToDelete = _db.Employees.Find(_currentEmployee.EmployeeID);
                    if (employeeToDelete != null)
                    {
                        _db.Employees.Remove(employeeToDelete);
                        _db.SaveChanges();
                        MessageBox.Show("Сотрудник удален", "Успех",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                        NavigationService.GoBack();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка удаления: {ex.Message}", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }
    }
}