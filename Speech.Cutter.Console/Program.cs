using Speech.Cutter.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Speech.Cutter.Console
{
    class Program
    {
        /// <summary>
        /// Get the value of an argurment.
        /// </summary>
        /// <param name="args">Argument list.</param>
        /// <param name="option">Key of the argument.</param>
        /// <returns>Value of the argument.</returns>
        static string GetArgument(IEnumerable<string> args, string option)
        => args.SkipWhile(i => i != option).Skip(1).Take(1).FirstOrDefault();

        static void Main(string[] args)
        {
            string sentences = GetArgument(args, "--sentences");
            string inputWav = GetArgument(args, "--input");
            string outputDir = GetArgument(args, "--output");
            string confidence = GetArgument(args, "--confidence");
            string lang = GetArgument(args, "--lang");
            try
            {
                using (var _speechCutter = new SpeechCutterClient(lang))
                {
                    //this is an example using manually selected sentences, you should use your own way to clean the text
                    //to achieve the best sentences try to use sentences splitted by , or . 
                    if (sentences.Contains(","))
                    {
                        _speechCutter.CutSentences(
                            sentences.Split(','),
                            inputWav,
                            outputDir,
                            Convert.ToDouble(confidence),
                            Formats.Wav);
                    }
                    else
                    {
                        _speechCutter.CutSentence(
                            sentences,
                            inputWav,
                            outputDir,
                            Convert.ToDouble(confidence),
                            Formats.Wav);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Console.WriteLine(ex.ToString());
            }
        }

        private static string[] DummySentences() => new string[]
        {
             "as soon as i reached the meeting place i would find out the wagon to which i was assigned",
             "and if i sat down and said nothing he would probably soon ask me if i wanted anything to eat",
             "and perhaps nodding to me",
             "he would usually grumble savagely and profanely about my having been put with his wagon",
             "after supper i would roll up in my bedding as soon as possible"
        };
    }
}