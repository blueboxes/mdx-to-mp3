//Sample used https://github.com/Azure-Samples/cognitive-services-speech-sdk/blob/master/samples/csharp/sharedcontent/console/speech_synthesis_samples.cs
//https://developer.mozilla.org/en-US/docs/Web/HTML/Element/audio
//https://stackoverflow.com/questions/20500796/convert-pcm-to-mp3-ogg?rq=2
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using NAudio.Lame;
using NAudio.Wave;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;

internal class Program
{
    private static async Task Main(string[] args)
    {
        if (args.Length != 2 || args[0] is null || args[1] is null)
            throw new ArgumentException("You must provide both a source and target file paths");

        if (!File.Exists(args[0]))
            throw new FileNotFoundException($"The file at the path {args[0]} provided as the source must exist");

        var configuration = new ConfigurationBuilder()
        .AddJsonFile("appsettings.json", true);

        var config = configuration.Build();
        var speechKey = config.GetValue<string>("Key");
        var speechRegion = config.GetValue<string>("Region");

        ArgumentException.ThrowIfNullOrEmpty(speechKey);
        ArgumentException.ThrowIfNullOrEmpty(speechRegion);

        await ConvertTextToAudio(speechKey, speechRegion, args[0], args[1]);
    }

    async static Task ConvertTextToAudio(string key, string region, string inputFile, string outputFile)
    {
        var config = SpeechConfig.FromSubscription(key, region);

        // Set the voice name see https://learn.microsoft.com/en-gb/azure/cognitive-services/speech-service/language-support?tabs=tts#prebuilt-neural-voices
        config.SpeechSynthesisVoiceName = "en-US-SaraNeural";

        using (var synthesizer = new SpeechSynthesizer(config, null as AudioConfig))
        {
            synthesizer.SynthesisCompleted += (s, e) =>
            {
                SaveOutput(e.Result.AudioData, outputFile);
            };

            string text = RemoveMdxFormatting(await File.ReadAllTextAsync(inputFile));
            using (var result = await synthesizer.SpeakTextAsync(text))
            {
                if (result.Reason == ResultReason.SynthesizingAudioCompleted)
                {
                    Console.WriteLine($"Speech synthesized Complete");
                }
                else if (result.Reason == ResultReason.Canceled)
                {
                    var cancellation = SpeechSynthesisCancellationDetails.FromResult(result);
                    Console.WriteLine($"CANCELED: Reason={cancellation.Reason}");

                    if (cancellation.Reason == CancellationReason.Error)
                    {
                        Console.WriteLine($"CANCELED: ErrorCode={cancellation.ErrorCode}");
                        Console.WriteLine($"CANCELED: ErrorDetails=[{cancellation.ErrorDetails}]");
                        Console.WriteLine($"CANCELED: Did you update the subscription info?");
                    }
                }
            }
        }
    }

    static void SaveOutput(byte[] wavFile, string fileName)
    {
        using (var inputStream = new MemoryStream(wavFile))
        using (var waveReader = new WaveFileReader(inputStream))
        using (var lameWriter = new LameMP3FileWriter(fileName, waveReader.WaveFormat, 128))
        {
            waveReader.CopyTo(lameWriter);
        }
    }


    static string RemoveMdxFormatting(string mdxInput)
    {
        var result = mdxInput;
        var firstIndex = mdxInput.IndexOf("---");
        if (firstIndex > -1)
        {
            var secondIndex = mdxInput.Substring(firstIndex + 3).IndexOf("---");
            if (secondIndex > -1)
            {
                result = mdxInput.Substring(secondIndex + 6);
            }
        }

        result = Regex.Replace(result, @"[^A-Z]#+\s", ""); // removes headings
        result = Regex.Replace(result, @"!?\[(.+?)\]\(.+?\)", "$1");//pulls alt text from images and links   
        result = Regex.Replace(result, @"(\*\*|\*)(.*?)(\*\*|\*)", "$2");//remove bold and italic formatting
        result = Regex.Replace(result, @"`{3}(?:[a-z]+)?\s+[\s\S]*?\n`{3}", "");//remove code samples
        result = Regex.Replace(result, @"<a\s+[^>]*>(.*?)<\/a>", "$1");//remove hyperlinks

        return result;
    }

}