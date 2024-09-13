using Avalonia.Collections;
using Avalonia.Controls.Presenters;
using Avalonia.Media;
using Avalonia.Media.TextFormatting;
using Avalonia.Utilities;
using System.Text;

namespace Avalonia.SpellChecker;

/// <summary>
/// This class needs a clean up, tests and probably a refactor. 
/// </summary>
public class SpellCheckerTextPresenter : TextPresenter
{

    private SpellChecker spellChecker;
    private List<SpellCheckEntry> spellCheckResults = null;

    public SpellChecker SpellChecker
    {
        get { return this.spellChecker; }
        set { this.spellChecker = value; }
    }

    protected override TextLayout CreateTextLayout()
    {
        TextLayout result;

        var caretIndex = CaretIndex;
        var preeditText = PreeditText;
        var text = GetCombinedText(Text, caretIndex, preeditText);
        var typeface = new Typeface(FontFamily, FontStyle, FontWeight, FontStretch);
        var selectionStart = SelectionStart;
        var selectionEnd = SelectionEnd;
        var start = Math.Min(selectionStart, selectionEnd);
        var length = Math.Max(selectionStart, selectionEnd) - start;

        IReadOnlyList<ValueSpan<TextRunProperties>>? textStyleOverrides = null;

        var foreground = Foreground;

        if (!string.IsNullOrEmpty(preeditText))
        {
            var preeditHighlight = new ValueSpan<TextRunProperties>(caretIndex, preeditText.Length,
                    new GenericTextRunProperties(typeface, FontFeatures, FontSize,
                    foregroundBrush: foreground,
                    textDecorations: TextDecorations.Underline));

            textStyleOverrides = new[]
            {
                    preeditHighlight
                };
        }
        else
        {
            if (length > 0 && SelectionForegroundBrush != null)
            {
                textStyleOverrides = new[]
                {
                        new ValueSpan<TextRunProperties>(
                               start,
                               length,
                                new GenericTextRunProperties(
                                    typeface, FontFeatures, FontSize,
                                    foregroundBrush: SelectionForegroundBrush))
                    };
            }
        }

        bool styled = false;

        if (Text?.Length > 1)
        {

            var fontFeatures = new FontFeatureCollection();
            //fontFeatures.Add(new FontFeature(){  "smcp", 1));
            var tf = new Typeface(FontFamily, FontStyle.Italic, FontWeight, FontStretch);

            var dec = new TextDecorationCollection();
            dec.Add(new TextDecoration()
            {
                Location = TextDecorationLocation.Underline,
                Stroke = Brushes.OrangeRed,
                StrokeDashArray = new AvaloniaList<double>(new[] { 1, 2.0 }),
                StrokeLineCap = PenLineCap.Round,

                StrokeThickness = 22.5
            });
            //dec.Add(new TextDecoration()
            //{
            //    Location = TextDecorationLocation.Underline,
            //    Stroke = Brushes.Green,
            //  //  StrokeDashArray = new AvaloniaList<double>(new[] { 2, 2.0 }),
            //     StrokeLineCap = PenLineCap.Round,

            //    StrokeThickness = 31
            //});

            //var words = SpellCheckService.SeparateWords(Text);
            spellCheckResults = spellChecker.CheckSpellingFullText(Text);
            List<ValueSpan<TextRunProperties>> overrides = new List<ValueSpan<TextRunProperties>>();

            var underlineProp = new GenericTextRunProperties(
                                    typeface, FontFeatures, FontSize,

                                    textDecorations: dec,
                                    foregroundBrush: this.Foreground);

            foreach (var word in spellCheckResults)
            {
                overrides.Add(new ValueSpan<TextRunProperties>(word.Start, word.Length, underlineProp));
            }

            textStyleOverrides = overrides.ToArray();


            styled = true;
        }

        if (styled)
        {
            result = CreateTextLayoutInternal(this.DesiredSize, Text, typeface, textStyleOverrides);

        }
        else

        if (PasswordChar != default(char) && !RevealPassword)
        {
            result = CreateTextLayoutInternal(this.DesiredSize, new string(PasswordChar, text?.Length ?? 0), typeface,
        textStyleOverrides);
        }
        else
        {
            result = CreateTextLayoutInternal(this.DesiredSize, text, typeface, textStyleOverrides);
        }

        return result;
    }

    public IEnumerable<SpellCheckSuggestion>? GetSuggestionsAt(Point point)
    {

        if (spellCheckResults == null)
        {
            return null;
        }

        var result = TextLayout.HitTestPoint(point);

        return spellCheckResults
            .Where(x => x.Start <= result.TextPosition && (x.Start + x.Length) >= result.TextPosition)
            .Select(x => spellChecker.GetSuggestions(x.Word, x.Start, x.Length)).FirstOrDefault();
    }

    private static string? GetCombinedText(string? text, int caretIndex, string? preeditText)
    {
        if (string.IsNullOrEmpty(preeditText))
        {
            return text;
        }

        if (string.IsNullOrEmpty(text))
        {
            return preeditText;
        }

        var sb = new StringBuilder(text.Length + preeditText.Length);

        sb.Append(text.Substring(0, caretIndex));
        sb.Insert(caretIndex, preeditText);
        sb.Append(text.Substring(caretIndex));

        return sb.ToString();
    }


    /// <summary>
    /// Creates the <see cref="TextLayout"/> used to render the text.
    /// </summary>
    /// <param name="constraint">The constraint of the text.</param>
    /// <param name="text">The text to format.</param>
    /// <param name="typeface"></param>
    /// <param name="textStyleOverrides"></param>
    /// <returns>A <see cref="TextLayout"/> object.</returns>
    private TextLayout CreateTextLayoutInternal(Size constraint, string? text, Typeface typeface,
        IReadOnlyList<ValueSpan<TextRunProperties>>? textStyleOverrides)
    {
        var foreground = Foreground;
        var maxWidth = MathUtilities.IsZero(constraint.Width) ? double.PositiveInfinity : constraint.Width;
        var maxHeight = MathUtilities.IsZero(constraint.Height) ? double.PositiveInfinity : constraint.Height;

        var textLayout = new TextLayout(text, typeface, FontFeatures, FontSize, foreground, TextAlignment,
            TextWrapping, maxWidth: maxWidth, maxHeight: maxHeight, textStyleOverrides: textStyleOverrides,
            flowDirection: FlowDirection, lineHeight: LineHeight, letterSpacing: LetterSpacing);

        return textLayout;
    }

    internal void SetSpellCheckerConfig(SpellCheckerConfig config)
    {
        throw new NotImplementedException();
    }
}
