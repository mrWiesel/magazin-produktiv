using System;
using System.Text;

class Program
{
    static void Main()
    {
        Console.OutputEncoding = Encoding.UTF8;

        // Товари
        string[] names = { "Хліб", "Молоко", "Сир", "Фініки", "Чай чорний", "Цукор", "Яйця", "Курятина(1 кг)", "Чіпси зі смаком кріб", "Стіральний порошок Gala" };
        decimal[] prices = { 29m, 49m, 99m, 79m, 30m, 39m, 59m, 300m, 99m, 1999m };

        string[] cartNames = new string[10];
        decimal[] cartPrices = new decimal[10];
        int cartCount = 0;

        Console.WriteLine("=== МАГАЗИН ПРОДУКТІВ ===");
        Console.WriteLine("1. Хліб - 29 грн");
        Console.WriteLine("2. Молоко - 49 грн");
        Console.WriteLine("3. Сир - 99 грн");
        Console.WriteLine("4. Фініки - 79 грн");
        Console.WriteLine("5. Чай чорний - 30 грн");
        Console.WriteLine("6. Цукор - 39 грн");
        Console.WriteLine("7. Яйця - 59 грн");
        Console.WriteLine("8. Курятина(1 кг) - 300 грн");
        Console.WriteLine("9. Чіпси зі смаком кріб - 99 грн");
        Console.WriteLine("10. 'Стіральний' порошок Gala - 1999 грн");

        // Вибір товарів (до 10, без циклів)
        Console.Write("\nВведіть номер товару (або 0, щоб завершити): ");
        int c1 = int.Parse(Console.ReadLine());
        if (c1 > 0 && c1 <= 10) { cartNames[cartCount] = names[c1 - 1]; cartPrices[cartCount] = prices[c1 - 1]; cartCount++; }

        Console.Write("Введіть номер наступного товару (0 - завершити): ");
        int c2 = int.Parse(Console.ReadLine());
        if (c2 > 0 && c2 <= 10) { cartNames[cartCount] = names[c2 - 1]; cartPrices[cartCount] = prices[c2 - 1]; cartCount++; }

        Console.Write("Введіть номер наступного товару (0 - завершити): ");
        int c3 = int.Parse(Console.ReadLine());
        if (c3 > 0 && c3 <= 10) { cartNames[cartCount] = names[c3 - 1]; cartPrices[cartCount] = prices[c3 - 1]; cartCount++; }

        Console.Write("Введіть номер наступного товару (0 - завершити): ");
        int c4 = int.Parse(Console.ReadLine());
        if (c4 > 0 && c4 <= 10) { cartNames[cartCount] = names[c4 - 1]; cartPrices[cartCount] = prices[c4 - 1]; cartCount++; }

        Console.Write("Введіть номер наступного товару (0 - завершити): ");
        int c5 = int.Parse(Console.ReadLine());
        if (c5 > 0 && c5 <= 10) { cartNames[cartCount] = names[c5 - 1]; cartPrices[cartCount] = prices[c5 - 1]; cartCount++; }

        Console.Write("Введіть номер наступного товару (0 - завершити): ");
        int c6 = int.Parse(Console.ReadLine());
        if (c6 > 0 && c6 <= 10) { cartNames[cartCount] = names[c6 - 1]; cartPrices[cartCount] = prices[c6 - 1]; cartCount++; }

        Console.Write("Введіть номер наступного товару (0 - завершити): ");
        int c7 = int.Parse(Console.ReadLine());
        if (c7 > 0 && c7 <= 10) { cartNames[cartCount] = names[c7 - 1]; cartPrices[cartCount] = prices[c7 - 1]; cartCount++; }

        Console.Write("Введіть номер наступного товару (0 - завершити): ");
        int c8 = int.Parse(Console.ReadLine());
        if (c8 > 0 && c8 <= 10) { cartNames[cartCount] = names[c8 - 1]; cartPrices[cartCount] = prices[c8 - 1]; cartCount++; }

        Console.Write("Введіть номер наступного товару (0 - завершити): ");
        int c9 = int.Parse(Console.ReadLine());
        if (c9 > 0 && c9 <= 10) { cartNames[cartCount] = names[c9 - 1]; cartPrices[cartCount] = prices[c9 - 1]; cartCount++; }

        Console.Write("Введіть номер наступного товару (0 - завершити): ");
        int c10 = int.Parse(Console.ReadLine());
        if (c10 > 0 && c10 <= 10) { cartNames[cartCount] = names[c10 - 1]; cartPrices[cartCount] = prices[c10 - 1]; cartCount++; }

        // Вивід кошика (без циклів)
        Console.WriteLine("\n=== ВАШ КОШИК ===");
        decimal total = 0;

        if (cartCount > 0) { Console.WriteLine($"{cartNames[0]} - {cartPrices[0]} грн"); total += cartPrices[0]; }
        if (cartCount > 1) { Console.WriteLine($"{cartNames[1]} - {cartPrices[1]} грн"); total += cartPrices[1]; }
        if (cartCount > 2) { Console.WriteLine($"{cartNames[2]} - {cartPrices[2]} грн"); total += cartPrices[2]; }
        if (cartCount > 3) { Console.WriteLine($"{cartNames[3]} - {cartPrices[3]} грн"); total += cartPrices[3]; }
        if (cartCount > 4) { Console.WriteLine($"{cartNames[4]} - {cartPrices[4]} грн"); total += cartPrices[4]; }
        if (cartCount > 5) { Console.WriteLine($"{cartNames[5]} - {cartPrices[5]} грн"); total += cartPrices[5]; }
        if (cartCount > 6) { Console.WriteLine($"{cartNames[6]} - {cartPrices[6]} грн"); total += cartPrices[6]; }
        if (cartCount > 7) { Console.WriteLine($"{cartNames[7]} - {cartPrices[7]} грн"); total += cartPrices[7]; }
        if (cartCount > 8) { Console.WriteLine($"{cartNames[8]} - {cartPrices[8]} грн"); total += cartPrices[8]; }
        if (cartCount > 9) { Console.WriteLine($"{cartNames[9]} - {cartPrices[9]} грн"); total += cartPrices[9]; }

        Console.WriteLine($"---------------------\nРазом: {total:F2} грн");

        // Код знижки
        Console.Write("\nЯкщо у вас є код знижки, введіть його (або Enter): ");
        string code = Console.ReadLine();
        decimal discount = 0;

        if (code == "1234")
        {
            Random rnd = new Random();
            discount = rnd.Next(0, 100);
            Console.WriteLine($"🎁 Ви отримали знижку {discount}%!");
            total -= total * (discount / 100);
        }

        Console.WriteLine($"\nЗагальна сума до оплати: {total:F2} грн");
        Console.WriteLine("\nДякуємо за покупку!");
    }
}
