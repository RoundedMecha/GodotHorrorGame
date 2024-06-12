using Godot;
using System;


public partial class EnemyCharBod : CharacterBody3D
{
	
	[ExportGroup("Properties")]
	[Export]
	NavigationAgent3D NavAgent3D;
	[Export]
	Marker3D[] Waypoints;
	[Export]
	float Speed = 3.0f;
	[Export]
	Timer WaitTimer;
	[Export]
	MeshInstance3D Head;
	
	
	int CurrentWaypoint; //Waypoint Array Index
	bool PlayerInHearRangeFar, PlayerInHearRangeClose,PlayerInSightRangeFar,PlayerInSightRangeClose; //Bools For Seeing/Hearing Player
	

	
	enum States
	{
		Patrol,
		Chasing,
		Hunting,
		Waiting
	}

	States CurrentState;
	
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		CurrentState = States.Patrol;
		NavAgent3D.SetTargetPosition(Waypoints[0].GlobalPosition);
	
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		switch(CurrentState)
		{
			case(States.Patrol):
				if(NavAgent3D.IsNavigationFinished()){ CurrentState = States.Waiting; WaitTimer.Start(); return;}
				MoveTowardsWaypoint(Speed);
				 break;
			case(States.Waiting): break;
			case(States.Hunting): 
				if(NavAgent3D.IsNavigationFinished()){ CurrentState = States.Waiting; WaitTimer.Start();  return;}
				MoveTowardsWaypoint(Speed/10);
				break;
			case(States.Chasing):
				if(NavAgent3D.IsNavigationFinished()){ CurrentState = States.Waiting; WaitTimer.Start();  return;}
				MoveTowardsWaypoint(Speed+2);
				break;
			default: break;
		}
			
	}
	

		public void MoveTowardsWaypoint(float speed)
		{
				var TargetPos = NavAgent3D.GetNextPathPosition();
				var Direction = GlobalPosition.DirectionTo(TargetPos);
				FaceDirection(TargetPos);
				Velocity = Direction * Speed;
				MoveAndSlide();
				CheckForPlayer();
				

		}

		public void CheckForPlayer() //CheckPlayer Checks Crouch or Not
		{

			//Check to See if Player Obscured
			var SpaceState = GetWorld3D().DirectSpaceState;
			var Querry = PhysicsRayQueryParameters3D.Create(Head.GlobalPosition,GetTree().GetNodesInGroup("Player")[0].GetNode<Camera3D>("Head/Camera3D").GlobalPosition);
			Querry.Exclude = new Godot.Collections.Array<Rid>{GetRid()};
			var Result = new Godot.Collections.Dictionary{};
			Result = SpaceState.IntersectRay(Querry);
			
			
			if (Result.Count > 0 )
			{
				if (GetTree().GetNodesInGroup("Player").Contains((Node)Result["collider"]))
				{
					var t = GetTree().GetNodesInGroup("Player")[0];
					var CharacterScriptReference = (Character)t;
					if(PlayerInHearRangeClose == true || PlayerInSightRangeClose == true) //Player Is too Close Begin Chasing
					{
						GD.Print("Chasing");
						if(CharacterScriptReference.Crouched == false)
						{
							CurrentState = States.Chasing;
							NavAgent3D.SetTargetPosition(GetTree().GetNodesInGroup("Player")[0].GetNode<Camera3D>("Head/Camera3D").GlobalPosition);
							
							return; //Needed Else Hunting State Will Always Override
						}
						
					}
					else if(PlayerInHearRangeFar == true || PlayerInSightRangeFar == true) //Player Is Within Rrange Begin Hunting
					{
						
						if(CharacterScriptReference.Crouched == false)
						{
							CurrentState = States.Hunting;
							NavAgent3D.SetTargetPosition(GetTree().GetNodesInGroup("Player")[0].GetNode<Camera3D>("Head/Camera3D").GlobalPosition);
						
						}
						
					}
					
				}
				else if(PlayerInHearRangeFar == true || PlayerInSightRangeFar == true) // Player is obscured but can be heard Needs Better Implementation should only go to characters current position
				{
					var t = GetTree().GetNodesInGroup("Player")[0];
					var CharacterScriptReference = (Character)t;
					if(CharacterScriptReference.AudioStreamPlayer.Playing == true && CharacterScriptReference.AudioStreamPlayer.VolumeDb > 0)
					{ 
						CurrentState = States.Hunting;
						NavAgent3D.SetTargetPosition(GetTree().GetNodesInGroup("Player")[0].GetNode<Camera3D>("Head/Camera3D").GlobalPosition);
						Console.WriteLine("CAN HEAR PLAYER");
					}
						
				}
			}
			
			
		}
	
		public void OnWaitTimeOut() //WaitTimeSignalThenChangeState
		{
			CurrentState = States.Patrol;
			if(CurrentWaypoint == Waypoints.Length - 1)
			{
				CurrentWaypoint = 0;
			}
			else
			{
				CurrentWaypoint++;
			}
			NavAgent3D.SetTargetPosition(Waypoints[CurrentWaypoint].GlobalPosition);
		}
		
		public void FaceDirection(Vector3 Dir) //FaceNextPatrolPoint
		{
			LookAt(new Vector3(Dir.X,GlobalPosition.Y,Dir.Z));
		}
	
		public void OnHearFarAreaBodyEntered(Node3D body) //Player Enters HearingZone Start Hunting
		{
			if(body.IsInGroup("Player"))
			{
				PlayerInHearRangeFar = true;
			}
			
		}
		public void OnHearFarAreaBodyExit(Node3D body) //Player Gone Back To Patrol
		{
			if(body.IsInGroup("Player"))
			{
				PlayerInHearRangeFar = false;
			}
			
		}
		public void OnHearCloseAreaBodyEntered(Node3D body) //Player Too Close Enter Chase
		{
			if(body.IsInGroup("Player"))
			{
				PlayerInHearRangeClose = true;
			}
			
		
		}
		public void OnHearCloseAreaBodyExit(Node3D body) //Player Escape Continue Hunt
		{
			if(body.IsInGroup("Player"))
			{
				PlayerInHearRangeClose = false;
			}
		
		}
		public void OnSightCloseAreaBodyEntered(Node3D body)
		{
			if(body.IsInGroup("Player"))
			{
				PlayerInSightRangeClose = true;
			}
		}
		public void OnSightCloseAreaBodyExit(Node3D body)
		{
			if(body.IsInGroup("Player"))
			{
				PlayerInSightRangeClose = false;
			}
		}
		public void OnSightFarAreaBodyEntered(Node3D body)
		{
			if(body.IsInGroup("Player"))
			{
				PlayerInSightRangeFar = true;
			}
		}
		public void OnSightFarAreaBodyExit(Node3D body)
		{
			if(body.IsInGroup("Player"))
			{
				PlayerInSightRangeFar = false;
			}
		}
	
	
	
	
}
