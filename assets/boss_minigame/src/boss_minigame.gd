extends  Node

# === Signals === (snake_case)
signal game_over(won : bool)
signal round_won()


# === Enums === (PascalCase, members CONSTANT_CASE)

# === Constants === (CONSTANT_CASE)

# === Exported Vars === (snake_case)
@export var sequence_len_by_level = [1, 2, 2, 5, 5, 8]
@export var win_amt = .1;
@export var loss_amt = .05;
@export var progress_update_time = .25;
@export var symbols : Array[Texture2D] = []
@export var colors : Array[Color] = []
@export_enum("RANDOM", "BAGGED", "VARY_ONE" ) var _pick_type : int
@export var bag_min_size = 8; # if bag has less than this in it left, it will reset
@export var vary_one_allow_repeats = true;

# === Public Vars === (snake_case)

var current_sequence : Array[Array] = [] # sub arrays are symbol (texture), color, done (bool)

# === Private Vars === (_underscored_snake_case)

@onready var _progress_disp = $ProgressContainer/ProgressDisplay
@onready var _progress_lead = $ProgressContainer/ProgressChangeDisplay
@onready var _input = $InputVP/BossInput
@onready var _display = $DisplayVP/BossDisplay
@onready var _input_timer = $InputTimer
@onready var _input_timer_disp = $ProgressContainer/InputTimeContainer/InputTimeBar
@onready var _color_symbol_bag : Array = []
var _curr_lvl = 0
var _progress = 0;
var _progress_anim : Tween;
var _timer_anim : Tween;

# === Godot Methods ===

func  _ready() -> void:
	_input.make_folders(symbols, colors)
	_input.connect("selected", handle_player_input)
	_input_timer.connect("timeout", func():
		start_round(_curr_lvl - 1)
		set_progress(_progress - loss_amt))
	pass

# === Further Methods === (public snake_case, private _underscored_snake_case, local vars snake_case)

## starts a new round of the game at the given difficulty level
func start_round(lvl : int):
	# fix level to be in bounds
	lvl = clampi(lvl, 0, len(sequence_len_by_level)-1)
	_curr_lvl = lvl
	
	create_new_sequence(sequence_len_by_level[lvl])
	_display.display_symbols(current_sequence)
	_input_timer.start()
	if is_instance_valid(_timer_anim) and _timer_anim.is_running():
		_timer_anim.kill();
	_timer_anim = create_tween()
	_timer_anim.tween_property(_input_timer_disp, "value", 0, _input_timer.wait_time).from(1)

func stop_game(gameWon : bool, emit = true):
	_display.clear_symbols() # clear visuals
	current_sequence.clear() # clear internal
	_input_timer.stop() # stop input timer
	_progress = 0;
	_progress_disp.value = 0;
	_progress_lead.value = 0;
	# kill anims
	if is_instance_valid(_timer_anim) and _timer_anim.is_running(): 
		_timer_anim.kill();
	# close
	
	if(emit):
		emit_signal("game_over", gameWon)
	

func handle_player_input(sym_i : int, col_i : int):
	var tex = symbols[sym_i]
	var col = colors[col_i]
	var submit_index =  try_submit_pair(tex, col)
	if submit_index != -1:
		_display.mark_symbol_complete(submit_index)
		if is_sequence_complete(current_sequence):
			emit_signal("round_won")
			set_progress( _progress + win_amt)
			if( _progress < 1):
				start_round(_curr_lvl + 1)
			else:
				emit_signal("game_over", true)
	else: 
		# TODO: replace with "error" and input freeze
		pass
	pass


func create_new_sequence(length : int) -> void:
	current_sequence.clear()
	
	for i in range(length):
		var new_pair = (
			_get_symbol_color_pair(current_sequence[i-1]) 
			if len(current_sequence) >= 1 else 
			_get_symbol_color_pair() )
		# mark uncompleted and add to seq
		new_pair.append(false)
		current_sequence.append(new_pair) 


## returns if the the sequence passed is complete (if for all subarrays the third element is not false)
## this means that empty seq are true, as 0 subarrays have a not-false 3rd element
## if any subarray does not have a third element (len < 3) returns false 
func is_sequence_complete(seq : Array[Array]) -> bool:
	for subarray_i in range(len(seq)):
		if len(seq[subarray_i]) < 3:
			return false
		if seq[subarray_i][2] == false:
			return false
	return true 

func try_submit_pair(sym : Texture2D, col : Color) -> int:
	# find first false entry
	var i = current_sequence.find_custom(func(pair): return !pair[2] and pair_match(pair, [sym, col]))
	if i == -1:
		return -1
	current_sequence[i][2] = true
	return i

func _get_symbol_color_pair(prev : Array = []) -> Array: # Symbol, Color
	var tex : Texture2D;
	var col : Color;
	match _pick_type:
		2:
			# if we can't do this, fallback to random
			if (not prev) or len(prev) < 2:
				tex = symbols.pick_random()
				col = colors.pick_random()
			elif vary_one_allow_repeats:
				if randf() > .5:
					# change color
					tex = prev[0]
					col = colors.pick_random()
				else :
					# change symbol
					tex = symbols.pick_random()
					col = prev[1]
			else:
				var options = _color_symbol_bag.filter(func(pair):
						return pair_match(pair, prev, false))
				if len(options) == 0:
					fill_bag()
					options = _color_symbol_bag.filter(func(pair):
						return pair_match(pair, prev, false))
					if len(options) == 0:
						push_error("bag fill did not fix no matching option")
						return [] #should be unreachable..
				var pick = options.pick_random()
				_color_symbol_bag.remove_at(_color_symbol_bag.find(pick))
				tex = pick[0]
				col = pick[1]
		1:
			if len(_color_symbol_bag) < max(bag_min_size, 1):
				fill_bag()
			var pick_i = randi_range(0, len(_color_symbol_bag)-1)
			tex = _color_symbol_bag[pick_i][0]
			col = _color_symbol_bag[pick_i][1]
			_color_symbol_bag.remove_at(pick_i)
		0, _:
			tex = symbols.pick_random()
			col = colors.pick_random()
	return [tex, col]

func pair_match(a : Array, b : Array, strict : bool = true) -> bool:
	if strict:
		return (a[0] == b[0]) and (a[1] == b[1])
	else:
		return (a[0] == b[0]) or (a[1] == b[1])

func fill_bag():
	_color_symbol_bag.clear()
	for sym_i in len(symbols):
		for col_i in len(colors):
			_color_symbol_bag.append([symbols[sym_i], colors[col_i]])
	_color_symbol_bag.shuffle()

func set_progress(new : float):
	if _progress == new: return;
	var more = new > _progress
	_progress = new
	_progress_disp.self_modulate = _progress_disp.get_theme_color("success_tint") if more else _progress_disp.get_theme_color("fail_tint")
	_progress_lead.value = new
	
	if is_instance_valid(_progress_anim) and _progress_anim.is_running():
		_progress_anim.kill()
	
	_progress_anim = create_tween()
	_progress_anim.tween_property(_progress_disp, "value", _progress, progress_update_time)
	_progress_anim.tween_property(_progress_disp, "self_modulate", Color(1,1,1,1), progress_update_time)
