using Shilenko_wpf1.Models;
using Shilenko_wpf1.Services;
using System;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace Shilenko_wpf1.Pages
{
    /// Страница авторизации пользователей
    public partial class Autho : Page
    {
        // Приватные поля для управления состоянием формы
        private int _attempts;                      // Счетчик неудачных попыток входа
        private bool _isBlocked;                    // Флаг блокировки формы
        private DispatcherTimer _blockTimer;        // Таймер блокировки
        private int _blockTimeRemaining;            // Оставшееся время блокировки

        // Свойство для проверки необходимости капчи
        private bool captchaRequired => _attempts >= 2;

        /// Конструктор страницы авторизации
        public Autho()
        {
            InitializeComponent();
            InitializeTimer(); // Инициализация таймера блокировки
        }

        // ==================== ОБРАБОТЧИКИ СОБЫТИЙ ====================

        /// Обработчик входа как гость
        private void btnEnterGuest_Click(object sender, RoutedEventArgs e) =>
            NavigateToClient(null, "Гость");

        /// Обработчик входа с учетными данными
        private void btnEnter_Click(object sender, RoutedEventArgs e)
        {
            // Проверка блокировки формы
            if (_isBlocked) return;

            // Проверка капчи при необходимости
            if (captchaRequired && !ValidateCaptcha()) return;

            // Получение и валидация введенных данных
            var login = tbLogin.Text.Trim();
            var password = tbPassword.Password.Trim();

            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Введите логин и пароль!");
                return;
            }

            try
            {
                using (var db = new AutobaseEntities())
                {
                    // Поиск пользователя в базе данных
                    var user = db.Users.FirstOrDefault(u => u.Email == login && u.Password == password);

                    if (user != null)
                    {
                        // Проверка рабочего времени для сотрудников
                        if (TimeService.IsEmployee(user) && !TimeService.IsWithinWorkingHours())
                        {
                            MessageBox.Show("Доступ только в рабочее время (10:00-19:00)!");
                            return;
                        }

                        LoginSuccess(user); // Успешный вход
                    }
                    else
                    {
                        HandleFailedLogin(); // Неудачная попытка входа
                    }
                }
            }
            catch
            {
                // При ошибке БД переходим в режим гостя
                NavigateToClient(null, "Гость");
            }
        }

        // ==================== МЕТОДЫ АВТОРИЗАЦИИ ====================

        /// Определение роли пользователя
        private string GetRole(Users user)
        {
            try
            {
                using (var db = new AutobaseEntities())
                {
                    // Загрузка связанных данных о сотруднике/клиенте
                    var dbUser = db.Users
                        .Include(u => u.Employees.EmployeePositions)
                        .Include(u => u.Clients)
                        .FirstOrDefault(u => u.UserID == user.UserID);

                    // Проверка роли сотрудника
                    if (dbUser?.Employees != null)
                    {
                        var position = dbUser.Employees.EmployeePositions?.PositionName;
                        if (!string.IsNullOrEmpty(position))
                        {
                            // Определение конкретной должности
                            if (position.Contains("Директор")) return "Директор";
                            if (position.Contains("Менеджер")) return "Менеджер";
                            if (position.Contains("Водитель")) return "Водитель";
                            if (position.Contains("Диспетчер")) return "Диспетчер";
                            if (position.Contains("Механик")) return "Механик";
                        }
                        return "Сотрудник"; // Роль по умолчанию для сотрудников
                    }

                    // Проверка роли клиента
                    if (dbUser?.Clients != null) return "Клиент";

                    // Fallback: определение роли по email
                    var email = user.Email.ToLower();
                    if (email.Contains("admin")) return "Администратор";
                    if (email.Contains("director")) return "Директор";
                    if (email.Contains("manager")) return "Менеджер";

                    return "Пользователь"; // Роль по умолчанию
                }
            }
            catch
            {
                return "Пользователь"; // При ошибке - роль по умолчанию
            }
        }

        /// Обработка успешного входа
        private void LoginSuccess(Users user)
        {
            var role = GetRole(user);
            MessageBox.Show($"Вы вошли как: {role}");
            NavigateToClient(user, role);
            ResetLoginAttempts(); // Сброс счетчика попыток
        }

        /// Обработка неудачного входа
        private void HandleFailedLogin()
        {
            _attempts++; // Увеличение счетчика попыток
            MessageBox.Show("Неверный логин или пароль!");

            // Показ капчи после 2х неудачных попыток
            if (captchaRequired) ShowCaptcha();
            tbPassword.Clear(); // Очистка поля пароля

            // Блокировка формы после 4х неудачных попыток
            if (_attempts >= 4) BlockForm(10);
        }

        // ==================== МЕТОДЫ КАПЧИ ====================

        /// Валидация введенной капчи
        private bool ValidateCaptcha()
        {
            // Сравнение введенного текста с капчей (игнорируя пробелы)
            if (tbCaptcha.Text.Trim() != tblCaptcha.Text.Replace(" ", ""))
            {
                MessageBox.Show("Неверная капча!");
                ShowCaptcha(); // Показ новой капчи
                return false;
            }
            return true;
        }

        /// Показ элементов капчи
        private void ShowCaptcha()
        {
            tblCaptcha.Text = Services.SimpleCaptcha.Create(); // Генерация капчи
            tblCaptcha.Visibility = Visibility.Visible;
            tbCaptcha.Visibility = Visibility.Visible;
            tbCaptcha.Clear(); // Очистка поля ввода капчи
        }

        /// Скрытие элементов капчи
        private void HideCaptcha()
        {
            tblCaptcha.Visibility = Visibility.Hidden;
            tbCaptcha.Visibility = Visibility.Hidden;
        }

        // ==================== МЕТОДЫ УПРАВЛЕНИЯ ФОРМОЙ ====================

        /// Навигация на страницу клиента
        private void NavigateToClient(Users user, string role) =>
            NavigationService.Navigate(new Client(user, role));

        /// Сброс счетчика попыток входа
        private void ResetLoginAttempts()
        {
            _attempts = 0;
            HideCaptcha(); // Скрытие капчи
            ResetForm();   // Очистка полей формы
        }

        /// Очистка полей формы
        private void ResetForm()
        {
            tbLogin.Clear();
            tbPassword.Clear();
            tbCaptcha.Clear();
        }

        /// Блокировка формы на указанное время
        private void BlockForm(int seconds)
        {
            _isBlocked = true;
            _blockTimeRemaining = seconds;

            SetControlsEnabled(false); // Отключение элементов управления
            tbTimer.Visibility = Visibility.Visible; // Показ таймера
            _blockTimer.Start(); // Запуск таймера блокировки
        }

        /// Разблокировка формы
        private void UnblockForm()
        {
            _blockTimer.Stop(); // Остановка таймера
            _isBlocked = false;
            _attempts = 0; // Сброс счетчика попыток

            SetControlsEnabled(true); // Включение элементов управления
            tbTimer.Visibility = Visibility.Collapsed; // Скрытие таймера
            ResetForm(); // Очистка полей формы
        }

        /// Установка состояния доступности элементов управления
        /// <param name="enabled">Доступность элементов</param>
        private void SetControlsEnabled(bool enabled)
        {
            tbLogin.IsEnabled = enabled;
            tbPassword.IsEnabled = enabled;
            tbCaptcha.IsEnabled = enabled;
            btnEnter.IsEnabled = enabled;
            btnEnterGuest.IsEnabled = enabled;
        }

        // ==================== МЕТОДЫ ТАЙМЕРА ====================

        /// Инициализация таймера блокировки
        private void InitializeTimer()
        {
            _blockTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            _blockTimer.Tick += (s, e) =>
            {
                // Уменьшение оставшегося времени
                if (--_blockTimeRemaining <= 0) UnblockForm();

                // Обновление текста таймера
                tbTimer.Text = $"До разблокировки: {_blockTimeRemaining} сек.";
            };
        }
    }
}