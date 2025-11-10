using System;


namespace FarmStoreApp
{
    class Program
    {
        static void Main(string[] args)
        {
            RenderIntro();
            ShowMainMenu();
        }

        static void RenderIntro()
        {
            Console.ForegroundColor = ConsoleColor.DarkMagenta;
            Console.WriteLine("===========================================");
            Console.WriteLine("======== Ласкаво просимо до Магаза ========");
            Console.WriteLine("===========================================");
            Console.ResetColor();
        }

        static double GetUserInput(string prompt = "Введiть число:")
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write(prompt + " ");
            Console.ResetColor();

            string? input = Console.ReadLine();
            if (!double.TryParse(input, out double number))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Ви ввели не ту цифру :( . Спробуйте ще раз.");
                Console.ResetColor();
                return GetUserInput(prompt);
            }

            return number;
        }

        static void ShowMainMenu()
        {
            Console.WriteLine();
            Console.WriteLine("=== Головне меню ===");
            Console.WriteLine("1. Товари");
            Console.WriteLine("2. Клiєнти");
            Console.WriteLine("3. Замовлення");
            Console.WriteLine("4. Пошук");
            Console.WriteLine("5. Статистика");
            Console.WriteLine("0. Вихiд");

            double choice = GetUserInput("Виберіть пункт меню:");

            switch (choice)
            {
                case 1: ShowProductMenu(); break;
                case 2: ShowClientsMenu(); break;
                case 3: ShowOrderMenu(); break;
                case 4:
                case 5:
                    Console.WriteLine("Функцiя в розробцi");
                    ShowMainMenu();
                    break;
                case 0:
                    Console.WriteLine("Бувай!");
                    Environment.Exit(0);
                    break;
                default:
                    Console.WriteLine("Неправильний вибiр! Спробуйте ще раз.");
                    ShowMainMenu();
                    break;
            }
        }

        static void ShowProductMenu()
        {
            Console.WriteLine("========= МЕНЮ ТОВАРIВ =========");
            Console.WriteLine("1. Додати новий товар");
            Console.WriteLine("2. Переглянути всi товари");
            Console.WriteLine("3. Редагувати товар");
            Console.WriteLine("4. Видалити товар");
            Console.WriteLine("5. Пошук товару");
            Console.WriteLine("0. Назад до головного меню");

            double choice = GetUserInput("Виберіть дiю:");

            if (choice == 0)
                ShowMainMenu();
            else
                Console.WriteLine("Функцiя в розробцi");
        }

        static void ShowClientsMenu()
        {
            Console.WriteLine("Функцiя в розробцi");
            ShowMainMenu();
        }

        static void ShowOrderMenu()
        {
            double priceGrain = 25, priceVegetables = 40, priceMeat = 120, priceMilk = 30, priceEggs = 60;

            Console.WriteLine("=== Замовлення ===");
            double grain = GetUserInput("Зерно:");
            double vegetables = GetUserInput("Овочi:");
            double meat = GetUserInput("М'ясо:");
            double milk = GetUserInput("Молоко:");
            double eggs = GetUserInput("Яйця:");

            double total = grain * priceGrain + vegetables * priceVegetables + meat * priceMeat + milk * priceMilk + eggs * priceEggs;

            double discount = (total > 1000 && total < 5000) ? 15 : (total > 10000 ? 50 : 5);
            double discountTotal = total * (discount / 100);

            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine($"Загальна вартiсть: {total}.");
            Console.WriteLine($"Знижка: {discount}%");
            Console.WriteLine($"До оплати: {total - discountTotal}.");
            Console.ResetColor();

            Console.WriteLine("Дякуємо за покупку :D");
            Console.WriteLine("Натиснiть будь-яку клавiшу, щоб повернутись у головне меню.");
            Console.ReadKey();
            ShowMainMenu();
        }
    }
}
