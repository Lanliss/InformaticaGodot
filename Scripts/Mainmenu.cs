using Godot;
using System;

public partial class Mainmenu : Control
{
    [Export] private string nextLevelSceneAdress;
    public void OnStartButton() { GetTree().ChangeSceneToFile(nextLevelSceneAdress); }

    public void OnQuitButton() { GetTree().Quit(); }
}
