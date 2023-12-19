using Net.Shared.Bots.Abstractions.Interfaces;

namespace Net.Shared.Bots.Abstractions;

public static class Constants
{
    public enum BotMessageType
    {
        /// <summary>
        /// The <see cref="IBotMessage"/> contains text
        /// </summary>
        Command,

        /// <summary>
        /// The <see cref="IBotMessage"/> contains an Image
        /// </summary>
        Image,

        /// <summary>
        /// The <see cref="IBotMessage"/> contains an Audio
        /// </summary>
        Audio,

        /// <summary>
        /// The <see cref="IBotMessage"/> contains a Video
        /// </summary>
        Video,

        /// <summary>
        /// The <see cref="IBotMessage"/> contains a Voice
        /// </summary>
        Voice,

        /// <summary>
        /// The <see cref="IBotMessage"/> contains a Document
        /// </summary>
        Document,

        /// <summary>
        /// The <see cref="IBotMessage"/> contains a Location
        /// </summary>
        Location,

        /// <summary>
        /// The <see cref="IBotMessage"/> contains a Contact
        /// </summary>
        Contact,

        /// <summary>
        /// The <see cref="IBotMessage"/> contains WebApp
        /// </summary>
        WebApp,
    }
}
