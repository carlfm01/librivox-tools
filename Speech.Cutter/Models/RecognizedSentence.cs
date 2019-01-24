using System;

namespace Speech.Cutter.Models
{
    /// <summary>
    /// Detected sentence model.
    /// </summary>
    public class RecognizedSentence
    {
        /// <summary>
        /// Recognized text.
        /// </summary>
        public string Sentence { get; set; }
        /// <summary>
        /// Confidence of the recognized sentence.
        /// </summary>
        public double Confidence { get; set; }
        /// <summary>
        /// Start time of the sentence.
        /// </summary>
        public TimeSpan Offset { get; set; }
        /// <summary>
        /// Duration of the sentence.
        /// </summary>
        public TimeSpan Duration { get; set; }
    }
}
