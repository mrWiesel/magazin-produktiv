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
                        SaveData();
                        Console.WriteLine("Збережено.");
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
                        // міняємо елементи місцями
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

            int id = clients.Count > 0 ? clients.Max(c => c.Id) + 1 : 1;
            clients.Add(new Client(id, name, phone, DateTime.Now));
            SaveData();
            Console.WriteLine("Клієнта додано.");
        }
        static void ListClients()
        {
            Console.WriteLine("=== Клієнти ===");
            if (clients.Count == 0) { Console.WriteLine("Порожньо."); return; }
            foreach (var c in clients) Console.WriteLine(c.ToString());
        }

        static void EditClient()
        {
            Console.Write("Введіть ID клієнта для редагування: ");
            if (!int.TryParse(Console.ReadLine(), out int id))
            {
                Console.WriteLine("Некоректний ввід.");
                return;
            }

            int index = -1;
            for (int i = 0; i < clients.Count; i++)
            {
                if (clients[i].Id == id)
                {
                    index = i;
                    break;
                }
            }

            if (index == -1)
            {
                Console.WriteLine("Клієнта з таким ID не знайдено.");
                return;
            }

            Client c = clients[index];

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

            // Оновлюємо в списку
            clients[index] = c;

            SaveData();

            Console.WriteLine("Клієнт успішно оновлений!");
        }

        static void DeleteClient()
        {
            Console.WriteLine("=== Видалити клієнта ===");
            if (clients.Count == 0)
            {
                Console.WriteLine("Список клієнтів порожній.");
                return;
            }

            ListClients();
            int id = GetIntInputWithPrompt("Введіть ID клієнта для видалення (0 - відміна):");
            if (id == 0) return;

            int idx = -1;
            for (int i = 0; i < clients.Count; i++)
            {
                if (clients[i].Id == id) { idx = i; break; }
            }
            if (idx == -1)
            {
                Console.WriteLine("Клієнта з таким ID не знайдено.");
                return;
            }

            // Перевіримо бронювання, пов'язані з клієнтом
            int relatedBookings = 0;
            for (int i = 0; i < bookings.Count; i++)
                if (bookings[i].ClientId == id) relatedBookings++;

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
                for (int i = bookings.Count - 1; i >= 0; i--)
                {
                    if (bookings[i].ClientId == id)
                    {
                        int pid = bookings[i].ProductId;
                        int qty = bookings[i].Quantity;
                        // знайдемо продукт і відновимо AvailableCount
                        for (int j = 0; j < products.Count; j++)
                        {
                            if (products[j].Id == pid)
                            {
                                var prod = products[j];
                                prod.AvailableCount += qty;
                                products[j] = prod;
                                break;
                            }
                        }
                        bookings.RemoveAt(i);
                    }
                }
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

            clients.RemoveAt(idx);
            SaveData();
            Console.WriteLine("Клієнта (і пов'язані бронювання, якщо були) видалено.");
        }

        static void SortClientsByName()
        {
            for (int i = 0; i < clients.Count - 1; i++)
            {
                for (int j = 0; j < clients.Count - i - 1; j++)
                {
                    if (string.Compare(clients[j].FullName, clients[j + 1].FullName) > 0)
                    {
                        var temp = clients[j];
                        clients[j] = clients[j + 1];
                        clients[j + 1] = temp;
                    }
                }
            }

            Console.WriteLine("Список клієнтів відсортовано.");
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
                clients.Add(new Client(cid, clientName, phone, DateTime.Now));
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
        static void EditBooking()
        {
            Console.WriteLine("=== Редагувати бронювання ===");
            if (bookings.Count == 0)
            {
                Console.WriteLine("Список бронювань порожній.");
                return;
            }

            ListBookings();
            int bid = GetIntInputWithPrompt("Введіть ID бронювання для редагування (0 - відміна):");
            if (bid == 0) return;

            int bidx = -1;
            for (int i = 0; i < bookings.Count; i++)
            {
                if (bookings[i].BookingId == bid) { bidx = i; break; }
            }
            if (bidx == -1)
            {
                Console.WriteLine("Бронювання з таким ID не знайдено.");
                return;
            }

            var booking = bookings[bidx];
            Console.WriteLine("Поточне бронювання: " + booking.ToString());

            // Відновимо товарний запас для поточного бронювання (тимчасово), щоб можна було змінити продукт/кількість
            bool restored = false;
            for (int i = 0; i < products.Count; i++)
            {
                if (products[i].Id == booking.ProductId)
                {
                    var p = products[i];
                    p.AvailableCount += booking.Quantity;
                    products[i] = p;
                    restored = true;
                    break;
                }
            }
            if (!restored)
            {
                Console.WriteLine("Увага: пов'язаний продукт не знайдено у списку товарів. Редагування може призвести до невірних даних.");
            }

            // Можливість змінити клієнта
            Console.Write("Новий клієнт (ім'я) (Enter - без змін): ");
            string newClientName = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(newClientName))
            {
                // знайдемо/додамо клієнта
                int clientId = -1;
                for (int i = 0; i < clients.Count; i++)
                {
                    if (clients[i].FullName.Equals(newClientName, StringComparison.OrdinalIgnoreCase))
                    {
                        clientId = clients[i].Id;
                        break;
                    }
                }
                if (clientId == -1)
                {
                    Console.Write("Клієнта не знайдено. Введіть телефон для створення нового клієнта: ");
                    string phone = Console.ReadLine();
                    clientId = clients.Count > 0 ? GetMaxClientId() + 1 : 1;
                    clients.Add(new Client(clientId, newClientName.Trim(), phone, DateTime.Now));
                    Console.WriteLine($"Клієнт доданий з ID={clientId}.");
                }
                booking.ClientId = clientId;
            }

            // Змінити продукт
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

            // Змінити кількість
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

            // Перевірка наявності нового продукту і резервування
            int prodIndex = -1;
            for (int i = 0; i < products.Count; i++)
            {
                if (products[i].Id == newProductId) { prodIndex = i; break; }
            }
            if (prodIndex == -1)
            {
                Console.WriteLine("Вказаний продукт не знайдено. Операцію скасовано.");
                return;
            }

            if (products[prodIndex].AvailableCount < newQty)
            {
                Console.WriteLine($"Недостатньо товару на складі. Доступно: {products[prodIndex].AvailableCount}. Операцію скасовано.");
                return;
            }

            // Відняти запас для нового бронювання
            {
                var prod = products[prodIndex];
                prod.AvailableCount -= newQty;
                products[prodIndex] = prod;
            }

            // Оновлюємо бронювання
            booking.ProductId = newProductId;
            booking.Quantity = newQty;
            // знайдемо ціну продукту для обчислення total
            double price = products[prodIndex].Price;
            booking.Total = price * newQty;
            booking.BookingDate = DateTime.Now;

            bookings[bidx] = booking;
            SaveData();
            Console.WriteLine("Бронювання оновлено.");
        }
        static void DeleteBooking()
        {
            Console.WriteLine("=== Видалити бронювання ===");
            if (bookings.Count == 0)
            {
                Console.WriteLine("Список бронювань порожній.");
                return;
            }

            ListBookings();
            int bid = GetIntInputWithPrompt("Введіть ID бронювання для видалення (0 - відміна):");
            if (bid == 0) return;

            int bidx = -1;
            for (int i = 0; i < bookings.Count; i++)
            {
                if (bookings[i].BookingId == bid) { bidx = i; break; }
            }
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

            // Повертаємо товар на склад
            for (int i = 0; i < products.Count; i++)
            {
                if (products[i].Id == booking.ProductId)
                {
                    var prod = products[i];
                    prod.AvailableCount += booking.Quantity;
                    products[i] = prod;
                    break;
                }
            }

            bookings.RemoveAt(bidx);
            SaveData();
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
        static void SearchBooking()
        {
            Console.Write("Введіть ID клієнта або товару: ");
            string q = Console.ReadLine();

            bool found = false;

            foreach (var b in bookings)
            {
                if (b.ClientId.ToString() == q || b.ProductId.ToString() == q)
                {
                    Console.WriteLine(b.ToString());
                    found = true;
                }
            }

            if (!found)
                Console.WriteLine("Бронювання не знайдено.");
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
            Console.WriteLine($"Кількість товарів: {products.Count}");
            Console.WriteLine($"Кількість клієнтів: {clients.Count}");
            Console.WriteLine($"Кількість бронювань: {bookings.Count}");
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
                var clientLines = clients.ConvertAll(c => $"{c.Id}|{c.FullName}|{c.Phone}|{c.Joined:o}");
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
                        // saved format: Id|FullName|Phone|Joined:o
                        if (p.Length >= 4 &&
                            int.TryParse(p[0], out int id) &&
                            DateTime.TryParse(p[3], null, System.Globalization.DateTimeStyles.RoundtripKind, out DateTime joined))
                        {
                            clients.Add(new Client(id, p[1], p[2], joined));
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

        static int GetMaxClientId()
        {
            if (clients == null || clients.Count == 0) return 0;
            return clients.Max(c => c.Id);
        }
    }
}
