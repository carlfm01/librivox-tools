namespace LibrivoxCollector
{
    /// <summary>
    /// Chapter model.
    /// </summary>
    public class Chapter
    {
        /// <summary>
        /// Reader name of the chapter.
        /// </summary>
        public string Reader { get; set; }
        /// <summary>
        /// Chapter duration.
        /// </summary>
        public string Duration { get; set; }
        /// <summary>
        /// Section of the chapter.
        /// </summary>
        public string Section { get; set; }
        /// <summary>
        /// Name of the chapter.
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Text of the chapter.
        /// </summary>
        public string Text { get; set; }
        /// <summary>
        /// Audio URL of the mp3.
        /// </summary>
        public string AudioLink { get; set; }
    }
}
