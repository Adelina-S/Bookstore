using Microsoft.AspNetCore.Routing.Tree;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using System.Collections.Frozen;
using System.Diagnostics;
using WebBookstore.Models;
using static System.Reflection.Metadata.BlobBuilder;
namespace WebBookstore
{
    public class ApplicationContext : DbContext
    {
        public DbSet<Role> Roles { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Author> Authors { get; set; }
        public DbSet<BookCategory> BookCategories { get; set; }
        public DbSet<BookCard> BookCards { get; set; }
        public DbSet<Book> Books { get; set; }
        public DbSet<BookReservation> BookReservations { get; set; }
        public DbSet<BookMove> BookMoves { get; set; }
        public DbSet<Message> Messages { get; set; }

        public ApplicationContext()
        {
            Database.EnsureCreated();
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=base.db");
        }
        //Метод, который заполняет базу данных при её создании
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            Role admin = new Role() { Id = 1, Type = RoleType.Administrator };
            Role user = new Role() { Id = 2, Type = RoleType.User };
            modelBuilder.Entity<Role>().HasData(new Role[] { admin, user });

            User adelina = new User("Adelina", "Synergy", "Аделина") { Id = 1, RoleId = 1 };
            User jack = new User("Jack", "123", "Евгений") { Id = 2, RoleId = 2 };
            User vika = new User("Vika", "123", "Виктория") { Id = 3, RoleId = 2 };
            modelBuilder.Entity<User>().HasData(new User[] { adelina, jack, vika });

            modelBuilder.Entity<User>().Navigation(t => t.Role).AutoInclude();
            //modelBuilder.Entity<BookCategory>().Navigation(t => t.Parent).AutoInclude();
            modelBuilder.Entity<BookCard>().Navigation(t => t.Author).AutoInclude();
            modelBuilder.Entity<BookCard>().Navigation(t => t.Category).AutoInclude();
            modelBuilder.Entity<Book>().Navigation(t => t.Card).AutoInclude();
            modelBuilder.Entity<Book>().Navigation(t => t.Owner).AutoInclude();
            modelBuilder.Entity<BookReservation>().Navigation(t => t.Book).AutoInclude();
            modelBuilder.Entity<BookReservation>().Navigation(t => t.User).AutoInclude();
            modelBuilder.Entity<BookMove>().Navigation(t => t.Book).AutoInclude();
            modelBuilder.Entity<BookMove>().Navigation(t => t.User).AutoInclude();
            modelBuilder.Entity<Message>().Navigation(t => t.Target).AutoInclude();
            modelBuilder.Entity<Message>().Navigation(t => t.Book).AutoInclude();
        }

