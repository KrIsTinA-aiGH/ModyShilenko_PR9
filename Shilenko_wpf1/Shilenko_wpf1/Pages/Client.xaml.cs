using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Shilenko_wpf1.Models;
using Shilenko_wpf1.Services;

namespace Shilenko_wpf1.Pages
{
    public partial class Client : Page
    {
        private Users _currentUser;
        private string _userRole;

        public Client(object user, string role)
        {
            InitializeComponent();

            _currentUser = user as Users;
            _userRole = role;

            DisplayUserGreeting(); //метод приветствия
        }

        private void DisplayUserGreeting()
        {
            //устанавливаем приветствие в зависимости от времени суток
            tbGreeting.Text = TimeService.GetTimeBasedGreeting();

            //устанавливаем полное имя пользователя
            if (_currentUser != null)
            {
                string fullName = TimeService.GetFullUserName(_currentUser); //определяет тип пользователя
                tbUserName.Text = fullName; 

                //дополнительная информация о пользователе
                string userType = TimeService.IsEmployee(_currentUser) ? "Сотрудник" :
                                 TimeService.IsClient(_currentUser) ? "Клиент" : "Пользователь";

                tbUserInfo.Text = $"Роль: {_userRole} | {userType}";
            }
            else
            {
                tbUserName.Text = "Гость";
                tbUserInfo.Text = "Вы вошли в систему как гость";
            }
        }
    }
}