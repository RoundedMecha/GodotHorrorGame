using Godot;
using System;
using System.Collections;

public partial class ObjectiveBringItemTo : StaticBody3D
{

	Character PlayerCharacter;
	AnimationPlayer AnimationPlayer;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		var playerChar = GetTree().GetNodesInGroup("Player")[0];
		PlayerCharacter = (Character)playerChar;

		AnimationPlayer = GetChild<AnimationPlayer>(3);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	void OnBodyEntered(Node3D Body)
	{
		if(Body.Name == PlayerCharacter.Name)
		{
			if(PlayerCharacter.Holding == true && PlayerCharacter.HoldItemSpace.GetChild(0).Name == "ItemPickUp")
			{
				AnimationPlayer.Play("MorphTest");
			}
		}
	}
}
