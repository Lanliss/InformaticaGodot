using Godot;
using System;
using static Messenger;

public partial class Messenger : Node
{
	[Signal]
	public delegate void TriggerCooldownTimerEventHandler(int index, float value);
	[Signal]
	public delegate void OnStartCooldownEventHandler(int index, float value);

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		OnStartCooldown += DoTriggerCooldownTimer;

	}
 	// Hello from github

	private void DoTriggerCooldownTimer(int index, float value) 
	{
		EmitSignal(SignalName.TriggerCooldownTimer, index, value);
		GD.Print("reached messenger, tried to call TriggerCooldownTimer event, with index = " + index.ToString() + " and value is " + value.ToString());
	}
}
