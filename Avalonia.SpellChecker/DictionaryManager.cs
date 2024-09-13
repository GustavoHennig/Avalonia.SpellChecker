using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text;
using WeCantSpell.Hunspell;

namespace Avalonia.SpellChecker
{
    /// <summary>
    /// 
    /// The initial version of this code was taken from the YouShouldSpellcheck.Analyzer project.
    /// https://github.com/BrightLight/YouShouldSpellcheck.Analyzer/blob/master/source/YouShouldSpellcheck.Analyzer/DictionaryManager.cs
    /// </summary>
    public static class DictionaryManager
    {
        private static readonly ConcurrentDictionary<string, WordList?> dictionaries = new();

        private static readonly ConcurrentDictionary<string, List<string>> customWordsByLanguage = new();

        private static readonly ConcurrentDictionary<Tuple<string, string>, bool> cache = new();

        public static SpellCheckerConfig DefaultConfig { get; set; } = new SpellCheckerConfig();

        public static bool IsWordCorrect(string word, string language)
        {
            if (string.IsNullOrEmpty(language))
            {
                return true;
            }

            var key = new Tuple<string, string>(language, word);
            if (cache.TryGetValue(key, out var wordIsOkay))
            {
                return wordIsOkay;
            }

            if (IsCustomWord(word, language))
            {
                cache.TryAdd(key, true);
                return true;
            }

            // Logger.Log($"IsWordCorrect: [{word}] [{language}]");
            var dictionary = GetDictionaryForLanguage(language);
            wordIsOkay = dictionary?.Check(word) ?? false;
            cache.TryAdd(key, wordIsOkay);
            return wordIsOkay;
        }

        public static bool Suggest(string word, out List<string>? suggestions, string language)
        {
            var dictionary = GetDictionaryForLanguage(language);
            if (dictionary != null)
            {
                suggestions = dictionary.Suggest(word).ToList();
                return true;
            }

            suggestions = null;
            return false;
        }

        private static string GetCustomDictionaryFileName(string language)
        {
            if (string.IsNullOrEmpty(DefaultConfig.DictionariesFolder))
            {
                return string.Empty;
            }

            return Path.Combine(DefaultConfig.DictionariesFolder, $"CustomDictionary.{language}.txt");
        }

        private static void AddToInMemoryCustomDictionary(string wordToIgnore, string language)
        {
            var customDictionary = GetInMemoryCustomDictionary(language);
            customDictionary.Add(wordToIgnore);
        }

        private static List<string> GetInMemoryCustomDictionary(string language)
        {
            if (!customWordsByLanguage.TryGetValue(language, out var customDictionary))
            {
                customDictionary = new List<string>();
                var customDictionaryPath = GetCustomDictionaryFileName(language);
                if (File.Exists(customDictionaryPath))
                {
                    using (var customDictionaryStream = File.Open(customDictionaryPath, FileMode.Open, FileAccess.Read))
                    using (var customDictionaryReader = new StreamReader(customDictionaryStream, Encoding.UTF8, true))
                    {
                        var customDictionaryContent = customDictionaryReader.ReadToEnd();
                        customDictionary.AddRange(customDictionaryContent.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries));
                        ////customWords.AddRange(File.ReadAllLines(customDictionaryPath, Encoding.UTF8));
                    }
                }

                customWordsByLanguage.TryAdd(language, customDictionary);
            }

            return customDictionary;
        }

        public static void AddToCustomDictionary(string wordToIgnore, string language)
        {
            if (!IsCustomWord(wordToIgnore, language))
            {
                AddToInMemoryCustomDictionary(wordToIgnore, language);
                var customDictionaryPath = GetCustomDictionaryFileName(language);
                try
                {
                    using (var customDictionaryStream = File.Open(customDictionaryPath, FileMode.OpenOrCreate, FileAccess.Write))
                    using (var customDictionaryWriter = new StreamWriter(customDictionaryStream, Encoding.UTF8))
                    {
                        foreach (var line in customWordsByLanguage[language])
                        {
                            customDictionaryWriter.WriteLine(line);
                        }

                        customDictionaryWriter.Flush();
                        customDictionaryWriter.Close();

                        ////File.WriteAllLines(customDictionaryPath, customWords, Encoding.UTF8);
                    }
                }
                catch (Exception e)
                {
                    Debug.Print($"An exception occurred while adding [{wordToIgnore}] to the custom dictionary [{customDictionaryPath}]:\r\n{e}");
                    throw;
                }

                // remove from internal cache
                var key = new Tuple<string, string>(language, wordToIgnore);
                cache.TryUpdate(key, true, false);
            }
        }

        public static bool IsCustomWord(string word, string language)
        {
            Debug.Print($"IsCustomWord ({language}): [{word}]");
            var customDictionary = GetInMemoryCustomDictionary(language);
            return customDictionary != null && customDictionary.Contains(word);
        }

        private static WordList? GetDictionaryForLanguage(string language)
        {
            if (!dictionaries.TryGetValue(language, out var dictionary))
            {
                dictionary = CreateDictionary(language);
                dictionaries.TryAdd(language, dictionary);
            }

            return dictionary;
        }

        private static WordList? CreateDictionary(string language)
        {
            var dictionariesFolder = DefaultConfig.DictionariesFolder;
            if (dictionariesFolder == null)
            {
                throw new ArgumentNullException(nameof(DefaultConfig.DictionariesFolder));
            }

            var affixFile = Path.Combine(dictionariesFolder, language + ".aff");
            var dictionaryFile = Path.Combine(dictionariesFolder, language + ".dic");

            if (!File.Exists(affixFile))
            {
                throw new Exception($"Affix file not found: [{affixFile}]");
            }

            if (!File.Exists(dictionaryFile))
            {
                throw new Exception($"Dictionary file not found: [{dictionaryFile}]");
            }

            Debug.Print($"Creating new dictionary instance with dictionary file [{dictionaryFile}] and affix file [{affixFile}]");
            return WordList.CreateFromFiles(dictionaryFile, affixFile);
        }
    }
}
