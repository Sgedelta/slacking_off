extends Node2D

signal selected(symbol_index : int, color_index : int)

@export var folder_to_symb : Dictionary[InputFolder, Texture2D] = {}

var opened_folder : InputFolder;

# Called when the node enters the scene tree for the first time.
func _ready() -> void:
	for i in range(len(folder_to_symb.keys())):
		folder_to_symb.keys()[i].set_symb(
			folder_to_symb[folder_to_symb.keys()[i]])
		folder_to_symb.keys()[i].connect("folder_selected", func():
			open_folder(folder_to_symb.keys()[i])
			)
		
	pass # Replace with function body.

func open_folder(folder : InputFolder):
	
	if opened_folder == folder:
		folder.set_open(false)
		opened_folder = null
		return
	
	var prev = opened_folder
	opened_folder = folder
	
	if is_instance_valid(prev):
		prev.set_open(false)
		await prev.on_closed or prev.on_opened
	
	if opened_folder:
		opened_folder.set_open(true)
