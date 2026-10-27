extends  Node

# === Signals === (snake_case)
signal game_over(won : bool)

# === Enums === (PascalCase, members CONSTANT_CASE)

# === Constants === (CONSTANT_CASE)

# === Exported Vars === (snake_case)
@export var win_amt = .1;
@export var loss_amt = .05;
@export var symbols : Array[Texture2D] = []
@export var colors : Array[Color] = []
@export_enum("RANDOM", "BAGGED", "VARY_ONE" ) var _pick_type : int
@export var bag_min_size = 8; # if bag has less than this in it left, it will reset

# === Public Vars === (snake_case)

# === Private Vars === (_underscored_snake_case)

@onready var _progress_disp = $ProgressDisplay
@onready var _input = $InputVP/BossInput
@onready var _color_symbol_bag : Array = []


# === Godot Methods ===

func  _ready() -> void:
	pass

func  _physics_process(delta: float) -> void:
	pass

# === Further Methods === (public snake_case, private _underscored_snake_case, local vars snake_case)


func _get_symbol_color_pair(prev : Array = []) -> Array: # Symbol, Color
	var tex : Texture2D;
	var col : Color;
	match _pick_type:
		2:
			# if we can't do this, fallback to random
			if not prev or len(prev) < 2:
				tex = symbols.pick_random()
				col = colors.pick_random()
				pass
			if randf() > .5:
				# change color
				tex = prev[0]
				col = colors.pick_random()
			else :
				# change symbol
				tex = symbols.pick_random()
				col = prev[1]
		1:
			if len(_color_symbol_bag) < max(bag_min_size, 1):
				_color_symbol_bag.clear()
				for sym_i in len(symbols):
					for col_i in len(colors):
						_color_symbol_bag.append([symbols[sym_i], colors[col_i]])
				_color_symbol_bag.shuffle()
			var pick_i = randi_range(0, len(_color_symbol_bag)-1)
			tex = _color_symbol_bag[pick_i][0]
			col = _color_symbol_bag[pick_i][1]
			_color_symbol_bag.remove_at(pick_i)
		0, _:
			tex = symbols.pick_random()
			col = colors.pick_random()
	return [tex, col]
