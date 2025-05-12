
if users[userToken].Data["progressionState"] ~= "joined" then
    Log("User not in joined state")
    return
end

data = {}
for u in string.gmatch(actionData, "%-?%d+") do
    data[#data + 1] = u
end

x = tonumber(data[1])
y = tonumber(data[2])

team = tonumber(data[3])

if team ~= 1 and team ~= 2 then
    Log("Invalid team")
    return
end

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

if userMakePurchase(userToken, 2500) == false then
    Log("User cannot afford HQ")
    return
end

Log("All checks passed")

userMakePurchaseApply(userToken, 2500)

if team == 1 then
    tiles[y * width + x].tile = "h"
else
    tiles[y * width + x].tile = "H"
end
tiles[y * width + x].owner = userToken



users[userToken].Data["progressionState"] = "placedHQ"
MessageUser(userToken, "progressionState", "placedHQ")
users[userToken].Data["team"] = tostring(team)

for k,v in iterateDict(users) do
    MessageUser(k, "board", tostring(y*width+x) .. "," .. tostring(tiles[y * width + x].tile))
    MessageUser(k, "owner", tostring(y*width+x) .. "," .. tostring(tiles[y * width + x].owner))	
    MessageUser(k, "userTeam", userToken .. "," .. tostring(team))
    MessageUser(k, "chat", "User " .. userToken .. " has placed their HQ at " .. tostring(x) .. "," .. tostring(y) .. " for team " .. tostring(team))
end

users[userToken].Data["hqPos"] = tostring(y*width+x)


users[userToken].Data["hqHealth"] = "350"
MessageUser(userToken, "hqHealth", "350")