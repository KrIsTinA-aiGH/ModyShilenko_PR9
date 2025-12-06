using Shilenko_wpf1.Models;
using Shilenko_wpf1.Services;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Shilenko_wpf1.Pages
{
    // Главная страница клиента после авторизации
    public partial class Client : Page
    {
        // Конструктор страницы клиента
        public Client(Users user, string role)
        {
            InitializeComponent();
            DisplayUserInfo(user, role); // Отображение информации о пользователе
        }

        // ==================== МЕТОДЫ ОТОБРАЖЕНИЯ ИНФОРМАЦИИ ====================

        // Отображение информации о пользователе на странице
        private void DisplayUserInfo(Users user, string role)
        {
            // Установка приветствия в зависимости от времени суток
            tbGreeting.Text = TimeService.GetTimeBasedGreeting();

            if (user != null)
            {
                // Для авторизованного пользователя
                tbUserName.Text = TimeService.GetFullUserName(user); // Полное имя пользователя

                // Определение типа пользователя (сотрудник/клиент)
                string userType = TimeService.IsEmployee(user) ? "Сотрудник" :
                                 TimeService.IsClient(user) ? "Клиент" : "Пользователь";

                tbUserInfo.Text = $"Роль: {role} | {userType}";

                // Добавление административных кнопок для соответствующих ролей
                if (IsAdminOrManager(role))
                    AddAdminButtons();
            }
            else
            {
                // Для гостевого входа
                tbUserName.Text = "Гость";
                tbUserInfo.Text = "Вы вошли как гость";
            }
        }

        // Проверка, является ли роль административной
        private bool IsAdminOrManager(string role) =>
            role == "Администратор" || role == "Менеджер" || role == "Директор";

        // ==================== МЕТОДЫ СОЗДАНИЯ ИНТЕРФЕЙСА ====================

        // Добавление панели с кнопками для администраторов
        private void AddAdminButtons()
        {
            var panel = new StackPanel
            {
                Orientation = Orientation.Horizontal,       // Горизонтальное расположение кнопок
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 20, 0, 0)        // Отступ сверху
            };

            // Создание и добавление кнопок
            panel.Children.Add(CreateButton("Управление сотрудниками", Brushes.LightBlue, EmployeesButton_Click));
            panel.Children.Add(CreateButton("Управление заказами", Brushes.LightGreen, OrdersButton_Click));

            dynamicContent.Content = panel; // Добавление панели в контейнер
        }

        // Создание кнопки с заданными параметрами
        private Button CreateButton(string content, Brush background, RoutedEventHandler clickHandler)
        {
            return new Button
            {
                Content = content,              // Текст на кнопке
                Width = 200,                    // Ширина кнопки
                Height = 40,                    // Высота кнопки
                Margin = new Thickness(10),     // Внешние отступы
                Background = background,        // Цвет фона
                FontWeight = FontWeights.Bold   // Жирный шрифт
            }.WithClick(clickHandler);         // Добавление обработчика события
        }

        // ==================== ОБРАБОТЧИКИ СОБЫТИЙ КНОПОК ====================

        // Обработчик кнопки "Управление сотрудниками"
        private void EmployeesButton_Click(object sender, RoutedEventArgs e) =>
            NavigationService.Navigate(new EmployeesList()); // Переход на страницу списка сотрудников

        // Обработчик кнопки "Управление заказами"
        private void OrdersButton_Click(object sender, RoutedEventArgs e) =>
            MessageBox.Show("Функция управления заказами будет реализована позже",
                "Информация", MessageBoxButton.OK, MessageBoxImage.Information); // Заглушка
    }

    // ==================== РАСШИРЕНИЯ ДЛЯ КНОПОК ====================

    // Методы расширения для упрощения работы с кнопками
    public static class ButtonExtensions
    {
        // Добавление обработчика события Click с возвратом кнопки (Fluent API)
        public static Button WithClick(this Button button, RoutedEventHandler handler)
        {
            button.Click += handler; // Подписка на событие
            return button;           // Возврат кнопки для цепочки вызовов
        }
    }
}