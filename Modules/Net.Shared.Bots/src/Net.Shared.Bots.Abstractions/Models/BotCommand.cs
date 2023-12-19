using System.Text;

using Net.Shared.Bots.Abstractions.Interfaces;

namespace Net.Shared.Bots.Abstractions.Models
{
    public sealed class BotCommand : IBotCommand
    {
        public BotCommand(string name, Dictionary<string, string> parameters)
        {
            Name = name;
            Parameters = parameters;
        }

        public BotCommand(IBotMessage message)
        {
            Name = message.Data.Substring(message.Data.IndexOf(' '));
            Parameters = Name.Split(' ').ToDictionary()
        }

        public string Name { get; }
        public Dictionary<string, string> Parameters { get; }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append(Name);
            if (Parameters.Count != 0)
            {
                sb.Append(' ');
                sb.Append(string.Join(" ", Parameters.Select(x => $"{x.Key}={x.Value}")));
            }
            return sb.ToString();
        }
    }
}
