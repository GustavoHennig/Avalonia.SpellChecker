using System.Collections.Concurrent;
using System.Text;

namespace Avalonia.SpellChecker
{
    public class CustomDictionary
    {
        private readonly ConcurrentDictionary<string, HashSet<string>> customWordsByLanguage = new();

        //private readonly ConcurrentDictionary<(string lang, string word), bool> wordCheckCache = new();
        private readonly SpellCheckerConfig spellCheckerConfig;
        private readonly object fileLock = new object();

        public CustomDictionary(SpellCheckerConfig spellCheckerConfig)
        {
            this.spellCheckerConfig = spellCheckerConfig;
        }

        private string BuildCustomDictionaryFileName(string language)
        {

            string? customDictionaryPath = spellCheckerConfig.CustomDictionariesFolder;
            if (string.IsNullOrEmpty(customDictionaryPath))
            {
                customDictionaryPath = spellCheckerConfig.DictionariesFolder;
            }

            if (string.IsNullOrEmpty(customDictionaryPath))
            {
                return $"CustomDictionary.{language}.txt";
            }

            return Path.Combine(customDictionaryPath, $"CustomDictionary.{language}.txt");
        }


        private HashSet<string> ReadHashsetFromFile(string language)
        {
            var customDictionaryPath = BuildCustomDictionaryFileName(language);
            var hashset = new HashSet<string>();

            if (File.Exists(customDictionaryPath))
            {
                lock (fileLock)
                {
                    using (var customDictionaryStream = File.Open(customDictionaryPath, FileMode.Open, FileAccess.Read))
                    using (var customDictionaryReader = new StreamReader(customDictionaryStream, Encoding.UTF8, true))
                    {
                        string? line;
                        while ((line = customDictionaryReader.ReadLine()) != null)
                        {
                            if (!string.IsNullOrWhiteSpace(line)) // Handle empty lines
                            {
                                hashset.Add(line);
                            }
                        }
                    }
                }
            }

            return hashset;
        }


        public void Add(string wordToIgnore, string language)
        {
            if (CheckWord(wordToIgnore, language))
            {
                return;
            }

            var dictionary = GetCustomDictionary(language);

            dictionary.Add(wordToIgnore);
            File.AppendAllText(BuildCustomDictionaryFileName(language), wordToIgnore + Environment.NewLine);
        }

        private HashSet<string> GetCustomDictionary(string language)
        {
            return customWordsByLanguage.GetOrAdd(language, key => ReadHashsetFromFile(language));
        }

        public bool CheckWord(string word, string language)
        {

            var hashet = GetCustomDictionary(language);
            return hashet.Contains(word);

        }


    }
}
