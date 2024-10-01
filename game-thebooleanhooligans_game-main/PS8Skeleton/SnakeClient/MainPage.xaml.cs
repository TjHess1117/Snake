//using Windows.Gaming.Input;
//using Microsoft.UI.Xaml.Controls;

namespace SnakeGame;

public partial class MainPage : ContentPage
{
    public event Action<World> drawWorld;
    private GameController controller = new GameController();
    public World world;
    public MainPage()
    {
        InitializeComponent();
        graphicsView.Invalidate();
        controller.updateView += RedrawWorld;
        controller.ErrorOccurred += NetworkErrorHandler;
    }
    /// <summary>
    /// this is the method that is subscribed to the event that is called once the world is updated
    /// it means we need to redraw the world start by givning the world and the graphics view to the world panel
    /// </summary>
    /// <param name="world"></param>
    private void RedrawWorld(World world)
    {
        lock(world) 
        { 
            this.world = world;
            worldPanel.GiveWorld(world, graphicsView);
            this.OnFrame();
        }
    }

    void OnTapped(object sender, EventArgs args)
    {
        keyboardHack.Focus();
    }
    /// <summary>
    /// calls the correct controller method based on the desired direction
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    void OnTextChanged(object sender, Microsoft.Maui.Controls.TextChangedEventArgs args)
    {
        Entry entry = (Entry)sender;
        String text = entry.Text.ToLower();
        if (text == "w")
        {
            controller.OnMoveUp();
        }
        else if (text == "a")
        {
            controller.OnMoveLeft();
        }
        else if (text == "s")
        {
            controller.OnMoveDown();
        }
        else if (text == "d")
        {
            controller.OnMoveRight();
        }
        else if(text == "")
        {
        controller.OnNoMovement();

        }
        entry.Text = "";
    }

    private void NetworkErrorHandler(string ErorrMessage)
    {
        DisplayAlert("Error", "Disconnected from server: \n Erorr Type: " + ErorrMessage, "OK");
        // display an error message if one occured
    }


    /// <summary>
    /// Event handler for the connect button
    /// We will put the connection attempt interface here in the view.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    private void ConnectClick(object sender, EventArgs args)
    {
        if (serverText.Text == "")
        {
            DisplayAlert("Error", "Please enter a server address", "Ok");
            return;
        }
        if (nameText.Text == "")
        {
            DisplayAlert("Error", "Please enter a name", "OK");
            return;
        }
        if (nameText.Text.Length > 16)
        {
            DisplayAlert("Error", "Name must be less than 16 characters", "OK");
            return;
        }
        // start atempt connection
        // invoke a method to start a connection in the controller with the correct IP and user Name
        controller.Connect(serverText.Text, nameText.Text);  // for some reason we are getting an erorr that says the networking library cant be found.
        keyboardHack.Focus();
    }

    /// <summary>
    /// Use this method as an event handler for when the controller has updated the world
    /// </summary>
    public void OnFrame()
    {
        Dispatcher.Dispatch(() => graphicsView.Invalidate());
    }

    private void ControlsButton_Clicked(object sender, EventArgs e)
    {
        DisplayAlert("Controls",
                     "W:\t\t Move up\n" +
                     "A:\t\t Move left\n" +
                     "S:\t\t Move down\n" +
                     "D:\t\t Move right\n",
                     "OK");
    }

    private void AboutButton_Clicked(object sender, EventArgs e)
    {
        DisplayAlert("About",
      "SnakeGame solution\nArtwork by Jolie Uk and Alex Smith\nGame design by Daniel Kopta and Travis Martin\n" +
      "Implementation by ...\n" +
        "CS 3500 Fall 2022, University of Utah", "OK");
    }

    private void ContentPage_Focused(object sender, FocusEventArgs e)
    {
        if (!connectButton.IsEnabled)
            keyboardHack.Focus();
    }
}