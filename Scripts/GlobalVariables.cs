using Godot;
using System;

public partial class GlobalVariables : Node
{
	private static GlobalVariables Instance;
	public static GlobalVariables _Instance;
	public int PlayerHealth {get;set;} = 100;
	public Node CurrentScene {get;set;}
    public Viewport CurViewPort{get;set;}


    public override void _EnterTree()
    {
        
		if(Instance != null)
		{
			this.QueueFree();
		}
		Instance = this;
		base._EnterTree();
    }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
		var Root = GetNode("/root");
		CurrentScene = Root.GetChild(Root.GetChildCount() - 1);
        CurViewPort = Root.GetViewport();
        GD.Print("Current ViewPort Size: " + CurViewPort.GetWindow().Size);
       
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		
	}
	public void GotoScene(string path)
{
    // This function will usually be called from a signal callback,
    // or some other function from the current scene.
    // Deleting the current scene at this point is
    // a bad idea, because it may still be executing code.
    // This will result in a crash or unexpected behavior.

    // The solution is to defer the load to a later time, when
    // we can be sure that no code from the current scene is running:

    CallDeferred(nameof(DeferredGotoScene), path);
}

public void DeferredGotoScene(string path)
{
    // It is now safe to remove the current scene
    CurrentScene.Free();

    // Load a new scene.
    var nextScene = (PackedScene)GD.Load(path);

	

    // Instance the new scene.
    CurrentScene = nextScene.Instantiate();

    // Add it to the active scene, as child of root.
    GetTree().GetRoot().AddChild(CurrentScene);

    // Optionally, to make it compatible with the SceneTree.change_scene() API.
    GetTree().SetCurrentScene(CurrentScene);
}



}
