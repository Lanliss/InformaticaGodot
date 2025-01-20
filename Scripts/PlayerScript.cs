using Godot;
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using static Messenger;

public partial class PlayerScript : CharacterBody3D
{
	[Export] private float speed = 5f;
	[Export] private float dashDistance = 2.5f;
	[Export] private float dashCooldownTime = 0.5f;
	[Export] private string deathScreenSceneAdress;

	[Export]
	private Node3D visualPlayer;

    [ExportCategory("Attack Settings")]
    [Export] private float attackRange = 1f;
    [Export] private float projectileSpeed = 10f;

	[ExportSubgroup("Attack1")]
    [Export] private Area3D attackHitbox;
    [Export] private float attackBuildUpTime1 = 0.5f;
    [Export] private float attackDuration1 = 0.25f;
	[Export] private float attackCooldown1 = 0.25f;

    [ExportSubgroup("Attack2")]
    [Export] private Area3D attackHitbox2;
    [Export] private float attackBuildUpTime2 = 0.5f;
    [Export] private float attackDuration2 = 0.3f;
    [Export] private float attackCooldown2 = 0.75f;

    private float attackBuildUpTimer = 0f;
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
	private float dashCooldown = 0f;
	private bool dead = false;
	
	public override void _Ready()
	{
		//visualPlayer.Position = new Vector3(0, -1, 0);
		attackHitbox.Visible = false;
		attackHitbox2.Visible = false;
		_messengerSingleton = GetNode<Messenger>("/root/Messenger"); //get the singleton
	}

	public void OnMousePosition(Vector3 mousePosition) {
		lookDirection = (mousePosition - GlobalPosition).Normalized();
        //visualPlayer.Rotation = new Vector3(0, Mathf.Atan2(lookDirection.X, lookDirection.Z), 0);
    }

	private void StartAttack(Area3D attackHitboxIn, float attackCooldownIn, float attackDurationIn, float attackBuilUpTimeIn) {
		currentAttackHitbox = attackHitboxIn;
		currentAttackCooldown = attackCooldownIn;
		currentAttackDuration = attackDurationIn;

        attackBuildUpTimer = attackBuilUpTimeIn;

        lastAttacked = 0;
		isAttacking = true;
		
		currentAttackHitbox.Position = new Vector3(lookDirection.X, currentAttackHitbox.Position.Y, lookDirection.Z).Normalized() * attackRange;
		projectileVector = currentAttackHitbox.Position;
		currentAttackHitbox.Rotation = new Vector3(0, Mathf.Atan2(lookDirection.X, lookDirection.Z), 0);

        visualPlayer.GetNode<AnimationPlayer>("AnimationPlayer").Play("Guy attack 1");
        visualPlayer.Rotation = new Vector3(0, Mathf.Atan2(lookDirection.X, lookDirection.Z), 0);
    }

    public void Die(int health)
    {
		if (health <= 0) { dead = true; CallDeferred(PlayerScript.MethodName.SwitchToDeathScene); }
    }

	private void SwitchToDeathScene() 
	{
        GetTree().ChangeSceneToFile(deathScreenSceneAdress);
    }

