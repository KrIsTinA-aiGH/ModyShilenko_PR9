using System;

namespace Shilenko_wpf1.Services
{
    // Сервис для генерации CAPTCHA
    public static class SimpleCaptcha
    {
        private static Random rnd = new Random(); // Генератор случайных чисел

        // Генерация случайной CAPTCHA строки
        public static string Create()
        {
            string result = ""; // Итоговая строка CAPTCHA

            // Символы для генерации CAPTCHA (буквы A-Z и цифры 0-9)
            string characters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

            // Генерация 6 случайных символов
            for (int i = 0; i < 6; i++)
            {
                result += characters[rnd.Next(36)]; // Выбор случайного символа из строки
            }

            return result; // Возврат сгенерированной CAPTCHA
        }
    }
}