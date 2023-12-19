namespace Net.Shared.Bots.Parsers;

public sealed class CommandParser
{
    public Queue<string> Commands { get; } = new();
    public Dictionary<string, string> Parameters { get; } = [];
    
    public CommandParser(string input)
    {
        var span = input.AsSpan();
        var delimiterPos = span.IndexOf('?');

        var commands = delimiterPos > -1 ? span[..delimiterPos] : span;
        var parameters = delimiterPos > -1 ? span[(delimiterPos + 1)..] : [];

        foreach (var item in commands.ToString().Split('/'))
        {
            Commands.Enqueue(item);
        }

        foreach (var item in parameters.ToString().Split('&'))
        {
            var keyValue = item.Split('=');
            
            if (keyValue.Length == 2)
            {
                Parameters[keyValue[0]] = keyValue[1];
            }
            else
            {
                throw new InvalidOperationException($"Parameter {item} is not valid.");
            }
        }
    }
}
