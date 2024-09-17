using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.VisualTree;
using System.Windows.Input;

namespace Avalonia.SpellChecker
{
    /// <summary>
    /// This class applies spell checking to TextBox controls without the need of extending the TextBox class.
    /// </summary>
    public class TextBoxSpellChecker
    {
        private readonly HashSet<TextBox> controls = new HashSet<TextBox>();
        private readonly SpellCheckerConfig config;

        public TextBoxSpellChecker(SpellCheckerConfig config)
        {
            this.config = config;
        }

        public void Initialize(TextBox textBox)
        {

            if (config is null)
            {
                return;
            }


            if (config.EnabledLanguages is null || config.EnabledLanguages.Count == 0)
            {
                return;
            }

            controls.Add(textBox);

            // Create a new StyleInclude instance
            var styleInclude = new StyleInclude(new Uri("avares://Avalonia.SpellChecker/"))
            {
                Source = new Uri("avares://Avalonia.SpellChecker/Styles/SpellCheckerStyles.axaml")
            };

            // Add the style to the Window's Styles collection
            textBox.Styles.Add(styleInclude);

            // Initialize the SpellCheckerTextPresenter setting
            textBox.TemplateApplied += OnTemplateApplied;

            // Clean up
            textBox.DetachedFromLogicalTree += OnTextBoxDisposed;

            textBox.AddHandler(Control.ContextRequestedEvent, TextBox_ContextRequested, handledEventsToo: true);
        }

        private void OnTemplateApplied(object? sender, Controls.Primitives.TemplateAppliedEventArgs e)
        {
            var textPresenter = e.NameScope.Find<SpellCheckerTextPresenter>("PART_TextPresenter");

            if (textPresenter is null)
            {
                return;
            }

            textPresenter.SpellChecker = new SpellChecker(config);

            if (sender is TextBox textBox)
            {
                textBox.TemplateApplied -= OnTemplateApplied;
            }
        }


        private void OnTextBoxDisposed(object? sender, Avalonia.LogicalTree.LogicalTreeAttachmentEventArgs e)
        {
            if (sender is null)
            {
                return;
            }

            if (sender is TextBox textBox)
            {
                textBox.DetachedFromLogicalTree -= OnTextBoxDisposed;
                textBox.RemoveHandler(Control.ContextRequestedEvent, TextBox_ContextRequested);
                controls.Remove(textBox);
            }
        }

        private void TextBox_ContextRequested(object? sender, ContextRequestedEventArgs e)
        {

            if (sender is not TextBox textBox)
            {
                return;
            }

            if (textBox.ContextFlyout is not MenuFlyout contextFlyout)
            {
                return;
            }


            if (!e.TryGetPosition(textBox, out Point point))
            {
                return;
            }


            var textPresenter = textBox.GetVisualDescendants().OfType<Avalonia.SpellChecker.SpellCheckerTextPresenter>().FirstOrDefault();


            if (textPresenter is null)
            {
                return;
            }

            var suggestions = textPresenter.GetSuggestionsAt(point);


            if (suggestions is null)
            {
                // No suggestions available
                return;
            }

            // Remove suggestions after the context menu is closed
            contextFlyout.Closed += this.TransientMenuFalyout_Closed;

            int insertIndex = 0;

            foreach (var suggestion in suggestions)
            {
                contextFlyout.Items.Insert(insertIndex++, new MenuItem
                {
                    Header = suggestion.WordSuggested,
                    Tag = "spell",
                    Command = new AcceptSuggestionCommand(SuggestionSelected, textBox, suggestion)
                });
            }

            if (insertIndex > 0)
            {
                contextFlyout.Items.Insert(insertIndex, new MenuItem() { Header = "-", Tag = "spell" });
            }

            contextFlyout.ShowAt(textBox, true);

            e.Handled = true;
        }

        private void TransientMenuFalyout_Closed(object? sender, EventArgs e)
        {
            if (sender is not MenuFlyout menu)
            {
                return;
            }

            menu.Closed -= this.TransientMenuFalyout_Closed;

            for (int i = menu.Items.Count - 1; i >= 0; i--)
            {
                if (menu.Items[i] is MenuItem mi && "spell".Equals(mi.Tag))
                {
                    menu.Items.RemoveAt(i);
                }
            }
        }

        private void SuggestionSelected(TextBox textBox, SpellCheckSuggestion suggestion)
        {
            textBox.Text = textBox.Text?
                .Remove(suggestion.OriginalWordPosition, suggestion.OriginalWordLength)
                .Insert(suggestion.OriginalWordPosition, suggestion.WordSuggested);
        }


    }
    // Custom ICommand implementation
    public class AcceptSuggestionCommand : ICommand
    {
        private readonly Action<TextBox, SpellCheckSuggestion> execute;
        private readonly TextBox textBox;
        private readonly SpellCheckSuggestion suggestion;

        public AcceptSuggestionCommand(Action<TextBox, SpellCheckSuggestion> execute, TextBox textBox, SpellCheckSuggestion suggestion)
        {
            this.execute = execute ?? throw new ArgumentNullException(nameof(execute));
            this.textBox = textBox;
            this.suggestion = suggestion;
        }

        public event EventHandler? CanExecuteChanged;

        public bool CanExecute(object? parameter)
        {
            return true;
        }

        public void Execute(object? parameter)
        {
            execute(this.textBox, this.suggestion);
        }
    }
}
