using Godot;
using System;
using static Messenger;

public partial class UiNumberScript : Control
{
	[Export]
	private Label label;
	[Export]
	private int timerID = 0;

    private Messenger _messengerSingleton; //Messenger singleton

    public override void _Ready()
	{
        _messengerSingleton = GetNode<Messenger>("/root/Messenger"); //get the singleton
		_messengerSingleton.TriggerCooldownTimer += DoCooldown;
    }

    public override void _Process(double delta)
    {
		if (float.Parse(label.Text) > 0f)
		{
			//GD.Print("label has value ");
			string newText;

			if (float.Parse(label.Text) - delta > 0)
			{
				newText = (float.Parse(label.Text) - delta).ToString("#.##");
			}
			else newText = 0.ToString("#.##");

			label.Text = newText;
		}
			
    }

	public void DoCooldown(int index, float value) 
	{
        GD.Print("trigger cooldown timer signal");
		if (index == timerID)
		{
			GD.Print("Correct index");
			label.Text = value.ToString("0.##");
		}
		else { GD.Print("Incorrect index"); }
	}
}
