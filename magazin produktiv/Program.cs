using System.Security.Cryptography;
using System.Text;
using System.Globalization;

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
            public DateTime Joined;        // DateTime

            public Client(int id, string fullName, string phone, DateTime joined)
            {
                Id = id;
                FullName = fullName;
                Phone = phone;
                Joined = joined;
            }

            public override string ToString()
            {
                return $"{Id}. {FullName} | {Phone}  | Joined: {Joined:d}";
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
        static Dictionary<string, string> users = new Dictionary<string, string>(); // email -> passwordhash
        static string currentUser = null;

        static string dataDir = "data_v2";

        static string productsCsv = Path.Combine(dataDir, "products.csv");
        static string clientsCsv = Path.Combine(dataDir, "clients.csv");
        static string bookingsCsv = Path.Combine(dataDir, "bookings.csv");
        static string usersCsv = Path.Combine(dataDir, "users.csv");


        static readonly string[] ProductsHeader = new[] { "Id", "Name", "Price", "AvailableCount", "IsActive", "AddedDate" };
        static readonly string[] ClientsHeader = new[] { "Id", "FullName", "Phone", "Joined" };
        static readonly string[] BookingsHeader = new[] { "BookingId", "ClientId", "ProductId", "Quantity", "Total", "BookingDate" };
        static readonly string[] UsersHeader = new[] { "Email", "PasswordHash", "Salt", "CreatedAt" };

        //мейн
        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            EnsureCsvFiles();
            try
            {
                // Load caches from CSV
                products = Csv.ReadProducts(productsCsv, ProductsHeader);
                clients = Csv.ReadClients(clientsCsv, ClientsHeader);
                bookings = Csv.ReadBookings(bookingsCsv, BookingsHeader);
                var readUsers = Csv.ReadUsers(usersCsv, UsersHeader);
                users = readUsers.ToDictionary(u => u.Email, u => u.PasswordHash);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Помилка при завантаженні даних: " + ex.Message);
            }

            // Якщо нема користувачів — створимо технічний акаунт
            if (!FileHasAtLeastOneDataRow(usersCsv))
            {
                var email = "admin@example.com"; // FIX: валідний email
                var salt = Security.GenerateSalt();
                var hash = Security.HashPassword("1234", salt);
                Csv.AppendUser(usersCsv, UsersHeader, email, hash, salt, DateTime.UtcNow);
                // Refresh cache
                var readUsers2 = Csv.ReadUsers(usersCsv, UsersHeader);
                users = readUsers2.ToDictionary(u => u.Email, u => u.PasswordHash);
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
                    if (LoginUser())
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
                Console.Write("Email: ");
                string email = Console.ReadLine();
                Console.Write("Пароль: ");
                string password = ReadPasswordMasked();

                var userRow = Csv.FindUserByEmail(usersCsv, UsersHeader, email);
                if (userRow != null)
                {
                    bool ok = Security.VerifyPassword(password, userRow.Salt, userRow.PasswordHash);
                    if (ok)
                    {
                        Console.WriteLine("Вхід успішний.");
                        currentUser = email;
                        success = true;
                        break;
                    }
                }

                attempts--;
                Console.WriteLine($"Невірний email/пароль. Залишилось спроб: {attempts}");
            } while (attempts > 0);

            return success;
        }

        static void RegisterUser()
        {
            Console.Write("Email (унікальний): ");
            string email = Console.ReadLine()?.Trim();
            if (string.IsNullOrWhiteSpace(email) || !email.Contains("@") || email.Length < 5)
            {
                Console.WriteLine("Некоректний email.");
                return;
            }
            if (Csv.FindUserByEmail(usersCsv, UsersHeader, email) != null)
            {
                Console.WriteLine("Користувач з таким email вже існує.");
                return;
            }

            Console.Write("Пароль: ");
            string pass = ReadPasswordMasked();
            if (string.IsNullOrWhiteSpace(pass))
            {
                Console.WriteLine("Пароль не може бути порожнім.");
                return;
            }

            var salt = Security.GenerateSalt();
            var hash = Security.HashPassword(pass, salt);

            Csv.AppendUser(usersCsv, UsersHeader, email, hash, salt, DateTime.UtcNow);

            // Refresh cache
            var readUsers = Csv.ReadUsers(usersCsv, UsersHeader);
            users = readUsers.ToDictionary(u => u.Email, u => u.PasswordHash);

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
                Console.WriteLine("7. Зберегти дані (CSV синхронізація)");
                Console.WriteLine("8. Видалити тестові дані");
                Console.WriteLine("9. Порівняти власне сортування з List.Sort()");
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
                        SyncCachesFromCsv(); // reload from CSV to memory
                        Console.WriteLine("Синхронізовано з CSV.");
                        break;
                    case "8":
                        ClearTestData();
                        break;
                    case "9":
                        CompareSorting();
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

        //Bubble Sort
        static List<Product> BubbleSortByPrice(List<Product> list)
        {
            List<Product> sorted = new List<Product>(list); // копія

            int n = sorted.Count;
            for (int i = 0; i < n - 1; i++)
            {
                for (int j = 0; j < n - i - 1; j++)
                {
                    if (sorted[j].Price > sorted[j + 1].Price)
                    {
                        Product temp = sorted[j];
                        sorted[j] = sorted[j + 1];
                        sorted[j + 1] = temp;
                    }
                }
            }
            return sorted;
        }

        //Порівняння з List.Sort()
        static void CompareSorting()
        {
            if (products.Count == 0)
            {
                Console.WriteLine("Немає продуктів для сортування.");
                return;
            }

            Console.WriteLine("\n=== Порівняння Bubble Sort і List.Sort ===\n");

            // Bubble Sort
            List<Product> bubbleSorted = BubbleSortByPrice(products);

            // List.Sort()
            List<Product> defaultSorted = new List<Product>(products);
            defaultSorted.Sort((a, b) => a.Price.CompareTo(b.Price));

            Console.WriteLine("=== Bubble Sort результат ===");
            foreach (var p in bubbleSorted)
                Console.WriteLine($"{p.Id}. {p.Name} — {p.Price}");

            Console.WriteLine("\n=== List.Sort результат ===");
            foreach (var p in defaultSorted)
                Console.WriteLine($"{p.Id}. {p.Name} — {p.Price}");

            Console.WriteLine("\nПорівняння завершено.\n");
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
            int id = Csv.NextId(productsCsv, ProductsHeader, 0); // generator scans file
            var product = new Product(id, name, price, count, active, DateTime.UtcNow);

            Csv.AppendProduct(productsCsv, ProductsHeader, product);

            // refresh caches
            products = Csv.ReadProducts(productsCsv, ProductsHeader);

            Console.WriteLine("Товар додано.");
        }

        static void ListProducts()
        {
            products = Csv.ReadProducts(productsCsv, ProductsHeader);
            Console.WriteLine("=== Список товарів ===");
            if (products.Count == 0)
            {
                Console.WriteLine("Порожньо.");
                return;
            }
            Format.PrintProductsTable(products);
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
            if (!string.IsNullOrWhiteSpace(priceStr) && double.TryParse(priceStr, NumberStyles.Any, CultureInfo.InvariantCulture, out double newPrice))
                prod.Price = newPrice;

            Console.Write("Нова кількість (Enter - без змін): ");
            string cntStr = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(cntStr) && int.TryParse(cntStr, out int newCnt))
                prod.AvailableCount = newCnt;

            // Rewrite file: read -> modify -> rewrite
            var list = Csv.ReadProducts(productsCsv, ProductsHeader);
            for (int i = 0; i < list.Count; i++)
                if (list[i].Id == prod.Id) { list[i] = prod; break; }
            Csv.RewriteProducts(productsCsv, ProductsHeader, list);

            products = Csv.ReadProducts(productsCsv, ProductsHeader);
            Console.WriteLine("Збережено.");
        }

        static void DeleteProduct()
        {
            ListProducts();
            if (products.Count == 0) return;
            int id = GetIntInputWithPrompt("Введіть ID товару для видалення (0 - відміна):");
            if (id == 0) return;

            var list = Csv.ReadProducts(productsCsv, ProductsHeader);
            int before = list.Count;
            list = list.Where(p => p.Id != id).ToList();
            if (list.Count == before)
            {
                Console.WriteLine("Не знайдено.");
                return;
            }

            Csv.RewriteProducts(productsCsv, ProductsHeader, list);
            products = Csv.ReadProducts(productsCsv, ProductsHeader);

            Console.WriteLine("Товар видалено.");
        }

        //кліент
        static void ClientsMenu()
        {
            while (true)
            {
                Console.WriteLine("\n=== Меню клієнтів ===");
                Console.WriteLine("1. Додати клієнтів");
                Console.WriteLine("2. Показати всіх");
                Console.WriteLine("3. Пошук клієнта");
                Console.WriteLine("4. Видалити клієнта");
                Console.WriteLine("5. Редагувати клієнта");
                Console.WriteLine("6. Сортувати за ім’ям");
                Console.WriteLine("0. Назад");

                Console.Write("Ваш вибір: ");
                string ch = Console.ReadLine();

                switch (ch)
                {
                    case "1": AddClientInteractive(); break;
                    case "2": ListClients(); break;
                    case "3": SearchClient(); break;
                    case "4": DeleteClient(); break;
                    case "5": EditClient(); break;
                    case "6": SortClientsByName(); break;
                    case "0": return;
                    default: Console.WriteLine("Невірний вибір."); break;
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

            int id = Csv.NextId(clientsCsv, ClientsHeader, 0);
            var c = new Client(id, name.Trim(), phone?.Trim() ?? "", DateTime.UtcNow);

            Csv.AppendClient(clientsCsv, ClientsHeader, c);
            clients = Csv.ReadClients(clientsCsv, ClientsHeader);

            Console.WriteLine("Клієнта додано.");
        }

        static void ListClients()
        {
            clients = Csv.ReadClients(clientsCsv, ClientsHeader);
            Console.WriteLine("=== Клієнти ===");
            if (clients.Count == 0) { Console.WriteLine("Порожньо."); return; }
            Format.PrintClientsTable(clients);
        }

        static void EditClient()
        {
            Console.Write("Введіть ID клієнта для редагування: ");
            if (!int.TryParse(Console.ReadLine(), out int id))
            {
                Console.WriteLine("Некоректний ввід.");
                return;
            }

            var all = Csv.ReadClients(clientsCsv, ClientsHeader);
            int index = all.FindIndex(x => x.Id == id);
            if (index == -1)
            {
                Console.WriteLine("Клієнта з таким ID не знайдено.");
                return;
            }

            Client c = all[index];
            Console.WriteLine("Редагування клієнта:");
            Console.WriteLine(c.ToString());

            Console.Write("Нове ім'я (Enter щоб пропустити): ");
            string newName = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(newName))
                c.FullName = newName;

            Console.Write("Новий телефон (Enter щоб пропустити): ");
            string newPhone = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(newPhone))
                c.Phone = newPhone;

            Console.Write("Нова дата приєднання (Enter щоб пропустити, формат yyyy-MM-dd): ");
            string newDate = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(newDate) &&
                DateTime.TryParse(newDate, out DateTime jd))
            {
                c.Joined = jd;
            }

            all[index] = c;
            Csv.RewriteClients(clientsCsv, ClientsHeader, all);
            clients = Csv.ReadClients(clientsCsv, ClientsHeader);

            Console.WriteLine("Клієнт успішно оновлений!");
        }

        static void DeleteClient()
        {
            Console.WriteLine("=== Видалити клієнта ===");
            clients = Csv.ReadClients(clientsCsv, ClientsHeader);
            bookings = Csv.ReadBookings(bookingsCsv, BookingsHeader);

            if (clients.Count == 0)
            {
                Console.WriteLine("Список клієнтів порожній.");
                return;
            }

            ListClients();
            int id = GetIntInputWithPrompt("Введіть ID клієнта для видалення (0 - відміна):");
            if (id == 0) return;

            int idx = clients.FindIndex(c => c.Id == id);
            if (idx == -1)
            {
                Console.WriteLine("Клієнта з таким ID не знайдено.");
                return;
            }

            int relatedBookings = bookings.Count(b => b.ClientId == id);

            if (relatedBookings > 0)
            {
                Console.WriteLine($"У клієнта є {relatedBookings} бронюван(ь). Видалити клієнта призведе до видалення цих бронювань і повернення товару на склад.");
                Console.Write("Підтвердьте видалення (y/n): ");
                string conf = Console.ReadLine().Trim().ToLower();
                if (conf != "y" && conf != "yes")
                {
                    Console.WriteLine("Операцію скасовано.");
                    return;
                }

                // Для кожного бронювання повертаємо товар на склад
                var productsAll = Csv.ReadProducts(productsCsv, ProductsHeader);
                var bookingsAll = Csv.ReadBookings(bookingsCsv, BookingsHeader);
                for (int i = bookingsAll.Count - 1; i >= 0; i--)
                {
                    if (bookingsAll[i].ClientId == id)
                    {
                        int pid = bookingsAll[i].ProductId;
                        int qty = bookingsAll[i].Quantity;
                        for (int j = 0; j < productsAll.Count; j++)
                        {
                            if (productsAll[j].Id == pid)
                            {
                                var prod = productsAll[j];
                                prod.AvailableCount += qty;
                                productsAll[j] = prod;
                                break;
                            }
                        }
                        bookingsAll.RemoveAt(i);
                    }
                }
                Csv.RewriteProducts(productsCsv, ProductsHeader, productsAll);
                Csv.RewriteBookings(bookingsCsv, BookingsHeader, bookingsAll);
            }
            else
            {
                Console.Write("Підтвердьте видалення клієнта (y/n): ");
                string conf = Console.ReadLine().Trim().ToLower();
                if (conf != "y" && conf != "yes")
                {
                    Console.WriteLine("Операцію скасовано.");
                    return;
                }
            }

            var newClients = clients.Where(c => c.Id != id).ToList();
            Csv.RewriteClients(clientsCsv, ClientsHeader, newClients);

            // refresh caches
            products = Csv.ReadProducts(productsCsv, ProductsHeader);
            clients = Csv.ReadClients(clientsCsv, ClientsHeader);
            bookings = Csv.ReadBookings(bookingsCsv, BookingsHeader);

            Console.WriteLine("Клієнта (і пов'язані бронювання, якщо були) видалено.");
        }

        static void SortClientsByName()
        {
            clients = Csv.ReadClients(clientsCsv, ClientsHeader);
            for (int i = 0; i < clients.Count - 1; i++)
            {
                for (int j = 0; j < clients.Count - i - 1; j++)
                {
                    if (string.Compare(clients[j].FullName, clients[j + 1].FullName, StringComparison.OrdinalIgnoreCase) > 0)
                    {
                        var temp = clients[j];
                        clients[j] = clients[j + 1];
                        clients[j + 1] = temp;
                    }
                }
            }
            Console.WriteLine("Список клієнтів відсортовано.");
            Format.PrintClientsTable(clients);
        }

