using System.Collections.Generic;
using WebBookstore.Models;

namespace WebBookstore
{
    public class Common
    {
        public static string BookMoveCreateReservationInfo(BookMoveType type)
        {
            switch (type)
            {
                case BookMoveType.Weeks2: return "хочет взять в аренду на 2 недели";
                case BookMoveType.Month: return "хочет взять в аренду на 1 месяц";
                case BookMoveType.Months3: return "хочет взять в аренду на 3 месяца";
                case BookMoveType.Buy: return "хочет купить";
                default: throw new Exception();

            }
        }
        public static string BookAppendMoveInfo(BookMoveType type)
        {
            switch (type)
            {
                case BookMoveType.Weeks2: return "аренда на 2 недели";
                case BookMoveType.Month: return "аренда на 1 месяц";
                case BookMoveType.Months3: return "аренда на 3 месяца";
                case BookMoveType.Buy: return "покупка";
                default: throw new Exception();

            }
        }
        public static DateTime CalcEndTime(BookMoveType type)
        {
            switch (type)
            {
                case BookMoveType.Weeks2: return DateOnly.FromDateTime(DateTime.Now).AddDays(14).ToDateTime(TimeOnly.MinValue);
                case BookMoveType.Month: return DateOnly.FromDateTime(DateTime.Now).AddMonths(1).ToDateTime(TimeOnly.MinValue);
                case BookMoveType.Months3: return DateOnly.FromDateTime(DateTime.Now).AddMonths(3).ToDateTime(TimeOnly.MinValue);
                case BookMoveType.Buy: return DateOnly.MaxValue.ToDateTime(TimeOnly.MinValue);
                default: throw new Exception();
            }
        }
        public static string TransliteText(string text)
        {
            return text.ToLower()
                .Replace(" ", "_")
                .Replace("а", "a")
                .Replace("б", "b")
                .Replace("в", "v")
                .Replace("г", "g")
                .Replace("д", "d")
                .Replace("е", "e")
                .Replace("ё", "yo")
                .Replace("ж", "zh")
                .Replace("з", "z")
                .Replace("и", "i")
                .Replace("й", "y")
                .Replace("к", "k")
                .Replace("л", "l")
                .Replace("м", "m")
                .Replace("н", "n")
                .Replace("о", "o")
                .Replace("п", "p")
                .Replace("р", "r")
                .Replace("с", "s")
                .Replace("т", "t")
                .Replace("у", "u")
                .Replace("ф", "f")
                .Replace("х", "kh")
                .Replace("ц", "ts")
                .Replace("ч", "ch")
                .Replace("ш", "sh")
                .Replace("щ", "sch")
                .Replace("ъ", "`")
                .Replace("ы", "y")
                .Replace("ь", "`")
                .Replace("э", "e")
                .Replace("ю", "yu")
                .Replace("я", "ya");

        }

        public static List<CategoryHTML> CreateCategoriesList(List<BookCategory> list, int step)
        {
            List<CategoryHTML> result = new List<CategoryHTML>();
            foreach (BookCategory category in list)
            {
                result.Add(new CategoryHTML(category.Name, step, false, category.Code, category.Childs.Count == 0));
                result.Add(new CategoryHTML($"Название новой категории в разделе \"{category.Name}\"", step + 1, true, category.Code, false));
                if (category.Childs.Count > 0)
                    result.AddRange(CreateCategoriesList(category.Childs, step + 1));

            }
            return result;
        }
        public static List<CategoryAddBook> CreateCategoryListAddBook(List<BookCategory> list)
        {
            List<CategoryAddBook> result = new List<CategoryAddBook>();
            foreach (BookCategory category in list)
            {
                if (category.Childs.Count > 0)
                {
                    result.Add(new CategoryAddBook(category.Name,"", true, true));
                    result.AddRange(CreateCategoryListAddBook(category.Childs));
                    result.Add(new CategoryAddBook(category.Name, "", true, false));
                }
                else
                {
                    result.Add(new CategoryAddBook(category.Name, category.Code, false, false));
                }
            }
            return result;

        }
       public static string GetCategoriesSelectHTML(List<BookCategory> list)
        {
            var cats = list.GroupBy(t => t.Parent.Code).ToDictionary(k => k.Key, v => v.ToList());
            List<string> catsHTML = new List<string>();
            foreach (var pair in cats)
            {
                string parentName = Common.GetParentName(pair.Value[0], "");
                catsHTML.Add($"<optgroup label='{parentName}'>");
                foreach (var item in pair.Value)
                    catsHTML.Add($"<option value='{item.Code}'>{item.Name}</option>");
                catsHTML.Add("</optgroup>");
            }
            return string.Join('\n', catsHTML);
        }
 
        private static string GetParentName(BookCategory category, string parentName)
        {
            if (category.Parent == null) return parentName.TrimEnd('-');
            else return GetParentName(category.Parent, $"{category.Parent.Name}-{parentName}");
        }
        public static List<BookCategory> GetAllChildsCategory(BookCategory category)
        {
            var result= new List<BookCategory>();
            foreach (var item in category.Childs)
            {
                if (item.Childs.Count > 0)
                    result.AddRange(GetAllChildsCategory(item));
                result.Add(item);
            }
            return result;
        }
    }
    public class CategoryHTML
    {
        public string Name { get; set; }
        public string Tabs { get; set; }
        public bool IsInput { get; set; }
        public string Code { get; set; }
        public string ButtonCode { get; set; }
        public bool Final { get; set; }
        public CategoryHTML(string name, int tabsCount, bool isInput, string code, bool final)
        {
            Name = name;
            Tabs = "";
            for (int i = 0; i < tabsCount; i++) Tabs += "----";
            IsInput = isInput;
            Code = code;
            ButtonCode = $"add_{code}";
            Final = final;
        }
    }
    public class CategoryAddBook
    {
        public string Name { get; set; }
        public bool IsStartGroup { get; set; }
        public bool IsEndGroup { get; set; }
        public bool IsOption { get; set; }
        public string Code { get; set; }
        public CategoryAddBook(string name, string code, bool isGroup, bool isStart)
        {
            Name = name;
            Code = code;
            if (isGroup == false)
                IsOption = true;
            else if (isStart)
                IsStartGroup = true;
            else
                IsEndGroup = true;
        }
    }
}

