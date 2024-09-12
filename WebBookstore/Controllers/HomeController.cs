using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Security.Claims;
using WebBookstore.Models;
using Microsoft.AspNetCore.Authorization;
using static System.Reflection.Metadata.BlobBuilder;

namespace WebBookstore.Controllers
{
    public class HomeController : Controller
    {
        private User User { get; set; }
        private ApplicationContext Database { get; set; }
        public HomeController(ApplicationContext database)
        {
            Database = database;
        }
        private void CheckUser()
        {
            if (HttpContext == null) return;
            Database.AnyUserEnter();
            var login = HttpContext.User?.Identity?.Name;
            if (login != null)
            {
                User = Database.GetUser(login);
                if (User != null)
                {
                    ViewData["Name"] = User.Name;
                    ViewData["IsHaveUser"] = true;
                    if (User.Role.Type == RoleType.Administrator)
                        ViewData["Role"] = "Admin";
                    else
                        ViewData["Role"] = "User";
                    ViewData["Messages"] = Database.GetMessages(User).Count;
                }
            }
            if (login == null || User==null)
                ViewData["IsHaveUser"] = false;
        }
        [HttpGet] 
        public IActionResult Index(string category, string author, string year)
        {
            CheckUser();
            BookCategory selectCategory = null;
            Author selectAuthor = null;
            int? selectYear = null;
            int parseYear = 0;
            if (string.IsNullOrEmpty(category) == false)
                selectCategory = Database.GetCategory(category);
            if (string.IsNullOrEmpty(author)==false)
                selectAuthor = Database.GetAuthor(author);
            if (Int32.TryParse(year, out parseYear) == true && parseYear > 1000 && parseYear < 3000)
                selectYear = parseYear;
            List<Book> books = null;
            if (selectCategory != null)
            {
                ViewData["BackCategory"] = selectCategory.Parent?.Code ?? "";
                ViewData["Categories"] = selectCategory.Childs;
                ViewData["CurrentCategory"] = selectCategory.Name;
                ViewData["CurrentCategoryCode"] = selectCategory.Code;
                if (selectAuthor == null)
                {
                    ViewData["Authors"] = Database.GetAuthorsForFilter(selectCategory, selectYear);
                    ViewData["CurrentAuthorCode"] = "";
                }
                if (selectYear == null)
                {
                    ViewData["Years"] = Database.GetYearsForFilter(selectCategory, selectAuthor);
                    ViewData["CurrentYear"] = "";
                }
            }
            if (selectAuthor!=null)
            {
                ViewData["BackAuthor"] = true;
                ViewData["CurrentAuthorCode"] = selectAuthor.Code;
                ViewData["CurrentAuthor"] = selectAuthor.FullName;
                if (selectCategory==null)
                {
                    ViewData["Categories"] = Database.GetBookCategories();
                    ViewData["CurrentCategoryCode"] = "";
                }
                if (selectYear==null)
                {
                    ViewData["Years"] = Database.GetYearsForFilter(selectCategory, selectAuthor);
                    ViewData["CurrentYear"] = "";
                }
            }
            if (selectYear!=null)
            {
                ViewData["BackYear"] = true;
                ViewData["CurrentYear"] = selectYear;
                if (selectCategory == null)
                {
                    ViewData["Categories"] = Database.GetBookCategories();
                    ViewData["CurrentCategoryCode"] = "";
                }
                if (selectAuthor == null)
                {
                    ViewData["Authors"] = Database.GetAuthorsForFilter(selectCategory, selectYear);
                    ViewData["CurrentAuthorCode"] = "";
                }
            }
            if (selectCategory == null && selectAuthor == null && selectYear==null)
            {
                ViewData["Categories"] = Database.GetBookCategories();
                ViewData["Authors"] = Database.GetAuthorsForFilter(null, null);
                ViewData["Years"] = Database.GetYearsForFilter(selectCategory, selectAuthor);
                ViewData["CurrentCategoryCode"] = "";
                ViewData["CurrentAuthorCode"] = "";
                ViewData["CurrentYear"] = "";
            }
            books = Database.GetBooks(selectCategory, selectAuthor, selectYear);
           
            return View(books);
        }
        [HttpPost, Authorize]
        public string TryReserveBook(int bookId, int reservType)
        {
            CheckUser();
            if (reservType < 0 || reservType > 3) return "������ �������� ������";
            return Database.CreateReservation(bookId, User, (BookMoveType)reservType);
        }
        [HttpGet]
        public IActionResult Login()
        {
            CheckUser();
            return View();
        }
        [HttpPost]
        public async Task<string> Login(string login, string password)
        {
            CheckUser();
            User result = Database.GetUser(login);
            if (result == null || result.Password != password)
                return "Неверный логин или пароль";
            else
            {
                List<Claim> claims = new List<Claim>()
                {
                    new Claim(ClaimsIdentity.DefaultNameClaimType, result.Login),
                    new Claim(ClaimsIdentity.DefaultRoleClaimType, result.Role.Type.ToString())
                };
                ClaimsIdentity claimsIdentity = new ClaimsIdentity(claims, "Cookies");
                var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
                await HttpContext.SignInAsync(claimsPrincipal);
                return "";
            }
        }
        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            CheckUser();
            if (User != null)
            {
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            }
            return Redirect("/Home/Index");
        }
        [HttpGet]
        public IActionResult Register()
        {
            CheckUser();
            return View();
        }
        [HttpPost]
        public string Register(string login, string password, string name)
        {
            CheckUser();
            return Database.AddUser(name, password, login, RoleType.User);
        }
        [HttpGet, Authorize(Roles = "Administrator")]
        public IActionResult AddBook()
        {
            CheckUser();
            ViewData["Authors"] = Database.GetAuthors();
            ViewData["Categories"] = Common.GetCategoriesSelectHTML(Database.GetBookCategories(true));
            return View("Views/Home/AddBook.cshtml");
        }
        [HttpPost, Authorize(Roles = "Administrator")]
        public string AddBook(string authorCode, string categoryCode, int year, string title, string description, string price)
         {
            CheckUser();
            var bookCard = Database.GetBookCard(title, year, authorCode, categoryCode);
            if (bookCard != null)
            {
                if (string.IsNullOrEmpty(description) == false && description != bookCard.Description)
                {
                    bookCard.Description = description;
                    Database.UpdateBookCard(bookCard);
                }
            }
            else
            {
                var bookCardCreateResult = Database.AddBookCard(title, description, year, authorCode, categoryCode);
                if (bookCardCreateResult!="")
                    return bookCardCreateResult;
                bookCard = Database.GetBookCard(title, year, authorCode, categoryCode);
            }
            return Database.AddBook(bookCard, User, price);
        }
        [HttpPost, Authorize(Roles = "Administrator")]
        public string AddAuthor(string name, string surname)
        {
            CheckUser();
            return Database.AddAuthor(name, surname, User);
        }
        [HttpPost]
        public string GetAuthorsList()
        {
            var authors=Database.GetAuthors();
            List<string> result = new List<string>();
            foreach (var author in authors)
            {
                //result.Add($"<option data_author={author.Code} value='{author.Surname} {author.Name}'></option>");
                result.Add($"<option data-author='{author.Code}'>{author.Surname} {author.Name}</option>");
            }
            return string.Join('\n', result);
        }
        [HttpGet, Authorize(Roles = "Administrator")]
        public IActionResult Categories()
        {
            CheckUser();
            var cats = Database.GetBookCategories();
            var list = Common.CreateCategoriesList(cats, 0);
            return View("Views/Home/Categories.cshtml", list);
        }
        [HttpPost, Authorize(Roles = "Administrator")]
        public string CreateNewCategory(string name, string parentCode=null)
        {
            CheckUser();
            return Database.AddCategory(name, parentCode);
        }
        [HttpGet, Authorize(Roles = "Administrator")]
        public IActionResult MyBooks()
        {
            CheckUser();
            var books = Database.GetBooks(User);
            return View("Views/Home/MyBooks.cshtml", books);
        }
        [HttpPost, Authorize(Roles = "Administrator")]
        public string ChangePrice(int bookId, string price)
        {
            CheckUser();
            return Database.ChangeBookPrice(User, bookId, price);
        }
        [HttpPost, Authorize(Roles = "Administrator")]
        public string ChangeBlock(int bookId, bool isBlock)
        {
            CheckUser();
            return Database.ChangeBookBlock(User, bookId, isBlock);
        }
        [HttpPost, Authorize(Roles = "Administrator")]
        public string DeclineReservation(int bookId)
        {
            CheckUser();
            return Database.DeclineMove(bookId, User);
        }
        [HttpPost, Authorize(Roles = "Administrator")]
        public string AcceptReservation(int bookId)
        {
            CheckUser();
            return Database.AcceptMove(bookId, User);
        }
        [HttpPost, Authorize(Roles = "Administrator")]
        public string RemindRent(int bookId)
        {
            CheckUser();
            return Database.RemindRent(bookId, User);
        }
        [HttpGet, Authorize]
        public IActionResult Messages()
        {
            CheckUser();
            var messages= Database.GetMessages(User);
            return View("Views/Home/Messages.cshtml", messages);
        }
        [HttpPost, Authorize]
        public string DeleteMessages(int[] messageList)
        {
            CheckUser();
            return Database.DeleteMessages(User, messageList);
        }
        [HttpGet, Authorize]
        public IActionResult MyRents()
        {
            CheckUser();
            var books = Database.GetRentBooks(User);
            return View("Views/Home/MyRents.cshtml", books);
        }
        [HttpPost, Authorize]
        public string ReturnBook(int bookId)
        {
            CheckUser();
            return Database.ReturnBook(bookId, User);
        }
        class Person()
        {
            public string name { get; set; }
            public int age { get; set; }
        }
    }
}
