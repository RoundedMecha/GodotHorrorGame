using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Xml.XPath;

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
	public Node3D HoldItemSpace;
	[Export]
	Control ControlCanvas;
	[Export]
	public AudioStreamPlayer3D AudioStreamPlayer;
	PlayerStates CurrentState;

	enum PlayerStates
	{
		Typing,
		Playing
	}

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

	
	public Dictionary RayCastForward()
	{
		
		var ScreenSize = GetViewport().GetVisibleRect().Size/2;
		var CamOrigin = Cam.ProjectRayOrigin(ScreenSize);
		var CamEnd = CamOrigin + Cam.ProjectRayNormal(ScreenSize) * 5;
		var SpaceState = Cam.GetWorld3D().DirectSpaceState;
		var Querry = PhysicsRayQueryParameters3D.Create(CamOrigin,CamEnd);
		Querry.Exclude = new Godot.Collections.Array<Rid>{this.GetRid()};
		Querry.CollideWithAreas = true;
		var ResultDictionary = new Godot.Collections.Dictionary{};
		ResultDictionary = SpaceState.IntersectRay(Querry);


		return ResultDictionary;
	}
	
	public void CheckForInteractable()
	{
		
		if(!Holding)
			{
				var Result = new Godot.Collections.Dictionary{};
				Result = RayCastForward();
				
				if(Result.Count > 0)
				{
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
							ObjectLoad.ShapeCast3DCheckForWorld.AddException(GetNode<CollisionObject3D>("."));
							
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
				ObjectLoad.ShapeCast3DCheckForWorld.AddException(GetNode<CollisionObject3D>("."));
				GD.Print("Name of COllision Body Exluded is : " + (GetNode<CollisionObject3D>(".")));
				GD.Print("Before Move:" + ObjectLoad.ShapeCast3DCheckForWorld.IsColliding());
				ObjectLoad.GlobalPosition = new Vector3(this.GlobalPosition.X,Mathf.Abs(Head.GlobalPosition.Y), this.GetChild<Node3D>(0).GlobalPosition.Z);
				ObjectLoad.ShapeCast3DCheckForWorld.ForceShapecastUpdate();
					if(ObjectLoad.ShapeCast3DCheckForWorld.IsColliding() && !GetTree().GetNodesInGroup("Interactable").Contains((Node3D)ObjectLoad.ShapeCast3DCheckForWorld.GetCollider(0)))
					{
						var t = GetTree().GetNodesInGroup("Interactable").Contains((Node3D)ObjectLoad.ShapeCast3DCheckForWorld.GetCollider(0));

						GD.Print("Count of Col" + ObjectLoad.ShapeCast3DCheckForWorld.GetCollisionCount());
						GD.Print("collider is in group Interactable: " + t  );
						GD.Print("After Move: " + ObjectLoad.ShapeCast3DCheckForWorld.IsColliding());
						GD.Print("Colliding With: " + ObjectLoad.ShapeCast3DCheckForWorld.GetCollider(0));
						
						ObjectLoad.Free();
					}
					else
					{
						
						GD.Print("UPDATED POS" + ObjectLoad.GlobalPosition);
						HoldItemSpace.GetChild(0).QueueFree();
						GD.Print("LOADED OBJ POS: " + ObjectLoad.GlobalPosition);				
						Holding = false;
					}
				
					
				}
					
			
					
	}
				
		
	
	public override void _Ready()
	{
		shapeCast.AddException(GetNode<CollisionObject3D>("."));
		CurrentState = PlayerStates.Playing;
	
		
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
			if(CurrentState == PlayerStates.Playing)
			{
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
				else if(@event.IsActionPressed("TestDirection"))
				{
					AudioStreamPlayer.Play();
					AudioStreamPlayer.VolumeDb = 1;
					GD.Print("TEST");
				}
				else if(@event.IsActionPressed("ActivateKeyboard"))
				{
					CurrentState = PlayerStates.Typing;
					ControlCanvas.GetChild(0).GetChild<TextEdit>(3).Visible = true;
			
				}
			}
			else
			{
				if(@event.IsActionPressed("ActivateKeyboard"))
				{
					CurrentState = PlayerStates.Playing;
					ControlCanvas.GetChild(0).GetChild<TextEdit>(3).Visible = false;
					ControlCanvas.GetChild(0).GetChild<TextEdit>(3).Clear();
				}
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
		if(CurrentState != PlayerStates.Typing)
		{
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
		}

	

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
