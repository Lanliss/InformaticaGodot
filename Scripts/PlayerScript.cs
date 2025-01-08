using Godot;
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using static Messenger;

public partial class PlayerScript : CharacterBody3D
{
	[Export] private RigidBody3D _lookObject;
	[Export] private float speed = 5f;
	[Export] private float dashDistance = 2.5f;

	[ExportCategory("Attack Settings")]
    [Export] private float attackRange = 1f;
    [Export] private float projectileSpeed = 10f;

	[ExportSubgroup("Attack1")]
    [Export] private Area3D attackHitbox;
    [Export] private float attackDuration1 = 0.25f;
	[Export] private float attackCooldown1 = 0.25f;

    [ExportSubgroup("Attack2")]
    [Export] private Area3D attackHitbox2;
	[Export] private float attackDuration2 = 0.3f;
    [Export] private float attackCooldown2 = 0.75f;

    private Vector3 projectileVector = new Vector3(0, 0, 0);
	private float currentAttackCooldown;
    private float currentAttackDuration;
    private Area3D currentAttackHitbox;
	private float lastAttacked;
	private bool isAttacking = false;
	private Vector3 lookDirection = new Vector3(0, 0, -1);
	private Vector3 moveDirection = new Vector3(0, 0, -1);
	private Vector3 rayCastHit = new Vector3(0, 0, 0);
	private Messenger _messengerSingleton; //Messenger singleton
	private float attack1Cooldown = 0f;
	private float attack2Cooldown = 0f;
	
	public override void _Ready()
	{
		attackHitbox.Visible = false;
		attackHitbox2.Visible = false;
		_messengerSingleton = GetNode<Messenger>("/root/Messenger"); //get the singleton
	}

	public void OnMousePosition(Vector3 mousePosition) {
		lookDirection = (mousePosition - GlobalPosition).Normalized();
	}

	private void StartAttack(Area3D attackHitboxIn, float attackCooldownIn, float attackDurationIn) {
		currentAttackHitbox = attackHitboxIn;
		currentAttackCooldown = attackCooldownIn;
		currentAttackDuration = attackDurationIn;

		lastAttacked = 0;
		isAttacking = true;
		currentAttackHitbox.Visible = true;
		currentAttackHitbox.GetChild<CollisionShape3D>(0).Disabled = false;
		currentAttackHitbox.Position = new Vector3(lookDirection.X, currentAttackHitbox.Position.Y, lookDirection.Z).Normalized() * attackRange;
		projectileVector = currentAttackHitbox.Position;
		currentAttackHitbox.Rotation = new Vector3(0, Mathf.Atan2(lookDirection.X, lookDirection.Z), 0);
	}

	public override void _PhysicsProcess(double delta)
	{
		Vector3 velocity = Velocity;

		// Add the gravity.
		if (!IsOnFloor())
		{
			velocity += GetGravity() * (float)delta;
		}


		if (Input.IsActionJustPressed("attack") && !isAttacking && attack1Cooldown == 0f)
		{
			attack1Cooldown = attackCooldown1;
			StartAttack(attackHitbox, attackCooldown1, attackDuration1);
			_messengerSingleton.EmitSignal(nameof(_messengerSingleton.OnStartCooldown), 1, attackCooldown1);
		}
		else if (Input.IsActionJustPressed("attack2") && !isAttacking && attack2Cooldown == 0f)
		{
			attack2Cooldown = attackCooldown2;
			StartAttack(attackHitbox2, attackCooldown2, attackDuration2);
			_messengerSingleton.EmitSignal(nameof(_messengerSingleton.OnStartCooldown), 2, attackCooldown2);
		}

		Vector2 inputDir = Input.GetVector("move_left", "move_right", "move_forward", "move_back");
		Vector3 direction = new Vector3(inputDir.X, 0, inputDir.Y).Normalized();

		if (!isAttacking)
		{
			if (direction != Vector3.Zero)
			{
				velocity.X = direction.X * speed;
				velocity.Z = direction.Z * speed;
				moveDirection = direction;
				moveDirection = moveDirection.Rotated(Vector3.Up, 0.25f * MathF.PI);
                _lookObject.Position = moveDirection;	//direction is already normalised
            }
			else
			{
				velocity.X = Mathf.MoveToward(Velocity.X, 0, speed);
				velocity.Z = Mathf.MoveToward(Velocity.Z, 0, speed);
			}
			
			// Handle Dash.
			if (Input.IsActionJustPressed("dash") && IsOnFloor())
			{
				if (moveDirection != Vector3.Zero)
				{
					GlobalPosition += new Vector3( moveDirection.X * dashDistance , 0, moveDirection.Z * dashDistance);
				}
			}
		} else {
			lastAttacked += (float)delta;
			velocity = Vector3.Zero;
			if (lastAttacked >= currentAttackDuration) //attack cooldown is done
			{
				currentAttackHitbox.GetChild<CollisionShape3D>(0).Disabled = true;
				isAttacking = false;
				currentAttackHitbox.Visible = false;
			}
			else if(currentAttackHitbox == attackHitbox2){
				currentAttackHitbox.Position += projectileVector * (float)delta * projectileSpeed;
            }
		}

        if (attack1Cooldown > 0)
        {
            if (attack1Cooldown - (float)delta >= 0) { attack1Cooldown -= (float)delta; } else { attack1Cooldown = 0; }
        }
        if (attack2Cooldown > 0)
        {
            if (attack2Cooldown - (float)delta >= 0) { attack2Cooldown -= (float)delta; } else { attack2Cooldown = 0; }
        }
        velocity = velocity.Rotated(Vector3.Up, 0.25f * MathF.PI);
        Velocity = velocity;
		MoveAndSlide();
	}
}
