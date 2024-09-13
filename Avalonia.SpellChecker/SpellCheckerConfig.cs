using System.Reflection;

namespace Avalonia.SpellChecker
{
    /// <summary>
    /// SpellChecker settings
    /// TODO: Dictionaries folder
    /// TODO: Custom words
    /// TODO: Enable/Disable spell checking
    /// TODO: Enabled languages
    /// </summary>
    public class SpellCheckerConfig
    {
        public string DictionariesFolder { get; set; } = "Dictionaries";
        public List<string> EnabledLanguages { get; set; } = new List<string>();

        public static SpellCheckerConfig Create(params string[] languages)
        {
            var config = new SpellCheckerConfig
            {
                DictionariesFolder = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Dictionaries")
            };

            config.EnabledLanguages.AddRange(languages);

            return config;
        }

    }
}