//бронь
        static void BookingMenu()
        {
            while (true)
            {
                Console.WriteLine("\n=== Меню бронювань ===");
                Console.WriteLine("1. Створити бронювання");
                Console.WriteLine("2. Переглянути бронювання");
                Console.WriteLine("3. Редагувати бронювання");
                Console.WriteLine("4. Видалення бронювання");
                Console.WriteLine("0. Назад");
                Console.Write("Вибір: ");
                string c = Console.ReadLine();
                if (c == "0") return;

                switch (c)
                {
                    case "1": CreateBooking(); break;
                    case "2": ListBookings(); break;
                    case "3": EditBooking(); break;
                    case "4": DeleteBooking(); break;
                    default: Console.WriteLine("Невірний ввід."); break;
                }
            }
        }
        static void CreateBooking()
        {
            products = Csv.ReadProducts(productsCsv, ProductsHeader);
            clients = Csv.ReadClients(clientsCsv, ClientsHeader);

            if (products.Count == 0)
            {
                Console.WriteLine("Немає товарів для бронювання.");
                return;
            }

            Console.Write("Ім'я клієнта (0 - відміна): ");
            string clientName = Console.ReadLine();
            if (clientName == "0") return;

            Client client = clients.FirstOrDefault(c => c.FullName.Equals(clientName, StringComparison.OrdinalIgnoreCase));
            if (client.Id == 0)
            {
                int cid = Csv.NextId(clientsCsv, ClientsHeader, 0);
                Console.Write("Телефон: ");
                string phone = Console.ReadLine();
                var newClient = new Client(cid, clientName.Trim(), phone?.Trim() ?? "", DateTime.UtcNow);
                Csv.AppendClient(clientsCsv, ClientsHeader, newClient);
                clients = Csv.ReadClients(clientsCsv, ClientsHeader);
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
            int bid = Csv.NextId(bookingsCsv, BookingsHeader, 0);

            var booking = new Booking(bid, client.Id, pid, qty, total, DateTime.UtcNow);

            // Decrease product stock, then append booking, rewrite products
            var productsAll = Csv.ReadProducts(productsCsv, ProductsHeader);
            for (int i = 0; i < productsAll.Count; i++)
            {
                if (productsAll[i].Id == prod.Id)
                {
                    var updated = productsAll[i];
                    updated.AvailableCount -= qty;
                    productsAll[i] = updated;
                    break;
                }
            }
            Csv.RewriteProducts(productsCsv, ProductsHeader, productsAll);
            Csv.AppendBooking(bookingsCsv, BookingsHeader, booking);

            // refresh caches
            products = Csv.ReadProducts(productsCsv, ProductsHeader);
            bookings = Csv.ReadBookings(bookingsCsv, BookingsHeader);

            Console.WriteLine("Бронювання створено.");
        }
        static void ListBookings()
        {
            bookings = Csv.ReadBookings(bookingsCsv, BookingsHeader);
            Console.WriteLine("=== Бронювання ===");
            if (bookings.Count == 0) { Console.WriteLine("Порожньо."); return; }
            Format.PrintBookingsTable(bookings);
        }
        static void EditBooking()
        {
            Console.WriteLine("=== Редагувати бронювання ===");
            bookings = Csv.ReadBookings(bookingsCsv, BookingsHeader);
            products = Csv.ReadProducts(productsCsv, ProductsHeader);
            clients = Csv.ReadClients(clientsCsv, ClientsHeader);

            if (bookings.Count == 0)
            {
                Console.WriteLine("Список бронювань порожній.");
                return;
            }

            ListBookings();
            int bid = GetIntInputWithPrompt("Введіть ID бронювання для редагування (0 - відміна):");
            if (bid == 0) return;

            int bidx = bookings.FindIndex(b => b.BookingId == bid);
            if (bidx == -1)
            {
                Console.WriteLine("Бронювання з таким ID не знайдено.");
                return;
            }

            var booking = bookings[bidx];
            Console.WriteLine("Поточне бронювання: " + booking.ToString());

            // Temporarily restore product stock for current booking
            var productsAll = Csv.ReadProducts(productsCsv, ProductsHeader);
            for (int i = 0; i < productsAll.Count; i++)
            {
                if (productsAll[i].Id == booking.ProductId)
                {
                    var p = productsAll[i];
                    p.AvailableCount += booking.Quantity;
                    productsAll[i] = p;
                    break;
                }
            }

            // Change client by name
            Console.Write("Новий клієнт (ім'я) (Enter - без змін): ");
            string newClientName = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(newClientName))
            {
                int clientId = clients.FirstOrDefault(c => c.FullName.Equals(newClientName, StringComparison.OrdinalIgnoreCase)).Id;
                if (clientId == 0)
                {
                    Console.Write("Клієнта не знайдено. Введіть телефон для створення нового клієнта: ");
                    string phone = Console.ReadLine();
                    clientId = Csv.NextId(clientsCsv, ClientsHeader, 0);
                    var newClient = new Client(clientId, newClientName.Trim(), phone?.Trim() ?? "", DateTime.UtcNow);
                    Csv.AppendClient(clientsCsv, ClientsHeader, newClient);
                    clients = Csv.ReadClients(clientsCsv, ClientsHeader);
                    Console.WriteLine($"Клієнт доданий з ID={clientId}.");
                }
                booking.ClientId = clientId;
            }

            // Change product
            Console.Write("Змінити товар? (y/n): ");
            string changeProd = Console.ReadLine().Trim().ToLower();
            int newProductId = booking.ProductId;
            if (changeProd == "y" || changeProd == "yes")
            {
                ListProducts();
                newProductId = GetIntInputWithPrompt("Введіть ID нового товару (0 - відміна зміни товару):");
                if (newProductId == 0)
                {
                    Console.WriteLine("Зміна товару скасована, буде використано старий товар.");
                    newProductId = booking.ProductId;
                }
            }

            // Change quantity
            Console.Write("Нова кількість (Enter - без змін): ");
            string qtyStr = Console.ReadLine();
            int newQty = booking.Quantity;
            if (!string.IsNullOrWhiteSpace(qtyStr))
            {
                if (!int.TryParse(qtyStr, out newQty) || newQty <= 0)
                {
                    Console.WriteLine("Некоректна кількість. Операцію скасовано.");
                    return;
                }
            }

            // Validate new product and stock
            productsAll = Csv.ReadProducts(productsCsv, ProductsHeader);
            int prodIndex = productsAll.FindIndex(p => p.Id == newProductId);
            if (prodIndex == -1)
            {
                Console.WriteLine("Вказаний продукт не знайдено. Операцію скасовано.");
                return;
            }

            if (productsAll[prodIndex].AvailableCount < newQty)
            {
                Console.WriteLine($"Недостатньо товару на складі. Доступно: {productsAll[prodIndex].AvailableCount}. Операцію скасовано.");
                return;
            }

            // Reserve stock for new booking
            var prod2 = productsAll[prodIndex];
            prod2.AvailableCount -= newQty;
            productsAll[prodIndex] = prod2;
            Csv.RewriteProducts(productsCsv, ProductsHeader, productsAll);

            // Update booking record in file
            var bookingsAll = Csv.ReadBookings(bookingsCsv, BookingsHeader);
            int idx = bookingsAll.FindIndex(b => b.BookingId == booking.BookingId);
            booking.ProductId = newProductId;
            booking.Quantity = newQty;
            booking.Total = productsAll[prodIndex].Price * newQty;
            booking.BookingDate = DateTime.UtcNow;
            if (idx != -1) bookingsAll[idx] = booking;

            Csv.RewriteBookings(bookingsCsv, BookingsHeader, bookingsAll);

            // refresh caches
            products = Csv.ReadProducts(productsCsv, ProductsHeader);
            bookings = Csv.ReadBookings(bookingsCsv, BookingsHeader);

            Console.WriteLine("Бронювання оновлено.");
        }
        static void DeleteBooking()
        {
            Console.WriteLine("=== Видалити бронювання ===");
            bookings = Csv.ReadBookings(bookingsCsv, BookingsHeader);
            products = Csv.ReadProducts(productsCsv, ProductsHeader);

            if (bookings.Count == 0)
            {
                Console.WriteLine("Список бронювань порожній.");
                return;
            }

            ListBookings();
            int bid = GetIntInputWithPrompt("Введіть ID бронювання для видалення (0 - відміна):");
            if (bid == 0) return;

            int bidx = bookings.FindIndex(b => b.BookingId == bid);
            if (bidx == -1)
            {
                Console.WriteLine("Бронювання з таким ID не знайдено.");
                return;
            }

            var booking = bookings[bidx];
            Console.WriteLine("Поточне бронювання: " + booking.ToString());
            Console.Write("Підтвердьте видалення бронювання (y/n): ");
            string conf = Console.ReadLine().Trim().ToLower();
            if (conf != "y" && conf != "yes")
            {
                Console.WriteLine("Операцію скасовано.");
                return;
            }

            // Return product to stock
            var productsAll = Csv.ReadProducts(productsCsv, ProductsHeader);
            for (int i = 0; i < productsAll.Count; i++)
            {
                if (productsAll[i].Id == booking.ProductId)
                {
                    var prod = productsAll[i];
                    prod.AvailableCount += booking.Quantity;
                    productsAll[i] = prod;
                    break;
                }
            }
            Csv.RewriteProducts(productsCsv, ProductsHeader, productsAll);

            // Remove booking and rewrite
            var bookingsAll = Csv.ReadBookings(bookingsCsv, BookingsHeader).Where(b => b.BookingId != bid).ToList();
            Csv.RewriteBookings(bookingsCsv, BookingsHeader, bookingsAll);

            // refresh caches
            products = Csv.ReadProducts(productsCsv, ProductsHeader);
            bookings = Csv.ReadBookings(bookingsCsv, BookingsHeader);

            Console.WriteLine("Бронювання видалено, товар повернено на склад.");
        }

