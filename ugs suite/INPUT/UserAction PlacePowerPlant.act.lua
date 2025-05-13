
if users[userToken].Data["progressionState"] ~= "placedHQ" and users[userToken].Data["progressionState"] ~= "placedBarracks" and users[userToken].Data["progressionState"] ~= "placedFactory" then
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

if cellCanBeBuiltOn(x,y) == false then
    Log("Cell cannot be built on")
    return
end

if userMakePurchase(userToken, 1000) == false then
    Log("User cannot afford Powerplant")
    return
end

Log("All checks passed")

userMakePurchaseApply(userToken, 1000)

tiles[y * width + x].tile = "p"
tiles[y * width + x].owner = userToken

users[userToken].Data["powerGeneration"] = tostring(tonumber(users[userToken].Data["powerGeneration"]) + 75)
users[userToken].Data["powerConsumption"] = tostring(tonumber(users[userToken].Data["powerConsumption"]) + 15)

MessageUser(userToken, "power", users[userToken].Data["powerGeneration"] .. " " .. users[userToken].Data["powerConsumption"])

for k, v in iterateDict(users) do
    MessageUser(k, "owner", tostring(y*width+x) .. "," .. tostring(tiles[y * width + x].owner))	
    MessageUser(k, "board", tostring(y*width+x) .. "," .. tostring(tiles[y * width + x].tile))
end

