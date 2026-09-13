extends Node
class_name BossMinigame

signal meter_changed(won: bool, value: float, max_value: float)
signal sequence_updated(sequence: Array, curr_step: int)
signal step_result(correct: bool)
signal sequence_completed
signal boss_speaks(symbol_idx: int, color: Color)
signal boss_finished

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

func _ready() -> void:
	tone_wheel.selected.connect(_on_tone_selected)
	_start_new_sequence()

func _start_new_sequence() -> void:
	level = max(level - 2, 0)
	curr_sequence.clear()
	_generate_sequence()
	
func _generate_sequence() -> void:
	var curr_length: int = sequence_lengths[level % sequence_lengths.size()]
	print("level: ", level)
	print("length: ", curr_length)

	var to_add: int = curr_length - curr_sequence.size()
	for i in range(to_add):
		var wheel_symbol: int = talking_wheel.slot_symbol_ids[randi() % talking_wheel.slot_symbol_ids.size()]
		curr_sequence.append({
			"symbol_idx": wheel_symbol,
			"color_idx": randi() % tone_wheel.colors.size(),
		})
		
	curr_step = 0
	sequence_updated.emit(curr_sequence, curr_step)
	_boss_sequence()
	
func _boss_sequence() -> void:
	input_enabled = false
	talking_wheel.set_locked(true)
	tone_wheel.set_locked(true)

	for item in curr_sequence:
		var color: Color = tone_wheel.colors[item.color_idx]
		boss_speaks.emit(item.symbol_idx, color)
		await get_tree().create_timer(reveal_duration).timeout
		await get_tree().create_timer(reveal_gap).timeout

	input_enabled = true
	talking_wheel.set_locked(false)
	tone_wheel.set_locked(false)
	boss_finished.emit()

func _on_tone_selected(position: int) -> void:
	if not input_enabled:
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
	print("correct")
	curr_step += 1
	if curr_step >= curr_sequence.size():
		meter = min(meter + meter_win, meter_max)
		meter_changed.emit(true, meter, meter_max)
		if meter >= meter_max:
			print("win")
		sequence_completed.emit()
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
