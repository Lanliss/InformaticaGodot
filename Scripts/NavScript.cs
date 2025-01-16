using Godot;
using System;

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
    [Export] private bool isRanged = false;

    [ExportSubgroup("Attack1")]
    [Export] private Area3D attackHitbox1;
    [Export] private float attackBuildUpTime = 0.5f;
    [Export] private float attackDuration1 = 0.25f;
    [Export] private float attackCooldown1 = 1f;

    [ExportSubgroup("Attack2")]
    [Export] private Area3D attackHitbox2;
    [Export] private float projectileSpeed = 100f;
    [Export] private float attackBuildUpTime2 = 0.5f;
    [Export] private float attackDuration2 = 0.25f;
    [Export] private float attackCooldown2 = 2.5f;

    [ExportCategory("Pathfinding settings")]
    [Export]
    private float pathDistance = 1f;
    [Export]
    private float targetDistance = 0.5f;
    [Export]
    private float targetRangedDistance = 2.5f;
    [Export]
    private float pathfindingUpdateTime = 0.01f;
    [Export]
    private float priority = 1f;
    [Export]
    private bool dead = false;
    //end of exports

    private Area3D attackHitbox;
    private Vector3 projectileVector;
    private float attackBuildUpTimer = 0f;
    private float attack1Cooldown = 0f;
    private float attack2Cooldown = 0f;
    private float lastAttacked = 0f;
    private bool isAttacking = false;
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
        else if (!dead && !playerFound)
        { //this part of the function is not death related

            playerDetectionShape3D.Shape.Set(CylinderShape3D.PropertyName.Radius, 100f);

        }
    }

    public override void _Ready()
    {
        _movementTargetPosition = GlobalPosition;

        _navigationAgent.PathDesiredDistance = pathDistance;
        _navigationAgent.AvoidancePriority = priority;
        if (isRanged) 
        {
            attackHitbox = attackHitbox2;
            _navigationAgent.TargetDesiredDistance = targetRangedDistance;
        }
        else
        {
            attackHitbox = attackHitbox1;
            _navigationAgent.TargetDesiredDistance = targetDistance;
        }

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
            Vector3 distanceVector = playerNode.GlobalPosition - GlobalPosition;

            if (_navigationAgent.IsNavigationFinished() && distanceVector.Length() <= _navigationAgent.TargetDesiredDistance)
            {
                targetRotation = Mathf.Atan2(distanceVector.X, distanceVector.Z);

                if (!isAttacking)
                {
                    if (!isRanged && attack1Cooldown == 0f)
                    {
                        //initiate attack1
                        lastAttacked = 0;
                        isAttacking = true;
                        attack1Cooldown = attackCooldown1;
                        attackBuildUpTimer = attackBuildUpTime;

                        attackHitbox.Position = new Vector3(distanceVector.X, attackHitbox.Position.Y, distanceVector.Z).Normalized() * attackRange;
                        attackHitbox.Rotation = new Vector3(0, Mathf.Atan2(distanceVector.X, distanceVector.Z), 0);
                    }
                    if (isRanged && attack2Cooldown == 0f) 
                    {
                        lastAttacked = 0;
                        isAttacking = true;
                        attack2Cooldown = attackCooldown2;
                        attackBuildUpTimer = attackBuildUpTime;

                        attackHitbox.Position = new Vector3(distanceVector.X, attackHitbox.Position.Y, distanceVector.Z).Normalized() * attackRange;
                        attackHitbox.Rotation = new Vector3(0, Mathf.Atan2(distanceVector.X, distanceVector.Z), 0);
                        projectileVector = attackHitbox.Position;
                    }
                }
                
            }

            if (isAttacking)
            {
                Velocity = Vector3.Zero;

                if (attackBuildUpTimer > 0f) //if in build up fase
                {
                    if (attackBuildUpTimer == attackBuildUpTime) { visualEnemy.GetNode<AnimationPlayer>("AnimationPlayer").Play("attack startup"); }
                    if (attackBuildUpTimer - (float)delta > 0f) { attackBuildUpTimer -= (float)delta; } 
                    else 
                    { 
                        attackBuildUpTimer = 0f; 
                        visualEnemy.GetNode<AnimationPlayer>("AnimationPlayer").Play("Attack hitbox");
                        attackHitbox.Visible = true;
                        attackHitbox.GetChild<CollisionShape3D>(0).Disabled = false;
                    }
                    
                }
                else
                {
                    if (isRanged)
                    {
                        attackHitbox.Position += projectileVector * (float)delta * projectileSpeed;
                    }



                    lastAttacked += (float)delta;
                    if ((lastAttacked >= attackDuration1 && !isRanged)||(lastAttacked >= attackDuration2 && isRanged)) //attack cooldown is done
                    {
                        attackHitbox.GetChild<CollisionShape3D>(0).Disabled = true;
                        isAttacking = false;
                        attackHitbox.Visible = false;
                        visualEnemy.GetNode<AnimationPlayer>("AnimationPlayer").Play("Guantlet_Idle"); //start idle anim again
                    }
                }
            }

            if (attack1Cooldown > 0f)
            {
                if (attack1Cooldown - (float)delta >= 0f) { attack1Cooldown -= (float)delta; } else { attack1Cooldown = 0f; }
            }
            if (attack2Cooldown > 0f)
            {
                if (attack2Cooldown - (float)delta >= 0f) { attack2Cooldown -= (float)delta; } else { attack2Cooldown = 0f; }
            }


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

            if (_navigationAgent.IsNavigationFinished()) { return; }
            Vector3 currentAgentPosition = GlobalTransform.Origin;
            Vector3 nextPathPosition = _navigationAgent.GetNextPathPosition();
            if (!isAttacking)
            {
                Vector3 unsafeVelocity = currentAgentPosition.DirectionTo(nextPathPosition) * moveSpeed;
                _navigationAgent.SetVelocity(unsafeVelocity);
            }
        }
    }
    private async void ActorSetup()
    {
        // Wait for the first physics frame so the NavigationServer can sync.
        await ToSignal(GetTree(), SceneTree.SignalName.PhysicsFrame);

        // Now that the navigation map is no longer empty, set the movement target.
        MovementTarget = _movementTargetPosition;
    }
}
