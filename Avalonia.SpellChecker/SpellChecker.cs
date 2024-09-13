using System.Text.RegularExpressions;

namespace Avalonia.SpellChecker
{
    public class SpellChecker
    {
        //private readonly WordList _wordList;


        /// <summary>
        /// Keeps track of the dictionaries and custom words
        /// </summary>
        private static DictionaryManager dictionaryManager;

        public SpellChecker(SpellCheckerConfig spellCheckerConfig)
        {
            //_wordList = WordList.CreateFromFiles(dictionaryPath);
            dictionaryManager ??= new DictionaryManager(spellCheckerConfig);
        }



        public List<SpellCheckResult> CheckSpellingFullText(string inputText)
        {
            var results = new List<SpellCheckResult>();

            if (string.IsNullOrWhiteSpace(inputText))
            {
                return results;
            }


            var words = SeparateWords(inputText);

            foreach (var word in words)
            {
                if (!dictionaryManager.CheckWord(word.Value))
                {
                    //var suggestions = _wordList.Suggest(word.Value);
                    results.Add(new SpellCheckResult
                    {
                        Start = word.Index,
                        Length = word.Length,
                        Word = word.Value
                        //Suggestions = suggestions.ToList()
                    });
                }
            }

            return results;
        }

        public IEnumerable<string> GetSuggestions(string word)
        {

            if (string.IsNullOrWhiteSpace(word))
            {
                return Enumerable.Empty<string>();
            }

            return dictionaryManager.Suggest(word);

        }

        public static IEnumerable<Match> SeparateWords(string text)
        {
            // Regex aprimorada para capturar palavras em idiomas ocidentais
            var regex = new Regex(@"\b['']?[\p{L}\p{M}]+(?:[''-][\p{L}\p{M}]+)*['']*\b");

            // Explicação detalhada da regex:
            // \b                     - limite de palavra
            // ['']?                  - apóstrofo opcional no início (incluindo variações tipográficas)
            // [\p{L}\p{M}]+          - uma ou mais letras ou marcas diacríticas
            // (?:[''-][\p{L}\p{M}]+)* - grupo não-capturante para hífens ou apóstrofos seguidos de letras, repetido zero ou mais vezes
            // ['']*                  - apóstrofos opcionais no final
            // \b                     - limite de palavra

            var matches = regex.Matches(text);
            foreach (Match match in matches)
            {
                yield return match;
            }
        }
        public void OnTextChange(string text)
        {
            // Check the spelling of the text
            // Highlight the misspelled words
            //nun       
        }

    }
}
