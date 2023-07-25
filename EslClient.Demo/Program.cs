using EslClient.Demo;
using Nesl.EslClient.Util;
using Nesl.EslClient.Inbound;

var client = new FsClient();

client.AddEventListener(new EslEventListener(client));

client.Connect("192.166.8.248", 8021, "ClueCon").Wait();
client.SetEventSubscriptions("PLAIN", "all").Wait();

var caller = "12345";
var callee = "1000@192.166.8.248";
var audioUrl =
    "http://192.166.8.248:30900/zhengwu/test/start.wav";
var gateway = "sofia/internal";
CommandUtil.Originate(client, caller, callee, audioUrl, gateway).Wait();

Console.ReadKey();
