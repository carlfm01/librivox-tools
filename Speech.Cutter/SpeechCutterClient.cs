using Frapper;
using Speech.Cutter.Enums;
using Speech.Cutter.Interfaces;
using Speech.Cutter.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Speech.Recognition;
using System.Text;
using System.Threading;

namespace Speech.Cutter
{
    /// <summary>
    /// VAD speech cutter using Windows Speech Recognition.
    /// </summary>
    public class SpeechCutterClient : ISpeechCutterClient, IDisposable
    {
        private readonly ManualResetEvent _eventLock;

        private readonly FFMPEG _ffmpegClient;

        private readonly IList<RecognizedSentence> _recognizedSentences;

        private SpeechRecognitionEngine _speechRecognitionEngine;

        private double _confidence;

        private bool _singleSentenceRecognition = false;

        /// <summary>
        /// Speech cutter using the Windows speech recognition to spot and cut sentences.
        /// </summary>
        /// <param name="ffmpegExePath">Path of the ffmpeg.exe.</param>
        /// <param name="languague">Sets the languague of the speech recognition.</param>
        public SpeechCutterClient(string languague, string ffmpegExePath = "ffmpeg.exe")
        {
            _ffmpegClient = new FFMPEG(ffmpegExePath);
            _speechRecognitionEngine = new SpeechRecognitionEngine(new CultureInfo(languague));
            _speechRecognitionEngine.RecognizeCompleted += _speechRecognitionEngine_RecognizeCompleted;
            _speechRecognitionEngine.SpeechRecognized += _speechRecognitionEngine_SpeechRecognized;
            _recognizedSentences = new List<RecognizedSentence>();
            _eventLock = new ManualResetEvent(false);
        }

        private void _speechRecognitionEngine_SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            if (e.Result.Confidence >= _confidence)
            {
                _recognizedSentences.Add(new RecognizedSentence
                {
                    Sentence = e.Result.Text,
                    Offset = e.Result.Audio.AudioPosition,
                    Duration = e.Result.Audio.Duration,
                    Confidence = e.Result.Confidence
                });
                if (!_singleSentenceRecognition)
                {
                    _speechRecognitionEngine.UnloadGrammar(e.Result.Grammar);
                }
            }
        }

        private void _speechRecognitionEngine_RecognizeCompleted(object sender, RecognizeCompletedEventArgs e)
        {
            _eventLock.Set();
        }

        private Grammar CreateGrammar(string sentence)
        {
            return new Grammar(new GrammarBuilder(new Choices(new string[] { sentence }))
            {
                Culture = _speechRecognitionEngine.RecognizerInfo.Culture
            });
        }

        #region ISpeechCutterClient

        /// <summary>
        /// Lookups for a sentence and cuts it. 
        /// </summary>
        /// <param name="sentences">Sentences to spot.</param>
        /// <param name="inputWav">Input audio file.</param>
        /// <param name="outputDirectory">Output directory.</param>
        /// <param name="confidence">Confidence level, filters out similar words. From 0.1 to 0.9.</param>
        /// <param name="targetFormat">Format to save the audio file that contains spotted word.</param>
        /// <returns>True if the sentence was found and then cutted.</returns>
        public bool CutSentences(string[] sentences, string inputWav, string outputDirectory, double confidence, Formats targetFormat)
        {
            _singleSentenceRecognition = false;
            _confidence = confidence;
            VerifyOutputDirectory(outputDirectory);
            _speechRecognitionEngine.SetInputToWaveFile(inputWav);
            foreach (string sentence in sentences)
            {
                _speechRecognitionEngine.LoadGrammar(CreateGrammar(sentence));
            }
            _speechRecognitionEngine.RecognizeAsync(RecognizeMode.Multiple);
            _eventLock.WaitOne();
            _eventLock.Reset();
            _speechRecognitionEngine.UnloadAllGrammars();
            CutRecognizedSentences(inputWav, outputDirectory, targetFormat);
            return _recognizedSentences.Count != 0;
        }

