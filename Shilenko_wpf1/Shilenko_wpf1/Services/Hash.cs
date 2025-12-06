using System;
using System.Security.Cryptography;
using System.Text;

namespace Shilenko_wpf1.Services
{
    // Сервис для хэширования паролей
    public static class Hash
    {
        // Хэширование пароля с использованием SHA256
        public static string HashPassword(string password)
        {
            using (SHA256 sha256Hash = SHA256.Create()) // Создание SHA256 хэшера
            {
                // Конвертация пароля в байтовый массив
                byte[] sourceBytePassword = Encoding.UTF8.GetBytes(password);

                // Вычисление хэша
                byte[] hash = sha256Hash.ComputeHash(sourceBytePassword);

                // Конвертация байтового массива в строку шестнадцатеричных чисел
                // Удаление дефисов для компактного представления
                return BitConverter.ToString(hash).Replace("-", String.Empty);
            }
        }
    }
}