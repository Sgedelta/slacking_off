using Godot;
using System;

namespace SlackingOff;

/// <summary>
/// Gamerunner is the primary game manager class. it controls the primary flow of the game and related values.
/// Additionally collects generic game inputs if required, such as for switching camera views.
/// </summary>
public partial class GameRunner : Node
{
	// === Signals === (PascalCaseEventHandler())

	// === Enums === (PascalCase, members CONSTANT_CASE)
	[Flags]
	public enum GameState
	{
		NONE = 0,
		VIRUS_GAME_RUNNING = 1 << 0,
		WORK_SCREEN_UP = 1 << 1,
		BOSS_PRESENT = 1 << 2,
	}

	// === Constants === (CONSTANT_CASE)

	// === Exported Vars === (PascalCase)
	[Export] public float VirusComposureMax = 100;
	[Export] public float VirusEnergyMax = 100;
	[Export] public float VirusEnergyGain = 1.0f;
	[Export] public float VirusEnergyDrainOnGameComplete = 25f;

    // === Public Vars === (PascalCase)
    public GameState State
	{
		get
		{
			return _state;
		}
        // use methods for set - SetSingleStateFlag (preferred), OverwriteStateFlags
    }



    // === Private Vars === (_underscoredCamelCase)
    private GameState _state;
	private OS _os;
	private Node3D _bossGame;
	private CameraController _cameraController;
	private float _virusEnergy = 20; //TEMP FOR PLAYTEST! should start at 0.

	private MultitaskGame _mtGameController;

    // === Godot Methods ===

    public GameRunner()
	{
		//runs when the object is created in memory, Parent -> Child
	}
	
	public override void _Ready()
	{
		//runs when the object and its children are ready, Child -> Parent
		_os = GetNode<OS>("%OS");
		_bossGame = GetNode<Node3D>("%BossMinigame");
		_cameraController = GetNode<CameraController>("%CameraController");

		_os.AppOpenedOrClosed += HandleAppOpenOrClose;
		_bossGame.Connect("game_over", Callable.From<bool>(BossGameWon));

		//Callable.From(() => _os.StartApp(OS.ProcessNames.WORK)).CallDeferred();

	}
	
	// Prefer using PhysicsProcess over Process
	public override void _PhysicsProcess(double delta)
	{
		float dt = (float)delta;
		// check states and update things as needed.
		if(State.HasFlag(GameState.WORK_SCREEN_UP) || !State.HasFlag(GameState.VIRUS_GAME_RUNNING))
		{
			_virusEnergy += VirusEnergyGain * dt;
		}

		// fix values if needed
		_virusEnergy = Mathf.Clamp(_virusEnergy, 0, VirusEnergyMax);

		// update downstream values
		if(IsInstanceValid(_mtGameController))
		{
            _mtGameController.SetSpawnTimeByNormalizedDifficulty(_virusEnergy / VirusEnergyMax);
		}
	}

    public override void _Input(InputEvent @event)
    {
        if(@event.IsActionPressed("DEBUG_force_boss_start"))
		{
			SetSingleStateFlag(GameState.BOSS_PRESENT, !_state.HasFlag(GameState.BOSS_PRESENT));
		}
    }

    // === Further Methods === (PascalCase, local variables camelCase)

    /// <summary>
    /// Sets the game state to the provided game state, calling additional methods as appropriate. 
    /// will only change a single flag. Preferred method of setting game flags.
    /// </summary>
    /// <param name="stateChange"></param>
    /// <param name="on">if the flag should be written to true or written to false</param>
    /// <returns>true if the state is able to be changed. false otherwise.</returns>
    public bool SetSingleStateFlag(GameState stateChange, bool on) 
	{
        // TODO: Improve, this is just getting a basic version in.

        // check invalid transitions - TODO: determine what transitions are invalid, other than no-ops
        GameState oldState = _state;
		GameState newState = on ? _state | stateChange : _state & ~stateChange;
        if (oldState == newState)
		{
			return false;
		}

		// update backing
		_state = newState;

		// call methods - based on new state and oldstate
		CallStateTransitionMethods(oldState, _state);

        return true;
	}

