// Ignore Spelling: Avalonia

using System.Collections.Concurrent;
using System.Diagnostics;
using WeCantSpell.Hunspell;

namespace Avalonia.SpellChecker
{
    /// <summary>
    /// 
    /// The initial version of this code was taken from the YouShouldSpellcheck.Analyzer project.
    /// https://github.com/BrightLight/YouShouldSpellcheck.Analyzer/blob/master/source/YouShouldSpellcheck.Analyzer/DictionaryManager.cs
    /// </summary>
    public class DictionaryManager
    {
        private readonly ConcurrentDictionary<string, WordList> dictionaries = new();


        private readonly ConcurrentDictionary<(string lang, string word), bool> wordCheckCache = new();
        private readonly SpellCheckerConfig spellCheckerConfig;
        private readonly CustomDictionary customDictionary;

        public DictionaryManager(SpellCheckerConfig spellCheckerConfig)
        {
            this.spellCheckerConfig = spellCheckerConfig;
            this.customDictionary = new CustomDictionary(spellCheckerConfig);
        }

        /// <summary>
        /// Checks if a word is valid in any of the enabled languages.
        /// </summary>
        /// <param name="value">The word to check.</param>
        /// <returns>True if the word is valid in any of the enabled languages, otherwise false.</returns>
        public bool CheckWord(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return true;
            }

            foreach (var language in spellCheckerConfig.EnabledLanguages)
            {
                if (CheckWord(value, language))
                {
                    return true;
                }
            }
            return false;
        }


        /// <summary>
        /// Checks if a word is valid in the specified language.
        /// </summary>
        /// <param name="word">The word to check.</param>
        /// <param name="language">The language to check the word against.</param>
        /// <returns>True if the word is valid in the specified language, otherwise false.</returns>
        public bool CheckWord(string word, string language)
        {
            if (string.IsNullOrEmpty(language))
            {
                return true;
            }

            return wordCheckCache.GetOrAdd((language, word), key =>
            {
                if (customDictionary.CheckWord(word, language))
                {
                    return true;
                }

                var dictionary = GetDictionary(language);


                if (dictionary is null)
                {
                    // Review the behavior of nonexistent dictionary
                    return true;
                }

                return dictionary.Check(word);
            });
        }

        /// <summary>
        /// Retrieves a list of suggested words for a given word and language.
        /// </summary>
        /// <param name="word">The word to get suggestions for.</param>
        /// <param name="language">The language to get suggestions in.</param>
        /// <returns>An enumerable collection of suggested words.</returns>
        public IEnumerable<string> Suggest(string word, string language)
        {
            var dictionary = GetDictionary(language);
            return dictionary.Suggest(word);
        }

        /// <summary>
        /// Retrieves a list of suggested words for a given word in all enabled languages.
        /// </summary>
        /// <param name="word">The word to get suggestions for.</param>
        /// <returns>An enumerable collection of suggested words.</returns>
        public IEnumerable<string> Suggest(string word)
        {
            var suggestions = new HashSet<string>();
            foreach (var language in spellCheckerConfig.EnabledLanguages)
            {
                var dictionary = GetDictionary(language);
                foreach (var suggestion in dictionary.Suggest(word))
                {
                    suggestions.Add(suggestion);
                }
            }
            return suggestions;
        }

        private WordList GetDictionary(string language)
        {
            return dictionaries.GetOrAdd(language, CreateDictionary);
        }

        private WordList CreateDictionary(string language)
        {
            var dictionariesFolder = spellCheckerConfig.DictionariesFolder;
            if (dictionariesFolder == null)
            {
                throw new ArgumentNullException(nameof(spellCheckerConfig.DictionariesFolder));
            }

            var affixFile = Path.Combine(dictionariesFolder, language + ".aff");
            var dictionaryFile = Path.Combine(dictionariesFolder, language + ".dic");

            if (!File.Exists(affixFile))
            {
                throw new FileNotFoundException($"Affix file not found: {affixFile}");
            }

            if (!File.Exists(dictionaryFile))
            {
                throw new FileNotFoundException($"Dictionary file not found: {dictionaryFile}");
            }

            Debug.Print($"Creating new dictionary instance with dictionary file [{dictionaryFile}] and affix file [{affixFile}]");
            return WordList.CreateFromFiles(dictionaryFile, affixFile);
        }

        internal void AddCustomWord(string misspelledWord)
        {
            customDictionary.Add(misspelledWord, spellCheckerConfig.EnabledLanguages.FirstOrDefault());
            foreach (var language in spellCheckerConfig.EnabledLanguages)
            {
                wordCheckCache.TryRemove((language, misspelledWord), out _);
            }
        }
    }
}