        /// <summary>
        /// Lookups for a sentence and cuts it. 
        /// </summary>
        /// <param name="sentence">Sentence to spot.</param>
        /// <param name="inputWav">Input audio file.</param>
        /// <param name="outputDirectory">Output directory.</param>
        /// <param name="confidence">Confidence level, filters out similar words. From 0.1 to 0.9.</param>
        /// <param name="targetFormat">Format to save the audio file that contains spotted word.</param>
        /// <returns>True if the sentence was found and then cutted.</returns>
        public bool CutSentence(string sentence, string inputWav, string outputDirectory, double confidence, Formats targetFormat)
        {
            _singleSentenceRecognition = true;
            _confidence = confidence;
            VerifyOutputDirectory(outputDirectory);
            _speechRecognitionEngine.SetInputToWaveFile(inputWav);
            _speechRecognitionEngine.LoadGrammar(CreateGrammar(sentence));
            _speechRecognitionEngine.RecognizeAsync(RecognizeMode.Multiple);
            _eventLock.WaitOne();
            _eventLock.Reset();
            _speechRecognitionEngine.UnloadAllGrammars();
            CutRecognizedSentences(inputWav, outputDirectory, targetFormat);
            return _recognizedSentences.Count != 0;
        }

        /// <summary>
        /// Cuts the audio using the recognized sentences.
        /// </summary>
        /// <param name="inputWav">Source file path.</param>
        /// <param name="outDirectory">Output directory.</param>
        /// <param name="targetFormat">Format to save the audio file that contains spotted word.</param>
        private void CutRecognizedSentences(string inputWav, string outDirectory, Formats targetFormat)
        {
            if (_recognizedSentences.Count != 0)
            {
                var sb = new StringBuilder();
                foreach (var recognizedSentence in _recognizedSentences)
                {
                    string fileName = Guid.NewGuid().ToString();
                    string command = string.Empty;
                    switch (targetFormat)
                    {
                        case Formats.Wav:
                            command = $" -ss {recognizedSentence.Offset.ToString().Replace(",", ".")}" +
                                $" -t {recognizedSentence.Duration.ToString().Replace(",", ".")}" +
                                $" -i \"{inputWav}\" -f wav -acodec pcm_s16le -ac 1" +
                                $" -sample_fmt s16 -ar 16000 \"{outDirectory}/{fileName}.wav\"";
                            break;
                        case Formats.Mp3:
                            command = $" -ss {recognizedSentence.Offset.ToString().Replace(",", ".")}" +
                                " -vn -f mp3 -ar 16000 -acodec libmp3lame" +
                                $" -t {recognizedSentence.Duration.ToString().Replace(",", ".")}" +
                                $" -i \"{inputWav}\" \"{outDirectory}/{fileName}.mp3\"";
                            break;
                    }
                    _ffmpegClient.RunCommand(command);
                    sb.AppendLine($"{fileName}.{targetFormat.ToString().ToLower()},{recognizedSentence.Sentence},{recognizedSentence.Confidence.ToString().Replace(",", ".")}");
                    
                }
                File.WriteAllText($"{outDirectory}/sentences.txt", sb.ToString().TrimEnd("\r\n".ToCharArray()));
                _recognizedSentences.Clear();
            }
        }

        public void Dispose()
        {
            _speechRecognitionEngine.RecognizeCompleted -= _speechRecognitionEngine_RecognizeCompleted;
            _speechRecognitionEngine.SpeechRecognized -= _speechRecognitionEngine_SpeechRecognized;
            _speechRecognitionEngine.RecognizeAsyncStop();
            _speechRecognitionEngine.UnloadAllGrammars();
            _speechRecognitionEngine.SetInputToNull();
            _speechRecognitionEngine = null;
        }

        private static void VerifyOutputDirectory(string outputDirectory)
        {
            if (!Directory.Exists(outputDirectory))
            {
                Directory.CreateDirectory(outputDirectory);
            }
        }
        #endregion
    }
}
