using System;
using System.Threading.Tasks;
using Godot;

public partial class skeletonEnemyScript : CharacterBody2D
{
	public const float Speed = 200;
	[Export] AnimatedSprite2D AnimSprite2D;
	private Node2D skeletonEnemyNode2d;
	[Export] Area2D HitboxArea2D;
	[Export] Area2D FovArea2D;
	[Export] CollisionShape2D Hitbox;
	[Export] CollisionShape2D Fov;

	private AnimatedSprite2D plrAnimSprite;
	private bool isAttacking = false;
	private int oldHealth;
	private Vector2 playerPosition;
	private bool IsHurt = false;
	private bool canAttack = true;

	public override void _Ready()
	{
		AnimSprite2D = GetNode<AnimatedSprite2D>("AnimSprite2D");
		skeletonEnemyNode2d = (Node2D)GetParent();

		HitboxArea2D = AnimSprite2D.GetNode<Area2D>("HitboxArea2D");
		FovArea2D = AnimSprite2D.GetNode<Area2D>("FovArea2D");
		Hitbox = HitboxArea2D.GetNode<CollisionShape2D>("Hitbox");
		Fov = FovArea2D.GetNode<CollisionShape2D>("Fov");

		oldHealth = (int)GetMeta("Health");
	}

	public override void _PhysicsProcess(double delta)
	{
		int Health = (int)GetMeta("Health");
		var bodies = FovArea2D.GetOverlappingBodies();

		bool plrDetected = false;

		foreach (Node2D body in bodies)
		{
			playerPosition = body.GlobalPosition;
			plrDetected = true;
		}

		if (!plrDetected)
		{
			Velocity = Vector2.Zero;
			if (!IsHurt && !isAttacking)
				AnimSprite2D.Play("Idle");

			healthChanged();
			return;
		}

		Vector2 velocity = Velocity;
		Vector2 Direction = playerPosition - GlobalPosition;
		float Distance = Direction.Length();
		var X = Hitbox.Position.X;
		if (Distance > 50)
		{
			Direction = Direction.Normalized();

			velocity.X = Direction.X * Speed;
			velocity.Y = Direction.Y * Speed;

			if (Direction.X > 0)
			{
				AnimSprite2D.FlipH = false;
				X = 47.0f;
				Hitbox.Position = new Vector2(X, -5.5f);
			}
			else if (Direction.X < 0)
			{
				AnimSprite2D.FlipH = true;
				X = 47.0f;
				Hitbox.Position = new Vector2(-X, -5.5f);
			}
			if (!IsHurt && !isAttacking)
				AnimSprite2D.Play("Walk");

			Velocity = velocity;
			MoveAndSlide();
		}
		else
		{
			Velocity = Vector2.Zero;

			if (!isAttacking && !IsHurt)
				AnimSprite2D.Play("Idle");

			if (!isAttacking && canAttack)
				Attack();
		}

		healthChanged();
	}

	private async Task Attack()
	{

		if (IsHurt){
			canAttack = false;
			await Task.Delay(800);
			canAttack = true;
			return;
		}
		if (isAttacking)
			return;
		
		


		isAttacking = true;
		AnimSprite2D.Play("Attack");

		canAttack = false;
		

		//HitboxStuff
		var hitboxBodies = HitboxArea2D.GetOverlappingBodies();
		GD.Print(hitboxBodies);

		foreach (Node2D body in hitboxBodies)
		{
			int plrHealth = (int)body.GetMeta("Health");
			body.SetMeta("Health", plrHealth - 10);
			GD.Print(body.GetMeta("Health"));
		}

		await Task.Delay(500);
		isAttacking = false;
		await Task.Delay(750);
		canAttack = true;

	}

	private async Task healthChanged()
	{
		int Health = (int)GetMeta("Health");

		if (Health < oldHealth)
		{
			if (Health > 0)
			{
				AnimSprite2D.Play("Hurt");
				IsHurt = true;
				oldHealth = Health;

				await Task.Delay(500);

				IsHurt = false;
			}
			else
			{
				oldHealth = Health;
				AnimSprite2D.Play("Death");
				IsHurt = true;

				await Task.Delay(500);

				skeletonEnemyNode2d.QueueFree();
			}
		}
	}
}
