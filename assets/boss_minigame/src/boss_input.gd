extends Node2D

signal selected(symbol_index : int, color_index : int)

@export var _folder_scene : PackedScene; 
@export var _folder_start = Vector2(100, 100)
@export var _folder_offset = Vector2(0, 250)
@export_range(0.0, 1.0, .01, "prefer_slider") var _page_tint_amount = 1.0;
var folders : Array[InputFolder] = []


@onready var colors_to_index : Dictionary [int, Color] = {}

@onready var _folder_root = $FolderRootOffset

var input_allowed = true
var freeze_tween : Tween

var opened_folder : InputFolder;

# Called when the node enters the scene tree for the first time.
func _ready() -> void:
	pass

func open_folder(folder : InputFolder):
	# if we misinput don't allow opening
	if is_instance_valid(freeze_tween) and freeze_tween.is_running():
		return;
	
	if opened_folder == folder and folder != null:
		folder.set_open(false)
		opened_folder = null
		return
	
	var prev = opened_folder
	opened_folder = folder
	
	if is_instance_valid(prev):
		prev.set_open(false)
		#@warning_ignore("standalone_expression")
		#await prev.on_closed or prev.on_opened
	
	if opened_folder:
		opened_folder.set_open(true)

func make_folders(symbs : Array[Texture2D], cols : Array[Color]):
	for i in range(len(symbs)):
		var folder = (_folder_scene.instantiate() as InputFolder)
		folder.connect("ready", func():
				folder.set_symb(symbs[i])
				folder.set_colors(cols, _page_tint_amount)
				folder.connect("folder_selected", func():
					open_folder(folder))
				folder.connect("option_selected", func(col_i):
					emit_signal("selected", i, col_i) )
				)
		folder.position = _folder_start + (_folder_offset * i)
		_folder_root.add_child(folder)
		folders.append(folder)
	
	pass

func freeze_input(shake_dur : float, shake_amt : float, duration : float) :
	
	if is_instance_valid(freeze_tween):
		freeze_tween.kill()
	
	var open_folder
	for f in folders: 
		if f.is_open:
			f.active = false
			open_folder = f;
	freeze_tween = create_tween()
	freeze_tween.tween_callback(Callable(func(): 
		open_folder.do_error_shake(shake_dur, shake_amt)
		))
	freeze_tween.tween_interval(shake_dur)
	# wait one frame to allow tween to end so we can deselect
	freeze_tween.tween_await(get_tree().process_frame) 
	freeze_tween.tween_callback(Callable(func(): 
		open_folder.select_input(-1)
		))
	freeze_tween.tween_interval(duration)
	freeze_tween.tween_callback(Callable(func(): 
		for f in folders: 

			if f == open_folder:
				f.active = true
				f.check_reselect()
		))
