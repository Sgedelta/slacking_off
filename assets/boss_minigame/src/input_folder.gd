class_name InputFolder
extends Node2D

signal on_opened;
signal folder_selected;
signal on_closed;
signal option_selected(selected_index);
 
@export var input_option_array : Array[Area2D] = []
@export var open_time = .5
@export var select_time = .5
@export var opened_start = Vector2(300, 0)
@export var opened_offset = Vector2(200, 0)
@export var selected_offset = Vector2(0, -300)
@export var tween_ease = Tween.EaseType.EASE_IN_OUT
@export var tween_trans = Tween.TransitionType.TRANS_SINE

@onready var folder_area = $FolderArea
@onready var folder_symbol = $FolderArea/SymbolDisplay

var input_display_array : Array[Sprite2D] = []
var active = false #used to allow/disallow input
var is_open = false #used to track open/close status
var selected_index = -1 #used to track which one is selected. -1 is none

var folder_tween : Tween;
var selecting_tween : Tween;



# Called when the node enters the scene tree for the first time.
func _ready() -> void:
	for i in range(1, len(input_option_array) + 1):
		input_display_array.append(get_node(str("InputOption", i, "/InputDisplay", i)))
		input_option_array[i-1].mouse_entered.connect(func():select_input(i-1))
		input_option_array[i-1].mouse_exited.connect(func():if selected_index == i-1: select_input(-1))
	


func set_open(status : bool) -> void:
	if status == is_open:
		return;
	
	is_open = status;
	
	if is_open:
		_open()
	else:
		_close()

func _open() -> void:
	var time_left = open_time
	# kill tween if it is running
	if is_instance_valid(folder_tween):
		if folder_tween.is_running():
			time_left = min(folder_tween.get_total_elapsed_time(), time_left)
		folder_tween.kill()
	# make a new tween, each part will run in parl
	folder_tween = create_tween()
	folder_tween.set_ease(tween_ease).set_trans(tween_trans)
	folder_tween.tween_callback(func(): active = false)
	for i in range(len(input_option_array)):
		folder_tween.parallel().tween_property(input_option_array[i], "position", opened_start + (opened_offset * i), time_left)

	# after it's open, allow input
	folder_tween.tween_callback(func(): active = true)
	folder_tween.tween_callback(func(): emit_signal("on_opened"))
	pass

func _close() -> void:
	var time_left = open_time
	# kill tween if it is running
	if is_instance_valid(folder_tween):
		if folder_tween.is_running():
			time_left = min(folder_tween.get_total_elapsed_time(), time_left)
		folder_tween.kill()
	
	# disallow input. then if we have any selected, deselect
	active = false
	await select_input(-1)
	
	# make a new tween and close
	folder_tween = create_tween()
	folder_tween.set_ease(tween_ease).set_trans(tween_trans)
	for i in range(len(input_option_array)):
		folder_tween.parallel().tween_property(input_option_array[i], "position", Vector2.ZERO, time_left)
	folder_tween.tween_callback(func(): emit_signal("on_closed"))


func select_input(i : int) -> void:
	if !active and i != -1:
		return
	# no change needed
	if i == selected_index:
		return
	
	# fix index
	var old_index = selected_index;
	selected_index = i;
	
	# kill other tween if it's running
	var time_left = select_time
	if is_instance_valid(selecting_tween):
		if selecting_tween.is_running():
			time_left = min(selecting_tween.get_total_elapsed_time(), time_left)
		selecting_tween.kill()
	
	# make 2 new tweens, one we store, one we don't care about
	var unselect_tween : Tween
	if old_index != -1:
		unselect_tween = create_tween()
		unselect_tween.set_ease(tween_ease).set_trans(tween_trans)
		unselect_tween.tween_property(input_display_array[old_index], "position", Vector2.ZERO, time_left)
	if i != -1:
		selecting_tween = create_tween()
		selecting_tween.set_ease(tween_ease).set_trans(tween_trans)
		selecting_tween.tween_property(input_display_array[i], "position", selected_offset, select_time)
		await selecting_tween.finished
	if i == -1 and is_instance_valid(unselect_tween) and unselect_tween.is_running():
		await unselect_tween.finished
	
	return

func process_input(vp : Node, event : InputEvent, indx : int, option : int) :
	if event.is_action_released("select_conv_option"):
		emit_signal("option_selected", option)

func folder_click(vp: Node, event : InputEvent, indx : int) :
	if event.is_action_pressed("select_boss_folder"):
		emit_signal("folder_selected")

func set_symb(tex : Texture2D) -> void:
	folder_symbol.texture = tex;
