width  = 75
height = 75

time = 0
t2 = 0

tiles = {}

generators = {}

entities = {}
ec = 0

for y = 0, height do
    for x = 0, width do
        if y == 0 or y == height or x == 0 or x == width - 1 then
            tiles[y * width + x] = {x = x, y = y, tile = "#", owner = "system"}
        elseif (y * width + x) % 13 == 3 then
            tiles[y * width + x] = {x = x, y = y, tile = "r", owner = ""}
        else
            tiles[y * width + x] = {x = x, y = y, tile = ".", owner = ""}
        end
    end
end

Log("Created board :p")


Log("Total tiles: " .. tostring(#tiles))

c = 0