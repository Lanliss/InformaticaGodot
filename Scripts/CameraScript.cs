using Godot;
using Godot.Collections;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

public partial class CameraScript : Camera3D
{
	[Export] private CharacterBody3D _player;
	[Export] private float camDetectDistanceX = 1f;
	[Export] private float camDetectDistanceZ = 1f;
	[Export] private int frameCount = 20; //How many frames should it take the camera to go the distance //perhaps better for fixed update
	[Export] private float speed = 1f;
	[Export] private bool useCustomOffset = false;
	[Export] private float camOffsetZ = 6f;   //used for correcting within the camera movement.;
	[Export] private float camOffsetX = 6f;

	[ExportCategory("Ray Scan Properties")]
	[ExportGroup("Ray Properties")]
	[Export] private float rayLength = 1000f;

	[ExportGroup("Collision Mask")]
	[Export(PropertyHint.Layers2DNavigation)]
	private uint collisionMask = 37;

	[Signal]
	public delegate void MousePosFoundEventHandler(Vector3 mousePosition);

	public override void _Ready()
	{
		if(useCustomOffset == true) { 
			camOffsetZ = GlobalPosition.Z;	//correct for cam position
			camOffsetX = GlobalPosition.X;
		}
	}

	public override void _PhysicsProcess(double delta)
	{
		Vector2 mousePos = GetViewport().GetMousePosition();
		Vector3 from = ProjectRayOrigin(mousePos);
		Vector3 to = from + ProjectRayNormal(mousePos) * rayLength;
		Godot.PhysicsDirectSpaceState3D space = GetWorld3D().DirectSpaceState;
		PhysicsRayQueryParameters3D ray = new PhysicsRayQueryParameters3D();
		ray.CollisionMask = collisionMask; //1, 4, 32. environment, enemy and pickups //collision mask is for what things the click should interact with

		ray.From = from;
		ray.To = to;

		Dictionary rayCastResult = space.IntersectRay(ray);
		if (rayCastResult.ContainsKey("position"))
		{
			var mousePosition = (Vector3)rayCastResult["position"];
			EmitSignal(SignalName.MousePosFound, mousePosition);
		}
		

		//move camera X
		if (_player.GlobalPosition.X - (GlobalPosition.X - camOffsetX) >= camDetectDistanceX) 
		{
			GlobalPosition += new Vector3((_player.GlobalPosition.X - (GlobalPosition.X - camOffsetX + camDetectDistanceX)) / (frameCount * (float)delta), 0, 0);
		}
		else if (_player.GlobalPosition.X - (GlobalPosition.X - camOffsetX) <= -1 * camDetectDistanceX) 
		{
			GlobalPosition += new Vector3((_player.GlobalPosition.X - (GlobalPosition.X - camOffsetX - camDetectDistanceX)) / (frameCount * (float)delta), 0, 0);
		}

		//move camera Y
		if (_player.GlobalPosition.Z - (GlobalPosition.Z - camOffsetZ) >= camDetectDistanceZ)
		{
			GlobalPosition += new Vector3(0, 0, (_player.GlobalPosition.Z - (GlobalPosition.Z - camOffsetZ + camDetectDistanceZ)) / (frameCount * (float)delta));
		}
		else if (_player.GlobalPosition.Z - (GlobalPosition.Z - camOffsetZ) <= -1 * camDetectDistanceZ)
		{
			GlobalPosition += new Vector3(0, 0, (_player.GlobalPosition.Z - (GlobalPosition.Z - camOffsetZ - camDetectDistanceZ)) / (frameCount * (float)delta));
		}
	}
}
