using Godot;
using System;

public partial class TitleScreen : Control
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		

	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		
	}

	public void ButtonPressed()
	{
		var GlobalVar = (GlobalVariables)GetNode("/root/GlobalVariables");
		GD.PrintT(GetNode("/root/GlobalVariables"));
		GlobalVar.GotoScene("res://Scenes/testScene.tscn");
	}
	public void SettingButtonPress(){ var GlobalVar = (GlobalVariables)GetNode("/root/GlobalVariables");  GlobalVar.CurViewPort.GetWindow().Size = new Vector2I(1920, 1080); GD.Print("NEW VIEWPORT SIZE" + GlobalVar.CurViewPort.GetWindow().Size); GetChild(1).GetChild<CanvasLayer>(2).Visible = false; GetChild<CanvasLayer>(1).Visible = true;}
	public void TransferToSettingsButton(){GetChild(1).GetChild<CanvasLayer>(2).Visible = true; GetChild<CanvasLayer>(1).Visible = false;}


}