//пошук
        static void SearchMenu()
        {
            Console.WriteLine("\n=== Пошук ===");
            Console.WriteLine("1. Пошук товару");
            Console.WriteLine("2. Пошук клієнта");
            Console.WriteLine("3. Пошук броні");
            Console.Write("Вибір: ");
            string c = Console.ReadLine();
            switch (c)
            {
                case "1": SearchProduct(); break;
                case "2": SearchClient(); break;
                case "3": SearchBooking(); break;
                default: Console.WriteLine("Невірний ввід."); break;
            }
        }

        static void SearchProduct()
        {
            Console.Write("Пошук товару: ");
            string q = Console.ReadLine() ?? "";
            var rows = Csv.ReadProducts(productsCsv, ProductsHeader)
                          .Where(p => (p.Name ?? "").IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0)
                          .ToList();
            if (rows.Count == 0) Console.WriteLine("Нічого не знайдено.");
            else Format.PrintProductsTable(rows);
        }

        static void SearchClient()
        {
            Console.Write("Пошук клієнта: ");
            string q = Console.ReadLine() ?? "";
            var rows = Csv.ReadClients(clientsCsv, ClientsHeader)
                          .Where(c => (c.FullName ?? "").IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0)
                          .ToList();
            if (rows.Count == 0) Console.WriteLine("Нічого не знайдено.");
            else Format.PrintClientsTable(rows);
        }
        static void SearchBooking()
        {
            Console.Write("Введіть ID клієнта або товару: ");
            string q = Console.ReadLine() ?? "";
            var rows = Csv.ReadBookings(bookingsCsv, BookingsHeader)
                          .Where(b => b.ClientId.ToString() == q || b.ProductId.ToString() == q)
                          .ToList();
            if (rows.Count == 0) Console.WriteLine("Бронювання не знайдено.");
            else Format.PrintBookingsTable(rows);
        }

