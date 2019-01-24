# Librivox tools
Collect information from the librivox books such as chapters, audio links, readers, text sources, duration and so on. Align speech with the text using the speech cutter.

### Prerequisites

If you want to use the speech cutter you have to make sure your language is available for the Windows speech recognition, English (United States, United Kingdom, Canada, India, and Australia), French, German, Japanese, Mandarin (Chinese Simplified and Chinese Traditional), and Spanish are supported.

If you language is supported install it on the language section of your Windows, you must see a little microphone icon in each of the languages mentioned above.

### Using the collector

Supported languages **en, es, fr, it, and de**. 
Using the collector is pretty straightforward, just check the available count of pages for a language on librivox and use it as a parameter --pages

Command example:
```
LibrivoxCollector.exe --lang "es" --pages 20
```

### Using the cutter

The cutter helps to align sentences with its audio, we feed in a single sentence or a list of them then creates a grammar for each of the sentences, the windows speech recognition will try to match the audio with the excepted sentences, when a sentence is spotted using confidence we filter out sentences that sound similar, then when a speech matches the sentence the offset and the duration are passed to ffmpeg to cut the recognized sentence.

Command example:
```
python cutter.py -sentences "as soon as i reached the meeting place i would find out the wagon to which i was assigned,and if i sat down and said nothing he would probably soon ask me if i wanted anything to eat,and perhaps nodding to me,he would usually grumble savagely and profanely about my having been put with his wagon,after supper i would roll up in my bedding as soon as possible" -input temp.wav -output sentences -confidence 0,69 -lang en
```
### Using the C# library 

Example:
```cs
using (var _speechCutter = new SpeechCutterClient("en"))
{
     _speechCutter.CutSentences(new string[]
     {
             "as soon as i reached the meeting place i would find out the wagon to which i was assigned",
             "and if i sat down and said nothing he would probably soon ask me if i wanted anything to eat",
             "and perhaps nodding to me",
             "he would usually grumble savagely and profanely about my having been put with his wagon",
             "after supper i would roll up in my bedding as soon as possible"
      },
     "temp.wav",
     "sentences",
     0.68,
     Formats.Wav);          
}
```

## Sentence recommendations

Avoid short sentences, try to use silence like comma and dot to cut sentences, and if the sentence is too short try appending it to the past sentence.

