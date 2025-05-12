extends Node

var LogInScene = load("res://LogIn.tscn")

var SoldierScene = load("res://soldier.tscn")
var DuckScene = load("res://entities/duck.tscn")
var BugScene = load("res://entities/bug.tscn")
var DeadScene = load("res://dead.tscn")


var width = 75
var height = 75

@onready var tml = $TileMapLayer

var dragging := false
var last_mouse_position := Vector2.ZERO



var tween: Tween

var progressionState = ""

var owners = {}

var power_consumption = 0
var power_generation = 0


#func _ready():
	#tween =

#	for x in range(width):
#		for y in range(height):
#			$TileMapLayer.set_cell(Vector2i(x, y), 0, to_tile_atlas("."))


func _handle_click(mouse_pos):
	var local_mouse_pos = tml.to_local(mouse_pos)
	var cell = tml.local_to_map(local_mouse_pos)
	var tile_id = tml.get_cell_atlas_coords(cell)
	print("Clicked at ",cell," with ID ",tile_id)
	
	#selection = Vector2(cell)
	if tween:
		tween.kill()
	tween =  get_tree().create_tween()
	tween.tween_property(Globals, "selection", Vector2(cell), 0.07*((Vector2(cell)-Globals.selection).length()))
	
	$GUI/Panel2/VBoxContainer/SelectionLabel.text = str(cell)
	show_owner_of_selection()
	

	#if tile_id != TileMap.INVALID_CELL:
	#	print("Clicked on tile at: ", cell, " Tile ID: ", tile_id)
	#else:
	#	print("Clicked on empty space.")


func _process(delta):
	Globals.ws.poll()
	
	var state = Globals.ws.get_ready_state()
	
	if state == WebSocketPeer.STATE_OPEN:
		while Globals.ws.get_available_packet_count():
			var data = JSON.parse_string(Globals.ws.get_packet().get_string_from_utf8())
			#print(data)
			match (data['MsgType']):
				'test':
					print("Server Test Data: "+data['Message'])
				'board':
					#print("board")
					for mes in data['Message'].split(':'):
						if mes == "":
							continue
						var pos = int(mes.split(',')[0])
						var dat = mes.split(',')[1]
						var y: int = pos / width
						var x: int = pos % width
						#print(x, " ", y, " ", pos)
						for c in dat:
							tml.set_cell(Vector2i(x, y), 0, to_tile_atlas(c))
							Globals.astar.set_point_solid(Vector2i(x,y), c in ["#", "r"])
							pos += 1
							y = pos / width
							x = pos % width
						#print(x, " ", y, " ", pos)
				'owner':
					for mes in data['Message'].split(':'):
						if mes == "":
							continue
						var x = mes.split(",")
						owners[x[0]] = x[1]
						show_owner_of_selection()
				'chat':
					$GUI/Chat.text = data['Message'] + '\n' + $GUI/Chat.text
				'cash':
					$GUI/Panel2/VBoxContainer/Cash.text = data['Message']
				'progressionState':
					progressionState = data['Message']
					match(progressionState):
						'joined':
							$GUI/Panel/ScrollContainer/FlowContainer/HeadquartersDuck.show()
							$GUI/Panel/ScrollContainer/FlowContainer/HeadquartersBug.show()
							$GUI/Panel/ScrollContainer/FlowContainer/Powerplant.hide()
							$GUI/Panel/ScrollContainer/FlowContainer/Barracks.hide()
							$GUI/Panel/ScrollContainer/FlowContainer/Mine.hide()
							$GUI/Panel/ScrollContainer/FlowContainer/Bug.hide()
							$GUI/Panel/ScrollContainer/FlowContainer/Duck.hide()
							$GUI/Panel/ScrollContainer/FlowContainer/Factory.hide()
							
						'placedHQ':
							$GUI/Panel/ScrollContainer/FlowContainer/HeadquartersDuck.hide()
							$GUI/Panel/ScrollContainer/FlowContainer/HeadquartersBug.hide()
							$GUI/Panel/ScrollContainer/FlowContainer/Powerplant.show()
							$GUI/Panel/ScrollContainer/FlowContainer/Barracks.show()
							$GUI/Panel/ScrollContainer/FlowContainer/Mine.show()
						'placedBarracks':
							$GUI/Panel/ScrollContainer/FlowContainer/Factory.show()
							if Globals.team == 1:
								$GUI/Panel/ScrollContainer/FlowContainer/Duck.show()
							else:
								$GUI/Panel/ScrollContainer/FlowContainer/Bug.show()
						'destroyed':
							Globals.change_scene_to_node(DeadScene.instantiate())
							
				'power':
					var x = data['Message'].split(" ")
					power_generation = x[0]
					power_consumption = x[1]
					$GUI/Panel2/VBoxContainer/PowerLabel.text = str(int(power_consumption)) + " / " + str(int(power_generation))
				'entity':
					var x = data['Message'].split(",")
					if $entities.has_node(x[0]):
						$entities.get_node(x[0]).set_data(
							int(x[0]),
							float(x[1]),
							float(x[2]),
							str(x[4]),
							float(x[5]),
							float(x[6]),
							int(x[7])
						)
					else:
						match(str(x[3])):
							"duck":
								var entity = DuckScene.instantiate()
								entity.name = str(int(x[0]))
								$entities.add_child(entity)
								entity.set_data(
									int(x[0]),
									float(x[1]),
									float(x[2]),
									str(x[4]),
									float(x[5]),
									float(x[6]),
									int(x[7])
								)
							"bug":
								var entity = BugScene.instantiate()
								entity.name = str(int(x[0]))
								$entities.add_child(entity)
								entity.set_data(
									int(x[0]),
									float(x[1]),
									float(x[2]),
									str(x[4]),
									float(x[5]),
									float(x[6]),
									int(x[7])
								)
						
				'entityDestroyed':
					var e = int(data['Message'])
					if $entities.has_node(str(e)):
						var child = $entities.get_node(str(e))
						$entities.remove_child(child)
						child.queue_free()
				'entityDamage':
					var x = data["Message"].split(",")
					if $entities.has_node(str(int(x[0]))):
						$entities.get_node(str(int(x[0])))._damage(int(x[2]))
				'hqHealth':
					$GUI/Panel2/VBoxContainer/HQHealth.text = data['Message']
				_:
					print("UNK MsgType")
					print(data)

	else:
		print("Websocket not open!")
		Globals.change_scene_to_node(LogInScene.instantiate())
		
	$SelectionShower.global_position = tml.global_position + Vector2(Globals.selection * 64)

		


