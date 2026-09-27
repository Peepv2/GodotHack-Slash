using Godot;
using System;
using System.Security.AccessControl;
using System.Threading.Tasks;

public partial class playerHandler : CharacterBody2D
{
    public const float Speed = 250;

	private AnimatedSprite2D AnimSprite2D;
	private Area2D Area2dHitbox;
    private CollisionShape2D Hitbox;
	private Vector2 hitboxOffset;

	public int Health;
    private int oldHealth;
    private bool IsHurt = false;

    public override void _Ready(){
		Health = (int)GetMeta("Health");

		AnimSprite2D = GetNode<AnimatedSprite2D>("AnimSprite2D");
        Area2dHitbox = GetNode<Area2D>("Area2dHitbox");

		Hitbox = Area2dHitbox.GetNode<CollisionShape2D>("Hitbox");
        hitboxOffset = Hitbox.Position;

        oldHealth = (int)GetMeta("Health"); //Gets health when first load in

    }
    public override void _PhysicsProcess(double delta){

		if (Health <= 0)
		{
			GD.Print("You Have Died");
			GetTree().ReloadCurrentScene();
		}
        var X = hitboxOffset.X;
        bool IsAttacking = (bool)GetMeta("IsAttacking");  // get it as a bool and then convert Godot.Variant ("IsAttacking") into a C# bool by calling (bool) first

        Vector2 velocity = Velocity;

        // Get the input direction and handle the movement/deceleration.As good practice, you should replace UI actions with custom gameplay actions.
        Vector2 direction = Input.GetVector("move_left", "move_right", "move_up", "move_down");
		if (direction != Vector2.Zero)
		{
			velocity.X = direction.X * Speed;
            velocity.Y = direction.Y * Speed;
        }
		else
		{
			velocity.X = Mathf.MoveToward(Velocity.X, 0, Speed);
            velocity.Y = Mathf.MoveToward(Velocity.Y, 0, Speed);
        }

		//Flip Horizontaly
		if (direction.X > 0){    //Facing Right
            AnimSprite2D.FlipH = false;
			X = 35.5f;
			Hitbox.Position = new Vector2 (X, 17); //Flip the Hitbox

        }
		else if (direction.X < 0)
		{
            AnimSprite2D.FlipH = true;
            X = -35.5f;
            Hitbox.Position = new Vector2(X, 17);
        }

		//Anims
		if (direction != Vector2.Zero && !IsAttacking)
		{ // (&&!IsAttacking) and is not attacking then play Run anim
			AnimSprite2D.Play("Run");
		}

		else
		{
			if (IsAttacking)  // Play Idle if not attacking
			{
				//do nun		
		     }
			else
			{
                AnimSprite2D.Play("Idle");
            }
	    }

		Velocity = velocity;
		MoveAndSlide();
		attackFunc();
    }

	private async Task attackFunc()
	{
        bool IsAttacking = (bool)GetMeta("IsAttacking");

        if (Input.IsActionJustPressed("attack"))
        {
			if (IsAttacking)
			{
				//Do nun
			}
			else
			{
				AnimSprite2D.Play("Attack");
                SetMeta("IsAttacking",true);

                var bodies = Area2dHitbox.GetOverlappingBodies(); // Returns a list of overlaping bodies or tiles

                foreach (Node2D body in bodies) // for each body inside the overlappingbodies do the func isnide the { }
                {
                    GD.Print("HIT: " + body.Name);
					int enemyHealth = (int)body.GetMeta("Health"); // get the value

                    body.SetMeta("Health", enemyHealth - 10); // and do the sum here
					GD.Print(body.GetMeta("Health"));

                }

                //Cooldown
                await Task.Delay(500); //miliseconds
                SetMeta("IsAttacking", false);
            }
		}
    }


    private async Task healthChanged()
    {
        //oldHealth

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
            else if (Health <= 0)
            {
                oldHealth = Health;
                AnimSprite2D.Play("Death");
                IsHurt = true;
                await Task.Delay(500);
                //skeletonEnemyNode2d.QueueFree();
            }
        }

    }


}
