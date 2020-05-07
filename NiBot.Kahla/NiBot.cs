using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Kahla.SDK.Abstract;
using Kahla.SDK.Events;
using Kahla.SDK.Models.ApiViewModels;
using Microsoft.AspNetCore.Mvc.Internal;
using NiBot.Kahla.Models;

namespace NiBot.Kahla
{
    public class NiBot : BotBase
    {
        public static readonly string CommandPrefix = "Ni ";

        private readonly NiCommand[] _commands;
        private Dictionary<int, List<NiBind>> _binds = new Dictionary<int, List<NiBind>>();

        public NiBot()
        {
            _commands = new[] {
                new NiCommand {
                    Cmd = "help",
                    ArgFormat = "",
                    Disruption = "显示所有可用指令",
                    Handler = async (cmd, context) =>
                    {
                        var sb = new StringBuilder();
                        sb.Append($"[i]命令前缀:{CommandPrefix}\n");
                        sb.Append($"显示所有可用的{_commands.Length}条命令.\n");
                        sb.AppendJoin('\n', _commands.Select(t => $"{t.Cmd} {t.ArgFormat}  {t.Disruption}"));
                        await SendMessage(sb.ToString(), context.ConversationId, context.AESKey);
                    }
                },
                new NiCommand {
                    Cmd = "about",
                    ArgFormat = "",
                    Disruption = "显示关于信息",
                    Handler = async (cmd, context) => await SendMessage("Ni（镍）原子序数为28，具磁性，银白色过渡金属。在自然界中以硅酸镍矿或硫、砷、镍化合物形式存在。性坚韧，有磁性和良好的可塑性，在空气中不被氧化，溶于硝酸。\n" +
                                                                        "Ni Bot By EdgeNeko\n" +
                                                                        "Github: @hv0905" +
                                                                        "祝使用愉快^_^", context.ConversationId, context.AESKey)
                },
                new NiCommand {
                    Cmd = "say",
                    ArgFormat = "(content)",
                    Disruption = "发送指定内容",
                    Handler = async (cmd, context) =>
                    {
                        cmd = cmd.Trim('"');
                        if (string.IsNullOrEmpty(cmd))
                        {
                            await SendMessage($"用法: {CommandPrefix}say (content)\n 可以发送图片, 视频等内容, 参考消息格式文档. \n", context.ConversationId, context.AESKey);
                        }
                        else
                        {
                            await SendMessage(cmd, context.ConversationId, context.AESKey);
                        }
                    }
                },
                new NiCommand {
                    Cmd = "bind",
                    ArgFormat = "(key) (command) [mode:fullMatch|match|regex]",
                    Disruption = "将指定关键字绑定为命令",
                    Handler = async (cmd, context) =>
                    {
                        Task SendDoc() =>
                            SendMessage($"用法: {CommandPrefix}bind (key) (command) [mode:fullMatch|match|regex]\n" +
                                        "key: 用于匹配的关键字\n" +
                                        "command: 触发时需执行的命令(使用 $& 替代用户输入的内容)" +
                                        "mode:\n" +
                                        "- fullMatch: 仅当聊天内容完全匹配key时会触发指令（默认）\n" +
                                        "- match: 只要聊天内容包含key即会除法指令\n" +
                                        "- regex: 使用正则表达式进行匹配（command中可使用 $[index] 来获取正则匹配的结果）\n" +
                                        "示例: bind ? \"say 你们又在说些难懂的东西呢~\" fullMatch \n" +
                                        "bind ^计算(.+)$ \"calc $1\" regex",
                                context.ConversationId, context.AESKey);

                        if (string.IsNullOrEmpty(cmd))
                        {
                            await SendDoc();
                            return;
                        }

                        var cmds = SplitArgs(cmd);
                        if (cmds.Count < 2 || cmds.Count > 3)
                        {
                            await SendDoc();
                            return;
                        }

                        var bind = new NiBind() {
                            Key = cmds[0],
                            Command = cmds[1]
                        };
                        switch (cmds.Count == 3 ? cmds[2] : string.Empty)
                        {
                            case "match":
                                bind.Mode = NiBind.NiBindMode.Match;
                                break;
                            case "regex":
                                try
                                {
                                    // ReSharper disable once ObjectCreationAsStatement
                                    new Regex(cmds[0]);
                                }
                                catch (ArgumentException)
                                {
                                    await SendMessage("❌ 正则分析错误", context.ConversationId, context.AESKey);
                                    return;
                                }

                                bind.Mode = NiBind.NiBindMode.Regex;
                                break;
                            default:
                                bind.Mode = NiBind.NiBindMode.FullMatch;
                                break;
                        }

                        if (!_binds.ContainsKey(context.Message.ConversationId))
                        {
                            _binds.Add(context.Message.ConversationId, new List<NiBind>());
                        }

                        if (_binds[context.Message.ConversationId].Any(t => t.Key == bind.Key))
                        {
                            await SendMessage("❌ 已存在相同的关键字.", context.ConversationId, context.AESKey);
                        }

                        _binds[context.Message.ConversationId].Add(bind);
                        await SendMessage("✔ bind成功", context.ConversationId, context.AESKey);
                    }
                },
                new NiCommand {
                    Cmd = "bind-ls",
                    ArgFormat = "",
                    Disruption = "显示所有绑定的命令",
                    Handler = async (cmd, context) =>
                    {
                        if (_binds.ContainsKey(context.Message.ConversationId) && _binds[context.Message.ConversationId].Count > 0)
                        {
                            var sb = new StringBuilder();
                            sb.Append($"对话id:{context.Message.ConversationId}\n显示所有{_binds[context.Message.ConversationId].Count}条绑定\n");
                            sb.AppendJoin("\n\n", _binds[context.Message.ConversationId].Select(t => $"关键字:{t.Key}\n命令:{t.Command}\n模式: {t.Mode}"));
                            await SendMessage(sb.ToString(), context.ConversationId, context.AESKey);
                        }
                        else
                        {
                            await SendMessage($"对话{context.Message.ConversationId}没有任何绑定.", context.ConversationId, context.AESKey);
                        }
                    }
                },
                new NiCommand {
                    Cmd = "bind-rm",
                    ArgFormat = "(key)",
                    Disruption = "取消绑定一条命令",
                    Handler = async (cmd, context) =>
                    {
                        if (string.IsNullOrEmpty(cmd))
                        {
                            await SendMessage($"用法: {CommandPrefix}bind-rm (key)", context.ConversationId, context.AESKey);
                            return;
                        }

                        if (_binds.ContainsKey(context.Message.ConversationId))
                        {
                            cmd = cmd.Trim('"');
                            if (_binds[context.Message.ConversationId].RemoveAll(t => t.Key == cmd) != 0)
                            {
                                await SendMessage("✔ 移除成功", context.ConversationId, context.AESKey);
                            }
                        }

                        await SendMessage("❌ 找不到这条绑定", context.ConversationId, context.AESKey);
                    }
                },
                new NiCommand {
                    Cmd = "schedule",
                    ArgFormat = "(m) (h) (dom) (mon) (dow) (command)",
                    Disruption = "创建一个计划任务"
                },
                new NiCommand {
                    Cmd = "schedule-ls",
                    ArgFormat = "",
                    Disruption = "显示所有计划任务"
                },
                new NiCommand {
                    Cmd = "schedule-rm",
                    ArgFormat = "(uuid)",
                    Disruption = "删除一个计划任务"
                },
                new NiCommand() {
                    Cmd = "calc",
                    ArgFormat = "(expression)",
                    Disruption = "计算指定数学表达式",
                    Handler = async (cmd, context) =>
                    {
                        if (string.IsNullOrWhiteSpace(cmd)) await SendMessage("❌ 无法计算空白的表达式", context.ConversationId, context.AESKey);
                        var proc = await Task.Run(() =>
                        {
                            var proc_ = Process.Start(new ProcessStartInfo("qalc/qalc", cmd) {
                                UseShellExecute = false,
                                CreateNoWindow = true,
                                RedirectStandardOutput = true
                            });
                            // ReSharper disable once PossibleNullReferenceException
                            proc_.WaitForExit();
                            return proc_;
                        });
                        var result = await proc.StandardOutput.ReadToEndAsync();
                        await SendMessage(result, context.ConversationId, context.AESKey);
                    }
                },
                new NiCommand {
                    Cmd = "exec",
                    ArgFormat = "(command1) (command2) ...",
                    Disruption = "按顺序执行多条命令",
                    Handler = async (cmd, context) =>
                    {
                        var cmds = SplitArgs(cmd);
                        foreach (var item in cmds)
                        {
                            await ExecuteCommand(item, context);
                        }
                    }
                },
                new NiCommand {
                    Cmd = "image-search",
                    ArgFormat = "[imgPath]",
                    Disruption = "调用图片搜索api搜索图片,若不提供路径可进入交互模式",
                    Handler = async (cmd, context) => { await SendMessage("WIP", context.ConversationId, context.AESKey); }
                },
                // secure risk maybe
                // new NiCommand() {
                //     Cmd = "wget",
                //     ArgFormat = "(url)",
                //     Disruption = "向指定网络地址发送HTTP GET请求,并返回所有内容",
                //     Handler = async (cmd, context) =>
                //     {
                //         if (string.IsNullOrEmpty(cmd))
                //         {
                //             return $"用法: {CommandPrefix}wget (url)";
                //         }
                //
                //         cmd = cmd.Trim('"');
                //         var wc = new WebClient();
                //         try
                //         {
                //             var str = await wc.DownloadStringTaskAsync(cmd);
                //             return str;
                //         }
                //         catch (Exception e)
                //         {
                //             return $"❌ {e.Message}";
                //         }
                //     }
                // },
            };
        }

