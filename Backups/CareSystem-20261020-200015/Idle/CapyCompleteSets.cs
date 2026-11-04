using System;
using UnityEngine;

namespace Capybara67.Idle {
    [Serializable] public sealed class CapySetDefinition {
        public string id,slug,name,color;
        public string[] legacy;
        public int stage;
        public double price;
    }
    [Serializable] sealed class CapySetList { public CapySetDefinition[] sets; }
    public static class CapyCompleteSets {
        static CapySetDefinition[] definitions;
        public static CapySetDefinition[] All {
            get {
                if(definitions==null) {
                    var text=Resources.Load<TextAsset>("CompleteSets/Sets");
                    definitions=text?JsonUtility.FromJson<CapySetList>(text.text).sets:Array.Empty<CapySetDefinition>();
                }
                return definitions;
            }
        }
        public static bool IsSet(string id)=>id!=null&&id.StartsWith("set_",StringComparison.Ordinal);
        public static bool Visible(IdleItem item)=>item!=null&&(item.IsSet||item.slot>=2);
        public static void Migrate(IdleSave save) {
            if(save.wardrobeVersion>=1)return;
            string oldHead=save.equipped[0],oldClothes=save.equipped[1],chosen=null;
            foreach(var set in All) {
                bool owned=set.id=="set_gardener";
                foreach(var legacy in set.legacy) {
                    if(save.owned.Contains(legacy))owned=true;
                    if(legacy==oldHead||(chosen==null&&legacy==oldClothes))chosen=set.id;
                }
                if(owned&&!save.owned.Contains(set.id))save.owned.Add(set.id);
            }
            if(chosen!=null)save.equipped[0]=chosen;
            if(IsSet(save.equipped[0]))save.equipped[1]="";
            // Old item ownership remains in the save for a reversible, idempotent migration.
            save.wardrobeVersion=1;
        }
    }
}
