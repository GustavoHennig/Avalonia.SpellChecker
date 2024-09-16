using Avalonia.Controls;
using Avalonia.Interactivity;

namespace Avalonia.SpellChecker.Demo;

public partial class MainWindow : Window
{
    private readonly TextBoxSpellChecker textBoxSpellChecker;

    public MainWindow()
    {
        InitializeComponent();
        tbNotes.UpdateLayout();
        //    TbNotes.Text

        var textBox = this.FindControl<TextBox>("tbUserName");


        textBoxSpellChecker = new TextBoxSpellChecker(SpellCheckerConfig.Create(/*"pt_BR", */"en_GB"));
        textBoxSpellChecker.Initialize(textBox);
        textBoxSpellChecker.Initialize(this.FindControl<TextBox>("tbNotes"));

    }

    public void CustomAction_Click(object? sender, RoutedEventArgs args)
    {

    }

}