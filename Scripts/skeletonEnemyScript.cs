using System;
using System.Security.AccessControl;
using System.Threading.Tasks;
using Godot;
using static Godot.TextServer;

public partial class skeletonEnemyScript : CharacterBody2D
{
    public const float Speed = 200;
    [Export] AnimatedSprite2D AnimSprite2D;
    private Node2D skeletonEnemyNode2d;
    [Export] Area2D HitboxArea2D;
    [Export] Area2D FovArea2D;
    [Export] CollisionShape2D Hitbox; // attack Hitbox
    [Export] CollisionShape2D Fov; // FOV


    private int oldHealth;
    private Vector2 playerPosition;
    private bool IsHurt = false;

    public override void _Ready()
    {
        AnimSprite2D = GetNode<AnimatedSprite2D>("AnimSprite2D");
        skeletonEnemyNode2d = (Node2D)GetParent();

        HitboxArea2D = AnimSprite2D.GetNode<Area2D>("HitboxArea2D");
        FovArea2D = AnimSprite2D.GetNode<Area2D>("FovArea2D");
        Hitbox = HitboxArea2D.GetNode<CollisionShape2D>("Hitbox");
        Fov = FovArea2D.GetNode<CollisionShape2D>("Fov");

        oldHealth = (int)GetMeta("Health"); //Gets health when first load in
    }

    public override void _PhysicsProcess(double delta) // Hearteat
    {


        //FOV
        int Health = (int)GetMeta("Health");
        var bodies = FovArea2D.GetOverlappingBodies();
        var hitboxBodies = HitboxArea2D.GetOverlappingBodies();

        bool plrDetected = false; // FovArea


        bool isAttacking = false;
        bool playerDetected = false; // HitboxArea

        foreach (Node2D body in bodies)
        {
            playerPosition = body.GlobalPosition;
            plrDetected = true;
        }

        foreach (Node2D body in hitboxBodies) // if detected in  the  actual hitbox not fov
        {
            int plrHealth = (int)body.GetMeta("Health");

            body.SetMeta("Health", plrHealth - 10);
            GD.Print(body.GetMeta("Health"));
            playerDetected = true;    //CurrThing
        }

        Vector2 velocity = Velocity;
        Vector2 Direction = (playerPosition - GlobalPosition); // makes the speed consistance not matter how far away the plr is Check the Normalized desc for more

        float Distance = Direction.Length();

        //GD.Print(Direction);

        if (Distance > 50)
        {

            Direction = Direction.Normalized();
            if (!plrDetected)
            {
                if (!IsHurt)
                {
                    AnimSprite2D.Play("Idle");
                }
                Velocity = Vector2.Zero;
                return;
            }

            if (Direction != Vector2.Zero)
            {
                velocity.X = Direction.X * Speed;
                velocity.Y = Direction.Y * Speed; //200
            }
            else
            {
                velocity.X = Mathf.MoveToward(Velocity.X, 0, Speed);
                velocity.Y = Mathf.MoveToward(Velocity.Y, 0, Speed);
            }

            //Flip Horizontaly
            if (Direction.X > 0)
            {    //Facing Right
                AnimSprite2D.FlipH = false;
            }
            else if (Direction.X < 0)
            {
                AnimSprite2D.FlipH = true;
            }

            //Anims
            if (Direction != Vector2.Zero)
            {
                if (!IsHurt)
                {
                    AnimSprite2D.Play("Walk");
                }
            }

            else
            {
                if (!IsHurt)
                {
                    AnimSprite2D.Play("Idle");
                }
            }

            Velocity = velocity;
            MoveAndSlide();
        }
        else if (Distance <= 50)
        {
            Velocity = Vector2.Zero;


            if (!IsHurt)
            {
                AnimSprite2D.Play("Idle");
            }
        }

        healthChanged();
    }


    private void chase()
    {

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
                skeletonEnemyNode2d.QueueFree();
            }
        }

    }

}
