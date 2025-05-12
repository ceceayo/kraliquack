extends Node


var token: String = ""
var server: String = ""
var worldId: int = 0
var ws: WebSocketPeer = null
var team: int = 0
var astar: AStarGrid2D = AStarGrid2D.new()
var selection = Vector2.ZERO


func _ready() -> void:
	astar.region = Rect2i(-2,-2,79,79)
	astar.cell_size = Vector2i(64,64)
	astar.offset = Vector2i.ZERO
	astar.diagonal_mode = AStarGrid2D.DIAGONAL_MODE_NEVER
	astar.update()

func change_scene_to_node(node):
	var tree = get_tree()
	var cur_scene = tree.get_current_scene()
	tree.get_root().add_child(node)
	tree.get_root().remove_child(cur_scene)
	cur_scene.queue_free()
	tree.set_current_scene(node)
