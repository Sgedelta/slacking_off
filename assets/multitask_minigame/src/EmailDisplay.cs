using Godot;
using Godot.Collections;
using System;
using System.Linq;

namespace SlackingOff;

public partial class EmailDisplay : Control
{
	// === Signals === (PascalCaseEventHandler())
	[Signal] public delegate void FinishedTypingEmailEventHandler();

	// === Enums === (PascalCase, members CONSTANT_CASE)

	// === Constants === (CONSTANT_CASE)
	public static readonly Array<Array<string>> EMAILS = new()
	{
		new() { "Important Work, Respond ASAP!", "Suspendisse quis iaculis nisl. Etiam rhoncus, nibh ac lacinia laoreet, velit magna laoreet nisl, non consequat sem tellus non tortor. Vestibulum in pulvinar eros. Nunc lobortis felis eleifend nisi venenatis, at finibus lectus fringilla. Morbi molestie hendrerit tortor a vestibulum. Sed vulputate bibendum lobortis. Lorem ipsum dolor sit amet, consectetur adipiscing elit. Fusce quis ligula dui. Curabitur consequat metus nec massa rhoncus mollis. Aliquam erat volutpat." },
        new() { "Circling Back on Prev", "In ut tristique odio. Nunc quis tincidunt ante. Fusce iaculis, ipsum id commodo suscipit, odio felis fermentum purus, eget luctus ex enim et nisi. Etiam ut libero non dui commodo porta. Pellentesque rhoncus semper justo non lacinia. Nullam vehicula euismod est non mollis. Donec nec maximus purus. " },
        new() { "Re:Your last Email", "Class aptent taciti sociosqu ad litora torquent per conubia nostra, per inceptos himenaeos. In hac habitasse platea dictumst. Etiam porta porta magna vel rhoncus. Duis massa nisl, faucibus eu consequat vitae, rhoncus a turpis. Duis tincidunt vehicula elit quis lobortis. In volutpat efficitur tellus, vitae tincidunt mi. Integer eget erat vel turpis rhoncus rhoncus. In et erat nec erat pharetra efficitur eget in velit. " },
        new() { "Following up on monday's email", "Class aptent taciti sociosqu ad litora torquent per conubia nostra, per inceptos himenaeos. Morbi pretium tincidunt orci nec tempus. Phasellus mollis et nisl et semper. In tempus dui ut neque viverra, ac bibendum leo ultricies. Duis dictum, augue commodo lacinia volutpat, ipsum lacus fringilla augue, a tempus ligula lacus sit amet tortor. Maecenas at purus quis nibh malesuada posuere sed et libero. Etiam pellentesque velit eros, quis malesuada erat pretium id. Vestibulum quis mauris non diam ornare eleifend non in nisl. Sed a sagittis nulla. Nullam feugiat malesuada sem at viverra. Proin faucibus, sem non hendrerit sodales, enim metus aliquam magna, vel pulvinar nisi ex vitae orci. Ut a velit feugiat, fermentum nibh in, posuere magna. " },
        new() { "Re:Responding to client", "Cras sed velit feugiat, imperdiet nisi ut, pharetra eros. Nam in odio ante. Phasellus eu mattis lectus. Fusce nisi enim, semper sit amet volutpat eget, fermentum id lorem. Integer aliquet eros vel felis ullamcorper consectetur. Sed nec dolor urna. Curabitur ultricies velit a justo venenatis, eget dictum lacus facilisis. Sed gravida enim sit amet mattis semper. Fusce eu mi vitae erat iaculis vehicula. Nulla suscipit nibh in tortor porttitor, vitae vulputate massa lacinia. Phasellus vitae mauris quis lorem dictum commodo. Aliquam erat volutpat. " },
        new() { "Respond, Now!!", "Vivamus ac mauris hendrerit, accumsan justo at, hendrerit urna. Nunc consequat cursus diam, sed cursus eros imperdiet at. Phasellus sit amet est ac lorem fermentum pellentesque. Aliquam erat volutpat. Cras auctor quis nisi eget vulputate. Fusce vulputate porta hendrerit. Duis pharetra, nisi mattis laoreet condimentum, metus tellus vehicula ex, in rhoncus enim nibh id tellus. Suspendisse molestie blandit felis in elementum. Praesent sem ligula, pharetra eget egestas a, vehicula vel leo. Class aptent taciti sociosqu ad litora torquent per conubia nostra, per inceptos himenaeos. Nullam id felis ante. " },
        new() { "Status Reports", "Mauris maximus congue elit, eu feugiat arcu convallis quis. Proin vel odio at augue tristique pretium. Pellentesque accumsan ullamcorper lacus, vitae tincidunt mauris auctor vitae. Integer quis feugiat enim, ut euismod nisl. Nullam non consectetur odio. Fusce condimentum lectus at ante sollicitudin, et facilisis mauris lacinia. Quisque odio dui, gravida non convallis et, consequat quis diam. Morbi ac elementum orci. Vestibulum quis maximus ligula. Fusce placerat egestas nisl, ac imperdiet tortor sollicitudin eu. Cras convallis lectus eros, eget convallis urna tincidunt eget. Curabitur posuere venenatis mauris vitae cursus. Donec ut tortor ante. Etiam non odio magna. Donec ac aliquam nunc, eget malesuada lectus. " },
        new() { "Upcoming Quarterly Review Schedule", "Mauris lacinia gravida eros nec dictum. Quisque sed nisl consequat, ornare leo a, dignissim ex. Quisque sodales fringilla tincidunt. Pellentesque ultrices, felis eu venenatis blandit, ante tortor interdum tellus, ac malesuada mauris dui vel mi. Nam vitae rhoncus neque, sed hendrerit est. Morbi fermentum risus nec lectus tempus, venenatis hendrerit libero vulputate. Sed elit ante, eleifend in quam sit amet, semper suscipit urna. " },
        new() { "Buisness Proposal", "Quisque venenatis mollis cursus. Donec eros odio, rhoncus nec luctus fermentum, tempor nec nisi. Nam luctus pellentesque nibh, ac semper nisl rhoncus vel. Etiam feugiat auctor lacus. Integer et nisl libero. Phasellus eu pellentesque velit, quis euismod nibh. Mauris volutpat porta est. Suspendisse vitae vulputate ante. Pellentesque fermentum quam sapien, et vestibulum ipsum faucibus sed. Donec quis mi a arcu ornare viverra. Donec et venenatis est, vitae interdum enim. Sed accumsan nunc arcu, quis semper lorem scelerisque in. Pellentesque volutpat quis dui non faucibus. " },

    };

