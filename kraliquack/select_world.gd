extends Control

var NewWorldScene = preload("res://new_world.tscn")
var ConnectWorldScene = preload("res://connect_world.tscn")

var worlds = []

func _ready():
	$GetWorldsHTTPRequest.request("http"+Globals.server+"worlds", PackedStringArray(["token: "+Globals.token]), HTTPClient.Method.METHOD_GET)


func _on_get_worlds_http_request_request_completed(result: int, response_code: int, headers: PackedStringArray, body: PackedByteArray) -> void:
	if response_code == 200:
		$ItemList.clear()
		worlds = []
		var json = JSON.parse_string(body.get_string_from_utf8())
		print(json)
		assert (json['success'] == true)
		for world in json['worlds']:
			print(world)
			$ItemList.add_item(str(int(world["id"]))+" : "+world['title'])
			worlds.append(int(world["id"]))
	else:
		print("error")


func _on_new_game_button_pressed() -> void:
	Globals.change_scene_to_node(NewWorldScene.instantiate())


func _on_item_list_item_activated(index: int) -> void:
	Globals.worldId = worlds[index]
	
	Globals.change_scene_to_node(ConnectWorldScene.instantiate())
