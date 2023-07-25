using Nesl.EslClient.Inbound;
using Nesl.EslClient.Transport;
using System.Globalization;

namespace Nesl.EslClient.Util
{
    public static class CommandUtil
    {
        public static async Task CallCommand(string uuid, FsClient client, string appName, string? appArgs = null)
        {
            var message = new SendMsg(uuid);
            message.AddCallCommand("execute");
            message.AddExecuteAppName(appName);
            if (appArgs != null)
            {
                message.AddExecuteAppArg(appArgs);
            }
            await client.SendMessage(message);
        }

        public static async Task<string> Originate(FsClient client, string caller, string callee, string audioUrl, string gateway, string recordPath = "/home/www/ftp/")
        {
            var format =
                "{{origination_uuid={0},execute_on_answer='sched_hangup +{1}',ignore_early_media=true,origination_caller_id_number={2},origination_caller_id_name={2}}}[first_play=1]{3}/{4} record_session:{5}{0}.wav,playback:'{6}',park inline";
            var command = string.Format(CultureInfo.InvariantCulture, format, Guid.NewGuid().ToString(), 300, caller,
                gateway, callee, recordPath, audioUrl);
            return await client.SendBackgroundApiCommand("originate", command);
        }

        public static async Task Hangup(string uuid, FsClient client)
        {
            await CallCommand(uuid, client, "hangup");
        }

        public static async Task Transfer(string uuid, FsClient client, string callee, string gateway)
        {
            await client.SendApiCommand("uuid_transfer", uuid + " -aleg 'set:hangup_after_bridge=false,bridge:" + gateway + "/" + callee + "' inline");
        }

        public static async Task PlayAndDetectSpeech(string uuid, FsClient client, string audioPath)
        {
            await CallCommand(uuid, client, "set", "play_detect_speech_var=play_detect_speech_var");
            await CallCommand(uuid, client, "play_and_detect_speech", audioPath + " detect:unimrcp {start-input-timers=true,no-input-timeout=40000,recognition-timeout=5000}hello");
        }

        public static async Task PlayBack(string uuid, FsClient client, string audioPath)
        {
            await CallCommand(uuid, client, "set", "play_detect_speech_var=play_back_var");
            await CallCommand(uuid, client, "playback", audioPath);
        }

        public static async Task ResumeDetectSpeech(string uuid, FsClient client)
        {
            await CallCommand(uuid, client, "detect_speech", "resume");
        }

        public static async Task DetectSpeech(string uuid, FsClient client)
        {
            await CallCommand(uuid, client, "detect_speech", "unimrcp {start-input-timers=true,no-input-timeout=4000,recognition-timeout=5000}builtin:hello/boolean?language=en-US;y=1;n=2 hello");
        }

        public static async Task ChangeFirstPlay(string uuid, FsClient client)
        {
            await CallCommand(uuid, client, "set", "first_play=2");
        }
    }
}
