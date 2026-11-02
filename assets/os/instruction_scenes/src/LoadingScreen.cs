using Godot;
using Godot.Collections;
using System;
using System.Linq;

namespace SlackingOff;

public partial class LoadingScreen : Control
{
	// === Signals === (PascalCaseEventHandler())
	[Signal] public delegate void GameSceneLoadedOutsideTreeEventHandler(Node GameSceneController);

	// === Enums === (PascalCase, members CONSTANT_CASE)

	// === Constants === (CONSTANT_CASE)

	// === Exported Vars === (PascalCase)

	[Export] public float LoadingTime = 3;
	[Export] public float WaitAfterLoad = 1;
    [Export] public OSInstructions Instructions;
	[Export(PropertyHint.Range, "0, 20, 1, prefer_slider")] public int RandomStutterCount = 0;

    // === Public Vars === (PascalCase)

    // === Private Vars === (_underscoredCamelCase)
    private Tween _loadingTween;
	private TextureProgressBar _loadingBar;
	private GridContainer _instructionGrid;
	private Node _gameScene;
	private RandomNumberGenerator _rng;
	
	// === Godot Methods ===
	
	public LoadingScreen()
	{
		//runs when the object is created in memory, Parent -> Child
	}
	
	public override void _Ready()
	{
		//runs when the object and its children are ready, Child -> Parent
		_loadingBar = GetNode<TextureProgressBar>("ProgressBar");
		_instructionGrid = GetNode<GridContainer>("InstructionContainer");
		_rng = new RandomNumberGenerator();	

        CreateInstructionGrid();

        SetupGameScene();

        SetupTween();
	}
	
	// === Further Methods === (PascalCase, local variables camelCase)
	public void SetupGameScene()
	{
        if (!IsInstanceValid(Instructions.GameToLoad))
        {
            GD.PushError("Failed to load game scene in loading scene because packed scene was invalid.");
            return;
        }

        _gameScene = Instructions.GameToLoad.Instantiate();

		EmitSignal(SignalName.GameSceneLoadedOutsideTree, _gameScene);
    }


	public void SetupTween()
	{
		//kill old tween if it's running
		if(IsInstanceValid(_loadingTween) && _loadingTween.IsRunning())
		{
			_loadingTween.Kill();
		}

		_loadingBar.Value = _loadingBar.MinValue;
		float timeRemaining = LoadingTime;
		Array<float> stutterTimes = new();
		Array<float> barPositions = new();

		_loadingTween = CreateTween();
		float barRange = (float)(_loadingBar.MaxValue - _loadingBar.MinValue);

        //random stutters
        for (int i = 0; i < RandomStutterCount; i++)
		{
			stutterTimes.Add(_rng.RandfRange(timeRemaining / (RandomStutterCount * 2), Mathf.Min(LoadingTime / (RandomStutterCount * .5f), timeRemaining)));
			timeRemaining -= stutterTimes.Last();
			barPositions.Add(_rng.RandfRange(
				(float)(_loadingBar.MinValue + 0.1f * barRange), 
				(float)_loadingBar.MaxValue - 0.2f * barRange)
			);
        }
		barPositions.Sort();
		for(int i = 0; i < RandomStutterCount; i++)
		{
            _loadingTween.TweenProperty(_loadingBar, "value",
				barPositions[i], stutterTimes[i]);
        }

        _loadingTween.TweenProperty(_loadingBar, "value", _loadingBar.MaxValue, timeRemaining);
        _loadingTween.TweenInterval(WaitAfterLoad);
		_loadingTween.Finished += ReplaceWithGameScene;

    }

	public void CreateInstructionGrid()
	{
        if (!IsInstanceValid(Instructions))
        {
			return;
        }

		_instructionGrid.Columns = Instructions.GridCols;
		foreach(var texture in Instructions.InstructionsInOrder)
		{
            var ratio = new AspectRatioContainer();
            var textRect = new TextureRect();

            ratio.SizeFlagsHorizontal = SizeFlags.ExpandFill;
            ratio.SizeFlagsVertical = SizeFlags.ExpandFill;

            textRect.StretchMode = TextureRect.StretchModeEnum.KeepAspect;
			textRect.Texture = texture;

			ratio.AddChild(textRect);
			_instructionGrid.AddChild(ratio);
        }

    }

	public void ReplaceWithGameScene()
	{
        if (!IsInstanceValid(_gameScene))
        {
            GD.PushError("Failed to load game scene in loading scene because created scene was invalid.");
            return;
        }

        GetParent().AddChild(_gameScene);

		QueueFree();
	}
}
