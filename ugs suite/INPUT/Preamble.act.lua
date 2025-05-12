function iterateDict(d)
    local enumerator = d:GetEnumerator()
    return function()
        if enumerator:MoveNext() then
            local pair = enumerator.Current
            return pair.Key, pair.Value
        end
    end
end

function dump(o)
    if type(o) == 'table' then
       local s = '{ '
       for k,v in pairs(o) do
          if type(k) ~= 'number' then k = '"'..k..'"' end
          s = s .. '['..k..'] = ' .. dump(v) .. ','
       end
       return s .. '} '
    else
       return tostring(o)
    end
end


 

function isValidCell(x,y)
    if x < 0 or x >= width then
        return false
    end
    if y < 0 or y >= height then
        return false
    end
    return true
end


function cellCanBeBuiltOn(x,y)
    tile = tiles[y * width + x]
    if tile.owner ~= "" then
        return false
    else
        if tile.tile == "." then
            return true
        else
            return false
        end
    end
end

function userMakePurchase(u,a)
    current_cash = tonumber(users[u].Data["cash"])
    if current_cash < a then
        MessageUser(u, "chat", "Purchase failed - not enough cash")
        return false
    else
        return true
    end
end

function userRequirePower(u,a)
    current_power = tonumber(users[u].Data["powerGeneration"]) - tonumber(users[u].Data["powerConsumption"])
    if current_power < a then
        MessageUser(u, "chat", "Out of power")
        return false
    else
        return true
    end
end

function userMakePurchaseApply(u,a)
    current_cash = tonumber(users[u].Data["cash"])
    if current_cash < a then
        return false
    else
        current_cash = current_cash - a
        users[u].Data["cash"] = tostring(current_cash)
        MessageUser(u, "cash", tostring(current_cash))
        return true
    end
end

function userRequirePowerApply(u,a)
    current_power = tonumber(users[u].Data["powerGeneration"]) - tonumber(users[u].Data["powerConsumption"])
    if current_power < a then
        return false
    else
        current_power = current_power - a
        users[u].Data["powerConsumption"] = tostring(tonumber(users[u].Data["powerConsumption"]) + a)
        MessageUser(u, "power", users[u].Data["powerGeneration"] .. " " .. users[u].Data["powerConsumption"])
        return true
    end
end

function split(str, pat)
    local t = {}  -- NOTE: use {n = 0} in Lua-5.0
    local fpat = "(.-)" .. pat
    local last_end = 1
    local s, e, cap = str:find(fpat, 1)
    while s do
       if s ~= 1 or cap ~= "" then
          table.insert(t, cap)
       end
       last_end = e+1
       s, e, cap = str:find(fpat, last_end)
    end
    if last_end <= #str then
       cap = str:sub(last_end)
       table.insert(t, cap)
    end
    return t
 end
