using Shilenko_wpf1.Models;
using System;
using System.Data.Entity;
using System.Linq;

namespace Shilenko_wpf1.Services
{
    // Сервис для работы с временем и пользователями
    public static class TimeService
    {
        // ==================== МЕТОДЫ ВРЕМЕНИ ====================

        // Получение приветствия в зависимости от времени суток
        public static string GetTimeBasedGreeting()
        {
            var hour = DateTime.Now.Hour; // Текущий час

            if (hour >= 10 && hour <= 12) return "Доброе утро!";    // Утро: 10:00-12:00
            if (hour >= 13 && hour <= 17) return "Добрый день!";    // День: 13:00-17:00
            if (hour >= 18 && hour <= 19) return "Добрый вечер!";   // Вечер: 18:00-19:00

            return "Добро пожаловать!"; // Вне рабочего времени
        }

        // Проверка нахождения в рабочее время (10:00-19:00)
        public static bool IsWithinWorkingHours() =>
            DateTime.Now.Hour >= 10 && DateTime.Now.Hour <= 19;

        // ==================== МЕТОДЫ РАБОТЫ С ПОЛЬЗОВАТЕЛЯМИ ====================

        // Получение полного имени пользователя
        public static string GetFullUserName(Users user)
        {
            if (user == null) return "Гость"; // Для гостевого входа

            try
            {
                using (var db = new AutobaseEntities())
                {
                    // Загрузка пользователя с связанными данными
                    var dbUser = db.Users
                        .Include(u => u.Employees)  // Загрузка данных сотрудника
                        .Include(u => u.Clients)    // Загрузка данных клиента
                        .FirstOrDefault(u => u.UserID == user.UserID);

                    if (dbUser == null) return user.Email; // Если пользователь не найден

                    // Формирование имени для сотрудника
                    if (dbUser.Employees != null)
                        return $"{dbUser.Employees.LastName} {dbUser.Employees.FirstName}".Trim();

                    // Формирование имени для клиента
                    if (dbUser.Clients != null)
                        return dbUser.Clients.ContactPerson ?? dbUser.Clients.CompanyName ?? user.Email;

                    return user.Email; // По умолчанию - email
                }
            }
            catch
            {
                return user.Email; // При ошибке - email
            }
        }

        // Проверка, является ли пользователь сотрудником
        public static bool IsEmployee(Users user)
        {
            if (user == null) return false;

            try
            {
                using (var db = new AutobaseEntities())
                {
                    return db.Users
                        .Include(u => u.Employees)  // Загрузка связи с сотрудником
                        .Any(u => u.UserID == user.UserID && u.Employees != null); // Проверка наличия сотрудника
                }
            }
            catch
            {
                return false; // При ошибке - не сотрудник
            }
        }

        // Проверка, является ли пользователь клиентом
        public static bool IsClient(Users user)
        {
            if (user == null) return false;

            try
            {
                using (var db = new AutobaseEntities())
                {
                    return db.Users
                        .Include(u => u.Clients)  // Загрузка связи с клиентом
                        .Any(u => u.UserID == user.UserID && u.Clients != null); // Проверка наличия клиента
                }
            }
            catch
            {
                return false; // При ошибке - не клиент
            }
        }
    }
}