//стат
        static void ShowStatistics()
        {
            var ps = Csv.ReadProducts(productsCsv, ProductsHeader);
            Console.WriteLine("\n=== Статистика по товарам ===");
            if (ps.Count == 0)
            {
                Console.WriteLine("Немає товарів.");
                return;
            }

            double totalValue = ps.Sum(p => p.Price * p.AvailableCount);
            int totalCount = ps.Sum(p => p.AvailableCount);
            double minPrice = ps.Min(p => p.Price);
            double maxPrice = ps.Max(p => p.Price);

            double avgPrice = ps.Average(p => p.Price);
            double avgCount = ps.Count > 0 ? (double)totalCount / ps.Count : 0;

            Console.WriteLine($"Кількість товарів (позицій): {ps.Count}");
            Console.WriteLine($"Загальна вартість (ціна * кількість): {totalValue.ToString("F2", CultureInfo.InvariantCulture)}");
            Console.WriteLine($"Середня ціна товару: {avgPrice.ToString("F2", CultureInfo.InvariantCulture)}");
            Console.WriteLine($"Середня кількість на позицію: {avgCount.ToString("F2", CultureInfo.InvariantCulture)}");
            Console.WriteLine($"Мінімальна ціна: {minPrice.ToString("F2", CultureInfo.InvariantCulture)}");
            Console.WriteLine($"Максимальна ціна: {maxPrice.ToString("F2", CultureInfo.InvariantCulture)}");

            var cs = Csv.ReadClients(clientsCsv, ClientsHeader);
            var bs = Csv.ReadBookings(bookingsCsv, BookingsHeader);
            Console.WriteLine($"Кількість клієнтів: {cs.Count}");
            Console.WriteLine($"Кількість бронювань: {bs.Count}");
        }

