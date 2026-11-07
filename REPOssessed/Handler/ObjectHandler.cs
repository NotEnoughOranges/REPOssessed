using Photon.Pun;
using REPOssessed.Manager;
using REPOssessed.Util;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace REPOssessed.Handler
{
    public class ObjectHandler
    {
        public PhysGrabObject? physGrabObject;
        public ItemAttributes? itemAttributes;
        public ValuableObject? valuableObject;
        public ValuableDiscoverCustom? valuableDiscoverCustom;
        public PhysGrabObjectImpactDetector? physGrabObjectImpactDetector;
        public Trap? trap;
        public EnemyRigidbody? enemyRigidbody;
        public ItemBattery? itemBattery;

        public ObjectHandler(PhysGrabObject physGrabObject)
        {
            this.physGrabObject = physGrabObject;
            this.itemAttributes = physGrabObject?.GetComponent<ItemAttributes>();
            this.valuableObject = physGrabObject?.GetComponent<ValuableObject>();
            this.valuableDiscoverCustom = physGrabObject?.GetComponent<ValuableDiscoverCustom>();
            this.physGrabObjectImpactDetector = physGrabObject?.GetComponent<PhysGrabObjectImpactDetector>();
            this.trap = physGrabObject?.GetComponent<Trap>();
            this.enemyRigidbody = physGrabObject?.GetComponent<EnemyRigidbody>();
            this.itemBattery = physGrabObject?.GetComponent<ItemBattery>();
        }

        public string GetName()
        {
            if (IsShopItem()) return itemAttributes?.item?.itemName ?? "Unknown";
            if (valuableDiscoverCustom != null)
            {
                string nameVal = physGrabObject?.name?.Replace("(Clone)", "").Trim() ?? "Unknown";
                if (nameVal.Contains("Cosmetic World Object -"))
                {
                    int dashIndex = nameVal.IndexOf('-');
                    if (dashIndex >= 0)
                    {
                        string rarity = nameVal[(dashIndex + 1)..].Trim();
                        return $"Cosmetic Crate ({rarity})";
                    }
                }
                return nameVal;
            }
            string name = physGrabObject?.name?.Replace("(Clone)", "").Replace("Valuable", "").Replace("Museum", "").Replace("Manor", "").Replace("Arctic", "").Replace("Wizard", "").Trim() ?? "Unknown";
            int dash = name.IndexOf('-');
            return dash < 0 ? name : $"{name[(dash + 1)..].Trim()} {name[..dash].Trim()}";
        }

        public void Break(bool effects)
        {
            if (!GameUtil.IsMasterClient() || IsEnemy() || IsPlayer()) return;
            if (!SemiFunc.IsMultiplayer())
            {
                physGrabObjectImpactDetector?.DestroyObjectRPC(effects);
                return;
            }
            physGrabObjectImpactDetector?.Reflect()?.GetValue<PhotonView>("photonView")?.RPC("DestroyObjectRPC", RpcTarget.All, effects);
        }
        public void Damage(int valueLost, int breakLevel = 1, bool loseValue = true)
        {
            if (!GameUtil.IsMasterClient() || IsEnemy() || IsPlayer()) return;
            if (!SemiFunc.IsMultiplayer())
            {
                physGrabObjectImpactDetector?.Reflect()?.Invoke("BreakRPC", valueLost, Vector3.zero, breakLevel, loseValue);
                return;
            }
            physGrabObjectImpactDetector?.Reflect()?.GetValue<PhotonView>("photonView")?.RPC("BreakRPC", RpcTarget.All, valueLost, Vector3.zero, breakLevel, loseValue);
        }
        public void Teleport(Vector3 position, Quaternion rotation) => physGrabObject?.Teleport(position, rotation);
        public bool IsShopItem() => itemAttributes != null;
        public bool IsCosmeticCrate() => valuableDiscoverCustom != null;
        public float GetValue() => valuableObject?.Reflect().GetValue<float>("dollarValueCurrent") ?? 0f;

        public string GetCosmeticRarity()
        {
            if (!IsCosmeticCrate()) return "";
            string name = GetName();
            int start = name.IndexOf('(');
            int end = name.IndexOf(')');
            return start >= 0 && end > start ? name[(start + 1)..end].Trim() : "";
        }

        public Color GetCosmeticRarityColor()
        {
            string rarity = GetCosmeticRarity();
            return rarity.ToLower() switch
            {
                "common" => Color.green,
                "uncommon" => Color.blue,
                "rare" => Color.magenta,
                "ultra rare" => Color.yellow,
                _ => Color.white
            };
        }

        public void SetValue(float value)
        {
            if (!GameUtil.IsMasterClient() || !IsValuable()) return;
            if (!SemiFunc.IsMultiplayer())
            {
                valuableObject?.DollarValueSetRPC(value);
                return;
            }
            valuableObject?.Reflect()?.GetValue<PhotonView>("photonView")?.RPC("DollarValueSetRPC", RpcTarget.All, value);
        }
        public int GetMaxBattery() => StatsManager.instance?.GetBatteryLevel(itemAttributes?.Reflect()?.GetValue<string>("instanceName")) ?? 0;
        public void ChargeBattery(int chargeAmount)
        {
            if (!GameUtil.IsMasterClient()) return;
            itemBattery?.SetBatteryLife(chargeAmount);
        }
        public bool IsPlayer() => physGrabObject?.Handle()?.GetName()?.Contains("Player") ?? false || (physGrabObject?.Reflect()?.GetValue<bool>("isPlayer") ?? false);
        public bool IsEnemy() => enemyRigidbody != null || (physGrabObject?.Reflect()?.GetValue<bool>("isEnemy") ?? false);
        public bool IsValuable() => (physGrabObject?.Reflect().GetValue<bool>("isValuable") ?? false) || (valuableDiscoverCustom != null);
        public bool IsNonValuable() => physGrabObject?.Reflect().GetValue<bool>("isNonValuable") ?? false;
        public bool IsHinge() => physGrabObject?.Reflect().GetValue<bool>("hasHinge") ?? false;
        public bool IsCart() => physGrabObject?.Reflect().GetValue<bool>("isCart") ?? false;
        public bool IsInCart() => GameObjectManager.carts?.Any(c => c?.physGrabInCart?.Reflect().GetValue<List<PhysGrabInCart.CartObject>>("inCartObjects")?.Any(i => i?.physGrabObject == physGrabObject) == true) ?? false;
        public bool IsInExtraction() => valuableObject?.Reflect()?.GetValue<RoomVolumeCheck>("roomVolumeCheck")?.CurrentRooms?.Any(r => r != null && r.Extraction) ?? false;
        public bool IsTrap() => trap != null;
        public bool CurrentlyHeld() => (physGrabObject?.grabbed ?? false) || (physGrabObject?.grabbedLocal ?? false);
    }

    public static class ObjectHandlerExtensions
    {
        public static Dictionary<int, ObjectHandler> ObjectHandlers = new Dictionary<int, ObjectHandler>();

        public static ObjectHandler? Handle(this PhysGrabObject physGrabObject)
        {
            if (physGrabObject == null) return null;
            int id = physGrabObject.GetInstanceID();
            if (!ObjectHandlers.TryGetValue(id, out ObjectHandler objectHandler))
            {
                objectHandler = new ObjectHandler(physGrabObject);
                ObjectHandlers[id] = objectHandler;
            }
            return objectHandler;
        }
    }
}
