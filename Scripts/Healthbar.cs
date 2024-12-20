using Godot;
using System;
using System.Diagnostics;

public partial class Healthbar : Node3D
{
	[Export] private Area3D parentCollisionArea;
	[Export] private ProgressBar progressBar;
	[Export] private int maxHealth = 100;

    [Signal]
    public delegate void updateHealthToNavEventHandler();
    public override void _Ready()
	{
		progressBar.MaxValue = maxHealth;
		progressBar.Value = progressBar.MaxValue;

		if (parentCollisionArea.HasSignal("area_entered"))
		{
			parentCollisionArea.AreaEntered += Parent_AreaEntered;
		}
		else { Debug.WriteLine("Healthbar parent doesnt have appropriate method"); }
	}

	private void Parent_AreaEntered(Area3D area)
	{

		if (area.HasMeta("dmg"))
		{

			if (progressBar.Value - (double)area.GetMeta("dmg") >= 0)
			{
				progressBar.Value -= (double)area.GetMeta("dmg");
			}
			else {
				progressBar.Value = 0; 
				EmitSignal(SignalName.updateHealthToNav);
			}
		}
		else { Debug.WriteLine("Area does not have the correct metadata"); }
	}
}