//звіт
        static void PrintReport()
        {
            var ps = Csv.ReadProducts(productsCsv, ProductsHeader);
            var cs = Csv.ReadClients(clientsCsv, ClientsHeader);
            var bs = Csv.ReadBookings(bookingsCsv, BookingsHeader);

            Console.WriteLine("\n==== ЗВІТ СИСТЕМИ ====");
            Console.WriteLine($"Дата: {DateTime.Now}");

            Console.WriteLine("\n--- Товари ---");
            if (ps.Count == 0) Console.WriteLine("Немає товарів.");
            else Format.PrintProductsTable(ps);

            Console.WriteLine("\n--- Клієнти ---");
            if (cs.Count == 0) Console.WriteLine("Немає клієнтів.");
            else Format.PrintClientsTable(cs);

            Console.WriteLine("\n--- Бронювання ---");
            if (bs.Count == 0) Console.WriteLine("Немає бронювань.");
            else Format.PrintBookingsTable(bs);

            Console.WriteLine("\n--- Підсумки ---");
            double productsTotalValue = ps.Sum(p => p.Price * p.AvailableCount);
            Console.WriteLine($"Усього товарів (позицій): {ps.Count}");
            Console.WriteLine($"Усього клієнтів: {cs.Count}");
            Console.WriteLine($"Усього бронювань: {bs.Count}");
            Console.WriteLine($"Загальна вартість всіх товарів (ціна*кількість): {productsTotalValue.ToString("F2", CultureInfo.InvariantCulture)}");

            Console.WriteLine("\nНатисніть будь-яку клавішу для продовження...");
            Console.ReadKey();
        }

