using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;


namespace pcsk1.src.Saves
{
    public sealed class SaveData
    {
        public int Version { get; set; } = 1;

        public string CurrentAct { get; set; } = "ato_1";
        public string CurrentMap { get; set; } = "mapaa";

        public Vector2 PlayerPosition { get; set; }

        public int Deaths { get; set; }

        public Dictionary<string, bool> Flags { get; set; }
            = new();

        public Dictionary<string, List<string>> TalkedNpcs { get; set; }
            = new();

        public bool HasFlag(string key)
        {
            return Flags.TryGetValue(key, out bool value) && value;
        }

        public void setFlag(string key, bool value = true)
        {
            Flags[key] = value;
        }

        public void RegisterNpc(string act, string npcId)
        {
            if(!TalkedNpcs.TryGetValue(act, out var npcs))
            {
                npcs = new List<string>();
                TalkedNpcs[act] = npcs;
            }
            if(!npcs.Contains(npcId))
            {
                npcs.Add(npcId);
            }

        }

        public bool HasTalkedTo(string act, string npcId)
        {
            return TalkedNpcs.TryGetValue(act, out var npcs) && npcs.Contains(npcId); 
        }

        public void RegisterDeath()
        {
            Deaths++;
        }
    }
}
