using Godot;
using System;

public partial class ItemPickUp : CharacterBody3D
{
	public const float Speed = 5.0f;
	public const float JumpVelocity = 4.5f;
	public bool BeingHeld;

	[ExportGroup("Properties")]
	[Export]
	MeshInstance3D ObjectMesh;
	[Export]
	public ShapeCast3D ShapeCast3DCheckForWorld;

    public override void _Ready()
    {
		ShapeCast3DCheckForWorld.AddException(GetNode<CollisionObject3D>("."));
		ObjectMesh.Layers = 1;
		ObjectMesh.GetChild<MeshInstance3D>(0).Layers = 1;
        base._Ready();
    }

    public override void _PhysicsProcess(double delta)
	{
		Vector3 velocity = Velocity;
		if (BeingHeld)
		{
			ObjectMesh.Layers = 2;
			ObjectMesh.GetChild<MeshInstance3D>(0).Layers = 2;
			
			
		}
		else
		{
			if (!IsOnFloor())
			{
			velocity += GetGravity() * (float)delta;
			}

			
			

	

	

			Velocity = velocity;
			MoveAndSlide();
		}

		
	}
}