//обробка вводу
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
                if (double.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out double v))
                    return v;
                Console.WriteLine("Введіть коректне число (наприклад 123.45).");
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
//csv files
        static void EnsureCsvFiles()
        {
            if (!Directory.Exists(dataDir)) Directory.CreateDirectory(dataDir);

            Csv.EnsureCsvWithHeader(productsCsv, ProductsHeader);
            Csv.EnsureCsvWithHeader(clientsCsv, ClientsHeader);
            Csv.EnsureCsvWithHeader(bookingsCsv, BookingsHeader);
            Csv.EnsureCsvWithHeader(usersCsv, UsersHeader);
        }

        static bool FileHasAtLeastOneDataRow(string path)
        {
            if (!File.Exists(path)) return false;
            try
            {
                using var sr = new StreamReader(path);
                string header = sr.ReadLine(); // skip header
                string line = sr.ReadLine();
                return !string.IsNullOrWhiteSpace(line);
            }
            catch { return false; }
        }

        static void SyncCachesFromCsv()
        {
            products = Csv.ReadProducts(productsCsv, ProductsHeader);
            clients = Csv.ReadClients(clientsCsv, ClientsHeader);
            bookings = Csv.ReadBookings(bookingsCsv, BookingsHeader);
            var readUsers = Csv.ReadUsers(usersCsv, UsersHeader);
            users = readUsers.ToDictionary(u => u.Email, u => u.PasswordHash);
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
                    EnsureCsvFiles(); // recreate empty with headers
                    Console.WriteLine("Дані видалено.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Помилка при видаленні файлів: " + ex.Message);
                }
            }
            else Console.WriteLine("Отмена.");
        }

