using Godot;
using System;

public partial class ItemPickUp : CharacterBody3D
{
	public const float Speed = 5.0f;
	public const float JumpVelocity = 4.5f;
	public bool BeingHeld;

	public override void _PhysicsProcess(double delta)
	{
		Vector3 velocity = Velocity;
		if (BeingHeld)
		{
			
			
			
		}
		else
		{
			if (!IsOnFloor())
			{
			velocity += GetGravity() * (float)delta;
			}

	

		// Get the input direction and handle the movement/deceleration.
		// As good practice, you should replace UI actions with custom gameplay actions.
		

			Velocity = velocity;
			MoveAndSlide();
		}

		// Add the gravity.
		
	}
}
