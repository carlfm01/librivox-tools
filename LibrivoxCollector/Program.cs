
using AngleSharp.Dom.Html;
using AngleSharp.Parser.Html;
using HtmlAgilityPack;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text; 
using System.Windows.Forms;

namespace LibrivoxCollector
{ 
    public class Program
    {
        private const LanguageEnum Lang = LanguageEnum.Spanish;
        private const int LanguageAvailablePages = 20;

        [STAThread]
        public static void Main(string[] args)
        {
            HtmlParser htmlParser = new HtmlParser();
            HtmlWeb webClient = new HtmlWeb
            {
                BrowserTimeout = TimeSpan.FromMinutes(4)
            };
            List<Book> books = LoadBooks(htmlParser, webClient, Lang, LanguageAvailablePages);
            foreach (Book book in books.Where(x => x.BookMeta.ToLower().Contains(Lang.ToString().ToLower()))) //filters multiling
            {
                LoadBookMeta(book, htmlParser, webClient);
            }
            File.WriteAllText($"books-{Lang.ToString()}.json", JsonConvert.SerializeObject(books), Encoding.UTF8);




            /*Just a test*/
            //List<Book> books = JsonConvert.DeserializeObject<List<Book>>(File.ReadAllText("books-es.json", Encoding.UTF8));
            //var duration = books.Where(x => x.BookMeta.ToLower().Contains("spanish") && !x.Title.ToLower().Contains("las cien mejores")).Select(x => x.Duration).ToList();
            //TimeSpan totalDuration = new TimeSpan();
            //foreach (var dur in duration)
            //{
            //    var time = dur.Split(':');
            //    totalDuration = totalDuration.Add(TimeSpan.FromSeconds(Convert.ToInt32(time[2])));
            //    totalDuration = totalDuration.Add(TimeSpan.FromMinutes(Convert.ToInt32(time[1])));
            //    totalDuration = totalDuration.Add(TimeSpan.FromHours(Convert.ToInt32(time[0])));
            //}
            //string totalHours = totalDuration.TotalHours.ToString();

            string line = Console.ReadLine();
        }

        /// <summary>
        /// Load information for every book.
        /// </summary>
        /// <param name="book">Book that contains the url.</param>
        /// <param name="htmlParser">HTML parser.</param>
        /// <param name="webClient">Wbepages downloader.</param>
        private static void LoadBookMeta(Book book, HtmlParser htmlParser, HtmlWeb webClient)
        {
            //listen-download clearfix
            book.Chapters = new List<Chapter>();
            string innerHtml = webClient.LoadFromWebAsync(book.Url).GetAwaiter().GetResult().DocumentNode.InnerHtml;
            IHtmlDocument document = htmlParser.Parse(innerHtml);
            var sidebar = document.QuerySelector("div.book-page-sidebar");
            AngleSharp.Dom.IHtmlCollection<AngleSharp.Dom.IElement> htmlCollection = document.QuerySelectorAll("dd");
            var bookInfoSide = htmlCollection.Select(x => x.TextContent).ToArray();
            book.Duration = bookInfoSide[4];


            var bookTextNode = document.QuerySelectorAll("a").Where(x => x.TextContent.ToLower() == "online text").FirstOrDefault();
            if (bookTextNode != null)
            {
                book.OnlineText = bookTextNode.GetAttribute("href");
            }
            var chapterNodes = document.QuerySelector("table.chapter-download").QuerySelector("tbody").QuerySelectorAll("tr");

            var columns = document.QuerySelector("table.chapter-download").QuerySelector("thead").QuerySelectorAll("th").Select(x => x.TextContent).ToList();

            int chapterIndex = columns.FindIndex(x => x.ToLower().Contains("chapter"));
            int sectionIndex = columns.FindIndex(x => x.ToLower().Contains("section"));
            int readerIndex = columns.FindIndex(x => x.ToLower().Contains("reader"));
            int durationIndex = columns.FindIndex(x => x.ToLower().Contains("time"));
            int languageIndex = columns.FindIndex(x => x.ToLower().Contains("language"));
            int sourceTextIndex = columns.FindIndex(x => x.ToLower().Contains("source"));

            foreach (var chapterNode in chapterNodes)
            {
                var chapterInfo = chapterNode.QuerySelectorAll("td").ToArray();
                string chapterMp3 = chapterIndex != -1 ? chapterInfo[chapterIndex].QuerySelector("a").GetAttribute("href") : string.Empty;
                string chapterName = chapterIndex != -1 ? chapterInfo[chapterIndex].QuerySelector("a").TextContent : string.Empty;
                AngleSharp.Dom.IElement readerNameElement = chapterInfo[readerIndex].QuerySelector("a");
                string readerName = readerNameElement != null ? readerNameElement.TextContent : "";
                string chapterDuration = chapterInfo[durationIndex].TextContent;

                book.Chapters.Add(new Chapter
                {
                    AudioLink = chapterMp3,
                    Section = chapterInfo[sectionIndex].TextContent,
                    Duration = chapterDuration,
                    Name = chapterName,
                    Reader = readerName,
                    TextSource = sourceTextIndex != -1 ? chapterInfo[sourceTextIndex].GetAttribute("href") : string.Empty
                });
            }
        }



