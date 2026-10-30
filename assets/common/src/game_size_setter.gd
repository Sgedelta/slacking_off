extends  Node

@export_group("Size Setting Enabled")
@export_custom(PROPERTY_HINT_GROUP_ENABLE, "") var enabled = false;
@export var size := Vector2i(1920, 1080);

func  _ready() -> void:
	if enabled:
		update_vp_size(size);


# === Further Methods === (public snake_case, private _underscored_snake_case, local vars snake_case)

# update the size to the given new size. Rescale the window to fit as well
func update_vp_size(new_size: Vector2i) :
	# grab sizes
	var old_content_size = get_window().content_scale_size
	

	
	# calc scale factor (un-normalized)
	var scale_factor = (new_size as Vector2) / (old_content_size as Vector2)
	# normalize it, then scale it so y dimension is 1. This keeps y scale at 1
	scale_factor = scale_factor.normalized()
	scale_factor = scale_factor / scale_factor.y
	
	print(scale_factor)
	
	# set the content size
	get_window().content_scale_size = new_size
	
	# set the window size, scaling so the Y is constant
	get_window().size = Vector2i((get_window().size as Vector2) * scale_factor)
	
	pass
