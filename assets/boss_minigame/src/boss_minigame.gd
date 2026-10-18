extends Node
class_name BossMinigame

# === Signals === (snake_case)

signal meter_changed(won: bool, value: float, max_value: float)
signal sequence_updated(sequence: Array, curr_step: int)
signal step_result(correct: bool)
signal sequence_completed
signal boss_speaks(symbol_idx: int, color: Color)
signal boss_finished
signal player_won

# === Enums === (PascalCase, members CONSTANT_CASE)
enum BossSequenceMode {
	RANDOM,
	LIMIT_CHANGE,
	LIMIT_INPUT_DIST,
	LIMIT_CHANGE_AND_INPUT
}

@export var sequence_mode : BossSequenceMode
@export var sequence_jump_weights : Dictionary[int, int] = {0:2, 1:1}

@export var talking_wheel: Node
@export var tone_wheel: Node

@export var meter_max: float = 100.0
@export var meter_win: float = 40.0
@export var meter_loss: float = 10.0

@export var sequence_lengths: Array[int] = [1,1,2,2,2]
@export var reveal_duration: float = 0.6
@export var reveal_gap: float = 0.15 



var meter: float = 0.0
var curr_sequence: Array[Dictionary] = []  # symbol : color
var curr_step: int = 0
var level := 0
var input_enabled := false
var finished := false
var _round_id := 0
var _jump_weight_bag: Array[int] = []
@onready var _rng = RandomNumberGenerator.new()

func _ready() -> void:
	tone_wheel.selected.connect(_on_tone_selected)
	_start_new_sequence()

func _start_new_sequence() -> void:
	level = max(level - 2, 0)
	curr_sequence.clear()
	_jump_weight_bag.clear()
	_generate_sequence()
	
func _generate_sequence() -> void:
	var curr_length: int = sequence_lengths[level % sequence_lengths.size()]
	print("level: ", level)
	print("length: ", curr_length)

	var to_add: int = curr_length - curr_sequence.size()
	print(to_add, " ", curr_length)
	for i in range(to_add):
		match sequence_mode:
			BossSequenceMode.LIMIT_CHANGE_AND_INPUT:
				curr_sequence.append(_get_fixed_symbol_color_dict(true, true))
			
			BossSequenceMode.LIMIT_CHANGE:
				curr_sequence.append(_get_fixed_symbol_color_dict(true, false))
			
			BossSequenceMode.LIMIT_INPUT_DIST:
				curr_sequence.append(_get_fixed_symbol_color_dict(false, true))
			
			BossSequenceMode.RANDOM, _:
				curr_sequence.append(_get_random_symbol_color_dict())
		
	curr_step = 0
	sequence_updated.emit(curr_sequence, curr_step)
	_boss_sequence()

# old system:
func _get_random_symbol_color_dict() -> Dictionary:
	var wheel_symbol: int = talking_wheel.slot_symbol_ids[randi() % talking_wheel.slot_symbol_ids.size()]
	var color_idx: int = randi() % tone_wheel.colors.size()
	return { "symbol_idx": wheel_symbol, "color_idx": color_idx }

func _get_fixed_symbol_color_dict(limit_change: bool, limit_dist: bool) -> Dictionary:
	# an empty sequence gets a random start, no matter what
	if curr_sequence.is_empty():
		return _get_random_symbol_color_dict()
	
	# otherwise, offset orientation (based on last input) and pick a random input
	# then, potentially discard part of the change
	
	var sym_wheel_size = talking_wheel.slot_symbol_ids.size()
	var color_size = tone_wheel.colors.size()
	var last_input = curr_sequence[-1]
	var last_orientation = talking_wheel.slot_symbol_ids.find(talking_wheel.get_symbol_at_tone(0))
	
	# decide how many "steps" we can take. this is number of rotation inputs
	var steps = _get_random_jump_weight() if limit_dist else _rng.randi_range(0, ceili(sym_wheel_size / 2.0))
	# account for both directions..
	steps = steps if _rng.randf() > .5 else -steps
	
	# now, we need to get a input in a new orientation steps away
	var color_idx : int = _rng.randi_range(0, color_size - 1)
	var wheel_symbol : int = talking_wheel.slot_symbol_ids[(last_orientation + steps + color_idx) % sym_wheel_size]

	
	# and if we limit change, we might need to discard part of it if they both change, 
	# then fix the other half to the new orientation
	if limit_change and steps != 0:
		if last_input["symbol_idx"] != wheel_symbol and last_input["color_idx"] != color_idx:
			if _rng.randf() > .5:
				wheel_symbol = last_input["symbol_idx"]
				color_idx = (last_input["color_idx"] + steps) % color_size
			else :
				color_idx = last_input["color_idx"]
				var prev_sym_ind = talking_wheel.slot_symbol_ids.find(last_input["symbol_idx"])
				wheel_symbol = talking_wheel.slot_symbol_ids[(prev_sym_ind + steps) % sym_wheel_size]
	
	return { "symbol_idx": wheel_symbol, "color_idx": color_idx }

func _get_random_jump_weight() -> int:
	# if bag is empty fill and shuffle
	if _jump_weight_bag.is_empty():
		for k in range(len(sequence_jump_weights.keys())):
			for v in range(sequence_jump_weights[k]):
				_jump_weight_bag.append(k)
		_jump_weight_bag.shuffle()
	return _jump_weight_bag.pop_front()

func _boss_sequence() -> void:
	var my_id := _round_id
	input_enabled = false
	talking_wheel.set_locked(true)
	tone_wheel.set_locked(true)

	for item in curr_sequence:
		if finished: return
		var color: Color = tone_wheel.colors[item.color_idx]
		boss_speaks.emit(item.symbol_idx, color)
		await get_tree().create_timer(reveal_duration).timeout
		await get_tree().create_timer(reveal_gap).timeout

	if finished or my_id != _round_id: return
	input_enabled = true
	talking_wheel.set_locked(false)
	tone_wheel.set_locked(false)
	boss_finished.emit()

func _on_tone_selected(position: int) -> void:
	if finished or not input_enabled:
		return
	var selected_symbol: int = talking_wheel.get_symbol_at_tone(position)
	var expected: Dictionary = curr_sequence[curr_step]
	var correct: bool = selected_symbol == expected.symbol_idx and position == expected.color_idx

	step_result.emit(correct)

	if correct:
		_on_correct_step()
	else:
		_on_mistake()

func _on_correct_step() -> void:
	curr_step += 1
	if curr_step >= curr_sequence.size():
		meter = min(meter + meter_win, meter_max)
		meter_changed.emit(true, meter, meter_max)
		sequence_completed.emit()
		if meter >= meter_max:
			_win()
			return
		level += 1
		_generate_sequence()
	else:
		sequence_updated.emit(curr_sequence, curr_step)

func _on_mistake() -> void:
	print("mistake")
	meter = max(meter - meter_loss, 0.0)
	meter_changed.emit(false, meter, meter_max)
	sequence_completed.emit()
	_start_new_sequence()
	
func _win() -> void:
	get_node("LossScreenLoader").load_scene();


func _on_again_button_pressed() -> void:
	_round_id += 1
	finished = false
	input_enabled = false
	meter = 0.0
	level = 0
	curr_step = 0
	curr_sequence.clear()
	%EndLayer.hide()
	meter_changed.emit(false, meter, meter_max)
	_start_new_sequence()
