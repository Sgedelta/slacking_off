using Godot;
using Godot.Collections;
using System;
using System.Linq;

namespace SlackingOff;

public partial class OS : Control
{
	// === Signals === (PascalCaseEventHandler())

	// === Enums === (PascalCase, members CONSTANT_CASE)
	public enum ProcessNames
	{
		NONE,
		WORK,
		SNAKE,

		PROCESS_MAX
	}

	// === Constants === (CONSTANT_CASE)

	// === Exported Vars === (PascalCase)
	[Export] public PackedScene AppWindowScene { get; set; }
	[Export] public Dictionary<ProcessNames, PackedScene> ProcessPackedScenes = new();

	// === Public Vars === (PascalCase)

	public Dictionary<int, AppWindow> RunningApps = new();

	// === Private Vars === (_underscoredCamelCase)
	Control _windowParent;


	
	// === Godot Methods ===
	
	public OS()
	{
		//runs when the object is created in memory, Parent -> Child
	}
	
	public override void _Ready()
	{
		//runs when the object and its children are ready, Child -> Parent
		_windowParent = GetNode<Control>("%WindowParent");



		StartApp(ProcessNames.WORK);
	}
	
	// Prefer using PhysicsProcess over Process
	public override void _PhysicsProcess(double delta)
	{
		
	}
	
	// === Further Methods === (PascalCase, local variables camelCase)


	public void StartApp(ProcessNames appType)
	{
		AppWindow appWin = (AppWindow)AppWindowScene.Instantiate();


        appWin.Setup(0, ProcessPackedScenes[appType]);


		// do specific setup here
		switch(appType)
		{



            case ProcessNames.NONE:
			default:
				GD.PushWarning("Attempted to start app with no process type! created node was ", appWin.Name);
				break;
		}

        _windowParent.AddChild(appWin);

    }

	public int GetFirstAvailAppID()
	{
		// keys as a sorted list
		var ids = RunningApps.Keys.OrderBy(n => n).ToList();

		return ids.Zip(ids.Skip(1), (curr, next) => new { curr, next })
			.Where(pair => pair.next - pair.curr > 1).
			Select(pair => (int?)(pair.curr + 1))
			.FirstOrDefault() ?? 0;

	}


}
