using Godot;
using System;
using System.Linq;

public partial class Character : CharacterBody3D
{
	public float Speed = 5.0f;
	public const float CrouchSpeed = 2.5f;
	public const float JumpVelocity = 4.5f;
	public const float sensitivity = 0.01f;
	public bool Crouched;
	public bool busy;
	public bool LightVis;
	public bool Holding = false;

	Node3D n;
	
	
	[ExportGroup("Properties")]
	[Export]
	public Node3D Head;
	[Export]
 	public Camera3D Cam;
	[Export]
	public AnimationPlayer AnimPlayer;
	[Export]
	public ShapeCast3D shapeCast;
	
	public void OnAnimationPlayerAnimationFinished(string anim_name)
	{
		if(anim_name == "CrouchAnim")
		{
			GD.Print("FIN");
		}
		
	}
	public void CheckForInteractable()
	{
		if(!Holding)
			{
				GD.Print("WORKS");
				var ScreenSize = GetViewport().GetVisibleRect().Size/2;
				var CamOrigin = Cam.ProjectRayOrigin(ScreenSize);
				var CamEnd = CamOrigin + Cam.ProjectRayNormal(ScreenSize) * 5;
				var SpaceState = Cam.GetWorld3D().DirectSpaceState;
				var Querry = PhysicsRayQueryParameters3D.Create(CamOrigin,CamEnd);
				Querry.Exclude = new Godot.Collections.Array<Rid>{this.GetRid()};
				Querry.CollideWithAreas = true;
				var Result = new Godot.Collections.Dictionary{};
				Result = SpaceState.IntersectRay(Querry);
				Vector3 old_pos;

				if(Result.Count > 0)
				{
					n = (Node3D)Result["collider"];
					n = n.GetParent<Node3D>();
					old_pos = n.GlobalPosition;
					GD.Print(n.Name.ToString());

					if(GetTree().GetNodesInGroup("Interactable").Contains((Node)Result["collider"]))
					{
								
					GD.Print("THIS NAME"+n.Name);
					GD.Print("UPDATE CHILD NUM " + GetChildCount());
					GD.Print("UPDATE CHILD NUM " + GetChildCount());
					n.GlobalPosition =  CamOrigin + Cam.ProjectRayNormal(ScreenSize) * 5f; 
					Holding = true;
					GD.Print("POS AFTER ADDED" + n.GlobalPosition);

					}
				

				}
						
				}
				else
				{
					GD.Print("Detach Item"+ n.Name.ToString());
					GD.Print("Spot Node Count B4" + GetChildCount());
					GD.Print("Spot Node Count B4" + GetChildCount());
					GD.Print("Root Node Count B4" + GetNode(".").GetChildCount());
					n.GlobalPosition = new Vector3(0.5f,0.5f,0.5f);
					GD.Print("Root Node Count Aft" + GetNode(".").GetChildCount());
					Holding = false;
					GD.Print("POS AFTER REMOV" + n.GlobalPosition);
				}
				
				
			

			
		
		
	}
	
	public override void _Ready()
	{
		shapeCast.AddException(GetNode<CollisionObject3D>("DefaultCollisionShape")); 
		shapeCast.AddException(GetNode<CollisionObject3D>("CrouchedCollisionShape3D"));
	}
	
	public override void _Input(InputEvent @event) //_Input for when input exists only
	{
		switch(@event)
		{
			case InputEventMouseMotion:
				InputEventMouseMotion mouseMotion = @event as InputEventMouseMotion;
				Head.RotateY(-mouseMotion.Relative.X * sensitivity);
				Cam.RotateX(-mouseMotion.Relative.Y * sensitivity);
		
				Vector3 CameraRot = Cam.Rotation;
				CameraRot.X = Mathf.Clamp(CameraRot.X,Mathf.DegToRad(-80f),Mathf.DegToRad(80f));
				Cam.Rotation = CameraRot;
				break;
			case InputEventKey:
				if(@event.IsActionPressed("Crouch")) //Handle Crouching
				{
					if (Input.IsActionPressed("Crouch"))
					{
						if(Crouched == false)
						{
							Speed = CrouchSpeed;
							AnimPlayer.Queue("CrouchAnim");
							Crouched = true;
						}
						else if(Crouched == true)
						{
							if(Crouched == true && shapeCast.IsColliding() == false)
							{
								AnimPlayer.Queue("unCrouchAnim");
								Crouched = false;
								Speed = 5.0f;
							}
						}
					}	
				}
				else if(@event.IsActionPressed("Torch"))
				{
					GD.Print("TEST");
					if(LightVis == true)
					{	
					AnimPlayer.Queue("HideTorch");
					LightVis = false;
				
					}
					else{ AnimPlayer.Queue("ShowTorch"); LightVis = true;}
				}
				else if(@event.IsActionPressed("Interact"))
				{
					CheckForInteractable();

				}	
				break;
			default:
				break;

		}
		
	}
	


	public override void _PhysicsProcess(double delta) //Called at a fixed rate
	{
		Vector3 velocity = Velocity;

		// Add the gravity.
		if (!IsOnFloor())
		{
			velocity += GetGravity() * (float)delta;
		}

		// Handle Jump.
		if (Input.IsActionJustPressed("ui_accept") && IsOnFloor())
		{
			velocity.Y = JumpVelocity;
		}

		// Get the input direction and handle the movement/deceleration.
		// As good practice, you should replace UI actions with custom gameplay actions.
		Vector2 inputDir = Input.GetVector("moveLeft", "moveRight", "moveForward", "moveBackward");
		Vector3 direction = (Head.Transform.Basis * new Vector3(inputDir.X, 0, inputDir.Y)).Normalized();
		if (direction != Vector3.Zero)
		{
			velocity.X = direction.X * Speed;
			velocity.Z = direction.Z * Speed;
		}
		else
		{
			velocity.X = Mathf.MoveToward(Velocity.X, 0, Speed);
			velocity.Z = Mathf.MoveToward(Velocity.Z, 0, Speed);
		}

		Velocity = velocity;
		MoveAndSlide();
	}
	
	
}
