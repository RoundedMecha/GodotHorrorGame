using Godot;
using System;
using System.Collections.Specialized;
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
	Vector3 old_pos;
	
	[ExportGroup("Properties")]
	[Export]
	public Node3D Head;
	[Export]
 	public Camera3D Cam;
	[Export]
	public Camera3D SubCam;
	[Export]
	public AnimationPlayer AnimPlayer;
	[Export]
	public ShapeCast3D shapeCast;
	[Export]
	Node3D HoldItemSpace;
	[Export]
	Control ControlCanvas;

	public void OnButtonPressed()
	{
		var GlobalVar = (GlobalVariables)GetNode("/root/GlobalVariables");
		GD.PrintT(GetNode("/root/GlobalVariables"));
		GlobalVar.PlayerHealth -=4;
		GlobalVar.GotoScene("res://Scenes/Title Screen.tscn");
		

	}

	
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
				
				if(Result.Count > 0)
				{

					n = (Node3D)Result["collider"];
						GD.Print(n.Name);
					if(GetTree().GetNodesInGroup("Interactable").Contains((Node3D)Result["collider"]))
					{
						n = (Node3D)Result["collider"];
						GD.Print(n.Name);
						if(n.Name == "ItemPickUp")
						{
							old_pos = n.GlobalPosition;
							n.Free();
							var SceneLoad = GD.Load<PackedScene>("res://Scenes/item_pick_up.tscn");
							ItemPickUp ObjectLoad = SceneLoad.Instantiate<ItemPickUp>();
							ObjectLoad.BeingHeld = true;
							
							ObjectLoad.Position = HoldItemSpace.Position;
							HoldItemSpace.AddChild(ObjectLoad);
							GD.Print(HoldItemSpace.GetChildCount());
							GD.Print(HoldItemSpace.GetChild<Node3D>(0).Name);
							GD.Print(HoldItemSpace.GetChild<Node3D>(0).GlobalPosition);

							Holding = true;
						
						}
					}
				

				}
						
				}
				else
				{
					var SceneLoad = GD.Load<PackedScene>("res://Scenes/item_pick_up.tscn");
					var ObjectLoad = SceneLoad.Instantiate<ItemPickUp>();
					GetParent().AddChild(ObjectLoad);
					ObjectLoad.GlobalPosition = HoldItemSpace.GetChild<Node3D>(0).GlobalPosition;
					ObjectLoad.GlobalTransform = HoldItemSpace.GetChild<Node3D>(0).GlobalTransform;
					HoldItemSpace.GetChild(0).QueueFree();
				

					Holding = false;
					
				}
				
				
			

			
		
		
	}
	
	public override void _Ready()
	{
		shapeCast.AddException(GetNode<CollisionObject3D>("."));
		
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

		if(Holding == true){ GD.Print(HoldItemSpace.GetChild<Node3D>(0).GlobalPosition);}

		Velocity = velocity;
		MoveAndSlide();
	}

    public override void _Process(double delta)
    {
		SubCam.GlobalTransform = Cam.GlobalTransform;
		ControlCanvas.GetChild<CanvasLayer>(0).GetChild<RichTextLabel>(0).Text = ("Health: " + GetNode<GlobalVariables>("/root/GlobalVariables").PlayerHealth);
        base._Process(delta);
    }

	
}
