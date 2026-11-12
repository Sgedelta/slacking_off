extends Node

@export var active : bool = false;
@export var viewport : SubViewport;
@export var node_quad : MeshInstance2D;
@export var active_area: Area2D;

# The controller node of the scene that this is input forwarding to.
# Used to send notifications down the tree from otherwise unnoticable events, like mouse enter/exit
@export var subscene_controller: Node; 

var _is_mouse_inside:bool = false;
var last_event_pos2D:=Vector2();
var last_event_time := -1.0;

func _ready() -> void:
	active_area.mouse_entered.connect(_mouse_entered_area);
	active_area.mouse_exited.connect(_mouse_exited_area);
	active_area.input_event.connect(_mouse_input_event);

func _mouse_entered_area() -> void:
	_is_mouse_inside = true;
	viewport.notification(NOTIFICATION_VP_MOUSE_ENTER);
	if(subscene_controller): 
		subscene_controller.notification(NOTIFICATION_VP_MOUSE_ENTER);

func _mouse_exited_area() -> void:
	viewport.notification(NOTIFICATION_VP_MOUSE_EXIT);
	if(subscene_controller): 
		subscene_controller.notification(NOTIFICATION_VP_MOUSE_EXIT);
	_is_mouse_inside = false;

func _unhandled_input(input_event: InputEvent) -> void:
	if !active:
		return;
	
	for mouse_event in [InputEventMouseButton, InputEventMouseMotion, InputEventScreenDrag, InputEventScreenTouch]:
		if is_instance_of(input_event, mouse_event):
			return; #ignore mouse events, they are pushed via physics picking
	print("pushing input")
	viewport.push_input(input_event);

func _mouse_input_event(_vp : Node, input_event : InputEvent, shape_idx : int) -> void:
	if !active:
		return;
	
	# ignore non-mouse events
	var type = InputEvent;
	for mouse_event in [InputEventMouseButton, InputEventMouseMotion, InputEventScreenDrag, InputEventScreenTouch]:
		if is_instance_of(input_event, mouse_event):
			type = mouse_event;
			break;
	if type == InputEvent:
		return;
	
	var quad_mesh_size:Vector2 = node_quad.mesh.size;
	var now := Time.get_ticks_msec() / 1000.0;
	
	
	#get position of event in 3D space, using affine inv to account for pos/rot/scale
	var event_pos2D := node_quad.to_local(input_event.global_position) 
	
	if _is_mouse_inside:
		#convert quad size event to normalized
		event_pos2D = event_pos2D / quad_mesh_size;
		#convert to viewport size
		event_pos2D *= viewport.size as Vector2;
		#flip Y
		event_pos2D *= Vector2(1, -1) 
	elif last_event_pos2D != null:
		#fallback
		event_pos2D = last_event_pos2D;
	
	# fix positions 
	input_event.position = event_pos2D;
	if input_event is InputEventMouse:
		input_event.global_position = event_pos2D;
	
	#relative event distance
	if input_event is InputEventMouseMotion or input_event is InputEventScreenDrag:
		if last_event_pos2D == null:
			input_event.relative = Vector2(0, 0);
		else:
			input_event.relative = event_pos2D - last_event_pos2D;
			input_event.velocity = input_event.relative / (now - last_event_time);
	
	last_event_pos2D = event_pos2D;
	last_event_time = now;
	viewport.push_input(input_event);