	private void CallStateTransitionMethods(GameState prev, GameState curr)
	{
		//NOTE: be careful about ifs and elses here. because these are flags, many can change at once. be sure to else/elif correctly!!

		// boss left
        if(prev.HasFlag(GameState.BOSS_PRESENT) && !curr.HasFlag(GameState.BOSS_PRESENT))
		{
            _bossGame.Call("stop_game", false, false);
            _cameraController.GoToState(CameraController.CameraPositions.COMPUTER);
        }
		// or boss arrived
		else if(!prev.HasFlag(GameState.BOSS_PRESENT) && curr.HasFlag(GameState.BOSS_PRESENT))
		{
            _bossGame.Call("start_round", 0);
            _cameraController.GoToState(CameraController.CameraPositions.BOSS);
			if(!curr.HasFlag(GameState.WORK_SCREEN_UP))
			{
				_os.StartApp(OS.ProcessNames.WORK);
			}
        }

    }

	/// <summary>
	/// Overwrites all state flags to a new state, disregarding all old flags.
	/// </summary>
	/// <param name="newState"></param>
	public void OverwriteStateFlags(GameState newState)
	{
		_state = newState;
		// TODO: Call methods?
	}


	public void HandleAppOpenOrClose(AppWindow win, int appTypeEnum, int id, bool opened)
	{
		OS.ProcessNames appType = (OS.ProcessNames)appTypeEnum;

		switch(appType)
		{
			case OS.ProcessNames.WORK:
				if(opened)
				{
					// TODO: store the work window's / mutlitask's controller to be updated?
					win.SetupCompleted += RegisterMTGameController;

                    SetSingleStateFlag(GameState.WORK_SCREEN_UP, true);
				}
				else
				{
					SetSingleStateFlag(GameState.WORK_SCREEN_UP, false);
				}
			break;
			case OS.ProcessNames.SNAKE:
				if(opened)
				{
					win.Connect(AppWindow.SignalName.CloseRequested, Callable.From<AppWindow, bool>(VirusGameOver), (uint)ConnectFlags.OneShot);

                    SetSingleStateFlag(GameState.VIRUS_GAME_RUNNING, true);
				}
				else
				{
                    SetSingleStateFlag(GameState.VIRUS_GAME_RUNNING, false);
                }
			break;


			default:
				GD.PushError("App Type ", appType, " from int ", appTypeEnum, " not recognized for close or open behavior!");
				return; //do nothing if not recognized
		}

	}

	private void RegisterMTGameController(AppWindow win)
	{
        _mtGameController = (MultitaskGame)win.SubsceneController;
		if(!IsInstanceValid(_mtGameController))
		{
			return;
		}

		win.Connect(AppWindow.SignalName.CloseRequested, Callable.From<AppWindow, bool>(WorkClosed), (uint)ConnectFlags.OneShot);
    }

	private void WorkClosed(AppWindow win, bool byVirus)
	{
		_os.CloseApp(win.AppID); //will cause HandleAppOpenOrClosed to run
		//TEMP: if you lose boss game ends, this would be getting yelled at later.
		if (byVirus) 
		{
            SetSingleStateFlag(GameState.BOSS_PRESENT, false);
        }

    }

	private void BossGameWon(bool won)
	{
		if(won)
		{
			// TODO: make more robust!! this is very TEMP
            SetSingleStateFlag(GameState.BOSS_PRESENT, false);
            
        }
	}

	private void VirusGameOver(AppWindow win, bool won)
	{
		_os.CloseApp(win.AppID);

		if(won)
		{
			_virusEnergy -= VirusEnergyDrainOnGameComplete;
			_virusEnergy = Mathf.Clamp(_virusEnergy, 0, VirusEnergyMax);

        }
	}
}
