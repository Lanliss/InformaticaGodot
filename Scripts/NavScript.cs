using Godot;

public partial class NavScript : CharacterBody3D
{
    [Export]
    private float moveSpeed = 3f;

    [Export]
    private NavigationAgent3D _navigationAgent;

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

    public void Die()
    {
        if (!dead)
        {
            deathPos = GlobalPosition;
            dead = true;
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
        if (dead) { if (MovementTarget != deathPos || GlobalPosition != deathPos) { MovementTarget = deathPos; GlobalPosition = deathPos; } return; /*Jezus arc?*/} 
        if (playerFound)
        {
            //GD.Print("player found");
            Vector3 distanceVector = GlobalPosition - playerNode.GlobalPosition;
            if (_navigationAgent.IsNavigationFinished() && distanceVector.Length() <= _navigationAgent.TargetDesiredDistance) { return; }


            if (timeSincePathfindingUpdate + (float)delta >= pathfindingUpdateTime)
            {
                timeSincePathfindingUpdate = 0f;
                MovementTarget = playerNode.GlobalPosition;
            }
            else
            {
                timeSincePathfindingUpdate += (float)delta;
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
