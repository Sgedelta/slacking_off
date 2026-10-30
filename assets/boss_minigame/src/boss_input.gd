extends Node2D

signal selected(symbol_index : int, color_index : int)

@export var _folder_scene : PackedScene; 
@export var _folder_start = Vector2(100, 100)
@export var _folder_offset = Vector2(0, 250)

@onready var colors_to_index : Dictionary [int, Color] = {}

@onready var _folder_root = $FolderRootOffset

var opened_folder : InputFolder;

# Called when the node enters the scene tree for the first time.
func _ready() -> void:
	pass

func open_folder(folder : InputFolder):
	
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
				folder.set_colors(cols)
				folder.connect("folder_selected", func():
					open_folder(folder))
				folder.connect("option_selected", func(col_i):
					emit_signal("selected", i, col_i) )
				)
		folder.position = _folder_start + (_folder_offset * i)
		_folder_root.add_child(folder)
	
	pass
