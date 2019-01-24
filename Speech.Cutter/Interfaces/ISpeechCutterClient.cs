using Speech.Cutter.Enums;

namespace Speech.Cutter.Interfaces
{
    public interface ISpeechCutterClient
    {
        bool CutSentence(string sentence, string inputWav, string outputDirectory, double confidence, Formats targetFormat);
        bool CutSentences(string[] sentences, string inputWav, string outputDirectory, double confidence, Formats targetFormat);
    }
}
