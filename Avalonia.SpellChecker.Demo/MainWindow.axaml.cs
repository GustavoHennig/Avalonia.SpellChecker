using Avalonia.Controls;
using Avalonia.Interactivity;

namespace Avalonia.SpellChecker.Demo;

public partial class MainWindow : Window
{
    private readonly TextBoxSpellChecker extendTextBox;

    public MainWindow()
    {
        InitializeComponent();
        tbNotes.UpdateLayout();
        //    TbNotes.Text

        var textBox = this.FindControl<TextBox>("tbUserName");


        extendTextBox = new TextBoxSpellChecker(SpellCheckerConfig.Create("pt_BR", "en_GB"));
        extendTextBox.Initialize(textBox);
        extendTextBox.Initialize(this.FindControl<TextBox>("tbNotes"));

    }

    public void CustomAction_Click(object? sender, RoutedEventArgs args)
    {

    }

}