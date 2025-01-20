using Godot;
using System;

public partial class DeathScreen : Control
{
    [Export]
    private string nextLevelSceneAdress;


    public void OnButtonPress()
	{
        GetTree().ChangeSceneToFile(nextLevelSceneAdress);
    }
}
