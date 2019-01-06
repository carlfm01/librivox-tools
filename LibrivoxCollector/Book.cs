using System.Collections.Generic;

namespace LibrivoxCollector
{
    /// <summary>
    /// Book model.
    /// </summary>
    public class Book
    {
        /// <summary>
        /// Book metadata for example Solo | Spanish.
        /// </summary>
        public string BookMeta { get; set; }
        /// <summary>
        /// Book duration.
        /// </summary>
        public string Duration { get; set; }
        /// <summary>
        /// Ttitle of the book.
        /// </summary>
        public string Title { get; set; }
        /// <summary>
        /// Zip URL of the book.
        /// </summary>
        public string ZipFile { get; set; }
        /// <summary>
        /// URL text of the book.
        /// </summary>
        public string OnlineText { get; set; }
        /// <summary>
        /// Url of the book.
        /// </summary>
        public string Url { get; set; } 
        /// <summary>
        /// Chapters of the book.
        /// </summary>
        public List<Chapter> Chapters { get; set; }
    }
}
