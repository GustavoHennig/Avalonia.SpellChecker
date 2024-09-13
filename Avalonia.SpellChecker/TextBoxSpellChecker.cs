using Avalonia.Controls;
using Avalonia.Markup.Xaml.Styling;
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

            var transientMenuFalyout = new MenuFlyout();


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

            foreach (var suggestion in suggestions)
            {
                transientMenuFalyout.Items.Add(new MenuItem { Header = suggestion, Command = new CustomCommand(SuggestionSelected) });

            }

            if (suggestions.Any())
            {
                transientMenuFalyout.Items.Add(new MenuItem() { Header = "-" });
            }

            foreach (var existingItem in contextFlyout.Items)
            {
                if (existingItem is MenuItem mi)
                {
                    transientMenuFalyout.Items.Add(new MenuItem
                    {
                        Header = mi.Header,
                        Command = mi.Command,
                        CommandParameter = mi.CommandParameter,
                        IsEnabled = mi.IsEnabled,
                        HotKey = mi.HotKey,
                        InputGesture = mi.InputGesture
                    });
                }
            }


            transientMenuFalyout.ShowAt(textBox, true);


            e.Handled = true;
        }

        private void SuggestionSelected()
        {
            // TODO: Receive TextBox, Suggestion position and replace the word
        }


    }
    // Custom ICommand implementation
    public class CustomCommand : ICommand
    {
        private readonly Action _execute;

        public CustomCommand(Action execute)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        }

        public event EventHandler? CanExecuteChanged;

        public bool CanExecute(object? parameter)
        {
            return true; // You can modify this to add custom logic
        }

        public void Execute(object? parameter)
        {
            _execute();
        }
    }
}