//CSV MODULE
        static class Csv
        {
            // Entities for users
            public class UserRow
            {
                public string Email { get; set; }
                public string PasswordHash { get; set; }
                public string Salt { get; set; }
                public DateTime CreatedAt { get; set; }
            }

            public static void EnsureCsvWithHeader(string path, string[] header)
            {
                try
                {
                    if (!File.Exists(path))
                    {
                        using var sw = new StreamWriter(path, false, Encoding.UTF8);
                        sw.WriteLine(string.Join(",", header));
                    }
                    else
                    {
                        // Validate header; if missing or wrong, fix it (do not drop data rows)
                        var lines = File.ReadAllLines(path).ToList();
                        if (lines.Count == 0 || !HeaderMatches(lines[0], header))
                        {
                            if (lines.Count == 0)
                            {
                                lines.Insert(0, string.Join(",", header));
                            }
                            else
                            {
                                // Replace first line header; keep rest
                                lines[0] = string.Join(",", header);
                            }
                            File.WriteAllLines(path, lines, Encoding.UTF8);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Помилка ініціалізації CSV '{path}': {ex.Message}");
                }
            }

            static bool HeaderMatches(string line, string[] header)
            {
                if (string.IsNullOrWhiteSpace(line)) return false;
                var cols = SplitCsv(line);
                if (cols.Length != header.Length) return false;
                for (int i = 0; i < header.Length; i++)
                    if (!string.Equals(cols[i], header[i], StringComparison.Ordinal)) return false;
                return true;
            }

            static string[] SplitCsv(string line)
            {
                // Simple CSV splitting by comma without quotes handling (fields here are simple)
                return (line ?? "").Split(',');
            }

            // Safe parse helpers with invariant culture
            static bool TryParseInt(string s, out int v) => int.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out v);
            static bool TryParseDouble(string s, out double v) => double.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out v);
            static bool TryParseBool(string s, out bool v) => bool.TryParse(s, out v);
            static bool TryParseDate(string s, out DateTime v) => DateTime.TryParse(s, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out v);

            // Read operations with error resilience
            public static List<Product> ReadProducts(string path, string[] header)
            {
                var result = new List<Product>();
                EnsureCsvWithHeader(path, header);
                try
                {
                    using var sr = new StreamReader(path, Encoding.UTF8);
                    sr.ReadLine(); // skip header
                    int lineNum = 1;
                    while (!sr.EndOfStream)
                    {
                        string line = sr.ReadLine();
                        lineNum++;
                        if (string.IsNullOrWhiteSpace(line)) continue;
                        var cols = SplitCsv(line);
                        if (cols.Length != header.Length)
                        {
                            Warn(path, lineNum, "Неправильна кількість полів.");
                            continue;
                        }
                        if (!TryParseInt(cols[0], out int id) ||
                            !TryParseDouble(cols[2], out double price) ||
                            !TryParseInt(cols[3], out int cnt) ||
                            !TryParseBool(cols[4], out bool isActive) ||
                            !TryParseDate(cols[5], out DateTime added))
                        {
                            Warn(path, lineNum, "Некоректні числа/дата.");
                            continue;
                        }
                        result.Add(new Product(id, cols[1], price, cnt, isActive, added));
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Помилка читання продуктів: {ex.Message}");
                }
                return result;
            }

            public static List<Client> ReadClients(string path, string[] header)
            {
                var result = new List<Client>();
                EnsureCsvWithHeader(path, header);
                try
                {
                    using var sr = new StreamReader(path, Encoding.UTF8);
                    sr.ReadLine();
                    int lineNum = 1;
                    while (!sr.EndOfStream)
                    {
                        string line = sr.ReadLine();
                        lineNum++;
                        if (string.IsNullOrWhiteSpace(line)) continue;
                        var cols = SplitCsv(line);
                        if (cols.Length != header.Length)
                        {
                            Warn(path, lineNum, "Неправильна кількість полів.");
                            continue;
                        }
                        if (!TryParseInt(cols[0], out int id) ||
                            !TryParseDate(cols[3], out DateTime joined))
                        {
                            Warn(path, lineNum, "Некоректні числа/дата.");
                            continue;
                        }
                        result.Add(new Client(id, cols[1], cols[2], joined));
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Помилка читання клієнтів: {ex.Message}");
                }
                return result;
            }

            public static List<Booking> ReadBookings(string path, string[] header)
            {
                var result = new List<Booking>();
                EnsureCsvWithHeader(path, header);
                try
                {
                    using var sr = new StreamReader(path, Encoding.UTF8);
                    sr.ReadLine();
                    int lineNum = 1;
                    while (!sr.EndOfStream)
                    {
                        string line = sr.ReadLine();
                        lineNum++;
                        if (string.IsNullOrWhiteSpace(line)) continue;
                        var cols = SplitCsv(line);
                        if (cols.Length != header.Length)
                        {
                            Warn(path, lineNum, "Неправильна кількість полів.");
                            continue;
                        }
                        bool ok =
                            TryParseInt(cols[0], out int bid) &&
                            TryParseInt(cols[1], out int cid) &&
                            TryParseInt(cols[2], out int pid) &&
                            TryParseInt(cols[3], out int qty) &&
                            TryParseDouble(cols[4], out double total) &&
                            TryParseDate(cols[5], out DateTime booked);
                        if (!ok)
                        {
                            Warn(path, lineNum, "Некоректні числа/дата.");
                            continue;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Помилка читання бронювань: {ex.Message}");
                }
                return result;
            }

            public static List<UserRow> ReadUsers(string path, string[] header)
            {
                var result = new List<UserRow>();
                EnsureCsvWithHeader(path, header);
                try
                {
                    using var sr = new StreamReader(path, Encoding.UTF8);
                    sr.ReadLine();
                    int lineNum = 1;
                    while (!sr.EndOfStream)
                    {
                        string line = sr.ReadLine();
                        lineNum++;
                        if (string.IsNullOrWhiteSpace(line)) continue;
                        var cols = SplitCsv(line);
                        if (cols.Length != header.Length)
                        {
                            Warn(path, lineNum, "Неправильна кількість полів.");
                            continue;
                        }
                        if (!TryParseDate(cols[3], out DateTime created))
                        {
                            Warn(path, lineNum, "Некоректна дата.");
                            created = DateTime.MinValue;
                        }
                        result.Add(new UserRow
                        {
                            Email = cols[0],
                            PasswordHash = cols[1],
                            Salt = cols[2],
                            CreatedAt = created
                        });
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Помилка читання користувачів: {ex.Message}");
                }
                return result;
            }

            static void Warn(string path, int lineNum, string message)
            {
                Console.WriteLine($"[CSV WARN] {Path.GetFileName(path)} рядок {lineNum}: {message}");
            }

            public static void AppendProduct(string path, string[] header, Product p)
            {
                EnsureCsvWithHeader(path, header);
                var line = string.Join(",", new[]
                {
                    p.Id.ToString(),
                    Escape(p.Name),
                    p.Price.ToString(CultureInfo.InvariantCulture),
                    p.AvailableCount.ToString(),
                    p.IsActive.ToString(),
                    p.AddedDate.ToString("o", CultureInfo.InvariantCulture)
                });
                AppendLine(path, line);
            }

            public static void AppendClient(string path, string[] header, Client c)
            {
                EnsureCsvWithHeader(path, header);
                var line = string.Join(",", new[]
                {
                    c.Id.ToString(),
                    Escape(c.FullName),
                    Escape(c.Phone),
                    c.Joined.ToString("o", CultureInfo.InvariantCulture)
                });
                AppendLine(path, line);
            }

            public static void AppendBooking(string path, string[] header, Booking b)
            {
                EnsureCsvWithHeader(path, header);
                var line = string.Join(",", new[]
                {
                    b.BookingId.ToString(),
                    b.ClientId.ToString(),
                    b.ProductId.ToString(),
                    b.Quantity.ToString(),
                    b.Total.ToString(CultureInfo.InvariantCulture),
                    b.BookingDate.ToString("o", CultureInfo.InvariantCulture)
                });
                AppendLine(path, line);
            }

            public static void AppendUser(string path, string[] header, string email, string hash, string salt, DateTime created)
            {
                EnsureCsvWithHeader(path, header);
                var line = string.Join(",", new[]
                {
                    Escape(email),
                    hash,
                    salt,
                    created.ToString("o", CultureInfo.InvariantCulture)
                });
                AppendLine(path, line);
            }

            static void AppendLine(string path, string line)
            {
                try
                {
                    using var sw = new StreamWriter(path, true, Encoding.UTF8);
                    sw.WriteLine(line);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Помилка дозапису '{Path.GetFileName(path)}': {ex.Message}");
                }
            }

// Rewrite operations
            public static void RewriteProducts(string path, string[] header, List<Product> list)
            {
                try
                {
                    using var sw = new StreamWriter(path, false, Encoding.UTF8);
                    sw.WriteLine(string.Join(",", header));
                    foreach (var p in list)
                    {
                        sw.WriteLine(string.Join(",", new[]
                        {
                            p.Id.ToString(),
                            Escape(p.Name),
                            p.Price.ToString(CultureInfo.InvariantCulture),
                            p.AvailableCount.ToString(),
                            p.IsActive.ToString(),
                            p.AddedDate.ToString("o", CultureInfo.InvariantCulture)
                        }));
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Помилка перезапису продуктів: {ex.Message}");
                }
            }

            public static void RewriteClients(string path, string[] header, List<Client> list)
            {
                try
                {
                    using var sw = new StreamWriter(path, false, Encoding.UTF8);
                    sw.WriteLine(string.Join(",", header));
                    foreach (var c in list)
                    {
                        sw.WriteLine(string.Join(",", new[]
                        {
                            c.Id.ToString(),
                            Escape(c.FullName),
                            Escape(c.Phone),
                            c.Joined.ToString("o", CultureInfo.InvariantCulture)
                        }));
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Помилка перезапису клієнтів: {ex.Message}");
                }
            }

            public static void RewriteBookings(string path, string[] header, List<Booking> list)
            {
                try
                {
                    using var sw = new StreamWriter(path, false, Encoding.UTF8);
                    sw.WriteLine(string.Join(",", header));
                    foreach (var b in list)
                    {
                        sw.WriteLine(string.Join(",", new[]
                        {
                            b.BookingId.ToString(),
                            b.ClientId.ToString(),
                            b.ProductId.ToString(),
                            b.Quantity.ToString(),
                            b.Total.ToString(CultureInfo.InvariantCulture),
                            b.BookingDate.ToString("o", CultureInfo.InvariantCulture)
                        }));
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Помилка перезапису бронювань: {ex.Message}");
                }
            }

// Find helpers
            public static UserRow FindUserByEmail(string path, string[] header, string email)
            {
                var all = ReadUsers(path, header);
                return all.FirstOrDefault(u => string.Equals(u.Email, email, StringComparison.OrdinalIgnoreCase));
            }

// ID generator
            public static int NextId(string path, string[] header, int idColumnIndex)
            {
                EnsureCsvWithHeader(path, header);
                int maxId = 0;
                try
                {
                    using var sr = new StreamReader(path, Encoding.UTF8);
                    sr.ReadLine(); // skip header
                    while (!sr.EndOfStream)
                    {
                        string line = sr.ReadLine();
                        if (string.IsNullOrWhiteSpace(line)) continue;
                        var cols = SplitCsv(line);
                        if (cols.Length != header.Length) continue;
                        if (int.TryParse(cols[idColumnIndex], NumberStyles.Integer, CultureInfo.InvariantCulture, out int id))
                        {
                            if (id > maxId) maxId = id;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Попередження: генератор ID помилка читання '{Path.GetFileName(path)}': {ex.Message}. Використано поточний max={maxId}.");
                }
                return maxId + 1;
            }

            static string Escape(string s)
            {
                // Basic escaping: trim and replace commas with semicolons to avoid field split issues
                if (s == null) return "";
                s = s.Trim();
                return s.Replace(",", ";");
            }
        }

//секуріті
        static class Security
        {
            public static string GenerateSalt(int size = 16)
            {
                var rng = RandomNumberGenerator.Create();
                byte[] saltBytes = new byte[size];
                rng.GetBytes(saltBytes);
                return Convert.ToBase64String(saltBytes);
            }

            public static string HashPassword(string password, string salt)
            {
                using var sha256 = SHA256.Create();
                var bytes = Encoding.UTF8.GetBytes(password + salt);
                var hash = sha256.ComputeHash(bytes);
                return Convert.ToBase64String(hash);
            }

            public static bool VerifyPassword(string password, string salt, string expectedHash)
            {
                var hash = HashPassword(password, salt);
                return string.Equals(hash, expectedHash, StringComparison.Ordinal);
            }
        }
        static class Format
        {
            public static void PrintProductsTable(List<Product> items)
            {
                var cols = new[] { "Id", "Name", "Price", "Count", "Active", "Added" };
                int[] widths = ComputeWidths(cols, items.Select(p => new[]
                {
                    p.Id.ToString(),
                    p.Name ?? "",
                    p.Price.ToString("F2", CultureInfo.InvariantCulture),
                    p.AvailableCount.ToString(),
                    p.IsActive.ToString(),
                    p.AddedDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)
                }));

                PrintHeader(cols, widths);
                foreach (var p in items)
                {
                    PrintRow(new[]
                    {
                        p.Id.ToString(),
                        p.Name ?? "",
                        p.Price.ToString("F2", CultureInfo.InvariantCulture),
                        p.AvailableCount.ToString(),
                        p.IsActive.ToString(),
                        p.AddedDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)
                    }, widths);
                }
                PrintSeparator(widths);
            }

            public static void PrintClientsTable(List<Client> items)
            {
                var cols = new[] { "Id", "FullName", "Phone", "Joined" };
                int[] widths = ComputeWidths(cols, items.Select(c => new[]
                {
                    c.Id.ToString(),
                    c.FullName ?? "",
                    c.Phone ?? "",
                    c.Joined.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)
                }));

                PrintHeader(cols, widths);
                foreach (var c in items)
                {
                    PrintRow(new[]
                    {
                        c.Id.ToString(),
                        c.FullName ?? "",
                        c.Phone ?? "",
                        c.Joined.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)
                    }, widths);
                }
                PrintSeparator(widths);
            }

            public static void PrintBookingsTable(List<Booking> items)
            {
                var cols = new[] { "BookingId", "ClientId", "ProductId", "Qty", "Total", "Date" };
                int[] widths = ComputeWidths(cols, items.Select(b => new[]
                {
                    b.BookingId.ToString(),
                    b.ClientId.ToString(),
                    b.ProductId.ToString(),
                    b.Quantity.ToString(),
                    b.Total.ToString("F2", CultureInfo.InvariantCulture),
                    b.BookingDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)
                }));

                PrintHeader(cols, widths);
                foreach (var b in items)
                {
                    PrintRow(new[]
                    {
                        b.BookingId.ToString(),
                        b.ClientId.ToString(),
                        b.ProductId.ToString(),
                        b.Quantity.ToString(),
                        b.Total.ToString("F2", CultureInfo.InvariantCulture),
                        b.BookingDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)
                    }, widths);
                }
                PrintSeparator(widths);
            }

            static int[] ComputeWidths(string[] headers, IEnumerable<string[]> rows)
            {
                int[] widths = headers.Select(h => h.Length).ToArray();
                foreach (var row in rows)
                {
                    for (int i = 0; i < headers.Length; i++)
                    {
                        widths[i] = Math.Max(widths[i], (row[i] ?? "").Length);
                    }
                }
                // add padding
                for (int i = 0; i < widths.Length; i++) widths[i] += 2;
                return widths;
            }

            static void PrintHeader(string[] headers, int[] widths)
            {
                PrintSeparator(widths);
                PrintRow(headers, widths);
                PrintSeparator(widths);
            }

            static void PrintRow(string[] cells, int[] widths)
            {
                var sb = new StringBuilder();
                sb.Append("|");
                for (int i = 0; i < cells.Length; i++)
                {
                    sb.Append(" " + (cells[i] ?? "").PadRight(widths[i] - 1));
                    sb.Append("|");
                }
                Console.WriteLine(sb.ToString());
            }

            static void PrintSeparator(int[] widths)
            {
                var sb = new StringBuilder();
                sb.Append("+");
                foreach (var w in widths)
                {
                    sb.Append(new string('-', w));
                    sb.Append("+");
                }
                Console.WriteLine(sb.ToString());
            }
        }
    }
}