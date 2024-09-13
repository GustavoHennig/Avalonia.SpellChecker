namespace Avalonia.SpellChecker
{
    public class SpellCheckSuggestion
    {
        public int OriginalWordPosition { get; set; }
        public int OriginalWordLength { get; set; }
        public string WordSuggested { get; set; }
    }
}
