using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace MagazApp
{
    class Program
    {
        //структури
        struct Product
        {
            public int Id;                 // int
            public string Name;            // string
            public double Price;           // double
            public int AvailableCount;     // int
            public bool IsActive;          // bool
            public DateTime AddedDate;     // DateTime

            public Product(int id, string name, double price, int availableCount, bool isActive, DateTime addedDate)
            {
                Id = id;
                Name = name;
                Price = price;
                AvailableCount = availableCount;
                IsActive = isActive;
                AddedDate = addedDate;
            }

            public override string ToString()
            {
                return $"{Id}. {Name} | Price: {Price} | Count: {AvailableCount} | Active: {IsActive} | Added: {AddedDate:d}";
            }
        }

        struct Client
        {
            public int Id;                 // int
            public string FullName;        // string
            public string Phone;           // string
            public bool IsPreferred;       // bool
            public DateTime Joined;        // DateTime

            public Client(int id, string fullName, string phone, bool isPreferred, DateTime joined)
            {
                Id = id;
                FullName = fullName;
                Phone = phone;
                IsPreferred = isPreferred;
                Joined = joined;
            }

            public override string ToString()
            {
                return $"{Id}. {FullName} | {Phone} | VIP: {IsPreferred} | Joined: {Joined:d}";
            }
        }

        struct Booking
        {
            public int BookingId;
            public int ClientId;
            public int ProductId;
            public int Quantity;
            public double Total;
            public DateTime BookingDate;

            public Booking(int bookingId, int clientId, int productId, int quantity, double total, DateTime bookingDate)
            {
                BookingId = bookingId;
                ClientId = clientId;
                ProductId = productId;
                Quantity = quantity;
                Total = total;
                BookingDate = bookingDate;
            }

            public override string ToString()
            {
                return $"B#{BookingId} | Client:{ClientId} | Prod:{ProductId} | Qty:{Quantity} | Total:{Total} | Date:{BookingDate:d}";
            }
        }

        //змінні
        static List<Product> products = new List<Product>();
        static List<Client> clients = new List<Client>();
        static List<Booking> bookings = new List<Booking>();
        static Dictionary<string, string> users = new Dictionary<string, string>();
        static string currentUser = null;

        static string dataDir = "data_v2";
        static string productsFile = Path.Combine(dataDir, "products.txt");
        static string clientsFile = Path.Combine(dataDir, "clients.txt");
        static string bookingsFile = Path.Combine(dataDir, "bookings.txt");
        static string usersFile = Path.Combine(dataDir, "users.txt");

        //мейн
        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            try
            {
                LoadData();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Помилка при завантаженні даних: " + ex.Message);
            }

            // Якщо нема користувачів
            if (users.Count == 0)
            {
                users["admin"] = "1234";
                SaveData();
            }

            ShowLoginMenu();
        }

        //логін меню
        static void ShowLoginMenu()
        {
            Console.WriteLine("=== Система авторизації ===");
            while (true)
            {
                Console.WriteLine("1. Увійти");
                Console.WriteLine("2. Зареєструватися");
                Console.WriteLine("0. Вийти");
                Console.Write("Ваш вибір: ");
                string ch = Console.ReadLine();

                if (ch == "1")
                {
                    if (LoginUser()) // do-while inside
                    {
                        ShowIntro();
                        ShowMainMenu();
                        break;
                    }
                    else
                    {
                        Console.WriteLine("Вхід не виконано. Програма завершує роботу.");
                        return;
                    }
                }
                else if (ch == "2")
                {
                    RegisterUser();
                }
                else if (ch == "0")
                {
                    Console.WriteLine("Бувай!");
                    return;
                }
                else
                {
                    Console.WriteLine("Невірний вибір, спробуйте ще раз.");
                }
            }
        }

        static bool LoginUser()
        {
            int attempts = 3;
            bool success = false;
            do
            {
                Console.Write("Логін: ");
                string login = Console.ReadLine();
                Console.Write("Пароль: ");
                string password = ReadPasswordMasked();

                if (users.ContainsKey(login) && users[login] == password)
                {
                    Console.WriteLine("Вхід успішний.");
                    currentUser = login;
                    success = true;
                    break;
                }
                else
                {
                    attempts--;
                    Console.WriteLine($"Невірний логін/пароль. Залишилось спроб: {attempts}");
                }
            } while (attempts > 0);

            return success;
        }

        static void RegisterUser()
        {
            Console.Write("Новий логін: ");
            string login = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(login))
            {
                Console.WriteLine("Логін не може бути порожнім.");
                return;
            }
            if (users.ContainsKey(login))
            {
                Console.WriteLine("Користувач з таким логіном вже існує.");
                return;
            }

            Console.Write("Пароль: ");
            string pass = ReadPasswordMasked();
            if (string.IsNullOrWhiteSpace(pass))
            {
                Console.WriteLine("Пароль не може бути порожнім.");
                return;
            }

            users[login] = pass;
            SaveData();
            Console.WriteLine("Реєстрація успішна.");
        }

        //рендер
        static void ShowIntro()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.DarkMagenta;
            Console.WriteLine("===========================================");
            Console.WriteLine("======== Ласкаво просимо до Магаза ========");
            Console.WriteLine("===========================================");
            Console.ResetColor();
        }

        //мейн меню
        static void ShowMainMenu()
        {
            bool exit = false;
            while (!exit)
            {
                Console.WriteLine();
                Console.WriteLine("=== Головне меню ===");
                Console.WriteLine("1. Товари");
                Console.WriteLine("2. Клієнти");
                Console.WriteLine("3. Бронювання");
                Console.WriteLine("4. Пошук");
                Console.WriteLine("5. Статистика");
                Console.WriteLine("6. Звіт");
                Console.WriteLine("7. Зберегти дані");
                Console.WriteLine("8. Видалити тестові дані");
                Console.WriteLine("0. Вихід");
                Console.Write("Виберіть пункт: ");

                string choice = Console.ReadLine();
                switch (choice)
                {
                    case "1": ProductsMenu(); break;
                    case "2": ClientsMenu(); break;
                    case "3": BookingMenu(); break;
                    case "4": SearchMenu(); break;
                    case "5": ShowStatistics(); break;
                    case "6": PrintReport(); break;
                    case "7":
                        SaveData();
                        Console.WriteLine("Збережено.");
                        break;
                    case "8":
                        ClearTestData();
                        break;
                    case "0":
                        Console.WriteLine("До побачення!");
                        exit = true;
                        break;
                    default:
                        Console.WriteLine("Невірний вибір.");
                        break;
                }
            }
        }

        //товари
        static void ProductsMenu()
        {
            while (true)
            {
                Console.WriteLine("\n=== Меню товарів ===");
                Console.WriteLine("1. Додати товар");
                Console.WriteLine("2. Переглянути товари");
                Console.WriteLine("3. Редагувати товар");
                Console.WriteLine("4. Видалити товар");
                Console.WriteLine("0. Назад");
                Console.Write("Ваш вибір: ");
                string c = Console.ReadLine();
                if (c == "0") return;

                switch (c)
                {
                    case "1": AddProductInteractive(); break;
                    case "2": ListProducts(); break;
                    case "3": EditProduct(); break;
                    case "4": DeleteProduct(); break;
                    default: Console.WriteLine("Невірний ввід."); break;
                }
            }
        }

        static void AddProductInteractive()
        {
            Console.WriteLine("=== Додавання товару (введіть 0 щоб скасувати) ===");
            Console.Write("Назва: ");
            string name = Console.ReadLine();
            if (name == "0") return;
            if (string.IsNullOrWhiteSpace(name))
            {
                Console.WriteLine("Неприпустима назва.");
                return;
            }

            double price = GetDoubleInputWithPrompt("Ціна:");
            if (price < 0) { Console.WriteLine("Ціна не може бути від'ємною."); return; }

            int count = GetIntInputWithPrompt("Кількість:");
            if (count < 0) { Console.WriteLine("Кількість не може бути від'ємною."); return; }

            bool active = true;
            int id = products.Count > 0 ? products.Max(p => p.Id) + 1 : 1;
            products.Add(new Product(id, name, price, count, active, DateTime.Now));
            SaveData();
            Console.WriteLine("Товар додано.");
        }

        static void ListProducts()
        {
            Console.WriteLine("=== Список товарів ===");
            if (products.Count == 0)
            {
                Console.WriteLine("Порожньо.");
                return;
            }
            foreach (var p in products)
                Console.WriteLine(p.ToString());
        }

        static void EditProduct()
        {
            ListProducts();
            if (products.Count == 0) return;
            int idx = GetIntInputWithPrompt("Введіть ID товару для редагування (0 - відміна):");
            if (idx == 0) return;

            var prod = products.FirstOrDefault(p => p.Id == idx);
            if (prod.Id == 0)
            {
                Console.WriteLine("Не знайдено товару.");
                return;
            }

            Console.Write("Нова назва (Enter - без змін): ");
            string newName = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(newName)) prod.Name = newName;

            Console.Write("Нова ціна (Enter - без змін): ");
            string priceStr = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(priceStr) && double.TryParse(priceStr, out double newPrice))
                prod.Price = newPrice;

            Console.Write("Нова кількість (Enter - без змін): ");
            string cntStr = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(cntStr) && int.TryParse(cntStr, out int newCnt))
                prod.AvailableCount = newCnt;

            //апгрейд
            for (int i = 0; i < products.Count; i++)
            {
                if (products[i].Id == prod.Id)
                {
                    products[i] = prod;
                    break;
                }
            }
            SaveData();
            Console.WriteLine("Збережено.");
        }

        static void DeleteProduct()
        {
            ListProducts();
            if (products.Count == 0) return;
            int id = GetIntInputWithPrompt("Введіть ID товару для видалення (0 - відміна):");
            if (id == 0) return;
            int idx = products.FindIndex(p => p.Id == id);
            if (idx == -1)
            {
                Console.WriteLine("Не знайдено.");
                return;
            }
            products.RemoveAt(idx);
            SaveData();
            Console.WriteLine("Товар видалено.");
        }


        //кліент
        static void ClientsMenu()
        {
            while (true)
            {
                Console.WriteLine("\n=== Меню клієнтів ===");
                Console.WriteLine("1. Додати клієнта");
                Console.WriteLine("2. Переглянути клієнтів");
                Console.WriteLine("0. Назад");
                Console.Write("Вибір: ");
                string c = Console.ReadLine();
                if (c == "0") return;

                switch (c)
                {
                    case "1": AddClientInteractive(); break;
                    case "2": ListClients(); break;
                    default: Console.WriteLine("Невірний ввід."); break;
                }
            }
        }

        static void AddClientInteractive()
        {
            Console.Write("ПІБ клієнта (0 - відміна): ");
            string name = Console.ReadLine();
            if (name == "0") return;
            if (string.IsNullOrWhiteSpace(name))
            {
                Console.WriteLine("Не таке ім'я.");
                return;
            }
            Console.Write("Телефон: ");
            string phone = Console.ReadLine();

            Console.Write("Привілейований? (y/n): ");
            bool vip = Console.ReadLine().Trim().ToLower() == "y";

            int id = clients.Count > 0 ? clients.Max(c => c.Id) + 1 : 1;
            clients.Add(new Client(id, name, phone, vip, DateTime.Now));
            SaveData();
            Console.WriteLine("Клієнта додано.");
        }

        static void ListClients()
        {
            Console.WriteLine("=== Клієнти ===");
            if (clients.Count == 0) { Console.WriteLine("Порожньо."); return; }
            foreach (var c in clients) Console.WriteLine(c.ToString());
        }

        //бронь
        static void BookingMenu()
        {
            while (true)
            {
                Console.WriteLine("\n=== Меню бронювань ===");
                Console.WriteLine("1. Створити бронювання");
                Console.WriteLine("2. Переглянути бронювання");
                Console.WriteLine("0. Назад");
                Console.Write("Вибір: ");
                string c = Console.ReadLine();
                if (c == "0") return;

                switch (c)
                {
                    case "1": CreateBooking(); break;
                    case "2": ListBookings(); break;
                    default: Console.WriteLine("Невірний ввід."); break;
                }
            }
        }

        static void CreateBooking()
        {
            if (products.Count == 0)
            {
                Console.WriteLine("Немає товарів для бронювання.");
                return;
            }

            Console.Write("Ім'я клієнта (0 - відміна): ");
            string clientName = Console.ReadLine();
            if (clientName == "0") return;

            //чи є клієнт
            Client client = clients.FirstOrDefault(c => c.FullName.Equals(clientName, StringComparison.OrdinalIgnoreCase));
            if (client.Id == 0)
            {
                int cid = clients.Count > 0 ? clients.Max(c => c.Id) + 1 : 1;
                Console.Write("Телефон: ");
                string phone = Console.ReadLine();
                clients.Add(new Client(cid, clientName, phone, false, DateTime.Now));
                client = clients.First(c => c.Id == cid);
                Console.WriteLine("Клієнта додано.");
            }

            ListProducts();
            int pid = GetIntInputWithPrompt("Введіть ID товару для бронювання:");
            var prod = products.FirstOrDefault(p => p.Id == pid);
            if (prod.Id == 0)
            {
                Console.WriteLine("Товар не знайдено.");
                return;
            }

            int qty = GetIntInputWithPrompt("Кількість для бронювання:");
            if (qty <= 0)
            {
                Console.WriteLine("Кількість повинна бути більше 0.");
                return;
            }
            if (qty > prod.AvailableCount)
            {
                Console.WriteLine("Недостатньо товару на складі.");
                return;
            }

            double total = qty * prod.Price;
            int bid = bookings.Count > 0 ? bookings.Max(b => b.BookingId) + 1 : 1;
            int clientId = clients.First(c => c.FullName.Equals(clientName, StringComparison.OrdinalIgnoreCase)).Id;
            bookings.Add(new Booking(bid, clientId, pid, qty, total, DateTime.Now));

            for (int i = 0; i < products.Count; i++)
            {
                if (products[i].Id == prod.Id)
                {
                    var updated = products[i];
                    updated.AvailableCount -= qty;
                    products[i] = updated;
                    break;
                }
            }

            SaveData();
            Console.WriteLine("Бронювання створено.");
        }

        static void ListBookings()
        {
            Console.WriteLine("=== Бронювання ===");
            if (bookings.Count == 0) { Console.WriteLine("Порожньо."); return; }
            foreach (var b in bookings) Console.WriteLine(b.ToString());
        }

        //пошук
        static void SearchMenu()
        {
            Console.WriteLine("\n=== Пошук ===");
            Console.WriteLine("1. Пошук товару");
            Console.WriteLine("2. Пошук клієнта");
            Console.Write("Вибір: ");
            string c = Console.ReadLine();
            if (c == "1") SearchProduct();
            else if (c == "2") SearchClient();
            else Console.WriteLine("Невірний ввід.");
        }

        static void SearchProduct()
        {
            Console.Write("Пошук товару: ");
            string q = Console.ReadLine();
            bool found = false;
            foreach (var p in products)
            {
                if (p.Name.IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    Console.WriteLine("Знайдено: " + p.ToString());
                    found = true;
                    break; // break приклад - зупиняємося на першому знайденому
                }
            }
            if (!found) Console.WriteLine("Нічого не знайдено.");
        }

        static void SearchClient()
        {
            Console.Write("Пошук клієнта: ");
            string q = Console.ReadLine();
            bool found = false;
            foreach (var c in clients)
            {
                if (c.FullName.IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    Console.WriteLine("Знайдено: " + c.ToString());
                    found = true;
                    break;
                }
            }
            if (!found) Console.WriteLine("Нічого не знайдено.");
        }

        //стат
        static void ShowStatistics()
        {
            Console.WriteLine("\n=== Статистика по товарам ===");
            if (products.Count == 0)
            {
                Console.WriteLine("Немає товарів.");
                return;
            }

            // Заг сума
            double totalValue = 0;
            int totalCount = 0;
            double minPrice = double.MaxValue;
            double maxPrice = double.MinValue;
            int countPriceOver500 = 0;

            foreach (var p in products)
            {
                totalValue += p.Price * p.AvailableCount;
                totalCount += p.AvailableCount;

                if (p.Price < minPrice) minPrice = p.Price;
                if (p.Price > maxPrice) maxPrice = p.Price;
                if (p.Price > 500) countPriceOver500++;
            }

            double avgPrice = products.Average(p => p.Price);
            double avgCount = products.Count > 0 ? (double)totalCount / products.Count : 0;

            Console.WriteLine($"Кількість товарів (позицій): {products.Count}");
            Console.WriteLine($"Загальна вартість (ціна * кількість): {totalValue}");
            Console.WriteLine($"Середня ціна товару: {avgPrice:F2}");
            Console.WriteLine($"Середня кількість на позицію: {avgCount:F2}");
            Console.WriteLine($"Кількість позицій з ціною > 500: {countPriceOver500}");
            Console.WriteLine($"Мінімальна ціна: {minPrice}");
            Console.WriteLine($"Максимальна ціна: {maxPrice}");
        }

        //звіт
        static void PrintReport()
        {
            Console.WriteLine("\n==== ЗВІТ СИСТЕМИ ====");
            Console.WriteLine($"Дата: {DateTime.Now}");
            Console.WriteLine("\n--- Товари ---");
            if (products.Count == 0) Console.WriteLine("Немає товарів.");
            else
            {
                foreach (var p in products)
                    Console.WriteLine(p.ToString());
            }

            Console.WriteLine("\n--- Клієнти ---");
            if (clients.Count == 0) Console.WriteLine("Немає клієнтів.");
            else
            {
                foreach (var c in clients)
                    Console.WriteLine(c.ToString());
            }

            Console.WriteLine("\n--- Бронювання ---");
            if (bookings.Count == 0) Console.WriteLine("Немає бронювань.");
            else
            {
                foreach (var b in bookings)
                    Console.WriteLine(b.ToString());
            }

            Console.WriteLine("\n--- Підсумки ---");
            double productsTotalValue = products.Sum(p => p.Price * p.AvailableCount);
            Console.WriteLine($"Усього товарів (позицій): {products.Count}");
            Console.WriteLine($"Усього клієнтів: {clients.Count}");
            Console.WriteLine($"Усього бронювань: {bookings.Count}");
            Console.WriteLine($"Загальна вартість всіх товарів (ціна*кількість): {productsTotalValue:F2}");

            Console.WriteLine("\nНатисніть будь-яку клавішу для продовження...");
            Console.ReadKey();
        }

        //обробка помилок
        static int GetIntInputWithPrompt(string prompt)
        {
            while (true)
            {
                Console.Write(prompt + " ");
                string s = Console.ReadLine();
                if (int.TryParse(s, out int v))
                    return v;
                Console.WriteLine("Введіть коректне ціле число.");
            }
        }

        static double GetDoubleInputWithPrompt(string prompt)
        {
            while (true)
            {
                Console.Write(prompt + " ");
                string s = Console.ReadLine();
                if (double.TryParse(s, out double v))
                    return v;
                Console.WriteLine("Введіть коректне число (наприклад 123,45).");
            }
        }

        static string ReadPasswordMasked()
        {
            StringBuilder pass = new StringBuilder();
            ConsoleKeyInfo key;
            while ((key = Console.ReadKey(true)).Key != ConsoleKey.Enter)
            {
                if (key.Key == ConsoleKey.Backspace)
                {
                    if (pass.Length > 0)
                    {
                        pass.Length--;
                        Console.Write("\b \b");
                    }
                }
                else if (!char.IsControl(key.KeyChar))
                {
                    pass.Append(key.KeyChar);
                    Console.Write("*");
                }
            }
            Console.WriteLine();
            return pass.ToString();
        }

        //сейв/лоад
        static void SaveData()
        {
            try
            {
                if (!Directory.Exists(dataDir)) Directory.CreateDirectory(dataDir);

                // products
                var prodLines = products.ConvertAll(p => $"{p.Id}|{p.Name}|{p.Price}|{p.AvailableCount}|{p.IsActive}|{p.AddedDate:o}");
                File.WriteAllLines(productsFile, prodLines);

                // clients
                var clientLines = clients.ConvertAll(c => $"{c.Id}|{c.FullName}|{c.Phone}|{c.IsPreferred}|{c.Joined:o}");
                File.WriteAllLines(clientsFile, clientLines);

                // bookings
                var bookingLines = bookings.ConvertAll(b => $"{b.BookingId}|{b.ClientId}|{b.ProductId}|{b.Quantity}|{b.Total}|{b.BookingDate:o}");
                File.WriteAllLines(bookingsFile, bookingLines);

                // users
                var userLines = users.Select(kvp => $"{kvp.Key}|{kvp.Value}").ToArray();
                File.WriteAllLines(usersFile, userLines);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Помилка збереження: " + ex.Message);
            }
        }

        static void LoadData()
        {
            try
            {
                if (!Directory.Exists(dataDir)) return;

                if (File.Exists(productsFile))
                {
                    foreach (var line in File.ReadAllLines(productsFile))
                    {
                        var p = line.Split('|');
                        if (p.Length >= 6 &&
                            int.TryParse(p[0], out int id) &&
                            double.TryParse(p[2], out double price) &&
                            int.TryParse(p[3], out int cnt) &&
                            bool.TryParse(p[4], out bool isActive) &&
                            DateTime.TryParse(p[5], null, System.Globalization.DateTimeStyles.RoundtripKind, out DateTime ad))
                        {
                            products.Add(new Product(id, p[1], price, cnt, isActive, ad));
                        }
                    }
                }

                if (File.Exists(clientsFile))
                {
                    foreach (var line in File.ReadAllLines(clientsFile))
                    {
                        var p = line.Split('|');
                        if (p.Length >= 5 &&
                            int.TryParse(p[0], out int id) &&
                            bool.TryParse(p[3], out bool vip) &&
                            DateTime.TryParse(p[4], null, System.Globalization.DateTimeStyles.RoundtripKind, out DateTime joined))
                        {
                            clients.Add(new Client(id, p[1], p[2], vip, joined));
                        }
                    }
                }

                if (File.Exists(bookingsFile))
                {
                    foreach (var line in File.ReadAllLines(bookingsFile))
                    {
                        var p = line.Split('|');
                        if (p.Length >= 6 &&
                            int.TryParse(p[0], out int bid) &&
                            int.TryParse(p[1], out int cid) &&
                            int.TryParse(p[2], out int pid) &&
                            int.TryParse(p[3], out int qty) &&
                            double.TryParse(p[4], out double total) &&
                            DateTime.TryParse(p[5], null, System.Globalization.DateTimeStyles.RoundtripKind, out DateTime bdate))
                        {
                            bookings.Add(new Booking(bid, cid, pid, qty, total, bdate));
                        }
                    }
                }

                if (File.Exists(usersFile))
                {
                    foreach (var line in File.ReadAllLines(usersFile))
                    {
                        var p = line.Split('|');
                        if (p.Length >= 2)
                            users[p[0]] = p[1];
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Помилка при завантаженні: " + ex.Message);
            }
        }

        //ютіліти
        static void ClearTestData()
        {
            Console.WriteLine("Ви впевнені, що хочете видалити всі дані? (y/n)");
            string a = Console.ReadLine();
            if (a.Trim().ToLower() == "y")
            {
                products.Clear();
                clients.Clear();
                bookings.Clear();
                users.Clear();
                try
                {
                    if (Directory.Exists(dataDir))
                    {
                        foreach (var f in Directory.GetFiles(dataDir))
                            File.Delete(f);
                    }
                    Console.WriteLine("Дані видалено.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Помилка при видаленні файлів: " + ex.Message);
                }
            }
            else Console.WriteLine("Отмена.");
        }
    }
}




//⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⢀⡀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀
//⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠈⠉⣠⠤⠶⠶⠤⠴⢤⠶⠤⠤⣄⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀
//⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⣴⡞⠉⠁⣴⡆⣸⣿⣿⣿⣿⠛⣷⣌⡻⣦⡀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀
//⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⢰⠘⠿⣿⣿⣿⣿⣿⣿⣦⣿⣿⣿⣿⣿⣿⣮⢻⡄⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀
//⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⣰⣯⣀⣶⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⡟⢿⡀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀
//⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⡿⢈⣽⣿⣿⠟⠛⠛⠉⠛⠉⠁⠀⠀⠀⠘⢻⣿⣧⢸⡇⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀
//⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⢥⣼⣿⡟⠁⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠈⣿⣿⢘⡇⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀
//⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⣿⣿⣿⡇⠀⣀⣀⣀⣠⣄⠀⠀⢠⣤⣄⣀⠀⣿⡿⣿⡇⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀
//⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⢠⢿⢿⣿⡇⠀⠻⣿⣭⣽⢹⣇⠀⠘⣿⣶⣮⠟⢹⣇⢛⡁⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀
//⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⣾⣘⣿⡇⠀⠚⠉⠀⠂⠈⣙⠀⠀⠈⠀⠀⠀⣘⣻⠿⣆⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀
//⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⡺⣧⠈⢷⡄⠀⠀⠀⠀⢠⣶⣤⠀⢠⡄⢀⡄⢸⣿⣴⡇⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀
//⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠣⣽⣶⣾⣄⡀⠀⠀⠀⢘⣇⣉⣀⡀⠃⠘⣶⢫⣴⠏⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀
//⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠈⠃⠈⣿⣇⠀⠀⠀⢿⡿⡶⢿⣟⣠⣤⣏⣾⠁⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀
//⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⣤⢹⡽⠆⠀⠀⢀⡽⠷⢶⣶⠶⣯⣯⡍⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀
//⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⣴⡟⢸⣷⠀⠀⠀⠀⢀⣠⢿⣿⡀⣿⡇⠁⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀
//⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⣀⣠⣾⣿⣿⢸⣿⠀⠀⠀⠀⠈⠁⣸⡏⡷⣏⢻⣦⣀⡀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀
//⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⣀⣠⣶⣿⣿⣿⣿⣿⣿⡆⠻⣆⢠⠂⣠⣀⣰⣟⣻⣯⣸⠈⣿⣿⣿⣿⣶⣦⣤⣀⡀⠀⠀⠀⠀⠀⠀
//⠀⠀⠀⠀⠀⠀⣀⣤⣶⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣧⠀⠙⢧⡀⠀⠀⠸⣯⣿⣿⣽⠀⢹⣿⣿⣿⣿⣿⣿⣿⣿⣷⣦⣄⡀⠀⠀
//⠀⠀⢀⣠⣴⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣇⣄⠈⢻⣄⠀⠀⢩⣟⡈⠁⠀⠀⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣦⠀
//⠀⣴⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⠈⠀⠀⠈⠓⠶⠿⠀⠀⠀⠀⠀⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⡄
//⠰⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⠿⠿⣿⣿⣿⣿⣿⣦⡔⠶⠶⢦⣤⣤⠀⠀⣤⣀⣼⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⡇
//⢸⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⡿⠁⢰⣼⠿⠿⣿⣿⣿⣿⠛⠛⠒⠶⠶⠶⢤⣠⣬⣽⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣷
//⣾⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⡿⠁⡞⣿⠉⠈⠀⠛⠻⢿⣿⣟⠛⠓⠒⠶⠶⢾⣧⣶⣼⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿
//⢹⣇⢻⣿⣿⣿⣿⣿⣿⣿⣿⠇⢾⠃⠙⠓⠒⠰⣴⣶⣾⣿⣿⠛⠛⠛⠒⠒⢻⠠⣶⣾⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿
//⠈⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣀⣸⠀⠀⠀⣤⠀⠈⢻⣿⣿⣿⡛⠛⠛⠛⠒⠚⠶⢶⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿
//⣼⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣏⠁⠀⠈⠀⢬⣤⣶⣿⣿⣿⣿⣟⠉⠛⠛⠛⣿⡾⢶⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