        /// <summary>
        /// Load all the available  books for a specific language.
        /// </summary>
        /// <param name="htmlParser">Html parser.</param>
        /// <param name="webClient">Webclient that loads the dynamic content.</param> 
        /// <param name="lang">Html parser.</param>
        /// <param name="languageAvailablePages">Books pages count.</param>
        /// <returns>List of books.</returns>
        private static List<Book> LoadBooks(HtmlParser htmlParser, HtmlWeb webClient, LanguageEnum lang, int languageAvailablePages)
        {
            List<Book> books = new List<Book>();
            for (int i = 1; i <= languageAvailablePages; i++) //for spanish just 20 pages
            {
                string booksPage = $"https://librivox.org/search?primary_key={(int)lang}&search_category=language&search_page={i}&search_form=get_results";
                var document = LoadDocument(htmlParser, webClient, booksPage, "li.catalog-result");
                if (document != null)
                {
                    var htmlBooksList = document.QuerySelectorAll("li.catalog-result");
                    foreach (var bookNode in htmlBooksList)
                    {
                        AngleSharp.Dom.IElement nameAndLink = bookNode.QuerySelector("h3").QuerySelector("a");

                        AngleSharp.Dom.IElement element = bookNode.QuerySelector("div.download-btn");
                        if (element != null)
                        {

                            Book book = new Book
                            {
                                Url = nameAndLink.GetAttribute("href"),
                                Title = nameAndLink.TextContent,
                                BookMeta = bookNode.QuerySelector("p.book-meta").TextContent,
                                ZipFile = element.QuerySelector("a").GetAttribute("href")
                            };
                            books.Add(book);
                            //Console.Clear();
                            Console.WriteLine($"Book found: {book.Title}");
                            Console.WriteLine($"Books count: {books.Count}");
                        }
                    }
                }
            }
            return books;
        }

        /// <summary>
        /// Waits for specific items to loads and then returns the dynamically rendered webpage.
        /// </summary>
        /// <param name="htmlParser">Html parser.</param>
        /// <param name="webClient">Webclient that loads the dynamic content.</param> 
        /// <param name="url">URL to load.</param>
        /// <param name="waitForTag">Wait for tag to load.</param>
        /// <returns>Loaded document.</returns>
        private static IHtmlDocument LoadDocument(HtmlParser htmlParser, HtmlWeb webClient, string url, string waitForTag)
        {
            bool result = false;
            IHtmlDocument document = null;

            var doc1 = webClient.LoadFromBrowser(url, o =>
            {
                if (!result)
                {
                    document = htmlParser.Parse(((WebBrowser)o).Document.Body.InnerHtml);
                    var dynamicContent = document.QuerySelector(waitForTag);
                    if (dynamicContent != null && dynamicContent.HasChildNodes)
                    {
                        result = true;
                    }
                }
                return result;
            });
            result = false;
            return document;
        }
    }
}
