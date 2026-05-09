
namespace GUI;

/// <summary>
///   
///   Author: Aspen Tobler and Josh Elieson
///   Date:   Spring 2024
///   
/// Displays a help page describing how to use the spreadsheet.
/// 
/// </summary>
public partial class HelpPage : ContentPage
{
    /// <summary>
    ///  Initialize GUI and add to it via code.
    /// </summary>
    public HelpPage()
    {
        InitializeComponent();

        ReturnToMainPageButton.Clicked += ReturnToMainPage;
    }

    /// <summary>
    ///   Pushes this page back and returns to the main page.
    /// </summary>
    /// <param name="sender"> ignored </param>
    /// <param name="e">      ignored </param>
    async void ReturnToMainPage(object sender, EventArgs e)
    {
        await Navigation.PopModalAsync();
    }

}

