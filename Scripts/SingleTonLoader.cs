using Godot;
using System;

public partial class SingleTonLoader : Node
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		GlobalVariables TypeV = new GlobalVariables();
		GD.Print(TypeV.PlayerHealth);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
