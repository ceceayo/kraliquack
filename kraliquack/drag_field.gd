extends Control

var dragging := false
var last_mouse_position := Vector2.ZERO


@onready var tml = $"../../TileMapLayer"
@onready var entities = $"../../entities"

func _gui_input(event: InputEvent) -> void:
	if event is InputEventMouseButton:
		
		if event.button_index == MOUSE_BUTTON_LEFT:
			if event.pressed:
				dragging = true
				last_mouse_position = event.position
			else:
				dragging = false
				print(event.position)
				$"../.."._handle_click(event.position)
	
	elif event is InputEventMouseMotion and dragging:
		var delta = event.position - last_mouse_position
		last_mouse_position = event.position
		tml.position += delta
		entities.position += delta
