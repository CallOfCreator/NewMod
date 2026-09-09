using System.Text;

namespace NewMod.RoleLogic;

public readonly record struct InfoChatCommand(string Name, string Query)
{
    public static bool TryParse(string text, out InfoChatCommand command)
    {
        text = text.Trim();
        var separator = 0;
        while (separator < text.Length && !char.IsWhiteSpace(text[separator]))
            separator++;

        var name = text[..separator].ToLowerInvariant();
        if (name is not ("/help" or "/roles" or "/role" or "/r" or "/gamemodes" or "/gamemode" or "/factions" or "/faction"))
        {
            command = default;
            return false;
        }

        var query = new StringBuilder();
        for (var index = separator; index < text.Length; index++)
            if (!char.IsWhiteSpace(text[index]))
                query.Append(text[index]);

        command = new InfoChatCommand(name, query.ToString());
        return true;
    }
}