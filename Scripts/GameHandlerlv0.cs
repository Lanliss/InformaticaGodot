using Godot;
using System;

public partial class GameHandlerlv0 : Node3D
{
	[Export] private int enemyCount = 1;
	[Export] private float nextLevelWaitAmount = 5f; //in Seconds
	[Export] private string nextLevelSceneAdress;

	private int deadEnemies = 0;
	private bool levelIsCLeared = false;
	private float timeWaited = 0f;

    private Messenger _messengerSingleton; //Messenger singleton

    public override void _Ready()
	{
        _messengerSingleton = GetNode<Messenger>("/root/Messenger"); //get the singleton
        _messengerSingleton.OnEnemyDeath += IncrementEnemyDeathCounter;
    }

	public void IncrementEnemyDeathCounter()
	{
		deadEnemies++;
		if (deadEnemies == enemyCount) {
			levelIsCLeared = true;
		}
	}

	public override void _Process(double delta)
	{
		if (!levelIsCLeared) { return; }
		
		if (timeWaited >= nextLevelWaitAmount) { GetTree().ChangeSceneToFile(nextLevelSceneAdress); }
		else { timeWaited += (float)delta; }

	}
}
