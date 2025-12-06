using Microsoft.Win32;
using Shilenko_wpf1.Models;
using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace Shilenko_wpf1.Pages
{
    // Страница для добавления и редактирования сотрудников
    public partial class EmployeeEdit : Page
    {
        private Employees _employee;    // Текущий сотрудник
        private string _imagePath;      // Путь к выбранному изображению

        // Конструктор страницы
        public EmployeeEdit(Employees employee)
        {
            InitializeComponent();
            _employee = employee ?? new Employees(); // Новый сотрудник если null
            LoadPositions();     // Загрузка списка должностей
            LoadEmployeeData();  // Загрузка данных сотрудника
        }

        // ==================== МЕТОДЫ ЗАГРУЗКИ ДАННЫХ ====================

        // Загрузка списка должностей из базы данных
        private void LoadPositions()
        {
            try
            {
                using (var db = new AutobaseEntities())
                {
                    cmbPosition.ItemsSource = db.EmployeePositions.ToList(); // Привязка к ComboBox
                    cmbPosition.DisplayMemberPath = "PositionName";   // Отображаемое поле
                    cmbPosition.SelectedValuePath = "PositionID";     // Значение поля
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки должностей: {ex.Message}");
            }
        }

        // Загрузка данных сотрудника в форму
        private void LoadEmployeeData()
        {
            bool isNew = _employee.EmployeeID == 0; // Проверка нового сотрудника

            // Настройка заголовка и видимости кнопок
            tbTitle.Text = isNew ? "Добавление сотрудника" : "Редактирование сотрудника";
            btnDelete.Visibility = isNew ? Visibility.Collapsed : Visibility.Visible; // Кнопка удаления только для существующих
            dpHireDate.SelectedDate = isNew ? DateTime.Today : _employee.HireDate; // Дата по умолчанию

            // Заполнение полей для существующего сотрудника
            if (!isNew)
            {
                txtLastName.Text = _employee.LastName;
                txtFirstName.Text = _employee.FirstName;
                cmbPosition.SelectedValue = _employee.PositionID;
                txtPhone.Text = _employee.Phone;
                txtEmail.Text = _employee.Email;
            }

            LoadImage(); // Загрузка фотографии
        }

        // ==================== МЕТОДЫ РАБОТЫ С ИЗОБРАЖЕНИЯМИ ====================

        // Загрузка изображения сотрудника
        private void LoadImage()
        {
            if (!string.IsNullOrEmpty(_employee.PhotoPath) && File.Exists(GetImageFullPath()))
            {
                SetImageSource(GetImageFullPath()); // Загрузка изображения
                txtImagePath.Text = _employee.PhotoPath;
            }
            else
            {
                SetDefaultImage(); // Установка изображения по умолчанию
                txtImagePath.Text = string.IsNullOrEmpty(_employee.PhotoPath) ?
                    "Изображение не сохранено" : "Файл не найден";
            }
        }

        // Установка источника изображения
        private void SetImageSource(string path)
        {
            try
            {
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(path);
                bitmap.CacheOption = BitmapCacheOption.OnLoad; // Загрузка в память
                bitmap.EndInit();
                bitmap.Freeze(); // Заморозка для использования в UI
                imgPhoto.Source = bitmap;
            }
            catch
            {
                SetDefaultImage(); // При ошибке - изображение по умолчанию
            }
        }

        // Установка изображения по умолчанию
        private void SetDefaultImage()
        {
            try
            {
                imgPhoto.Source = new BitmapImage(new Uri("pack://application:,,,/Resources/picture.png"));
            }
            catch
            {
                imgPhoto.Source = null; // Если изображение не найдено
            }
        }

        // ==================== МЕТОДЫ РАБОТЫ С ФАЙЛАМИ ====================

        // Получение пути к папке с изображениями
        private string GetImagesFolder()
        {
            string folder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "EmployeeImages");
            if (!Directory.Exists(folder)) Directory.CreateDirectory(folder); // Создание если не существует
            return folder;
        }

        // Получение полного пути к изображению
        private string GetImageFullPath()
        {
            return Path.Combine(GetImagesFolder(), _employee.PhotoPath);
        }

        // ==================== ОБРАБОТЧИКИ СОБЫТИЙ ====================

        // Обработчик выбора изображения
        private void btnSelectImage_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Filter = "Изображения (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg|Все файлы|*.*",
                Title = "Выберите фотографию"
            };

            if (dialog.ShowDialog() == true)
            {
                _imagePath = dialog.FileName; // Сохранение пути
                txtImagePath.Text = _imagePath; // Отображение пути
                SetImageSource(_imagePath); // Отображение изображения
            }
        }

        // Обработчик кнопки сохранения
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateForm()) return; // Валидация формы

            try
            {
                UpdateEmployeeData();    // Обновление данных объекта
                SaveEmployee();         // Сохранение в базу данных
                SaveImageIfNeeded();    // Сохранение изображения
                MessageBox.Show("Данные сохранены", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                NavigationService.GoBack(); // Возврат на предыдущую страницу
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Обработчик кнопки удаления
        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Удалить сотрудника?", "Подтверждение",
                MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                try
                {
                    using (var db = new AutobaseEntities())
                    {
                        var employee = db.Employees.Find(_employee.EmployeeID);
                        if (employee != null)
                        {
                            db.Employees.Remove(employee); // Удаление из базы
                            db.SaveChanges();
                        }
                    }
                    MessageBox.Show("Сотрудник удален", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    NavigationService.GoBack();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка удаления: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        // Обработчик кнопки отмены
        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack(); // Возврат без сохранения
        }

        // ==================== МЕТОДЫ ВАЛИДАЦИИ И СОХРАНЕНИЯ ====================

        // Валидация данных формы
        private bool ValidateForm()
        {
            if (string.IsNullOrWhiteSpace(txtLastName.Text) || string.IsNullOrWhiteSpace(txtFirstName.Text) ||
                cmbPosition.SelectedItem == null || dpHireDate.SelectedDate == null ||
                string.IsNullOrWhiteSpace(txtEmail.Text))
            {
                MessageBox.Show("Заполните все обязательные поля", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            return true;
        }

        // Обновление данных объекта сотрудника из формы
        private void UpdateEmployeeData()
        {
            _employee.LastName = txtLastName.Text;
            _employee.FirstName = txtFirstName.Text;
            _employee.PositionID = (int)cmbPosition.SelectedValue;
            _employee.HireDate = dpHireDate.SelectedDate.Value;
            _employee.Phone = txtPhone.Text;
            _employee.Email = txtEmail.Text;
        }

        // Сохранение сотрудника в базу данных
        private void SaveEmployee()
        {
            using (var db = new AutobaseEntities())
            {
                if (_employee.EmployeeID == 0) // Добавление нового
                {
                    db.Employees.Add(_employee);
                }
                else // Обновление существующего
                {
                    var existing = db.Employees.Find(_employee.EmployeeID);
                    if (existing != null)
                    {
                        existing.LastName = _employee.LastName;
                        existing.FirstName = _employee.FirstName;
                        existing.PositionID = _employee.PositionID;
                        existing.HireDate = _employee.HireDate;
                        existing.Phone = _employee.Phone;
                        existing.Email = _employee.Email;
                        existing.PhotoPath = _employee.PhotoPath;
                    }
                }
                db.SaveChanges(); // Сохранение изменений
            }
        }

        // Сохранение изображения если оно было выбрано
        private void SaveImageIfNeeded()
        {
            if (!string.IsNullOrEmpty(_imagePath) && File.Exists(_imagePath))
            {
                string fileName = $"emp_{_employee.EmployeeID}_{Guid.NewGuid():N}{Path.GetExtension(_imagePath)}";
                string destPath = Path.Combine(GetImagesFolder(), fileName);

                File.Copy(_imagePath, destPath, true); // Копирование файла
                _employee.PhotoPath = fileName; // Сохранение имени файла

                // Обновление пути в базе данных
                using (var db = new AutobaseEntities())
                {
                    var employee = db.Employees.Find(_employee.EmployeeID);
                    if (employee != null)
                    {
                        employee.PhotoPath = fileName;
                        db.SaveChanges();
                    }
                }
            }
        }
    }
}