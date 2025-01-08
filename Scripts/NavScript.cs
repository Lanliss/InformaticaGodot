using Godot;

public partial class NavScript : CharacterBody3D
{
	[Export]
	private float moveSpeed = 3f;
	[Export]
	private float rotationSpeed = 1.0f;

	[Export]
	private NavigationAgent3D _navigationAgent;
	[Export]
	private CollisionShape3D playerDetectionShape3D;
	[Export]
	private Node3D visualEnemy;

	[ExportCategory("Attack Settings")]
	[Export] private float attackRange = 1f;

	[ExportSubgroup("Attack1")]
	[Export] private Area3D attackHitbox;
	[Export] private float attackDuration1 = 0.25f;
	[Export] private float attackCooldown1 = 0.25f;

	[ExportCategory("Pathfinding settings")]
	[Export]
	private float pathDistance = 1f;
	[Export]
	private float targetDistance = 0.5f;
	[Export]
	private float pathfindingUpdateTime = 0.01f;
	[Export]
	private float priority = 1f;
	[Export]
	private bool dead = false;
	//end of exports



	private float targetRotation;
	private Node3D playerNode; 
	private float timeSincePathfindingUpdate = 0;
	private bool playerFound = false;
	private Vector3 _movementTargetPosition = new Vector3(0, 0, 0);
	private Vector3 deathPos;

	public Vector3 MovementTarget
	{
		get { return _navigationAgent.TargetPosition; }
		set { _navigationAgent.TargetPosition = value; }
	}

	public void Die(int health)
	{
		if (!dead && health == 0)
		{
			deathPos = GlobalPosition;
			dead = true;
		}
		else if (!dead && !playerFound) { //this part of the function is not death related

			playerDetectionShape3D.Shape.Set(CylinderShape3D.PropertyName.Radius, 100f);

		}
	}

	public override void _Ready()
	{
		_movementTargetPosition = GlobalPosition;

		_navigationAgent.PathDesiredDistance = pathDistance;
		_navigationAgent.TargetDesiredDistance = targetDistance;
		_navigationAgent.AvoidancePriority = priority;

		Callable.From(ActorSetup).CallDeferred();
	}
	 
	public void OnVelocity(Vector3 safeVelocity)
	{
		Velocity = safeVelocity;
		targetRotation = Mathf.Atan2(safeVelocity.X, safeVelocity.Z);
		MoveAndSlide();
	}

	public void OnPlayerDetected(Node3D playerNode3D) //connects the player, when they enter the detection area, signal needs to be set up to work for this.
	{
		playerNode = playerNode3D;
		MovementTarget = playerNode.GlobalPosition;
		playerFound = true;
	}

	public override void _PhysicsProcess(double delta)
	{
		if (dead) { if (MovementTarget != deathPos || GlobalPosition != deathPos) { MovementTarget = deathPos; GlobalPosition = deathPos; } return; /*Jezus arc?*/} //sets position to be dead so that it doesn't move // returns if dead cuz we don't need to update anymore
		if (playerFound)
		{
			Vector3 distanceVector = GlobalPosition - playerNode.GlobalPosition;
			if (_navigationAgent.IsNavigationFinished() && distanceVector.Length() <= _navigationAgent.TargetDesiredDistance) { 
				//initiate attack
				
				
				
				return; } //stops when player is in range

			if (timeSincePathfindingUpdate + (float)delta >= pathfindingUpdateTime)
			{
				timeSincePathfindingUpdate = 0f;
				MovementTarget = playerNode.GlobalPosition;
			}
			else
			{
				timeSincePathfindingUpdate += (float)delta;
			}

			//rotation
			if (visualEnemy.Rotation.Y != targetRotation)
			{
				float difference = targetRotation - visualEnemy.Rotation.Y;
				if (difference > Mathf.Pi) { difference -= 2 * Mathf.Pi; }          //correct the rotation so that it rotates to the side that is closest
				if (-1 * difference > Mathf.Pi) { difference += 2 * Mathf.Pi; }

				//determine what direction calc is needed in
				if (Mathf.Abs(difference) > rotationSpeed * (float)delta) 
				{
					if (difference < 0)
					{
						visualEnemy.Rotation -= new Vector3(0, rotationSpeed * (float)delta, 0); 
					}
					else
					{
						visualEnemy.Rotation -= new Vector3(0, -1 * rotationSpeed * (float)delta, 0);
					}
				}
				else { visualEnemy.Rotation = new Vector3(0, targetRotation, 0); }//just rotate, no calc needed
			}
		}
		else if (_navigationAgent.IsNavigationFinished()) { return; }
		Vector3 currentAgentPosition = GlobalTransform.Origin;
		Vector3 nextPathPosition = _navigationAgent.GetNextPathPosition();

		Vector3 unsafeVelocity = currentAgentPosition.DirectionTo(nextPathPosition) * moveSpeed;
		_navigationAgent.SetVelocity(unsafeVelocity);
	}

	private async void ActorSetup()
	{
		// Wait for the first physics frame so the NavigationServer can sync.
		await ToSignal(GetTree(), SceneTree.SignalName.PhysicsFrame);

		// Now that the navigation map is no longer empty, set the movement target.
		MovementTarget = _movementTargetPosition;
	}
}
