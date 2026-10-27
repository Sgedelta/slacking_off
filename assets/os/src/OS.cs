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
	private readonly Theme _OS_THEME = ResourceLoader.Load<Theme>("res://assets/os/themes/os_main.tres");
	private const int _BUTTON_MAX_WIDTH = 350;
	private const int _BUTTON_MIN_WIDTH = 150;


	// === Exported Vars === (PascalCase)
	[Export] public PackedScene AppWindowScene { get; set; }
	[Export] public Dictionary<ProcessNames, PackedScene> ProcessPackedScenes = new();
	
	// === Public Vars === (PascalCase)

	public Dictionary<int, AppWindow> RunningApps = new();
	public Dictionary<int, Button> AppButtons = new();

	// === Private Vars === (_underscoredCamelCase)
	Control _windowParent;
	Control _taskBarContainer;


	
	// === Godot Methods ===
	
	public OS()
	{
		//runs when the object is created in memory, Parent -> Child
	}
	
	public override void _Ready()
	{
		//runs when the object and its children are ready, Child -> Parent
		_windowParent = GetNode<Control>("%WindowParent");
		_taskBarContainer = GetNode<Control>("%TaskBarAlign");


        StartApp(ProcessNames.NONE);
        StartApp(ProcessNames.NONE);
        StartApp(ProcessNames.WORK);

    }

    // Prefer using PhysicsProcess over Process
    public override void _PhysicsProcess(double delta)
	{
		
	}

	// === Further Methods === (PascalCase, local variables camelCase)


	public void StartApp(ProcessNames appType)
	{
		// create
		AppWindow appWin = (AppWindow)AppWindowScene.Instantiate();

		int id = GetFirstAvailAppID();
        GD.Print("adding app with id ", id);
		GD.Print("num running apps: ", RunningApps.Count);
        RunningApps.Add(id, appWin);

		appWin.Setup(id, ProcessPackedScenes[appType]);

        appWin.AppName = appType switch
        {
            ProcessNames.WORK => "Very Important Work",
            _ => "WIN_ERR"
        };

        // hook up closing effects
        appWin.Connect(AppWindow.SignalName.TreeExited, Callable.From(() => { CleanupApp(id); }), (uint)ConnectFlags.OneShot);

		// do specific setup here
		switch (appType)
		{



			case ProcessNames.NONE:
			default:
				GD.PushWarning("Attempted to start app with no valid process type! created node was ", appWin.Name);
				break;
		}

		// create button for task
		Button taskButton = new Button();
		taskButton.Theme = _OS_THEME;
		taskButton.ThemeTypeVariation = "TaskbarButton";

		taskButton.Text = appWin.AppName;

		taskButton.CustomMaximumSize = new Vector2(_BUTTON_MAX_WIDTH, -1);
		taskButton.CustomMinimumSize = new Vector2(_BUTTON_MIN_WIDTH, 0);
		taskButton.SizeFlagsHorizontal = SizeFlags.Fill;

		taskButton.Pressed += () => { appWin.GrabFocus(); };


		// put in scene
        _windowParent.AddChild(appWin);

		_taskBarContainer.AddChild(taskButton);

    }

	public int GetFirstAvailAppID()
	{
		// base cases
		if (RunningApps.Count == 0)
		{
			return 0;
		}
		else if (RunningApps.Count == 1)
		{
			// we want 1 if the only thing is zero, otherwise we take the 0 slot
			return RunningApps.Keys.First() == 0 ? 1 : 0;
		}

		// keys as an ascending sorted list
		var ids = RunningApps.Keys.OrderBy(n => n).ToList();

		// if the earliest element is larger than 0, take 0 slot
		if(ids[0] > 0)
		{
			return 0;
		}

		// otherwise we do some linq stuff to grab the lowest pair
		var lowest_pair = ids.Zip(ids.Skip(1), (curr, next) => new { curr, next }) // zip every element with the element after, (drops last element)
			.Where(pair => pair.next - pair.curr > 1) // only gaps we can fit in
			.FirstOrDefault();

		if (lowest_pair != null)
		{
			return lowest_pair.curr + 1;
		}
		else
		{
			// no gaps, return the next number
			return ids.Last() + 1;
		}

	}

	private void CleanupApp(int appID)
	{
		if(!RunningApps.ContainsKey(appID))
		{
			return;
		}
		AppWindow window = RunningApps[appID];
        RunningApps.Remove(appID);

		// clear the button
		Button b = AppButtons[appID];
		AppButtons.Remove(appID);

		b.QueueFree();
    }


}
