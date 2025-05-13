if users[userToken].Data["progressionState"] ~= "placedFactory" then
    Log("User in wrong state")
    return
end

data = {}
for u in string.gmatch(actionData, "%-?%d+") do
    data[#data + 1] = u
end

x = tonumber(data[1])
y = tonumber(data[2])


data = nil

Log("X: " .. tostring(x))
Log("Y: " .. tostring(y))


if isValidCell(x,y) == false then
    Log("Invalid cell")
    return
end

if userMakePurchase(userToken, 1000) == false then
    Log("User cannot afford Barracks")
    return
end

if generators[y * width + x] ~= nil then
    Log("Generator already exists")
    return
end

if tiles[y * width + x].tile ~= "f" then
    Log("Tile is not a factory")
    return
end

if tiles[y * width + x].owner ~= userToken then
    Log("Tile is not owned by user")
    return
end

Log("All checks passed")	

userMakePurchaseApply(userToken, 1000)

generators[y * width + x] = {}
generators[y * width + x].owner = userToken
generators[y * width + x].type = "tank"
generators[y * width + x].timeNeeded = 25
generators[y * width + x].time = 0
generators[y * width + x].x = x
generators[y * width + x].y = y