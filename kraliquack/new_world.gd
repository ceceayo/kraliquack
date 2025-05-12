extends Control

var SelectWorldScene = load("res://select_world.tscn")
var ConnectWorldScene = preload("res://connect_world.tscn")

func _on_go_back_pressed() -> void:
	Globals.change_scene_to_node(SelectWorldScene.instantiate())


func _on_start_world_pressed() -> void:
	$HTTPRequest.request(
		"http"+Globals.server+"worlds", 
		PackedStringArray([
			"token: "+Globals.token,
			"Content-Type: application/json"
		]), 
		HTTPClient.METHOD_POST, 
		JSON.stringify({"worldName": $WorldName.text}))


func _on_http_request_request_completed(result: int, response_code: int, headers: PackedStringArray, body: PackedByteArray) -> void:
	if response_code == 200:
		var json = JSON.parse_string(body.get_string_from_utf8())
		print(json)
		assert (json['success'] == true)
		Globals.worldId = int(json['message'])
		
		Globals.change_scene_to_node(ConnectWorldScene.instantiate())
	else:
		print("error")
