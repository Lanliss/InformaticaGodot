using Godot;
using System;
using static Messenger;

public partial class UiNumberScript : Control
{
	[Export]
	private Label label;
	[Export]
	private int timerID = 0;
	private float timerValue = 0;
    private Messenger _messengerSingleton; //Messenger singleton

    public override void _Ready()
	{
        _messengerSingleton = GetNode<Messenger>("/root/Messenger"); //get the singleton
		_messengerSingleton.TriggerCooldownTimer += DoCooldown;
    }

    public override void _Process(double delta)
    {
		if (timerValue > 0)
		{
			//GD.Print("label has value ");
			string newText;

			if (timerValue - (float)delta > 0.01f)
			{
				timerValue -= (float)delta;
				newText = ((double)Math.Round(timerValue, 1)).ToString("0.0");
			}
			else
			{
				timerValue = 0;
				newText = "0.0";
			}

			label.Text = newText;
		}

        //if (double.Parse(label.Text) > 0)
        //{
        //    //GD.Print("label has value ");
        //    string newText;

        //    if (float.Parse(label.Text) - delta > 0.01)
        //    {
        //        newText = ((double)Math.Round((float.Parse(label.Text) - (float)delta), 1)).ToString("#.#");
        //    }
        //    else newText = "00";

        //    label.Text = newText;
        //}

    }

	public void DoCooldown(int index, float value) 
	{
		if (index == timerID)
		{
			GD.Print("Correct index");
			timerValue = value;
		}
		else { GD.Print("Incorrect index"); }
	}
}
