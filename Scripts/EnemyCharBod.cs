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
	int CurrentWaypoint;
	
	
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
				var TargetPos = NavAgent3D.GetNextPathPosition();
				var Direction = GlobalPosition.DirectionTo(TargetPos);
				FaceDirection(TargetPos);
				Velocity = Direction * Speed;
				MoveAndSlide();
				 break;
			case(States.Waiting): break;
			case(States.Hunting): break;
			case(States.Chasing): break;
			default: break;
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
				GD.Print("PLAYERHEAR");
			}
			
		}
		public void OnHearFarAreaBodyExit(Node3D body) //Player Gone Back To Patrol
		{
			if(body.IsInGroup("Player"))
			{
				GD.Print("PLAYGONE");
			}
			
		}
		public void OnHearCloseAreaBodyEntered(Node3D body) //Player Too Close Enter Chase
		{
			if(body.IsInGroup("Player"))
			{
				GD.Print("PLAYTOOCLOSE");
			}
			
		
		}
		public void OnHearCloseAreaBodyExit(Node3D body) //Player Escape Continue Hunt
		{
			if(body.IsInGroup("Player"))
			{
				GD.Print("PLAYTOOCLOSEEXIT");
			}
		
		}
		public void OnSightCloseAreaBodyEntered(Node3D body)
		{
			if(body.IsInGroup("Player"))
			{
				GD.Print("PLAYERSEECLOSE");
			}
		}
		public void OnSightCloseAreaBodyExit(Node3D body)
		{
			if(body.IsInGroup("Player"))
			{
				GD.Print("PLAYSEETOOCLOSEEXIT");
			}
		}
		public void OnSightFarAreaBodyEntered(Node3D body)
		{
			if(body.IsInGroup("Player"))
			{
				GD.Print("PLAYERFARSEE");
			}
		}
		public void OnSightFarAreaBodyExit(Node3D body)
		{
			if(body.IsInGroup("Player"))
			{
				GD.Print("PLAYTFARSEEEEXIT");
			}
		}
	
	
	
	
}