	public override void _PhysicsProcess(double delta)
	{
		if (dead) {return; }//the jezus arc returns

		if (GlobalPosition.Y <= -1f) { GetTree().ReloadCurrentScene(); }
		Vector3 velocity = Velocity;

		// Add the gravity.
		if (!IsOnFloor())
		{
			velocity += GetGravity() * (float)delta;
		}


		if (Input.IsActionJustPressed("attack") && !isAttacking && attack1Cooldown == 0f)
		{
			attack1Cooldown = attackCooldown1 + attackDuration1 + attackBuildUpTime1;
			StartAttack(attackHitbox, attackCooldown1, attackDuration1, attackBuildUpTime1);
			_messengerSingleton.EmitSignal(nameof(_messengerSingleton.OnStartCooldown), 1, attack1Cooldown);
		}
		else if (Input.IsActionJustPressed("attack2") && !isAttacking && attack2Cooldown == 0f)
		{
			attack2Cooldown = attackCooldown2 + attackDuration2 + attackBuildUpTime2;
			StartAttack(attackHitbox2, attackCooldown2, attackDuration2, attackBuildUpTime2);
			_messengerSingleton.EmitSignal(nameof(_messengerSingleton.OnStartCooldown), 2, attack2Cooldown);
		}

		Vector2 inputDir = Input.GetVector("move_left", "move_right", "move_forward", "move_back");
		Vector3 direction = new Vector3(inputDir.X, 0, inputDir.Y).Normalized();

		if (!isAttacking)
		{
			if (direction != Vector3.Zero)
			{
                if (dashCooldown == 0 && visualPlayer.GetNode<AnimationPlayer>("AnimationPlayer").CurrentAnimation != "guy run") { visualPlayer.GetNode<AnimationPlayer>("AnimationPlayer").Play("guy run"); }

                velocity.X = direction.X * speed;
				velocity.Z = direction.Z * speed;
				moveDirection = direction;
				moveDirection = moveDirection.Rotated(Vector3.Up, 0.25f * MathF.PI);
				visualPlayer.Rotation = new Vector3(0, Mathf.Atan2(moveDirection.X, moveDirection.Z), 0);
				//direction is already normalised
			}
			else
			{
				if (dashCooldown == 0 && visualPlayer.GetNode<AnimationPlayer>("AnimationPlayer").CurrentAnimation != "guy idle") { visualPlayer.GetNode<AnimationPlayer>("AnimationPlayer").Play("guy idle"); }
				velocity.X = Mathf.MoveToward(Velocity.X, 0, speed);
				velocity.Z = Mathf.MoveToward(Velocity.Z, 0, speed);
			}

			// Handle Dash.
			if (Input.IsActionJustPressed("dash") && dashCooldown == 0f)
			{
				if (moveDirection != Vector3.Zero)
				{
                    visualPlayer.GetNode<AnimationPlayer>("AnimationPlayer").Play("guy dash");

                    GlobalPosition += new Vector3(moveDirection.X, 0, moveDirection.Z).Normalized() * dashDistance;
					dashCooldown = dashCooldownTime;
                    _messengerSingleton.EmitSignal(nameof(_messengerSingleton.OnStartCooldown), 3, dashCooldownTime);
                }
			}
		}
		else
		{
            velocity = Vector3.Zero;

            if (attackBuildUpTimer > 0f) //if in build up fase
			{
				if (attackBuildUpTimer - (float)delta >= 0f) { attackBuildUpTimer -= (float)delta; } else { attackBuildUpTimer = 0f; }
				//TODO: play telegraph animation
			}
			else
			{
                currentAttackHitbox.Visible = true;
                currentAttackHitbox.GetChild<CollisionShape3D>(0).Disabled = false;
                lastAttacked += (float)delta;
				if (lastAttacked >= currentAttackDuration) //attack cooldown is done
				{
					currentAttackHitbox.GetChild<CollisionShape3D>(0).Disabled = true;
					isAttacking = false;
					currentAttackHitbox.Visible = false;
                    visualPlayer.GetNode<AnimationPlayer>("AnimationPlayer").Play("guy idle");
                }
				else if (currentAttackHitbox == attackHitbox2)
				{
					currentAttackHitbox.Position += projectileVector * (float)delta * projectileSpeed;
				}
			}
		}


		//cooldown section
        if (attack1Cooldown > 0)
        {
            if (attack1Cooldown - (float)delta >= 0) { attack1Cooldown -= (float)delta; } else { attack1Cooldown = 0; }
        }
        if (attack2Cooldown > 0)
        {
            if (attack2Cooldown - (float)delta >= 0) { attack2Cooldown -= (float)delta; } else { attack2Cooldown = 0; }
        }
        if (dashCooldown > 0)
        {
            if (dashCooldown - (float)delta >= 0) { dashCooldown -= (float)delta; } else { dashCooldown = 0; }
        }
        velocity = velocity.Rotated(Vector3.Up, 0.25f * MathF.PI);
        Velocity = velocity;
		MoveAndSlide();
	}
}
