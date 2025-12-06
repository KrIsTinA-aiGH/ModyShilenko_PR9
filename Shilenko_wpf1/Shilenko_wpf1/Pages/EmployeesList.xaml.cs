using Shilenko_wpf1.Models;
using System;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Shilenko_wpf1.Pages
{
    // Страница для отображения списка сотрудников с фильтрацией
    public partial class EmployeesList : Page
    {
        // Конструктор страницы
        public EmployeesList()
        {
            InitializeComponent();
            Loaded += (s, e) => LoadData(); // Загрузка данных при загрузке страницы
        }

        // ==================== МЕТОДЫ ЗАГРУЗКИ ДАННЫХ ====================

        // Основная загрузка данных сотрудников и должностей
        private void LoadData()
        {
            try
            {
                using (var db = new AutobaseEntities())
                {
                    // Загрузка списка сотрудников с должностями
                    var employees = db.Employees.Include(e => e.EmployeePositions).ToList();
                    lvEmployees.ItemsSource = employees; // Привязка к ListView

                    // Загрузка должностей для фильтра
                    var positions = db.EmployeePositions.Select(p => p.PositionName).ToList();
                    cmbPositionFilter.Items.Clear();
                    cmbPositionFilter.Items.Add("Все должности");
                    positions.ForEach(p => cmbPositionFilter.Items.Add(p)); // Добавление должностей
                    cmbPositionFilter.SelectedIndex = 0; // Выбор элемента по умолчанию

                    UpdateStatus(employees.Count); // Обновление статуса
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}"); // Обработка ошибок
            }
        }

        // Обновление статусной строки с количеством сотрудников
        private void UpdateStatus(int count) => tbStatus.Text = $"Найдено сотрудников: {count}";

        // ==================== МЕТОДЫ ФИЛЬТРАЦИИ ====================

        // Применение фильтров к списку сотрудников
        private void ApplyFilters()
        {
            using (var db = new AutobaseEntities())
            {
                var query = db.Employees.Include(e => e.EmployeePositions).AsQueryable(); // Базовый запрос

                // Фильтр по поиску в ФИО
                if (!string.IsNullOrWhiteSpace(txtSearch.Text))
                {
                    string search = txtSearch.Text.ToLower();
                    query = query.Where(e =>
                        e.LastName.ToLower().Contains(search) || // Поиск по фамилии
                        e.FirstName.ToLower().Contains(search));  // Поиск по имени
                }

                // Фильтр по должности (если выбрано не "Все должности")
                if (cmbPositionFilter.SelectedIndex > 0)
                {
                    string position = cmbPositionFilter.SelectedItem.ToString();
                    query = query.Where(e => e.EmployeePositions.PositionName == position); // Фильтр по должности
                }

                var result = query.ToList(); // Выполнение запроса
                lvEmployees.ItemsSource = result; // Обновление списка
                UpdateStatus(result.Count); // Обновление счетчика
            }
        }

        // ==================== ОБРАБОТЧИКИ СОБЫТИЙ ====================

        // Обработчик изменения текста поиска
        private void txtSearch_TextChanged(object sender, TextChangedEventArgs e) => ApplyFilters();

        // Обработчик изменения выбора должности
        private void cmbPositionFilter_SelectionChanged(object sender, SelectionChangedEventArgs e) => ApplyFilters();

        // Обработчик кнопки добавления нового сотрудника
        private void btnAddEmployee_Click(object sender, RoutedEventArgs e) =>
            NavigationService.Navigate(new EmployeeEdit(null)); // Переход на форму редактирования

        // Обработчик двойного клика по сотруднику
        private void lvEmployees_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (lvEmployees.SelectedItem is Employees employee) // Проверка выбора
                NavigationService.Navigate(new EmployeeEdit(employee)); // Редактирование сотрудника
        }
    }
}