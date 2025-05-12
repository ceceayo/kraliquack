values = split(actionData, " ")

if #values ~= 5 then
    Log("Invalid number of arguments")
    return
end


id = tonumber(values[1])
rotation = tonumber(values[2])
speed = tonumber(values[3])
x = tonumber(values[4])
y = tonumber(values[5])

if id == nil or rotation == nil or speed == nil or x == nil or y == nil then
    Log("Invalid arguments")
    return
end

if entities[id] == nil then
    Log("Entity does not exist")
    return
end

if entities[id].owner ~= userToken then
    Log("Entity does not belong to user")
    return
end

entities[id].x = x
entities[id].y = y
entities[id].orientation = rotation
entities[id].speed = speed


for k,v in iterateDict(users) do
    MessageUser(k, "entity", tostring(id) .. "," .. tostring(x) .. "," .. tostring(y) .. "," .. tostring(entities[id].type) .. "," .. tostring(entities[id].owner) .. "," .. tostring(rotation) .. "," .. tostring(speed) .. "," .. tostring(entities[id].health))
end