        public override async Task OnBotInit()
        {
            Console.WriteLine("===================");
            Console.WriteLine("Ni bot Ver 1.0.0");
            Console.WriteLine("Built by EdgeNeko");
            Console.WriteLine("===================");
            await Task.CompletedTask;
        }

        public override async Task OnFriendRequest(NewFriendRequestEvent arg)
        {
            await CompleteRequest(arg.Request.Id, true);
            var targetConversation = (await ConversationService.AllAsync())
                .Items
                .Single(t => t.UserId == arg.Request.CreatorId);
            await SendMessage($"Welcome to Ni Bot by EdgeNeko.\n输入 {CommandPrefix}help查看可用命令列表", targetConversation.ConversationId, targetConversation.AesKey);
        }

        public override async Task OnGroupConnected(SearchedGroup @group) { }

        public override async Task OnMessage(string inputMessage, NewMessageEvent eventContext)
        {
            if (eventContext.Message.SenderId == Profile.Id) return;
            if (inputMessage.StartsWith(CommandPrefix)) // cmd mode
            {
                await ExecuteCommand(inputMessage.Substring(CommandPrefix.Length), eventContext);
            }
            else if (_binds.ContainsKey(eventContext.Message.ConversationId))
            {
                var result = _binds[eventContext.Message.ConversationId].FirstOrDefault(t =>
                {
                    switch (t.Mode)
                    {
                        case NiBind.NiBindMode.Match:
                            return inputMessage.Contains(t.Key);
                        case NiBind.NiBindMode.FullMatch:
                            return t.Key == inputMessage;
                        case NiBind.NiBindMode.Regex:
                            return Regex.IsMatch(inputMessage, t.Key);
                    }

                    return false;
                });
                if (result != null)
                {
                    if (result.Mode == NiBind.NiBindMode.Regex)
                    {
                        var match = Regex.Match(inputMessage, result.Key);
                        await ExecuteCommand(match.Result(result.Command), eventContext);
                    }
                    else
                    {
                        await ExecuteCommand(result.Command.Replace("$&", inputMessage), eventContext);
                    }
                }
            }
        }

