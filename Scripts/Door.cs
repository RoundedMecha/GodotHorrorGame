using Godot;
using System;

public partial class Door : Node3D
{
	enum DoorState
	{
		Open,
		Close,
	}

	[ExportGroup("Properties")]
	[Export]
	DoorState DefaultState;
	AnimationPlayer AnimationPlayerDoor;

	private bool Interactable;



	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		GD.Print("LLOOK HERE" + GetChildCount());
		AnimationPlayerDoor = GetChild<AnimationPlayer>(2);
		
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{

		if(Interactable)
		{
			if(Input.IsActionJustPressed("Interact"))
			{
				switch(DefaultState)
				{
					case(DoorState.Open):
						AnimationPlayerDoor.Queue("DoorClose");
						DefaultState = DoorState.Close;
						break;
					case(DoorState.Close):
						AnimationPlayerDoor.Queue("DoorOpen");
						DefaultState = DoorState.Open;
						break;
					default:
						break;
				}
			}

		}

	}

	public void OnArea3DBodyEntered(Node3D Body)
	{
		if(Body.IsInGroup("Player"))
		{
			Interactable = true;
			
		}
	}
	public void OnArea3DBodyExit(Node3D Body)
	{
		if(Body.IsInGroup("Player"))
		{
			Interactable = false;
			
		}
	}
}
