time = time + delta
t2 = t2 + delta


for k, v in iterateDict(users) do
    da = ""
    db = ""
    --Log(tostring(c))
    for i = 0, 99 do
        da = da .. tostring((c+i)%#tiles) .. "," .. tostring(tiles[(c+i)%#tiles].tile) .. ":"
        if tiles[(c+i)%#tiles].owner ~= "" then
            db = db .. tostring((c+i)%#tiles) .. "," .. tostring(tiles[(c+i)%#tiles].owner) .. ":"
        end
    end
    MessageUser(k, "board", da) -- Send the constructed message to the user
    MessageUser(k, "owner", db) -- Send the constructed message to the user

    
end

c = c + 99
c = c % #tiles


-- Each second, add cashGeneration to cash
if t2 >= 1 then
    t2 = t2 - 1
    for k, v in iterateDict(users) do
        users[k].Data["cash"] = tostring(tonumber(v.Data["cash"]) + tonumber(v.Data["cashGeneration"]))
        MessageUser(k, "cash", users[k].Data["cash"])
    end
    for k, v in pairs(generators) do
        generators[k].time = generators[k].time + 1
        if generators[k].time == generators[k].timeNeeded then
            if generators[k].type == "soldier" then
                if users[generators[k].owner].Data["team"] == "1" then
                    entities[ec] = {x = generators[k].x, y = generators[k].y, type = "duck", owner = generators[k].owner, orientation = 0, speed = 0, health = 25}
                else
                    entities[ec] = {x = generators[k].x, y = generators[k].y, type = "bug", owner = generators[k].owner, orientation = 0, speed = 0, health = 25}
                end
                
                for k2,v2 in iterateDict(users) do
                    MessageUser(k2, "entity", tostring(ec) .. "," .. tostring(entities[ec].x) .. "," .. tostring(entities[ec].y) .. "," .. tostring(entities[ec].type) .. "," .. tostring(entities[ec].owner) .. "," .. tostring(entities[ec].orientation) .. "," .. tostring(entities[ec].speed) .. "," .. tostring(entities[ec].health))
                end
                ec = ec + 1
            elseif generators[k].type == "tank" then
                if users[generators[k].owner].Data["team"] == "1" then
                    entities[ec] = {x = generators[k].x, y = generators[k].y, type = "ducktank", owner = generators[k].owner, orientation = 0, speed = 0, health = 80}
                else
                    entities[ec] = {x = generators[k].x, y = generators[k].y, type = "bugtank", owner = generators[k].owner, orientation = 0, speed = 0, health = 80}
                end
                
                for k2,v2 in iterateDict(users) do
                    MessageUser(k2, "entity", tostring(ec) .. "," .. tostring(entities[ec].x) .. "," .. tostring(entities[ec].y) .. "," .. tostring(entities[ec].type) .. "," .. tostring(entities[ec].owner) .. "," .. tostring(entities[ec].orientation) .. "," .. tostring(entities[ec].speed) .. "," .. tostring(entities[ec].health))
                end
                ec = ec + 1
            end
            generators[k] = nil
        end
    end
end