	public static readonly Array<string> JARGON = new()
	{
		"Synthesize", "Alignment", "Circling back", "Bandwidth", "Close the loop", "Paradigm", "Touch Base", "Deliverable", "KPI", "ROI", "EOD", "Hard Stop", "Move the Needle", "Piggyback", "Across the Board", 
		"Align Expectations", "All Hands", "All Hands on Deck", "Ping", "Punt", "Scale", "Scalable", "Synergy", "Table this", "Wheelhouse", "Utilize", "In the loop", "put a pin in that", "Per my last email", 
		"get the ball rolling", "North Star", "pick your brain", "Correct me if I'm wrong", "Roadmap", "Brain Dump", "Hit the ground running", "Backlog", "Scope Creep", "Scrum", "Retrospective", "Standup",
		"Jira", "Agile", "Pivot", "Low-Hanging Fruit", "ETA", "Red Flag", "On the same page", "OKR", "like a family here", "Open-Door Policy", "Run the Numbers", "In the weeds",  "Above my pay grade", "Vision", 
		"Trimming the Fat", "Ball's in your court", "Bullish", "Bearish", "Gentle Reminder", "Encourage", "quick question", "Piggyback", "Have an ask", "Stakeholders", "Churn", "My radar", "Worried about the optics",
		"Pull the trigger", "Drill Down", "MVP", "Run this up the flagpole", "Putting out fires", "culture fit", "status report", "Quarterly", "Buisness Buisness Buisness"
	};

	public static readonly Array<string> EMAIL_TITLES = new() 
	{ 
		"Status Report", "Circling Back", "Upcoming Quarterly Review Schedule", "Buisness Proposal", "Following up on Monday's email", "Need your feedback", "Respond ASAP", "Checking in", "Meeting Confirmation",
		"Meeting Invite"
	};