func to_tile_atlas(char: String):
	match char:
		'.':
			return Vector2i(0,0)
		'r':
			return Vector2i(1,0)
		'#':
			return Vector2i(2,0)
		'h':
			return Vector2i(3,0)
		'H':
			return Vector2i(4,0)
		'p':
			return Vector2i(5,0)
		'b':
			return Vector2i(6,0)
		'm':
			return Vector2i(7,0)
		'f':
			return Vector2i(0,1)

func _on_place_headquarters_pressed(team: int) -> void:
	Globals.team = team
	Globals.ws.send_text(JSON.stringify({
		"ActionName": "PlaceHeadQuarters",
		"Data": str(int(Globals.selection.x)) + " " + str(int(Globals.selection.y)) + " " + str(int(team))
	}))


func _on_chat_line_text_submitted(new_text: String) -> void:
	Globals.ws.send_text(JSON.stringify({
		"ActionName": "Chat",
		"Data": new_text
	}))
	$GUI/ChatLine.text = ""


func show_owner_of_selection():
	if Globals.selection.y <= 50 and Globals.selection.y >= 0 and Globals.selection.x <= 50 and Globals.selection.x >= 0 and owners.has(str(int(Globals.selection.y*width+Globals.selection.x))):
		$GUI/Panel2/VBoxContainer/OwnerLabel.text = owners[str(int(Globals.selection.y*width+Globals.selection.x))]
	else:
		$GUI/Panel2/VBoxContainer/OwnerLabel.text = ""


func _on_powerplant_pressed() -> void:
	Globals.ws.send_text(JSON.stringify({
		"ActionName": "PlacePowerPlant",
		"Data": str(int(Globals.selection.x)) + " " + str(int(Globals.selection.y))
	}))


func _on_barracks_pressed() -> void:
	Globals.ws.send_text(JSON.stringify({
		"ActionName": "PlaceBarracks",
		"Data": str(int(Globals.selection.x)) + " " + str(int(Globals.selection.y))
	}))


func _on_mine_pressed() -> void:
	Globals.ws.send_text(JSON.stringify({
		"ActionName": "PlaceMine",
		"Data": str(int(Globals.selection.x)) + " " + str(int(Globals.selection.y))
	}))


func _on_summon_soldier() -> void:
	Globals.ws.send_text(JSON.stringify({
		"ActionName": "SummonSoldier",
		"Data": str(int(Globals.selection.x)) + " " + str(int(Globals.selection.y))
	}))


func _unhandled_key_input(event: InputEvent) -> void:
	if event.is_action_released("set_target"):
		for child in $entities.get_children():
			if child.player == Globals.token:
				child.target = Globals.selection + Vector2(randi_range(-2,2),randi_range(-2,2))


func _on_factory_pressed() -> void:
	Globals.ws.send_text(JSON.stringify({
		"ActionName": "PlaceFactory",
		"Data": str(int(Globals.selection.x)) + " " + str(int(Globals.selection.y))
	}))