        public override async Task OnGroupInvitation(int groupId, NewMessageEvent eventContext) { }

        public List<string> SplitArgs(string commandLine)
        {
            var isLastCharSpace = false;
            var parmChars = commandLine.ToCharArray().ToList();
            var inQuote = false;
            for (var index = 0; index < parmChars.Count; index++)
            {
                if (parmChars[index] == '\\')
                {
                    parmChars.RemoveAt(index);
                    continue;
                }

                if (parmChars[index] == '"')
                    inQuote = !inQuote;
                if (!inQuote && parmChars[index] == ' ' && !isLastCharSpace)
                    parmChars[index] = '\n';
                isLastCharSpace = parmChars[index] == '\n' || parmChars[index] == ' ';
            }

            return new string(parmChars.ToArray()).Split('\n').Select(t => t.Trim('"')).ToList();
        }

        public async Task ExecuteCommand(string cmd, NewMessageEvent context)
        {
            string key = cmd.Split(' ').Length > 0 ? cmd.Split(' ')[0] : string.Empty;
            if (string.IsNullOrEmpty(key)) await SendMessage($"输入 {CommandPrefix} help 查看可用指令.", context.ConversationId, context.AESKey);
            var matchCommand = _commands.FirstOrDefault(t => t.Cmd == key);
            if (matchCommand != null)
            {
                await matchCommand.Handler(cmd.Length == key.Length ? string.Empty : cmd.Substring(key.Length + 1), context);
            }
            else
            {
                await SendMessage($"未知指令, 输入 {CommandPrefix} help 查看可用指令.", context.ConversationId, context.AESKey);
            }
        }

        public new Task SendMessage(string message, int conversationId, string aesKey)
        {
            if (message.Length > 1000)
            {
                message = message.Substring(0, 1000) + "...余下部分被截断";
            }

            return base.SendMessage(message, conversationId, aesKey);
        }
    }
}