        public Dictionary<RoleType, Role> GetRoles() => Roles.ToDictionary(t => t.Type);
        public string AddUser(string name, string password, string login, RoleType roleType)
        {
            if (GetUser(login) != null) return "Пользователь с такой учётной записью уже зарегистрирован";
            var role = GetRoles()[roleType];
            User user = new User(login, password, name) { Role = role };
            Users.Add(user);
            SaveChanges();
            return "";
        }
        public User GetUser(string login) => Users.FirstOrDefault(t => t.Login == login);
        public BookCategory GetCategory(string code)
        {
            var result = BookCategories.Include(t => t.Childs).ToList();
            return result.FirstOrDefault(t=>t.Code == code);
        }
        public string CreateCategoryCode(string name)//Метод создаёт строковый идентификатор категории
        {
            string basicCode = "";
            if (string.IsNullOrEmpty(name) == false)
                basicCode = Common.TransliteText(name);
            else
                basicCode = "empty_category";
            string code = basicCode;
            for (int i = 2; ; i++)
            {
                if (GetCategory(code) == null) return code;
                else code = $"{basicCode}_{i}";
            }
        }
        public string AddCategory(string name, string parentCategoryCode)
        {
            BookCategory parent = null;
            if (string.IsNullOrEmpty(parentCategoryCode) == false)
            {
                parent = GetCategory(parentCategoryCode);
                if (parent == null) return "Родительская категория не найдена";
            }
            string code = CreateCategoryCode(name);
            BookCategory category = new BookCategory(code, name, parent);
            BookCategories.Add(category);
            SaveChanges();
            return "";
        }
        public List<BookCategory> GetBookCategories(bool childs = false)
        {
            var result = BookCategories.Include(t => t.Childs).ToList();
            if (childs)
                return result.Where(t => t.Childs.Count == 0).ToList();
            else
                return result.Where(t => t.Parent == null).ToList();
        }
        public Author GetAuthor(string code) => Authors.FirstOrDefault(t => t.Code == code);
       /* public List<(Author, int)> GetAuthorsForFilter(Author author)
        {
            var query = Books.Where(t => t.IsSold == false);
            if (author!=null) query=query.Where(t=>t.Card.Author== author);
            var authorBookCount = query.GroupBy(b => b.Card.Author)
               .Select(t => new { Author = t.Key, Count = t.Count() }).Where(t=>t.Count>0).ToList();
            return authorBookCount.Select(t=>(t.Author, t.Count)).ToList(); 
        }*/
        public List<(Author, int)> GetAuthorsForFilter(BookCategory category, int? year)
        {
            var query = Books.Where(t => t.IsSold == false);
            if (category!=null)
            {
                var childs = Common.GetAllChildsCategory(category);
                childs.Add(category);
                query = query.Where(t => childs.Contains(t.Card.Category));
            }
            if (year!=null)
            {
                query=query.Where(t=>t.Card.Year== year);
            }
            var authorBookCount = query.GroupBy(b => b.Card.Author)
               .Select(t => new { Author = t.Key, Count = t.Count() }).Where(t => t.Count > 0).ToList();
            return authorBookCount.Select(t => (t.Author, t.Count)).ToList();
        }
        public List<(int, int)> GetYearsForFilter(BookCategory category, Author author)
        {
            var query = Books.Where(t => t.IsSold == false);
            if (category != null)
            {
                var childs = Common.GetAllChildsCategory(category);
                childs.Add(category);
                query = query.Where(t => childs.Contains(t.Card.Category));
            }
            if (author != null)
            {
                query = query.Where(t => t.Card.Author == author);
            }
            var yearBookCount = query.GroupBy(b => b.Card.Year)
               .Select(t => new { Year = t.Key, Count = t.Count() }).Where(t => t.Count > 0).ToList();
            return yearBookCount.Select(t => (t.Year, t.Count)).ToList();
        }
        public string CreateAuthorCode(string name, string surname)//Метод создаёт строковый идентификатор автора
        {
            string basicCode = "";
            if (string.IsNullOrEmpty(name) == false && string.IsNullOrEmpty(surname) == false)
                basicCode = Common.TransliteText($"{name}_{surname}");
            else if (string.IsNullOrEmpty(name) == false)
                basicCode = Common.TransliteText(name);
            else if (string.IsNullOrEmpty(surname) == false)
                basicCode = Common.TransliteText(surname);
            else
                basicCode = "no_author";
            string code = basicCode;
            for (int i = 2; ; i++)
            {
                if (GetAuthor(code) == null) return code;
                else code = $"{basicCode}_{i}";
            }
        }
        public string AddAuthor(string name, string surname, User user)
        {
            if (user.Role.Type != RoleType.Administrator) return "У вас нат прав на добавление писателя";
            Author sameAuthor = Authors.FirstOrDefault(t => t.Name.ToLower() == name.ToLower() && t.Surname.ToLower() == surname.ToLower());
            if (sameAuthor != null) return "Такой автор уже добавлен";
            string authorCode = CreateAuthorCode(name, surname);
            Author author = new Author(authorCode, name, surname);
            Authors.Add(author);
            SaveChanges();
            return "";
        }
        public List<Author> GetAuthors(string filters=null) => Authors.ToList();
        public string AddBookCard(string title, string description, int year, string authorCode, string categoryCode)
        {
            Author author=GetAuthor(authorCode);
            if (author == null) return "Автор не найден";
            BookCategory category=GetCategory(categoryCode);
            if (category == null) return "Категория не найдена";
            if (string.IsNullOrEmpty(title)) return "Название не может быть пустым";
            if (year < 1000 || year > 3000) return "Указан неправильный год";
            BookCard card=new BookCard(title, description, author, category, year);
            BookCards.Add(card);
            SaveChanges();
            return "";
        }
        public void UpdateBookCard(BookCard bookCard)
        {
            BookCards.Update(bookCard);
            SaveChanges();
        }
        public BookCard GetBookCard(string title, int year, string authorCode, string categoryCode)
        {
             var allbooks = BookCards.Where(t => t.Year == year && t.Author.Code == authorCode && t.Category.Code == categoryCode).ToList();
             return allbooks.FirstOrDefault(t => t.Title.ToLower() == title.ToLower());
            //return BookCards.FirstOrDefault(t => t.Title.ToLower() == title.ToLower() && t.Author.Code == authorCode && t.Category.Code == categoryCode);
        }
        /* public BookCard GetBookCard(string title, int year, string authorCode, string categoryCode)
             => BookCards.FirstOrDefault(t => t.Title.ToLower() == title.ToLower()
             && t.Year == year
             && t.Author.Code == authorCode
             && t.Category.Code == categoryCode);*/
        public BookCard GetBookCard(int id)=>BookCards.FirstOrDefault(t=> t.Id == id);
        public string AddBook(BookCard bookCard, User user, string price)
        {
            decimal priceDecimal = 0;
            if (Decimal.TryParse(price, out priceDecimal)==false) { return "Невозможно преобразовать цену к числовому значению"; }
            if (priceDecimal < 0) return "Цена не может быть отрицательной";
            Book book=new Book(bookCard, user, priceDecimal);
            Books.Add(book);
            SaveChanges();
            return "";
        }
        public Book GetBook(int id)=>Books.FirstOrDefault(t=>t.Id == id);
        public List<Book> GetBooks(User user)
        {
            var books = Books.Where(t => t.Owner == user && t.IsSold == false).ToList();
            AddStatuses(books);
            return books;
        }
        public void AddStatuses(List<Book> books)
        {
            var reservations = BookReservations.Where(r => r.isFinished == false && books.Select(b => b.Id).Contains(r.Book.Id)).ToList();
            var moves = BookMoves.Where(r => r.isFinished == false && books.Select(b => b.Id).Contains(r.Id)).ToList();
            foreach (var book in books)
            {
                book.Status = "";
                if (book.IsBlocked)
                    book.Status += "Заблокирована";
                var reservation = reservations.FirstOrDefault(t => t.Book == book);
                if (reservation != null)
                    switch (reservation.MoveType)
                    {
                        case BookMoveType.Weeks2: book.Status += "Забронирована на 2 недели"; book.IsReserved = true; break;
                        case BookMoveType.Month: book.Status += "Забронирована на 1 месяц"; book.IsReserved = true; break;
                        case BookMoveType.Months3: book.Status += "Забронирована на 3 месяца"; book.IsReserved = true; break;
                        case BookMoveType.Buy: book.Status += "Забронирована на выкуп"; book.IsReserved = true; break;
                    }
                var move = moves.FirstOrDefault(t => t.Book == book);
                if (move != null)
                    switch (move.BlockType)
                    {
                        case BookMoveType.Weeks2:
                        case BookMoveType.Month:
                        case BookMoveType.Months3: book.Status += $"В аренде до {move.EndTime.ToString("dd.MM.yyyy")}"; book.IsMoved = true; break;
                    }
                if (book.Status == "") book.Status = "Свободна";
            }
        }
        public List<Book> GetBooks(BookCategory bookCategory, Author author, int? year)
        {
            var query = Books.Where(t => t.IsSold == false);
            if (bookCategory != null)
                query = query.Where(t => t.Card.Category == bookCategory);
            if (author!=null)
                query=query.Where(t=>t.Card.Author== author);
            if (year!=null)
                query=query.Where(t=>t.Card.Year== year);
            var books = query.ToList();
            if (bookCategory!=null && bookCategory.Childs.Count>0)
            {
                var childCategories = Common.GetAllChildsCategory(bookCategory);
                var query2 = Books.Where(t => t.IsSold== false && childCategories.Contains(t.Card.Category));
                if (author != null)
                    query2 = query2.Where(t => t.Card.Author == author);
                if (year != null)
                    query2 = query2.Where(t => t.Card.Year == year);
                var books2=query2.ToList();
                books.AddRange(books2);
            }
            AddStatuses(books);
            return books;
        }
        /*public List<Book> GetAllBooks()
        {
            var books = Books.Where(t => t.IsSold == false).ToList();
            AddStatuses(books);
            return books;
        }*/
        public string ChangeBookPrice(User user, int bookId, string price)
        {
            decimal priceDecimal = 0;
            price = price.Replace('.', ',');
            if (Decimal.TryParse(price, out priceDecimal) == false) { return "Невозможно преобразовать цену к числовому значению"; }
            if (priceDecimal < 0) return "Цена не может быть отрицательной";
            Book book = GetBook(bookId);
            if (book == null) return "Книга не найдена";
            if (book.Owner != user) return "Вы не являетесь владельцем данной книги";
            if (BookReservations.FirstOrDefault(t => t.isFinished == false && t.Book == book && t.MoveType == BookMoveType.Buy) != null)
                return "Книга забронирована на продажу с этой ценой";
            if (book.IsSold) return "Книга продана";
            book.Price = priceDecimal;
            SaveChanges();
            return "";
        }
        public string ChangeBookBlock(User user, int bookId, bool isBlock)
        {
            Book book = GetBook(bookId);
            if (book == null) return "Книга не найдена";
            if (book.Owner != user) return "Вы не являетесь владельцем данной книги";
            if (BookReservations.FirstOrDefault(t => t.isFinished == false && t.Book == book) != null)
                return "Книга забронирована";
            if (BookMoves.FirstOrDefault(t => t.isFinished == false && t.Book == book) != null)
                return "Книга в аренде";
            if (book.IsSold) return "Книга продана";
            book.IsBlocked = isBlock;
            SaveChanges();
            return "";
        }
        public string CheckBookAvailable(Book book)
        {
            if (book == null) return "Книга не найдена";
            if (book.IsBlocked) return "Книга заблокирована владельцем";
            if (book.IsSold) return "Книга продана";
            if (BookMoves.FirstOrDefault(t => t.Book == book && t.isFinished == false) != null) return "Книга уже забронирована";
            if (BookReservations.FirstOrDefault(t => t.Book == book && t.isFinished == false) != null) return "Книга находится в аренде";
            return "";
        }
        //public BookReservation GetReservation(int id) => BookReservations.FirstOrDefault(t => t.Id == id);
        public BookReservation GetReservation(int bookId) => BookReservations.FirstOrDefault(t => t.Book.Id == bookId && t.isFinished == false);
        public string CreateReservation(int bookId, User target, BookMoveType moveType)
        {
            if (target.Role.Id == 1) return "Администратор не может брать книги в аренду";
            Book book=GetBook(bookId);
            if (book == null) return "Книга не найдена";
            if (book.Owner == target) return "Нельзя забронировать книгу у себя";
            string bookAvailableResult=CheckBookAvailable(book);
            if (string.IsNullOrEmpty(bookAvailableResult) == false) return bookAvailableResult;
            BookReservation bookReservation = new BookReservation(book, target, moveType);
            BookReservations.Add(bookReservation);
            Message toOwnerMessage = new Message(book.Owner, $"Пользователь {target.Name} хочет {Common.BookMoveCreateReservationInfo(moveType)} книгу {book.Card.Title}.", book);
            Messages.Add(toOwnerMessage);
            SaveChanges();
            return "";
        }
        public string AcceptMove(int bookId, User owner)
        {
            BookReservation bookReservation = GetReservation(bookId);
            if (bookReservation == null) return "Бронирование не найдено";
            if (bookReservation.Book.Owner != owner) return "Вы не являетесь владельцем данной книги";
            DateTime endTime = Common.CalcEndTime(bookReservation.MoveType);
            BookMove move = new BookMove(bookReservation.Book, bookReservation.User, bookReservation.MoveType, endTime);
            if (bookReservation.MoveType==BookMoveType.Buy)
            {
                bookReservation.Book.IsSold = true;
                move.isFinished = true;
            }
            bookReservation.isFinished = true;
            BookMoves.Add(move);
            string messageText = "";
            messageText = $"Ваша {Common.BookAppendMoveInfo(bookReservation.MoveType)} книги {bookReservation.Book.Card.Title} подтверждена владельцем.";
            if (bookReservation.MoveType != BookMoveType.Buy)
                messageText += $" Окончание действия аренды {endTime.ToString("dd.MM.yyyy")}.";
            Message toTargetMessage = new Message(bookReservation.User, messageText, bookReservation.Book);
            Messages.Add(toTargetMessage);
            SaveChanges();
            return "";
        }
        public string DeclineMove(int bookId, User owner)
        {
            BookReservation bookReservation = GetReservation(bookId);
            if (bookReservation == null) return "Бронирование не найдено";
            if (bookReservation.Book.Owner != owner) return "Вы не являетесь владельцем данной книги";
            bookReservation.isFinished = true;
            string messageText = "";
            messageText = $"Ваша {Common.BookAppendMoveInfo(bookReservation.MoveType)} книги {bookReservation.Book.Card.Title} отклонена владельцем.";
            Message toTargetMessage = new Message(bookReservation.User, messageText, bookReservation.Book);
            Messages.Add(toTargetMessage);
            SaveChanges();
            return "";
        }
        public List<Message> GetMessages(User user) => Messages.Where(t => t.Target == user && t.IsDeleted==false).OrderByDescending(t=>t.DateTime).ToList();
        public string DeleteMessages(User user, int[] messagesList)
        {
            Messages.Where(t => t.Target == user && messagesList.Contains(t.Id)).ExecuteUpdate(t => t.SetProperty(t => t.IsDeleted, t => true));
            return "";
        }
        //Необходимо запускать перед действием любого пользователя - проверяет бд на окончание сроков аренды и информирует пользователей и администраторов об этом
        public void AnyUserEnter()
        {
            var endMoves = BookMoves.Where(t => t.isFinished == false && t.EndTime <= DateTime.Now).ToList();
            if (endMoves.Count == 0) return;
            foreach (var move in endMoves)
            {
                var messageToUser = new Message(move.User, $"Ваша {Common.BookAppendMoveInfo(move.BlockType)} книги {move.Book.Card.Title} истекла.", move.Book);
                var messageToOwner = new Message(move.Book.Owner, $"А{Common.BookAppendMoveInfo(move.BlockType).Substring(1)} книги {move.Book.Card.Title} истекла.", move.Book);
                Messages.Add(messageToUser);
                Messages.Add(messageToOwner);
                move.isFinished = true;
            }
            SaveChanges();
        }
    }
}