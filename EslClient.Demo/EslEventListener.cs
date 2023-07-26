using Nesl.EslClient.Util;
using Nesl.EslClient.Inbound;
using Nesl.EslClient.Internal;
using Nesl.EslClient.Transport.Event;

namespace EslClient.Demo
{
    internal class EslEventListener : IEslEventListener
    {
        private readonly FsClient _client;

        public EslEventListener(FsClient client)
        {
            _client = client;
        }

        public void OnEslEvent(Context ctx, EslEvent e)
        {

            var headers = e.GetEventHeaders();
            var eventName = e.GetEventName();

            Console.WriteLine(eventName);

            if ("CHANNEL_EXECUTE_COMPLETE" == eventName)
                if (headers != null && headers.ContainsKey("Application")
                                                && headers["Application"] == "play_and_detect_speech")
                {
                    var result = headers["variable_detect_speech_result"];
                    Console.WriteLine(result);
                }

            if ("DETECTED_SPEECH" == eventName)
            {
                if (headers != null && headers.ContainsKey("Speech-Type") &&
                    headers["Speech-Type"] == "detected-speech")
                {
                    var result = e.GetEventBodyLines();
                    if (result != null)
                    {
                        foreach (var line in result)
                        {
                            Console.WriteLine(line);
                        }
                    }
                }
            }

            var uuid = headers != null && headers.ContainsKey("Unique-ID") ? headers["Unique-ID"] : string.Empty;

            if ("PLAYBACK_START" == eventName)
            {
                if (headers != null && headers.ContainsKey("variable_first_play") &&
                    headers["variable_first_play"] == "1")
                {
                    CommandUtil.ChangeFirstPlay(uuid, _client).Wait();
                    CommandUtil.DetectSpeech(uuid, _client).Wait();
                }
                else
                {
                    CommandUtil.ResumeDetectSpeech(uuid, _client).Wait();
                }
            }

            if ("PLAYBACK_STOP" == eventName)
            {
                if (headers != null && headers.ContainsKey("variable_play_detect_speech_var") &&
                    headers["variable_play_detect_speech_var"] == "play_detect_speech_var=play_detect_speech_var")
                {
                    string detectResult = headers["variable_detect_speech_result"];
                    Console.WriteLine(detectResult);
                }
                else
                {
                    CommandUtil.ResumeDetectSpeech(uuid, _client).Wait();
                }
            }

        }
    }
}