	public static readonly Array<string> BAD_WORDS = new()
	{
		
	};

	// alphabet, some vowels, common letters, center row of keyboard
	private const string SMASH_POSSIBILITIES = "abcdefghijklmnopqrstuvwxyz" + "aeiouaeiuoaei" + "rstlnrslnrsln" + "asdfghjkl";
	private const string PUNCTUATION = "??.....,,,,,,,";

	private const float PUNCTUATION_CHANCE = .15f;

	// === Exported Vars === (PascalCase)
	[Export] public int CharactersToCompleteEmail = -1;
	[Export] public int CharactersPerPress = 1;

	// === Public Vars === (PascalCase)
	public int EmailIndex { get; private set; }
	public bool EmailTyped { get; private set; } = false;
	public Button SendButton;

	// === Private Vars === (_underscoredCamelCase)
	private bool _readied = false;
	private RandomNumberGenerator _rng;
	private RichTextLabel _bgText;
	private RichTextLabel _filledText;

	// === Godot Methods ===
	
	public EmailDisplay()
	{
		//runs when the object is created in memory, Parent -> Child
	}
	
	public override void _Ready()
	{
		//runs when the object and its children are ready, Child -> Parent
		_bgText = GetNode<RichTextLabel>("BGText");
		_filledText = GetNode<RichTextLabel>("FilledText");
		SendButton = GetNode<Button>("SendButton");

		_rng = new RandomNumberGenerator();

        _readied = true;

		FinishedTypingEmail += () => { SendButton.Disabled = false;  };

	}

    public override void _UnhandledInput(InputEvent @event)
    {

        if(@event.IsActionPressed("type_spam") && Visible)
		{
			if(CharactersToCompleteEmail == -1 || CharactersToCompleteEmail == 0)
			{
                _filledText.VisibleCharacters += CharactersPerPress;
            }
			else
			{
				_filledText.VisibleRatio += (float)CharactersPerPress / CharactersToCompleteEmail;
			}

			if (_filledText.VisibleRatio >= 1 && !EmailTyped)
			{
				EmailTyped = true;
				EmitSignal(SignalName.FinishedTypingEmail);
			}
		}
    }
	
	// === Further Methods === (PascalCase, local variables camelCase)

	// returns a email string of minimum minCharCount length. might be slightly over. 
	public string GenerateEmail(int minCharCount, Vector2I smashLength)
	{
		string email = "";

		bool doJargon = true;

		while (email.Length < minCharCount || doJargon)
		{
			if (doJargon)
			{
				if(email.Length != 0) email += " ";
                email += JARGON.PickRandom();
			}
			else
			{
                email += " ";
                int len = _rng.RandiRange(smashLength.X, smashLength.Y);
				var smash = "";
				for (int i = 0; i < len; i++)
				{
                    smash += SMASH_POSSIBILITIES[_rng.RandiRange(0, SMASH_POSSIBILITIES.Length - 1)];
                }

				foreach (string badWord in BAD_WORDS)
				{
					smash.ReplaceN(badWord, "abcde");
				}
				
			}

			if(_rng.Randf() <= PUNCTUATION_CHANCE)
			{
				email += PUNCTUATION[_rng.RandiRange(0, PUNCTUATION.Length-1)];
			}
            


            doJargon = !doJargon;

		}

		char last = email[email.Length - 1];

        switch(last)
		{
			case '!':
			case '?':
			case '.':
				break;

			case ',':
                email = email.Substring(0, email.Length-1) + ".";
                break;

            default:
				email = email.Substring(0, email.Length) + ".";
				break;
		}

		return email;
	}

	public void SetEmail(int minCharCount)
	{
		if (!_readied)
		{
			Connect(SignalName.Ready, Callable.From(() => { SetEmail(minCharCount); }), (uint)ConnectFlags.OneShot);
			return;
		}

		string email = GenerateEmail(minCharCount, new Vector2I(5, 17));

		_bgText.Text = email;
		_filledText.Text = email;
		_filledText.VisibleCharacters = 0;
	}

}
