extends Node

@export_category("Node References")
@export var path := Path3D
@export var follower := PathFollow3D
@export var offset_node := Node3D
@export var remote := RemoteTransform3D
@export_category("Offset Curves")
@export_group("Offset By Ratio")
@export_custom(PROPERTY_HINT_GROUP_ENABLE, "") var ratio_curves := true
@export var offset_r_x_pos := Curve.new()
@export var offset_r_y_pos := Curve.new()
@export var offset_r_z_pos := Curve.new()
@export var offset_r_x_rot := Curve.new()
@export var offset_r_y_rot := Curve.new()
@export var offset_r_z_rot := Curve.new()
@export_group("Offset By Progress")
@export_custom(PROPERTY_HINT_GROUP_ENABLE, "") var progress_curves := false
@export var offset_p_x_pos := Curve.new()
@export var offset_p_y_pos := Curve.new()
@export var offset_p_z_pos := Curve.new()
@export var offset_p_x_rot := Curve.new()
@export var offset_p_y_rot := Curve.new()
@export var offset_p_z_rot := Curve.new()



func  _ready() -> void:
	# asserts
	assert(is_instance_valid(path), "path of camera_path %s not found!" % name)
	assert(is_instance_valid(follower), "follower of camera_path %s not found!" % name)
	assert(is_instance_valid(offset_node), "offset_node of camera_path %s not found!" % name)
	assert(is_instance_valid(remote), "remote of camera_path %s not found!" % name)
	# no need to assert curves as they are optional
	pass

# === Further Methods === (public snake_case, private _underscored_snake_case, local vars snake_case)
func get_node_refs() -> Array:
	return [path, follower, offset_node, remote];

func get_position_offset_at_ratio(ratio:float) -> Vector3:
	var pos = Vector3()
	var offset = path.curve.get_baked_length() * ratio
	if(ratio_curves):
		if(offset_r_x_pos):
			pos.x = offset_r_x_pos.sample(ratio)
		if(offset_r_y_pos):
			pos.y = offset_r_y_pos.sample(ratio)
		if(offset_r_z_pos):
			pos.z = offset_r_z_pos.sample(ratio)
	elif(progress_curves):
		if(offset_p_x_pos):
			pos.x = offset_r_x_pos.sample(offset)
		if(offset_p_y_pos):
			pos.y = offset_r_y_pos.sample(offset)
		if(offset_p_z_pos):
			pos.z = offset_r_z_pos.sample(offset)
		
	return pos

func get_position_offset_at_progress(offset:float) -> Vector3:
	var pos = Vector3()
	var ratio = offset / path.curve.get_baked_length()
	if(ratio_curves):
		if(offset_r_x_pos):
			pos.x = offset_r_x_pos.sample(ratio)
		if(offset_r_y_pos):
			pos.y = offset_r_y_pos.sample(ratio)
		if(offset_r_z_pos):
			pos.z = offset_r_z_pos.sample(ratio)
	elif(progress_curves):
		if(offset_p_x_pos):
			pos.x = offset_r_x_pos.sample(offset)
		if(offset_p_y_pos):
			pos.y = offset_r_y_pos.sample(offset)
		if(offset_p_z_pos):
			pos.x = offset_r_z_pos.sample(offset)
		
	return pos

func get_rotation_offset_at_ratio(ratio:float) -> Vector3:
	var rot = Vector3()
	var offset = path.curve.get_baked_length() * ratio
	if(ratio_curves):
		if(offset_r_x_rot):
			rot.x = offset_r_x_rot.sample(ratio)
		if(offset_r_y_rot):
			rot.y = offset_r_y_rot.sample(ratio)
		if(offset_r_z_rot):
			rot.z = offset_r_z_rot.sample(ratio)
	elif(progress_curves):
		if(offset_p_x_rot):
			rot.x = offset_r_x_rot.sample(offset)
		if(offset_p_y_rot):
			rot.y = offset_r_y_rot.sample(offset)
		if(offset_p_z_rot):
			rot.z = offset_r_z_rot.sample(offset)
		
	return rot

func get_rotation_offset_at_progress(offset:float) -> Vector3:
	var rot = Vector3()
	var ratio = offset / path.curve.get_baked_length()
	if(ratio_curves):
		if(offset_r_x_rot):
			rot.x = offset_r_x_rot.sample(ratio)
		if(offset_r_y_rot):
			rot.y = offset_r_y_rot.sample(ratio)
		if(offset_r_z_rot):
			rot.z = offset_r_z_rot.sample(ratio)
	elif(progress_curves):
		if(offset_p_x_rot):
			rot.x = offset_r_x_rot.sample(offset)
		if(offset_p_y_rot):
			rot.y = offset_r_y_rot.sample(offset)
		if(offset_p_z_rot):
			rot.z = offset_r_z_rot.sample(offset)
		
	return rot
