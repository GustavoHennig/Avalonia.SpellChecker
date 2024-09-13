using Avalonia.Controls;
using Avalonia.Interactivity;

namespace Avalonia.SpellChecker.Demo;

public partial class MainWindow : Window
{
    private readonly TextBoxSpellChecker extendTextBox = new TextBoxSpellChecker();

    public MainWindow()
    {
        InitializeComponent();
        tbNotes.UpdateLayout();
        //    TbNotes.Text

        var textBox = this.FindControl<TextBox>("tbUserName");
        extendTextBox.Initialize(textBox, new SpellCheckerConfig { });
        extendTextBox.Initialize(this.FindControl<TextBox>("tbNotes"), new SpellCheckerConfig { });



        // Make sure the ContextMenu is initialized
        //textBox.ContextMenu ??= new ContextMenu();

        //var defaultContextMenu = (ContextMenu)this.Resources["DefaultTextBoxContextMenu"];

        //var r1 = new Avalonia.Themes.Simple.SimpleTheme().Resources["DefaultTextBoxContextMenu"];
        //var r2 = textBox.ContextMenu.Resources["DefaultTextBoxContextMenu"];

        //// Add a custom item to the existing ContextMenu
        //textBox.ContextMenu = defaultContextMenu ?? new ContextMenu();


        //var contextFlyout = (MenuFlyout)textBox.ContextFlyout;

        //contextFlyout.Items.Add(

        //    new MenuItem { Header = "Custom Action", Command = new CustomCommand(OnCustomAction) }
        //);

        //textBox.ContextMenu.Items
        //    .Add(
        //        new MenuItem { Header = "Custom Action", Command = new CustomCommand(OnCustomAction) }
        //    );
        //textBox.AddHandler(Control.ContextRequestedEvent, TextBox_ContextRequested, handledEventsToo: true);


        //textBox.ContextRequested += this.TextBox_ContextRequested;
        //  textBox.PointerPressed += OnPointerPressed;
    }


    //  private void OnPointerPressed(object sender, PointerPressedEventArgs e)
    //  {
    //      var textBox = (TextBox)sender;

    //      // Get the position of the click (where the user clicked in the TextBox)
    //      var point = e.GetPosition(textBox);


    //      var mf = new MenuFlyout();


    //      mf.Items.Add(


    //    new MenuItem { Header = "Custom Action", Command = new CustomCommand(OnCustomAction) }
    //);

    //      mf.ShowAt(textBox, true);

    //  }


    public void CustomAction_Click(object? sender, RoutedEventArgs args)
    {

    }




}