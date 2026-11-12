using Godot;
using Godot.Collections;
using System;

namespace SlackingOff;

public partial class EmailTask : Control
{
	// === Signals === (PascalCaseEventHandler())
	[Signal] public delegate void AllEmailsAnsweredEventHandler();
	[Signal] public delegate void AnyEmailAnsweredEventHandler();

	// === Enums === (PascalCase, members CONSTANT_CASE)

	// === Constants === (CONSTANT_CASE)

	// === Exported Vars === (PascalCase)
	[Export] public PackedScene EmailDisplayPacked;

	// === Public Vars === (PascalCase)

	// === Private Vars === (_underscoredCamelCase)


	private ItemList _tabs;
	private TabContainer _display;
	private RandomNumberGenerator _rng;
	
	// === Godot Methods ===
	
	public EmailTask()
	{
		//runs when the object is created in memory, Parent -> Child
		_rng = new RandomNumberGenerator();
	}
	
	public override void _Ready()
	{
		//runs when the object and its children are ready, Child -> Parent

		_tabs = GetNode<ItemList>("EmailList");
		_display = GetNode<TabContainer>("EmailDisplay");

		ClearOldEmails();
		CreateEmails(8);


	}
	
	// Prefer using PhysicsProcess over Process
	public override void _PhysicsProcess(double delta)
	{
		
	}
	
	// === Further Methods === (PascalCase, local variables camelCase)

	public void SetTabVisible(int index)
	{
		_display.CurrentTab = index;
	}

	public void DeleteCurrentEmail()
	{
		int prevSelectionIndex = _display.CurrentTab;
		_tabs.RemoveItem(prevSelectionIndex);
		_display.GetCurrentTabControl().QueueFree();

		_tabs.Select(prevSelectionIndex == 0 ? prevSelectionIndex : prevSelectionIndex - 1);
	}

	public void ClearOldEmails()
	{
		_tabs.Clear();

		var children = _display.GetChildren();
		foreach (var child in children )
		{
			child.QueueFree();
		}
	}

	public void CreateEmails(int amt)
	{
		Array<Array<string>> bag = new();

		for (int i = 0; i < amt; i++)
		{
			if (bag.Count == 0)
			{
				bag = new(EmailDisplay.EMAILS);
				bag.Shuffle();
			}

			var disp = EmailDisplayPacked.Instantiate<EmailDisplay>();

			int emailIndex = _rng.RandiRange(0, bag.Count - 1);
			var email = bag[emailIndex];
			bag.RemoveAt(emailIndex);

			disp.SetEmailByTitle((string)email[0]);

            disp.Ready += () =>
            {

                disp.SendButton.Pressed += () =>
                {
                    DeleteCurrentEmail();
					EmitSignal(SignalName.AnyEmailAnswered);
					if(_tabs.ItemCount == 0)
					{
						EmitSignal(SignalName.AllEmailsAnswered);
					}
                };
            };

            _display.AddChild(disp);

			

			_tabs.AddItem(email[0]);
			_tabs.SetItemTooltipEnabled(_tabs.ItemCount - 1, false);

		}

        _tabs.Select(0);
    }
}
